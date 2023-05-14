using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using JoiBridge.Brain;
using JoiBridge.Speak;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;

//https://learn.microsoft.com/zh-cn/azure/cognitive-services/speech-service/how-to-recognize-speech?pivots=programming-language-javascript

class Program
{
    public static string DataDirPath = "/Data/";

    static bool Runing = false;

    static HumanSpeechRecognizer MySpeechRecognizer = null;

    static async Task<string> GetHumanInput()
    {
        if ((MySpeechRecognizer != null) && (MySpeechRecognizer.Valid()))
        {
            return await MySpeechRecognizer.RecordTextFromVoice();
        }
        else
        {
            return await Task.Run(() =>
            {
                return Console.ReadLine();
            });
        }
    }

    static bool ParseGM(string HumanInputString, BrainBase Brain)
    {
        if (HumanInputString.ToLower() == "退出")
        {
            Runing = false;
            return true;
        }

        return Brain.ParseGM(HumanInputString);
    }

    async static Task Main(string[] args)
    {
        MySpeechRecognizer = new HumanSpeechRecognizer();
        MySpeechRecognizer.InitSpeechToTextService();

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
