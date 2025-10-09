using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace WBC
{
    public class Conversation : MonoBehaviour
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

        private async void Start()
        {
            await Task.Delay(1000);
            string response = await GetLLMResponse();
            Debug.Log(response);
        }

        /// <summary>
        /// 生成AIからの回答を得る
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetLLMResponse()
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
            byte[] bodyRow = new UTF8Encoding().GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRow);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            await request.SendWebRequest();

            string response = "やあ。次は何の講義？";
            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                var jsonResponseData = JsonConvert.DeserializeObject<JsonResponseData>(jsonResponse);
                if (jsonResponseData != null && jsonResponseData.choices.Length > 0)
                {
                    response = jsonResponseData.choices[0].message.content;
                }
            }
            else
            {
                Debug.Log(request.error);
                Debug.Log(request.result);
            }

            return response;
        }
    }
}