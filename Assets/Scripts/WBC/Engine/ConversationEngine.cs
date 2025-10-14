using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace WBC.Engine
{
    public class ConversationEngine : EngineCore
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

        public class ConversationSession
        {
            public ConversationSession()
            {

            }
        }

        public ConversationSession session;
        public ConversationEngine host;

        public override void Started()
        {
            base.Started();
        }

        public override void Updated()
        {
            base.Updated();
        }

        /// <summary>
        /// 対話の受付
        /// </summary>
        /// <param name="id"></param>
        public void SYN(int id)
        {
            if (session != null)
            {
                Debug.Log("add session");
            }
            else if (host != null)
            {
                Debug.Log("other host add session");
            }
            else if (base.id > id) //IDが大きいNPCにセッション権限を与える（Player=-1は除外）
            {
                Debug.Log("create session");
            }
        }

        /// <summary>
        /// 生成AIからの回答を得る
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetLLMResponse()
        {
            string apiKey = "";
            string apiUrl = "https://api.openai.com/v1/chat/completions";

            var payload = new
            {
                model = "gpt-4o-mini",
                messages = new object[]
                {
                    new { role = "system", content = "あなたは徳島大学の学生です。日常会話で返答してください。" },
                    new { role = "user", content = "話しかけました。" }
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
            string apiKey = "";
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
        /// 音声ファイルを取得
        /// </summary>
        /// <param name="audioData"></param>
        /// <returns></returns>
        private async Task<AudioClip> GetAudioClip(byte[] audioData)
        {
            string tempPath = Path.Combine(Application.temporaryCachePath, "temp.mp3");
            File.WriteAllBytes(tempPath, audioData);

            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(tempPath, AudioType.MPEG);
            await request.SendWebRequest();

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            return clip;
        }
    }
}