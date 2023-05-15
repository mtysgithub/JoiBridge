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

        private string _lastPickupVoiceResult;

        public string LastPickupVoiceResult
        {
            get
            {
                if (_lastPickupVoiceResult != null)
                {
                    var result = _lastPickupVoiceResult;
                    _lastPickupVoiceResult = null;
                    return result;
                }

                return null;
            }
            private set => _lastPickupVoiceResult = value;
        }

        public bool Valid()
        {
            return SpeechToTextService != null;
        }

        public void InitSpeechToTextService()
        {
            var SpeechConfig = Microsoft.CognitiveServices.Speech.SpeechConfig.FromSubscription(SpeechKey, SpeechRegion);
            SpeechConfig.SpeechRecognitionLanguage = "zh-CN";

            //https://learn.microsoft.com/zh-cn/azure/cognitive-services/speech-service/language-support?tabs=tts
            SpeechConfig.SpeechSynthesisVoiceName = "zh-CN-XiaochenNeural";

            var AutoDetectSourceLanguageConfig =
                Microsoft.CognitiveServices.Speech.AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", "zh-CN" });

            AudioConf = AudioConfig.FromDefaultMicrophoneInput();

            try
            {
                string KeywordModel_StartRecord_FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + Program.DataDirPath, "Kd_Joi_EN.table");
                KeywordModel_StartRecord = KeywordRecognitionModel.FromFile(KeywordModel_StartRecord_FilePath);
                KdRecognizer = new KeywordRecognizer(AudioConf);

                SpeechToTextService = new SpeechRecognizer(SpeechConfig, AudioConf);
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteLine(ex.ToString(), ConsoleColor.Red);
            }

            // 在新线程中启动 RecordTextFromVoice
            Task.Run(() => RecordTextFromVoice());
        }

        async Task Wakeup()
        {
            KeywordRecognitionResult SpeechRecognitionResult = await KdRecognizer.RecognizeOnceAsync(KeywordModel_StartRecord);

            switch (SpeechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedKeyword:
                    Console.WriteLine($"RECOGNIZED: Text={SpeechRecognitionResult.Text}");
                    break;
                case ResultReason.RecognizingKeyword:
                    Console.WriteLine($"RECOGNIZED: Text={SpeechRecognitionResult.Text}");
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

        public async void RecordTextFromVoice()
        {
            while (true)
            {
                if (Valid())
                {
                    Console.WriteLine("## 来自你的消息: ");

                    var text = await GetTextFromVoice();
                    _lastPickupVoiceResult = text;
                }
            }
        }

        private async Task<string> GetTextFromVoice()
        {
            await Wakeup();

            string ret = string.Empty;

            Console.WriteLine("正在采集你的声音");
            await Program.Brain.Speaker.Speak("我在听");
            var speechRecognitionResult = await SpeechToTextService.RecognizeOnceAsync();
            ret = OutputSpeechRecognitionResult(speechRecognitionResult);

            return ret;
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
