using UnityEngine;
using UnityEngine.Windows.Speech;
using WBC.Engine;

namespace WBC.Interaction
{
    public class PTCInteraction : MonoBehaviour
    {
        [SerializeField] private bool _voiceRecorder = true;
        private const int PLAYER_ID = -1;
        private ConversationEngine _host;
        private DictationRecognizer _dictationRecognizer;

        private void Start()
        {
            StartRecorder(_voiceRecorder);
        }

        private void OnDestroy()
        {
            StopRecoder(_voiceRecorder);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<ConversationEngine>(out var engine))
            {
                _host = engine.SYN(PLAYER_ID);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<ConversationEngine>(out var engine))
            {
                _host = null;
            }
        }

        /// <summary>
        /// 録音開始
        /// </summary>
        private void StartRecorder(bool enable)
        {
            if (!enable) return;
            _dictationRecognizer = new DictationRecognizer();
            _dictationRecognizer.DictationResult += (text, confidence) =>
            {
                Speech(text); //プレイヤーが会話セッション内で発話
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

        private void Speech(string text)
        {
            if (_host != null)
            {
                _host.session.ListenPlayerSpeech(text);
            }
        }
    }
}
