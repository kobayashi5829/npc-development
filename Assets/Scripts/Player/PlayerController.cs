using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private InputActionReference _move;
    [SerializeField] private InputActionReference _turn;
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _mouseSensitive = 1f;
    [SerializeField] private int _micNo = 2;

    private void Awake()
    {
        _move.action.Enable();
        _turn.action.Enable();

        string micName = Microphone.devices[_micNo];
        _audioSource.clip = Microphone.Start(micName, true, 10, 44100);
        while (!(Microphone.GetPosition(micName) > 0))
        {

        }
    }

    private void Update()
    {
        var moveInput = _move.action.ReadValue<Vector2>();
        var movement = new Vector3(moveInput.x, 0, moveInput.y);
        movement = transform.TransformDirection(movement);
        _characterController.Move(movement * Time.deltaTime * _speed);

        var turnInput = _turn.action.ReadValue<Vector2>();
        var rot = new Vector3(0f, turnInput.x, 0f);
        transform.Rotate(rot * Time.deltaTime * _mouseSensitive);
    }
}
