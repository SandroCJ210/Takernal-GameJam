using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour {

    [SerializeField] private float _acceleration;
    [SerializeField] private float _maxSpeed;
    [Range(0, 1)]
    [SerializeField] private float _drag;
    [SerializeField] private Animator _animator;

    private Rigidbody2D _rb;
    private Vector2 _bufferedMovement;
    private Vector2 _previousRawInput;
    private Vector2 _previousFacing;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start() {
        InputHandler.Instance.OnMoveRecieved += OnMove;
    }

    private void OnMove(Vector2 direction) {
        _bufferedMovement = direction * _acceleration;

        bool xActive = !Mathf.Approximately(direction.x, 0f);
        bool yActive = !Mathf.Approximately(direction.y, 0f);
        bool prevXActive = !Mathf.Approximately(_previousRawInput.x, 0f);
        bool prevYActive = !Mathf.Approximately(_previousRawInput.y, 0f);

        Vector2 facing = direction;

        if (xActive && yActive) {
            if (xActive && !prevXActive) facing = new Vector2(direction.x, 0f);
            else if (yActive && !prevYActive) facing = new Vector2(0f, direction.y);
            else facing = _previousFacing; 
        }

        _previousRawInput = direction;

        if (direction.Equals(Vector2.zero)) return;

        _animator.SetFloat("xInput", facing.x);
        _animator.SetFloat("yInput", facing.y);
        _previousFacing = facing;
    }

    void FixedUpdate() {
        Move();
    }

    private void LateUpdate()
    {
        _animator.SetFloat("speed", _rb.linearVelocity.magnitude);
    }

    private void Move()
    {
        if (Mathf.Approximately(_bufferedMovement.magnitude, 0f)) {

            _rb.linearVelocity *= 1 - _drag;

            if (Mathf.Approximately(_rb.linearVelocity.magnitude, 0f))
                _rb.linearVelocity = Vector2.zero;

            return;
        }

        if (_rb.linearVelocity.magnitude >= _maxSpeed) {
            _rb.linearVelocity = _rb.linearVelocity.normalized * _maxSpeed;
        }

        _rb.AddForce(_bufferedMovement);
    }
}