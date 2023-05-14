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

    static string SpeechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
    static string SpeechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");

    static bool Runing = false;
    static bool HumanVoiceRecording = false;
    static SpeechRecognizer SpeechToTextService = null;
    static Microsoft.CognitiveServices.Speech.Audio.AudioConfig AudioConf = null;
    static async Task<string> GetHumanInput()
    {
        return await Task.Run(() =>
        {
            return Console.ReadLine();
        });
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

    static void InitSpeechToTextService()
    {
        var SpeechConfig = Microsoft.CognitiveServices.Speech.SpeechConfig.FromSubscription(SpeechKey, SpeechRegion);
        SpeechConfig.SpeechRecognitionLanguage = "zh-CN";
        SpeechConfig.SpeechSynthesisVoiceName = "zh-CN-XiaochenNeural";

        var AutoDetectSourceLanguageConfig =
            Microsoft.CognitiveServices.Speech.AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", "zh-CN" });

        AudioConf = AudioConfig.FromDefaultMicrophoneInput();

        try
        {
            SpeechToTextService = new SpeechRecognizer(SpeechConfig, AudioConf);
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex);
        }
    }

    async static Task<string> RecordTextFromVoice()
    {
        string Ret = string.Empty;

        Console.WriteLine("正在采集你的声音");
        var SpeechRecognitionResult = await SpeechToTextService.RecognizeOnceAsync();
        Ret = OutputSpeechRecognitionResult(SpeechRecognitionResult);

        return Ret;
    }

    static string OutputSpeechRecognitionResult(SpeechRecognitionResult SpeechRecognitionResult)
    {
        string Ret = string.Empty;

        switch (SpeechRecognitionResult.Reason)
        {
            case ResultReason.RecognizedSpeech:
                Console.WriteLine($"RECOGNIZED: Text={SpeechRecognitionResult.Text}");
                Ret = SpeechRecognitionResult.Text;
                break;
            case ResultReason.NoMatch:
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                break;
            case ResultReason.Canceled:
                var cancellation = CancellationDetails.FromResult(SpeechRecognitionResult);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                }
                break;
        }

        return Ret;
    }

    async static Task Main(string[] args)
    {
        InitSpeechToTextService();

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
