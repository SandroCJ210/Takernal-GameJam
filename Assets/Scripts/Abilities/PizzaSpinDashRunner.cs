using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaSpinDashRunner : MonoBehaviour
{
    private readonly HashSet<IDamageable> damagedThisSpin = new HashSet<IDamageable>();

    private Collider2D[] overlapBuffer;
    private Coroutine spinRoutine;
    private PlayerCombat lockedCombat;
    private Rigidbody2D rb;
    private Animator animator;
    private int spinTriggerHash;
    private int spinningBoolHash;
    private string spinTriggerParameter;
    private string spinningBoolParameter;
    private float spinElapsedTime;
    private float hitboxActiveStartTime;
    private float hitboxActiveEndTime;

    public bool IsRunning => spinRoutine != null;

    public bool TryStart(
        PlayerCombat combat,
        Vector2 direction,
        float forwardDistance,
        float forwardDuration,
        float returnDuration,
        float hitRadius,
        float hitboxActiveDelay,
        float hitboxActiveDuration,
        float damage,
        float knockbackForce,
        LayerMask targetLayers,
        int maxHits,
        string spinTrigger,
        string spinningBool)
    {
        if (combat == null || IsRunning || combat.IsMovementLocked) return false;

        rb = combat.GetComponent<Rigidbody2D>();
        if (rb == null) return false;

        animator = combat.GetComponentInChildren<Animator>();
        spinTriggerParameter = spinTrigger;
        spinningBoolParameter = spinningBool;
        spinTriggerHash = string.IsNullOrEmpty(spinTriggerParameter) ? 0 : Animator.StringToHash(spinTriggerParameter);
        spinningBoolHash = string.IsNullOrEmpty(spinningBoolParameter) ? 0 : Animator.StringToHash(spinningBoolParameter);

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            direction = transform.right;

        direction.Normalize();
        EnsureOverlapBuffer(maxHits);
        spinElapsedTime = 0f;
        hitboxActiveStartTime = Mathf.Max(0f, hitboxActiveDelay);
        hitboxActiveEndTime = hitboxActiveDuration < 0f
            ? float.PositiveInfinity
            : hitboxActiveStartTime + Mathf.Max(0f, hitboxActiveDuration);

        combat.CancelCurrentAttack();
        lockedCombat = combat;
        lockedCombat.BeginExternalCombatMovement();

        damagedThisSpin.Clear();
        SetSpinAnimation(true);
        spinRoutine = StartCoroutine(SpinRoutine(
            direction,
            Mathf.Max(0f, forwardDistance),
            Mathf.Max(0f, forwardDuration),
            Mathf.Max(0f, returnDuration),
            Mathf.Max(0f, hitRadius),
            Mathf.Max(0f, damage),
            Mathf.Max(0f, knockbackForce),
            targetLayers
        ));

        return true;
    }

    private IEnumerator SpinRoutine(
        Vector2 direction,
        float forwardDistance,
        float forwardDuration,
        float returnDuration,
        float hitRadius,
        float damage,
        float knockbackForce,
        LayerMask targetLayers)
    {
        Vector2 start = rb.position;
        Vector2 forwardTarget = start + direction * forwardDistance;

        ApplyDamageIfActive(hitRadius, damage, knockbackForce, targetLayers);
        yield return MoveAndDamage(start, forwardTarget, forwardDuration, hitRadius, damage, knockbackForce, targetLayers);
        yield return MoveAndDamage(rb.position, start, returnDuration, hitRadius, damage, knockbackForce, targetLayers);

        FinishSpin();
    }

    private IEnumerator MoveAndDamage(
        Vector2 from,
        Vector2 to,
        float duration,
        float hitRadius,
        float damage,
        float knockbackForce,
        LayerMask targetLayers)
    {
        if (duration <= 0f)
        {
            rb.MovePosition(to);
            ApplyDamageIfActive(hitRadius, damage, knockbackForce, targetLayers);
            yield return new WaitForFixedUpdate();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            spinElapsedTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rb.MovePosition(Vector2.Lerp(from, to, t));
            ApplyDamageIfActive(hitRadius, damage, knockbackForce, targetLayers);
            yield return new WaitForFixedUpdate();
        }
    }

    private void ApplyDamageIfActive(float hitRadius, float damage, float knockbackForce, LayerMask targetLayers)
    {
        if (spinElapsedTime < hitboxActiveStartTime || spinElapsedTime > hitboxActiveEndTime) return;

        ApplyDamage(hitRadius, damage, knockbackForce, targetLayers);
    }

    private void ApplyDamage(float hitRadius, float damage, float knockbackForce, LayerMask targetLayers)
    {
        if (hitRadius <= 0f || overlapBuffer == null) return;

        int hitCount = Physics2D.OverlapCircleNonAlloc(rb.position, hitRadius, overlapBuffer, targetLayers);
        for (int i = 0; i < hitCount; i++)
            ApplyToCollider(overlapBuffer[i], damage, knockbackForce);
    }

    private void ApplyToCollider(Collider2D hit, float damage, float knockbackForce)
    {
        if (hit == null || IsOwnerCollider(hit)) return;

        IDamageable damageable = hit.GetComponentInParent<IDamageable>();
        if (damageable == null || !damageable.IsAlive || !damagedThisSpin.Add(damageable)) return;

        if (damage > 0f)
            damageable.TakeDamage(damage);

        Rigidbody2D targetBody = hit.attachedRigidbody;
        if (targetBody == null || knockbackForce <= 0f) return;

        Vector2 knockbackDirection = ((Vector2)hit.transform.position - rb.position).normalized;
        if (knockbackDirection.sqrMagnitude <= Mathf.Epsilon)
            knockbackDirection = transform.right;

        targetBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }

    private bool IsOwnerCollider(Collider2D other)
    {
        return other.gameObject == gameObject || other.transform.IsChildOf(transform);
    }

    private void SetSpinAnimation(bool isSpinning)
    {
        if (animator == null) return;

        if (spinningBoolHash != 0 && HasAnimatorParameter(spinningBoolHash, AnimatorControllerParameterType.Bool))
            animator.SetBool(spinningBoolHash, isSpinning);

        if (isSpinning && spinTriggerHash != 0 && HasAnimatorParameter(spinTriggerHash, AnimatorControllerParameterType.Trigger))
            animator.SetTrigger(spinTriggerHash);
    }

    private bool HasAnimatorParameter(int parameterHash, AnimatorControllerParameterType type)
    {
        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].nameHash == parameterHash && parameters[i].type == type)
                return true;
        }

        return false;
    }

    private void FinishSpin()
    {
        SetSpinAnimation(false);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (lockedCombat != null)
            lockedCombat.EndExternalCombatMovement();

        lockedCombat = null;
        spinRoutine = null;
    }

    private void EnsureOverlapBuffer(int maxHits)
    {
        int safeMaxHits = Mathf.Max(1, maxHits);
        if (overlapBuffer == null || overlapBuffer.Length != safeMaxHits)
            overlapBuffer = new Collider2D[safeMaxHits];
    }

    private void OnDisable()
    {
        if (spinRoutine != null)
        {
            StopCoroutine(spinRoutine);
            spinRoutine = null;
        }

        FinishSpin();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
