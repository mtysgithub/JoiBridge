using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JoiBridge.Brain;
using JoiBridge.Speak;


//https://learn.microsoft.com/zh-cn/azure/cognitive-services/speech-service/how-to-recognize-speech?pivots=programming-language-javascript

class Program
{
    static bool Runing = false;

    static async Task<string> GetHumanInput()
    {
        return await Task.Run(() =>
        {
            return Console.ReadLine();
        });
    }

    static bool ParseGM(string HumanInputString, BrainBase Brain)
    {
        if (HumanInputString.ToLower() == "{退出}")
        {
            Runing = false;
            return true;
        }

        if (HumanInputString.ToLower() == "{导出}")
        {
            Brain.OutputHistoricalMessages();
            return true;
        }

        // 使用正则表达式匹配 {面具 "文件名"} 的字符串并提取文件名
        string Pattern = @"\{面具 (.+?)\}";
        Match MatchRet = Regex.Match(HumanInputString, Pattern);

        if (MatchRet.Success)
        {
            string FileName = MatchRet.Groups[1].Value;
            Console.WriteLine("提取到的文件名为：" + FileName);

            Brain.SetMask(FileName);
            return true;

        }

        return false;
    }

    async static Task Main(string[] args)
    {
        SpeakerBase Speaker = new JoiSpeaker();
        Speaker.Build();

        BrainChatGPTImpl Brain = new BrainChatGPTImpl();
        Brain.Build(Speaker);

        Runing = true;
        
        while (Runing)
        {
            Console.WriteLine("## 来自你的消息: ");
            string HumanInputString = await GetHumanInput();

            if (!ParseGM(HumanInputString, Brain))
            {
                await Brain.Talk(HumanInputString);
            }
        }
    }
}
