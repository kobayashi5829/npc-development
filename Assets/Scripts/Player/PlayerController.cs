using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] private CharacterController _characterController;
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _move;
    [SerializeField] private InputActionReference _turn;
    [Header("Behaviour")]
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _mouseSensitive = 1f;

    private void Update()
    {
        Move();
        Turn();
    }

    /// <summary>
    /// ˆÚ“®
    /// </summary>
    private void Move()
    {
        var input = _move.action.ReadValue<Vector2>();
        var movement = new Vector3(input.x, 0, input.y);
        movement = transform.TransformDirection(movement);
        movement = movement * Time.deltaTime * _speed;
        _characterController.Move(movement);
    }

    /// <summary>
    ///  ‰ñ“]
    /// </summary>
    private void Turn()
    {
        var input = _turn.action.ReadValue<Vector2>();
        var rotate = new Vector3(0f, input.x, 0f);
        rotate = rotate * Time.deltaTime * _mouseSensitive;
        transform.Rotate(rotate);
    }
}
