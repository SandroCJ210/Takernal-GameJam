using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour {

    [SerializeField] private float _acceleration;
    [SerializeField] private float _maxSpeed;
    [Range(0, 1)]
    [SerializeField] private float _drag;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerCombat _combat;
    [SerializeField] private PlayerStats _stats;

    private Rigidbody2D _rb;
    private Vector2 _bufferedMovement;
    private Vector2 _rawMovementInput;
    private Vector2 _previousRawInput;
    private Vector2 _previousFacing;
    private Vector2 _pendingFacingAfterAttack;
    private bool _hasPendingFacingAfterAttack;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        if (_animator == null) _animator = GetComponentInChildren<Animator>();
        if (_combat == null) _combat = GetComponent<PlayerCombat>();
        if (_stats == null) _stats = GetComponent<PlayerStats>();
    }

    private void Start() {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnMoveRecieved += OnMove;
    }

    private void OnDestroy() {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnMoveRecieved -= OnMove;
    }

    private void OnMove(Vector2 direction) {
        _rawMovementInput = direction;
        _bufferedMovement = direction * GetEffectiveAcceleration();

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

        if (direction.Equals(Vector2.zero)) {
            _hasPendingFacingAfterAttack = false;
            return;
        }

        if (_combat != null && _combat.IsMovementLocked) {
            if (_combat.IsAttacking)
                _combat.BufferAttackDirection(facing);

            _pendingFacingAfterAttack = facing;
            _hasPendingFacingAfterAttack = true;
            return;
        }

        ApplyFacing(facing);
    }

    void FixedUpdate() {
        Move();
    }

    private void LateUpdate()
    {
        if (_hasPendingFacingAfterAttack && (_combat == null || !_combat.IsMovementLocked)) {
            ApplyFacing(_pendingFacingAfterAttack);
            _hasPendingFacingAfterAttack = false;
        }

        if (_animator != null)
            _animator.SetFloat("speed", _rb.linearVelocity.magnitude);
    }

    private void Move()
    {
        if (_combat != null && _combat.IsMovementLocked) {
            if (!_combat.IsPerformingCombatMovement)
                _rb.linearVelocity = Vector2.zero;

            return;
        }

        Vector2 effectiveMovement = _rawMovementInput * GetEffectiveAcceleration();
        if (Mathf.Approximately(effectiveMovement.magnitude, 0f)) {

            _rb.linearVelocity *= 1 - _drag;

            if (Mathf.Approximately(_rb.linearVelocity.magnitude, 0f))
                _rb.linearVelocity = Vector2.zero;

            return;
        }

        float effectiveMaxSpeed = GetEffectiveMaxSpeed();
        if (_rb.linearVelocity.magnitude >= effectiveMaxSpeed) {
            _rb.linearVelocity = _rb.linearVelocity.normalized * effectiveMaxSpeed;
        }

        _bufferedMovement = effectiveMovement;
        _rb.AddForce(_bufferedMovement);
    }

    public void SetMovementTuning(float acceleration, float maxSpeed, float drag)
    {
        _acceleration = Mathf.Max(0f, acceleration);
        _maxSpeed = Mathf.Max(0f, maxSpeed);
        _drag = Mathf.Clamp01(drag);
    }

    public void SetAnimator(Animator newAnimator)
    {
        if (newAnimator != null)
            _animator = newAnimator;
    }

    private void ApplyFacing(Vector2 facing)
    {
        if (_animator == null) return;

        _animator.SetFloat("xInput", facing.x);
        _animator.SetFloat("yInput", facing.y);
        _previousFacing = facing;
    }

    private float GetEffectiveAcceleration()
    {
        float multiplier = _stats != null ? _stats.SpeedMultiplier : 1f;
        return Mathf.Max(0f, _acceleration * multiplier);
    }

    private float GetEffectiveMaxSpeed()
    {
        float flatBonus = _stats != null ? _stats.FlatSpeedBonus : 0f;
        float multiplier = _stats != null ? _stats.SpeedMultiplier : 1f;
        return Mathf.Max(0f, (_maxSpeed + flatBonus) * multiplier);
    }
}
