using System;
using System.IO;
using System.Threading.Tasks;
using JoiBridge.Brain;
using JoiBridge.Speak;


//https://learn.microsoft.com/zh-cn/azure/cognitive-services/speech-service/how-to-recognize-speech?pivots=programming-language-javascript

class Program
{
    static string Organization = Environment.GetEnvironmentVariable("Organization");
    static string ApiKey = Environment.GetEnvironmentVariable("ApiKey");

    static async Task<string> GetHumanInput()
    {
        return await Task.Run(() =>
        {
            return Console.ReadLine();
        });
    }

    async static Task Main(string[] args)
    {
        JoiSpeaker Speaker = new JoiSpeaker();
        Speaker.Build();

        BrainChatGPTImpl Brain = new BrainChatGPTImpl();
        Brain.Build(Speaker);
        await Brain.Begin();

        while (true)
        {
            Console.Write("User: ");
            string HumanInputString = await GetHumanInput();

            if (HumanInputString.ToLower() == "退出")
            {
                break;
            }

            await Brain.Talk(HumanInputString);
        }
    }
}
