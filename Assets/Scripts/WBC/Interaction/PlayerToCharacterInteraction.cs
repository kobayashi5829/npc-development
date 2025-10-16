using System.Collections.Generic;
using UnityEngine;
using WBC.Engine;

namespace WBC.Interaction
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerToCharacterInteraction : MonoBehaviour
    {
        private class VoiceRecorder
        {
            private AudioClip micClip;
            private List<float> recordedSamples = new List<float>();
            private string micName;
            private int sampleRate;
            private float threshold;
            private float silenceDuration;
            private int lastPos = 0;
            private bool isSpeaking = false;
            private float silenceTimer = 0f;

            public VoiceRecorder(int sampleRate, float threshold, float silenceDuration)
            {
                this.sampleRate = sampleRate;
                this.threshold = threshold;
                this.silenceDuration = silenceDuration;
            }

            public void Start()
            {
                if (Microphone.devices.Length > 0)
                {
                    micName = Microphone.devices[0];
                    micClip = Microphone.Start(micName, true, 10, sampleRate);
                }
                else
                {
                    Debug.LogError("マイクが見つかりません。");
                }
            }

            public AudioClip Recording()
            {
                //音量検出
                int pos = Microphone.GetPosition(micName);
                int length = pos - lastPos;
                if (length < 0) length += micClip.samples;

                if (length <= 0) return null;

                float[] samples = new float[length * micClip.channels];
                micClip.GetData(samples, lastPos);
                lastPos = pos;

                float level = 0f;
                foreach (var s in samples)
                    level += Mathf.Abs(s);
                level /= samples.Length;

                //録音処理
                if (level > threshold)
                {
                    recordedSamples.AddRange(samples);
                    isSpeaking = true;
                    silenceTimer = 0f;
                }
                else if (isSpeaking == true)
                {
                    silenceTimer += Time.deltaTime;
                    if (silenceTimer >= silenceDuration)
                    {
                        AudioClip clip = SaveClip();
                        recordedSamples.Clear();
                        isSpeaking = false;
                        silenceTimer = 0f;

                        return clip;
                    }
                }

                return null;
            }

            private AudioClip SaveClip()
            {
                if (recordedSamples.Count == 0) return null;

                AudioClip clip = AudioClip.Create("player_voice", recordedSamples.Count, 1, sampleRate, false);
                clip.SetData(recordedSamples.ToArray(), 0);

                return clip;
            }
        }

        private const int PLAYER_ID = -1;
        [Header("Use Module")]
        [SerializeField] private bool _talking = true;
        [Header("Talking Settings")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private int _sampleMicRate = 16000; //音声認識のサンプルレート
        [SerializeField] private float _threshold = 0.01f; //音量判定の閾値
        [SerializeField] private float _silenceDuration = 0f; //話している最中の無音時間
        private List<ConversationEngine> _conversationList = new List<ConversationEngine>();
        private VoiceRecorder _voiceRecorder;

        private void Start()
        {
            // NPCとの会話機能
            if (_talking == true)
            {
                _voiceRecorder = new VoiceRecorder(_sampleMicRate, _threshold, _silenceDuration);
                _voiceRecorder.Start();
            }
        }

        private void Update()
        {
            if (_talking == true)
            {
                AudioClip clip = _voiceRecorder.Recording();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<ConversationEngine>(out var engine))
            {
                //_conversationList.Add(engine);
                engine.SYN(PLAYER_ID);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<ConversationEngine>(out var engine))
            {
                //_conversationList.Remove(engine);
            }
        }
    }
}
