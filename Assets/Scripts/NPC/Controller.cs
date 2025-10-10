using UnityEngine;
using WBC;

public class Controller : MonoBehaviour
{
    [SerializeField] private Conversation _conversation;
    [SerializeField] private AudioSource _audioSource;

    private async void Start()
    {
        string text = await _conversation.GetLLMResponse();
        byte[] audioData = await _conversation.GetTTSResponse(text);
        AudioClip clip = await _conversation.GetAudioClip(audioData);

        Speak(clip);
    }

    private void Speak(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}
