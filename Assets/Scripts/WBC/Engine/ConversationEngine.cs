using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace WBC.Engine
{
    [RequireComponent(typeof(AudioSource))]
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
            public struct Talk
            {
                public string text;
                public byte[] audioData;
            }

            private enum SessionState
            {
                Start,
                Awkward, //気まずい
                DisbandJudge, //解散の判定
                End,
                Conv, //会話状態
            }

            private SessionState sessionState; //セッションの状態
            private Talk[] talks; //会話履歴
            private float awkwardTime = 0f; //気まずい時間の上限
            private float awkwardTimer = 0f; //気まずい時間
            private float disbandProvAcc = 0f; //解散確率増加
            private float disbandProv = 0f; //解散確率

            public ConversationSession(float awkwardTime, float disbandProvAcc)
            {
                //データの取得
                this.awkwardTime = awkwardTime;
                this.disbandProvAcc = disbandProvAcc;

                sessionState = SessionState.Start; //初期フラグの設定
            }

            public void Updated()
            {
                //状態マシン
                switch (sessionState)
                {
                    case SessionState.Start:
                        sessionState = SessionState.Awkward;
                        break;
                    case SessionState.Awkward:
                        if (Awkward() == true)
                            sessionState = SessionState.DisbandJudge;
                        break;
                    case SessionState.DisbandJudge:
                        if (DisbandJudge() == true) { sessionState = SessionState.End; }
                        else { sessionState = SessionState.Conv; }
                        break;
                    case SessionState.Conv:
                        Debug.Log("Conversation ongoing...");
                        break;
                    case SessionState.End:
                        break;
                }
            }

            /// <summary>
            /// 気まずい時間の計測
            /// </summary>
            /// <returns></returns>
            private bool Awkward()
            {
                awkwardTimer += Time.deltaTime;

                if (awkwardTimer > awkwardTime)
                {
                    awkwardTimer = 0f;
                    return true;
                }
                else { return false; }
            }

            /// <summary>
            /// 解散の決定
            /// </summary>
            /// <returns></returns>
            private bool DisbandJudge()
            {
                disbandProv += disbandProvAcc;
                if (Random.value < disbandProv) { return true; }
                else { return false; }
            }

            /// <summary>
            /// プレイヤーの発言を受け取る
            /// </summary>
            /// <param name="text"></param>
            public void ListenPlayerSpeech(string text)
            {
                sessionState = SessionState.Conv; //会話状態に移行
            }

            private async Task RunTalk(int num)
            {
                //結果格納用
                Talk[] results = new Talk[num];

                //完了通知用
                TaskCompletionSource<bool>[] ready = new TaskCompletionSource<bool>[num];
                for (int i = 0; i < num; i++) { ready[i] = new TaskCompletionSource<bool>(); }

                for (int i = 0; i < num; i++)
                {
                    _ = Task.Run(async () =>
                    {
                        
                    });
                }
            }
        }

        private const string API_KEY = "";
        [SerializeField] private AudioSource _audioSource;
        public ConversationSession session { private set; get; }
        public ConversationEngine host { private set; get; }

        public override void Started()
        {
            base.Started();
        }

        public override void Updated()
        {
            base.Updated();
            if (session != null) session.Updated();
        }

        /// <summary>
        /// 対話の受付
        /// </summary>
        /// <param name="id"></param>
        public ConversationEngine SYN(int id)
        {
            if (session != null) //自分がセッション権限を持っている
            {
                //Debug.Log("added " + id + " conversation session");
                return this;
            }
            else if (host != null) //他NPCがセッション権限を持っている
            {
                //Debug.Log("other host add conversation session");
                return host;
            }
            else if (base.id > id) //IDが大きいNPCにセッション権限を与える（Player=-1は除外）
            {
                //Debug.Log("create conversation session");
                session = new ConversationSession(
                    1f, //気まずい時間
                    0.1f //解散確率増加
                    );
                return this;
            }
            else //セッション権限を与える
            {
                return null;
            }
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
                model = "gpt-4o-mini",
                messages = new object[]
                {
                    new { role = "system", content = "あなたは徳島大学の学生です。" },
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
    }
}