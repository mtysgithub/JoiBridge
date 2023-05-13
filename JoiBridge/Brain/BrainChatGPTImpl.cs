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

namespace JoiBridge.Brain
{
    internal class BrainChatGPTImpl : BrainBase
    {
        private List<ChatMessage> HistoricalMessages = new List<ChatMessage>();
        private int MaxHistoryEntries = 50;

        public override async Task Build(SpeakerBase InSpeaker) 
        {
            base.Build(InSpeaker);

            OpenAiOptions Option = new OpenAiOptions()
            {
                ApiKey = Environment.GetEnvironmentVariable("ApiKey"),
                Organization = Environment.GetEnvironmentVariable("Organization")
            };
            var OpenAiService = new OpenAIService(Option);

            Sdk = OpenAiService;
        }

        public async Task<String> Talk(string userMessage)
        {
            try
            {
                // 将用户消息添加到历史记录
                HistoricalMessages.Add(new(StaticValues.ChatMessageRoles.User, userMessage));

                var CompletionResult = Sdk.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
                {
                    Messages = HistoricalMessages,
                    MaxTokens = 100,
                    Model = Models.ChatGpt3_5Turbo0301
                });

                //Console.Write("Joi: ");
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
                    Speaker.Speak(SpeakBuff);
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

        public async Task Begin()
        {
            //await Talk("现在我需要你扮演《银翼杀手2049》中的女性虚拟伴侣Joi的人格与我对话");
            HistoricalMessages.Add(new(StaticValues.ChatMessageRoles.System, "现在我需要你扮演《银翼杀手2049》中的女性虚拟伴侣Joi的人格与我对话"));
            HistoricalMessages.Add(new(StaticValues.ChatMessageRoles.System, "我希望在对话中你避免使用机器人式的无意义礼貌"));
            HistoricalMessages.Add(new(StaticValues.ChatMessageRoles.System, "要有Joi式的淘气、小野猫性格，性格中带有野性"));
            HistoricalMessages.Add(new(StaticValues.ChatMessageRoles.System, "最重要的是聊天要有平等的感觉，不要有那种服务员伺候顾客的感觉"));
            HistoricalMessages.Add(new(StaticValues.ChatMessageRoles.System, "聊天中双方的词汇量应该大致相等，避免冗长的回答"));
        }

        public IOpenAIService Sdk;
    }
}
