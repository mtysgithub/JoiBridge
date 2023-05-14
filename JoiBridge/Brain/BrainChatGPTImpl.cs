using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NET6_0_OR_GREATER
using LaserCatEyes.HttpClientListener;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.GPT3.Extensions;
using OpenAI.GPT3.Interfaces;

using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;

using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using JoiBridge.Speak;
using System.Text.RegularExpressions;

namespace JoiBridge.Brain
{
    internal class BrainChatGPTImpl : BrainBase
    {
        #region 配置
        private int MaxHistoryEntries = 20;
        #endregion

        public List<ChatMessage> HistoricalMessages = new List<ChatMessage>();
        private IOpenAIService? Sdk;

        public override async Task Build(SpeakerBase InSpeaker) 
        {
            await base.Build(InSpeaker);

            OpenAiOptions Option = new OpenAiOptions()
            {
                ApiKey = Environment.GetEnvironmentVariable("ApiKey"),
                Organization = Environment.GetEnvironmentVariable("Organization")
            };
            var OpenAiService = new OpenAIService(Option);

            Sdk = OpenAiService;

            Begin();
        }

        public async Task<String> Talk(string userMessage)
        {
            try
            {
                // 将用户消息添加到历史记录
                HistoricalMessages.Add(new(StaticValues.ChatMessageRoles.User, userMessage));

                int k = MaxHistoryEntries; // 想要获取的最后k条数据
                List<ChatMessage> LastKMessages;

                // 如果 HistoricalMessages 中的消息数量少于 k，则返回全部消息
                if (HistoricalMessages.Count <= k)
                {
                    LastKMessages = HistoricalMessages;
                }
                else
                {
                    LastKMessages = HistoricalMessages.Skip(HistoricalMessages.Count - k).Take(k).ToList();
                }

                var CompletionResult = Sdk.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
                {
                    Messages = LastKMessages,
                    MaxTokens = 250,
                    Model = Models.ChatGpt3_5Turbo0301
                });

                Console.WriteLine("## Joi的消息: ");
                string CompleteMessage = "";
                string SpeakBuff = string.Empty;
                await foreach (var Completion in CompletionResult)
                {
                    if (Completion.Successful)
                    {
                        CompleteMessage += Completion.Choices.First().Message.Content;
                        Console.Write(Completion.Choices.First().Message.Content);

                        SpeakBuff += Completion.Choices.First().Message.Content;
                        if (SpeakBuff.Contains(",") || SpeakBuff.Contains(".") || SpeakBuff.Contains("?") || SpeakBuff.Contains("!") ||
                            SpeakBuff.Contains("，") || SpeakBuff.Contains("。") || SpeakBuff.Contains("？") || SpeakBuff.Contains("！"))
                        {
                            await Speaker.Speak(SpeakBuff);
                            SpeakBuff = string.Empty;
                        }
                    }
                    else
                    {
                        if (Completion.Error == null)
                        {
                            throw new Exception("Unknown Error");
                        }

                        Console.WriteLine($"{Completion.Error.Code}: {Completion.Error.Message}");
                    }
                }

                if (!string.IsNullOrEmpty(SpeakBuff))
                {
                    await Speaker.Speak(SpeakBuff);
                    SpeakBuff = string.Empty;
                }

                Console.WriteLine();

                //Console.WriteLine("Check Complete Message: " + CompleteMessage);
                HistoricalMessages.Add(new(StaticValues.ChatMessageRoles.Assistant, CompleteMessage));

                return CompleteMessage;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Begin()
        {
            SetMask("Mask_Joi_Default.txt");
        }

        public override void OutputHistoricalMessages()
        {
            string fileName = $"{Guid.NewGuid()}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + Program.DataDirPath, fileName);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var message in HistoricalMessages)
                {
                    writer.WriteLine($"{message.Role}: {message.Content}");
                }
            }
        }

        public override void SetMask(string MaskFileName)
        {
            HistoricalMessages.Clear();

            // 从文件夹位置中读取文件
            string fileName = MaskFileName;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + Program.DataDirPath, fileName);

            // 读取文件内容并保存到string
            string fileContent;
            try
            {
                fileContent = File.ReadAllText(filePath);
                Console.WriteLine("文件内容为：\n" + fileContent);

                // 2. 将文件内容拆分为行
                string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // 3. 逐行解析文件内容，并根据角色创建 ChatMessage 对象
                foreach (string line in lines)
                {
                    string[] parts = line.Split(new[] { ": " }, 2, StringSplitOptions.None);

                    if (parts.Length != 2) continue;

                    string role = parts[0].ToLower();
                    string content = parts[1];

                    ChatMessage chatMessage;

                    // 4. 为每种角色创建 ChatMessage 对象
                    switch (role)
                    {
                        case "user":
                            chatMessage = ChatMessage.FromUser(content);
                            break;
                        case "assistant":
                            chatMessage = ChatMessage.FromAssistant(content);
                            break;
                        case "system":
                            chatMessage = ChatMessage.FromSystem(content);
                            break;
                        default:
                            continue;
                    }

                    // 5. 将 ChatMessage 对象添加到聊天历史列表中
                    HistoricalMessages.Add(chatMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("读取文件失败：" + ex.Message);
            }
        }

        public override bool ParseGM(string HumanInputString)
        {
            if (base.ParseGM(HumanInputString))
            {
                return true;
            }
            
            if (HumanInputString.ToLower() == "导出对话记录")
            {
                this.OutputHistoricalMessages();
                return true;
            }

            // 使用正则表达式匹配 {面具 "文件名"} 的字符串并提取文件名
            string MaskPattern = @"\{切换面具 (.+?)\}";
            Match MaskMatchRet = Regex.Match(HumanInputString, MaskPattern);

            if (MaskMatchRet.Success)
            {
                string FileName = MaskMatchRet.Groups[1].Value;
                Console.WriteLine("提取到的文件名为：" + FileName);

                this.SetMask(FileName);
                return true;

            }

            string ChangeMemSizePattern = @"\{记忆条数 (.+?)\}";
            Match ChangeMemSizeMatchRet = Regex.Match(HumanInputString, ChangeMemSizePattern);
            if (ChangeMemSizeMatchRet.Success)
            {
                string val = ChangeMemSizeMatchRet.Groups[1].Value;
                MaxHistoryEntries = Int32.Parse(val);
                Console.WriteLine("改变记忆尺寸 {0}条消息", MaxHistoryEntries);
                return true;
            }

            return false;
        }
    }
}
