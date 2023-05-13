using System;
using System.IO;
using System.Threading.Tasks;
using JoiBridge.Brain;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

//https://learn.microsoft.com/zh-cn/azure/cognitive-services/speech-service/how-to-recognize-speech?pivots=programming-language-javascript

class Program
{
    // This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
    static string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
    static string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");

    static string Organization = Environment.GetEnvironmentVariable("Organization");
    static string ApiKey = Environment.GetEnvironmentVariable("ApiKey");

    static void OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
    {
        switch (speechRecognitionResult.Reason)
        {
            case ResultReason.RecognizedSpeech:
                Console.WriteLine($"RECOGNIZED: Text={speechRecognitionResult.Text}");
                break;
            case ResultReason.NoMatch:
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                break;
            case ResultReason.Canceled:
                var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                }
                break;
        }
    }

    static async Task<string> GetHumanInput()
    {
        return await Task.Run(() =>
        {
            return Console.ReadLine();
        });
    }

    async static Task Main(string[] args)
    {
        BrainChatGPTImpl Brain = new BrainChatGPTImpl();
        Brain.Build();
        await Brain.Begin();

        while (true)
        {
            Console.Write("User: ");
            string HumanInputString = await GetHumanInput();

            if (HumanInputString.ToLower() == "退出")
            {
                break;
            }

            string JoiResponse = await Brain.Talk(HumanInputString);

            //TODO.
            //Speak
        }

        //var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        //speechConfig.SpeechRecognitionLanguage = "zh-CN";

        //var autoDetectSourceLanguageConfig =
        //    AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", "zh-CN" });

        //using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        //SpeechRecognizer speechRecognizer = null;
        //try
        //{
        //    Console.WriteLine("Speak into your microphone.");
        //    speechRecognizer = new SpeechRecognizer(speechConfig, autoDetectSourceLanguageConfig, audioConfig);
        //    var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
        //    OutputSpeechRecognitionResult(speechRecognitionResult);
        //}
        //finally
        //{
        //    if (speechRecognizer != null)
        //    {
        //        speechRecognizer.Dispose();
        //    }
        //}
    }
}
