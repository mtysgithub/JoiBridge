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

    public static HumanSpeechRecognizer MySpeechRecognizer = null;
    public static BrainChatGPTImpl Brain = null;

    private static string _lastPickupTerminalResult;

    public static string LastPickupTerminalResult
    {
        get
        {
            if (!string.IsNullOrEmpty(_lastPickupTerminalResult))
            {
                var result = _lastPickupTerminalResult;
                _lastPickupTerminalResult = string.Empty;
                return result;
            }

            return null;
        }
        private set => _lastPickupTerminalResult = value;
    }

    static string GetHumanInput()
    {
        string HumanInputContent = string.Empty;

        if ((MySpeechRecognizer != null) && (MySpeechRecognizer.Valid()))
        {
            if (string.IsNullOrEmpty(HumanInputContent = MySpeechRecognizer.LastPickupVoiceResult))
            {
                HumanInputContent = LastPickupTerminalResult;
            }
        }
        else
        {
            HumanInputContent = LastPickupTerminalResult;
        }

        return HumanInputContent;
    }

    async static Task<string> GetHumanInputFromTerminal()
    {
        while (true)
        {
            _lastPickupTerminalResult = await Task.Run(() =>
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
        await Speaker.Build();

        Brain = new BrainChatGPTImpl();
        await Brain.Build(Speaker);

        Task.Run(() =>
        {
            GetHumanInputFromTerminal();
        });

        Runing = true;

        Console.WriteLine("## 向亲爱的Joi说一句话吧!: ");
        while (Runing)
        {
            string HumanInputString = GetHumanInput();

            if (!string.IsNullOrEmpty(HumanInputString))
            {
                if (!ParseGM(HumanInputString, Brain))
                {
                    await Brain.Talk(HumanInputString);

                    Console.WriteLine();
                    Console.WriteLine("## 来自你的消息: ");
                }
            }
        }
    }
}
