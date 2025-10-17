using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private AudioSource _audioSource;
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _move;
    [SerializeField] private InputActionReference _turn;
    [Header("Behaviour")]
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _mouseSensitive = 1f;
    [Header("Voice Recorder")]
    [SerializeField] private int _sampleRate = 16000;
    [SerializeField] private float _threshold = 0.01f;
    [SerializeField] private float _silenceDuration = 0.05f;
    private List<float> _recordedSamples = new List<float>();
    private AudioClip _micClip;
    private string _micName;
    private int _lastPos;
    private bool _isSpeaking = false;
    private float _silenceTimer = 0f;

    private void Start()
    {
        //入力有効化
        _move.action.Enable();
        _turn.action.Enable();

        //ボイスレコーダー
        if (Microphone.devices.Length > 0)
        {
            _micName = Microphone.devices[0];
            _micClip = Microphone.Start(_micName, true, 10, _sampleRate);
        }
        else
        {
            Debug.LogError("マイクが見つかりません。");
        }
    }

    private void Update()
    {
        //動作関係
        var moveInput = _move.action.ReadValue<Vector2>();
        var movement = new Vector3(moveInput.x, 0, moveInput.y);
        movement = transform.TransformDirection(movement);
        _characterController.Move(movement * Time.deltaTime * _speed);

        var turnInput = _turn.action.ReadValue<Vector2>();
        var rot = new Vector3(0f, turnInput.x, 0f);
        transform.Rotate(rot * Time.deltaTime * _mouseSensitive);
    }

    private AudioClip Recording()
    {
        //音量検出
        int pos = Microphone.GetPosition(_micName);
        int length = pos - _lastPos;
        if (length < 0) length += _micClip.samples;

        if (length <= 0) return null;

        float[] samples = new float[length * _micClip.channels];
        _micClip.GetData(samples, _lastPos);
        _lastPos = pos;

        float level = 0f;
        foreach (var s in samples)
            level += Mathf.Abs(s);
        level /= samples.Length;

        //録音処理
        if (level > _threshold)
        {
            _recordedSamples.AddRange(samples);
            _isSpeaking = true;
            _silenceTimer = 0f;
        }
        else if (_isSpeaking == true)
        {
            _silenceTimer += Time.deltaTime;
            if (_silenceTimer >= _silenceDuration)
            {
                AudioClip clip = SaveClip();
                _recordedSamples.Clear();
                _isSpeaking = false;
                _silenceTimer = 0f;

                return clip;
            }
        }

        return null;
    }

    private AudioClip SaveClip()
    {
        if (_recordedSamples.Count == 0) return null;

        AudioClip clip = AudioClip.Create("player_voice", _recordedSamples.Count, 1, _sampleRate, false);
        clip.SetData(_recordedSamples.ToArray(), 0);

        return clip;
    }
}
