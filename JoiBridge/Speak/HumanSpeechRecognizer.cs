using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace JoiBridge.Speak
{
    internal class HumanSpeechRecognizer
    {
        string SpeechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
        string SpeechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");

        SpeechRecognizer SpeechToTextService = null;
        Microsoft.CognitiveServices.Speech.Audio.AudioConfig AudioConf = null;

        KeywordRecognizer KdRecognizer = null;
        KeywordRecognitionModel KeywordModel_StartRecord = null;

        bool HumanVoiceRecording = false;

        public bool Valid()
        {
            return SpeechToTextService != null;
        }

        public void InitSpeechToTextService()
        {
            var SpeechConfig = Microsoft.CognitiveServices.Speech.SpeechConfig.FromSubscription(SpeechKey, SpeechRegion);
            SpeechConfig.SpeechRecognitionLanguage = "zh-CN";
            SpeechConfig.SpeechSynthesisVoiceName = "zh-CN-XiaochenNeural";

            var AutoDetectSourceLanguageConfig =
                Microsoft.CognitiveServices.Speech.AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", "zh-CN" });

            AudioConf = AudioConfig.FromDefaultMicrophoneInput();

            try
            {
                string KeywordModel_StartRecord_FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + Program.DataDirPath, "6cff1c10-91d5-4fef-9b42-76c36ac75161.table");
                KeywordModel_StartRecord = KeywordRecognitionModel.FromFile(KeywordModel_StartRecord_FilePath);
                KdRecognizer = new KeywordRecognizer(AudioConf);

                SpeechToTextService = new SpeechRecognizer(SpeechConfig, AudioConf);
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteLine(ex.ToString(), ConsoleColor.Red);
            }
        }

        async Task Wakeup()
        {
            KeywordRecognitionResult result = await KdRecognizer.RecognizeOnceAsync(KeywordModel_StartRecord);
        }

        public async Task<string> RecordTextFromVoice()
        {
            await Wakeup();

            string Ret = string.Empty;

            Console.WriteLine("正在采集你的声音");
            var SpeechRecognitionResult = await SpeechToTextService.RecognizeOnceAsync();
            Ret = OutputSpeechRecognitionResult(SpeechRecognitionResult);

            return Ret;
        }

        string OutputSpeechRecognitionResult(SpeechRecognitionResult SpeechRecognitionResult)
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
    }
}
