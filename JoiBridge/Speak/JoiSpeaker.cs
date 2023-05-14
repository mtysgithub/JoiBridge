using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using static System.Net.Mime.MediaTypeNames;

namespace JoiBridge.Speak
{
    internal class JoiSpeaker : SpeakerBase
    {
        static string SpeechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
        static string SpeechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");

        SpeechSynthesizer SpeakHandler;

        public override async Task Build()
        {
            var SpeechConfig = Microsoft.CognitiveServices.Speech.SpeechConfig.FromSubscription(SpeechKey, SpeechRegion);
            SpeechConfig.SpeechRecognitionLanguage = "zh-CN";
            SpeechConfig.SpeechSynthesisVoiceName = "zh-CN-XiaochenNeural";

            var AutoDetectSourceLanguageConfig =
                Microsoft.CognitiveServices.Speech.AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", "zh-CN" });

            SpeakHandler = new SpeechSynthesizer(SpeechConfig);
        }

        public override async Task Speak(string content)
        {
            base.Speak(content);

            var SpeechSynthesisResult = await SpeakHandler.SpeakTextAsync(content);
            OutputSpeechSynthesisResult(SpeechSynthesisResult, content);
        }

        static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
        {
            switch (speechSynthesisResult.Reason)
            {
                case ResultReason.SynthesizingAudioCompleted:
                    //Console.WriteLine($"Speech synthesized for text: [{text}]");
                    break;
                case ResultReason.Canceled:
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
                default:
                    break;
            }
        }


        static void OutputSpeechRecognitionResult(SpeechRecognitionResult SpeechRecognitionResult)
        {
            switch (SpeechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    Console.WriteLine($"RECOGNIZED: Text={SpeechRecognitionResult.Text}");
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
        }
    }
}
