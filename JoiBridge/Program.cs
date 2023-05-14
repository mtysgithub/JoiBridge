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

        return Brain.ParseGM(HumanInputString);
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
