using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Test
{
    [RequireComponent(typeof(AudioSource))]
    public class ConversationTest : MonoBehaviour
    {
        [System.Serializable]
        public class JsonResponseData
        {
            [System.Serializable]
            public class Choice
            {
                [System.Serializable]
                public class Message
                {
                    public string role;
                    public string content;
                }

                public Message message;
            }

            public Choice[] choices;
        }

        private const string API_KEY = "";
        [SerializeField] private AudioSource _audioSource;
        private DictationRecognizer _dictationRecognizer;

        private void Start()
        {
            StartRecorder(true);
        }

        private void OnDestroy()
        {
            StopRecoder(true);
        }

        /// <summary>
        /// 録音開始
        /// </summary>
        private void StartRecorder(bool enable)
        {
            if (!enable) return;
            _dictationRecognizer = new DictationRecognizer();
            _dictationRecognizer.DictationResult += async (text, confidence) =>
            {
                Debug.Log("Player Said: " + text);
                var response = await GetLLMResponse(text); //プレイヤーが会話セッション内で発話
                Debug.Log("LLM Response: " + response);
                var audioBytes = await GetTTSResponse(response);
                Debug.Log("TTS Audio Bytes Length: " + audioBytes.Length);
                var AudioClip = await GetAudioClipFromBytes(audioBytes);
                Debug.Log("Generated AudioClip Length: " + AudioClip.length);
                _audioSource.clip = AudioClip;
                _audioSource.Play();
            };
            _dictationRecognizer.DictationComplete += (completionCause) =>
            {
                _dictationRecognizer.Start();
            };
            _dictationRecognizer.Start();
        }

        /// <summary>
        /// 録音停止
        /// </summary>
        private void StopRecoder(bool enable)
        {
            if (!enable) return;
            _dictationRecognizer.Stop();
            _dictationRecognizer.Dispose();
        }

        /// <summary>
        /// 生成AIからの回答を得る
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetLLMResponse(string text)
        {
            string apiKey = API_KEY;
            string apiUrl = "https://api.openai.com/v1/chat/completions";

            var payload = new
            {
                model = "gpt-5-nano",
                messages = new object[]
                {
                    new { role = "system", content = "あなたは徳島大学の学生です。短く会話をして下さい。" },
                    new { role = "user", content = text }
                }
            };
            string jsonData = JsonConvert.SerializeObject(payload);

            UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
            byte[] bodyRaw = new UTF8Encoding().GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            await request.SendWebRequest();

            string response = null;
            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                var jsonResponseData = JsonConvert.DeserializeObject<JsonResponseData>(jsonResponse);
                if (jsonResponseData != null && jsonResponseData.choices.Length > 0)
                {
                    response = jsonResponseData.choices[0].message.content;
                }
                else
                {
                    response = "やあ。次は何の講義？";
                }
            }

            return response;
        }

        /// <summary>
        /// テキストを音声データに変換する
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task<byte[]> GetTTSResponse(string text)
        {
            string apiKey = API_KEY;
            string apiUrl = "https://api.openai.com/v1/audio/speech";

            var payload = new
            {
                model = "gpt-4o-mini-tts",
                voice = "alloy",
                input = text,
                format = "mp3"
            };
            string jsonData = JsonConvert.SerializeObject(payload);

            UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
            byte[] bodyRaw = new UTF8Encoding().GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            await request.SendWebRequest();

            byte[] response = null;
            if (request.result == UnityWebRequest.Result.Success)
            {
                response = request.downloadHandler.data;
            }

            return response;
        }

        /// <summary>
        /// 音声データをAudioClipに変換する
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private async Task<AudioClip> GetAudioClipFromBytes(byte[] bytes)
        {
            string path = Application.temporaryCachePath + "/tts.mp3";
            System.IO.File.WriteAllBytes(path, bytes);

            using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG);

            await request.SendWebRequest();

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            return clip;
        }
    }
}
