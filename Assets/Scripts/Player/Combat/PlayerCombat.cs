using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour, IDamageable
{
    private static readonly int XInputHash = Animator.StringToHash("xInput");
    private static readonly int YInputHash = Animator.StringToHash("yInput");

    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private HealthComponent health;
    [SerializeField] private Rigidbody2D rb;

    [Header("Combo base")]
    [SerializeField] private AttackDataSO firstAttack;

    private AttackDataSO currentAttack;
    private int currentComboStep;
    private bool isAttacking;
    private bool queuedNextAttack;
    private bool canQueueNextAttack;
    private bool isPerformingCombatMovement;
    private bool hasBufferedAttackDirection;
    private int externalCombatMovementLocks;
    private Coroutine lungeRoutine;
    private Vector2 currentAttackDirection = Vector2.right;
    private Vector2 bufferedAttackDirection = Vector2.right;

    [SerializeField] private AbilityController abilities;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerFormController formController;

    public bool IsAlive => health != null && health.IsAlive;
    public bool IsAttacking => isAttacking;
    public bool IsMovementLocked => isAttacking || externalCombatMovementLocks > 0;
    public bool IsPerformingCombatMovement => isPerformingCombatMovement || externalCombatMovementLocks > 0;
    public AttackDataSO CurrentAttack => currentAttack;
    public int CurrentComboStep => currentComboStep;
    public Vector2 FacingDirection => GetAttackDirection();

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (hitbox == null) hitbox = GetComponentInChildren<Hitbox>();
        if (health == null) health = GetComponent<HealthComponent>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (abilities == null) abilities = GetComponent<AbilityController>();
        if (stats == null) stats = GetComponent<PlayerStats>();
        if (formController == null) formController = GetComponent<PlayerFormController>();

        if (hitbox != null)
            hitbox.OnHitLanded += HandleHitLanded;
    }

    private void Start()
    {
        if (InputHandler.Instance == null) return;

        InputHandler.Instance.OnAttackRecieved += HandleAttackInput;
        InputHandler.Instance.OnAbility1Recieved += HandleAbility1Input;
    }

    private void OnDestroy()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnAttackRecieved -= HandleAttackInput;
            InputHandler.Instance.OnAbility1Recieved -= HandleAbility1Input;
        }

        if (hitbox != null)
            hitbox.OnHitLanded -= HandleHitLanded;
    }

    private void OnDisable()
    {
        StopLunge();
        externalCombatMovementLocks = 0;
    }

    private void HandleAbility1Input()
    {
        UseActiveAbility(0);
    }

    private void HandleAttackInput()
    {
        if (externalCombatMovementLocks > 0) return;

        if (!isAttacking)
        {
            StartAttack(firstAttack, 1);
            return;
        }
        
        if (canQueueNextAttack && currentAttack != null && currentAttack.nextAttackInCombo != null)
        {
            queuedNextAttack = true;
        }
    }

    private void StartAttack(AttackDataSO attack, int comboStep)
    {
        if (attack == null) return;

        StopLunge();
        currentAttackDirection = ConsumeAttackDirection();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        currentAttack = attack;
        currentComboStep = comboStep;
        isAttacking = true;
        queuedNextAttack = false;
        canQueueNextAttack = false;

        AttackContext context = CreateAttackContext();
        if (abilities != null)
            abilities.TriggerAttackStarted(context);

        if (animator != null)
        {
            animator.SetFloat(XInputHash, currentAttackDirection.x);
            animator.SetFloat(YInputHash, currentAttackDirection.y);

            if (!string.IsNullOrEmpty(attack.animatorTrigger))
                animator.SetTrigger(attack.animatorTrigger);
        }

        if (attack.swingSfx != null)
            AudioSource.PlayClipAtPoint(attack.swingSfx, transform.position);
    }
    

    public void AE_OpenComboWindow() => canQueueNextAttack = true;

    public void AE_CloseComboWindow() => canQueueNextAttack = false;

    public void BufferAttackDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon) return;

        bufferedAttackDirection = direction.normalized;
        hasBufferedAttackDirection = true;
    }

    public void AE_LungeForward()
    {
        if (currentAttack == null || rb == null || currentAttack.lungeDistance <= 0f) return;

        StopLunge();
        lungeRoutine = StartCoroutine(LungeForward(
            currentAttack.lungeDistance,
            currentAttack.lungeDuration,
            GetAttackDirection()
        ));
    }

    public void AE_ActivateHitbox()
    {
        if (currentAttack == null || hitbox == null) return;

        AttackContext context = CreateAttackContext();
        float finalDamage = currentAttack.baseDamage;
        if (stats != null)
            finalDamage += stats.FlatDamageBonus;

        if (abilities != null)
        {
            abilities.TriggerAttackActivated(context);
            finalDamage = abilities.ApplyDamageModifiers(finalDamage, context);
        }

        if (stats != null)
            finalDamage *= stats.DamageMultiplier;

        hitbox.Configure(gameObject, finalDamage, currentAttack.knockbackForce);
        hitbox.ResetLocalRotation();
        hitbox.SetShapes(currentAttack.GetHitboxShapes(GetAttackDirectionType()));
        hitbox.Activate(currentAttack.hitboxActiveTime);
    }

    public void AE_AttackEnd()
    {
        StopLunge();

        if (queuedNextAttack && currentAttack != null && currentAttack.nextAttackInCombo != null)
        {
            StartAttack(currentAttack.nextAttackInCombo, currentComboStep + 1);
        }
        else
        {
            isAttacking = false;
            hasBufferedAttackDirection = false;
            currentAttack = null;
            currentComboStep = 0;
        }
    }
    

    public void UseActiveAbility(int slotIndex)
    {
        if (abilities == null) return;

        if (formController != null && formController.IsDishFormActive)
        {
            abilities.ActivateFormAbility(slotIndex);
            return;
        }

        abilities.ActivateAbility(slotIndex);
    }

    public void SetFirstAttack(AttackDataSO attack)
    {
        firstAttack = attack;
    }

    public void SetCombatReferences(Animator newAnimator, Hitbox newHitbox)
    {
        if (newAnimator != null)
            animator = newAnimator;

        if (newHitbox == null || newHitbox == hitbox) return;

        if (hitbox != null)
            hitbox.OnHitLanded -= HandleHitLanded;

        hitbox = newHitbox;
        hitbox.OnHitLanded += HandleHitLanded;
    }

    public void CancelCurrentAttack()
    {
        StopLunge();
        queuedNextAttack = false;
        canQueueNextAttack = false;
        isAttacking = false;
        hasBufferedAttackDirection = false;
        currentAttack = null;
        currentComboStep = 0;

        if (hitbox != null)
            hitbox.Deactivate();
    }

    public void BeginExternalCombatMovement()
    {
        externalCombatMovementLocks++;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public void EndExternalCombatMovement()
    {
        externalCombatMovementLocks = Mathf.Max(0, externalCombatMovementLocks - 1);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
    

    public void TakeDamage(float damage) => TakeDamage(damage, null);

    public void TakeDamage(float damage, GameObject source)
    {
        if (health == null || damage <= 0f) return;

        float finalDamage = damage;
        DamageTakenContext context = new DamageTakenContext(this, source, damage, finalDamage);

        if (abilities != null)
            finalDamage = abilities.ApplyDamageTakenModifiers(finalDamage, context);

        if (finalDamage <= 0f) return;

        DamageTakenContext finalContext = new DamageTakenContext(this, source, damage, finalDamage);
        health.TakeDamage(finalDamage);

        if (abilities != null)
            abilities.TriggerDamageTaken(finalContext);
    }

    private void HandleHitLanded(IDamageable target, GameObject source)
    {
        if (abilities != null)
            abilities.TriggerOnHit(target, CreateAttackContext());
    }

    private Vector2 GetAttackDirection()
    {
        if (isAttacking && currentAttackDirection.sqrMagnitude > Mathf.Epsilon)
            return currentAttackDirection;

        return ReadFacingDirection();
    }

    private Vector2 ConsumeAttackDirection()
    {
        if (!hasBufferedAttackDirection)
            return ReadFacingDirection();

        hasBufferedAttackDirection = false;
        return bufferedAttackDirection;
    }

    private Vector2 ReadFacingDirection()
    {
        if (animator != null)
        {
            Vector2 animatorDirection = new Vector2(
                animator.GetFloat(XInputHash),
                animator.GetFloat(YInputHash)
            );

            if (animatorDirection.sqrMagnitude > Mathf.Epsilon)
                return animatorDirection.normalized;
        }

        return transform.right;
    }

    private AttackDirection GetAttackDirectionType()
    {
        Vector2 direction = GetAttackDirection();

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
            return direction.x < 0f ? AttackDirection.Left : AttackDirection.Right;

        return direction.y < 0f ? AttackDirection.Down : AttackDirection.Up;
    }

    private IEnumerator LungeForward(float distance, float duration, Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
            direction = transform.right;

        direction.Normalize();
        isPerformingCombatMovement = true;
        Vector2 start = rb.position;
        Vector2 target = start + direction * distance;

        if (duration <= 0f)
        {
            rb.MovePosition(target);
            yield return new WaitForFixedUpdate();
            rb.linearVelocity = Vector2.zero;
            isPerformingCombatMovement = false;
            lungeRoutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rb.MovePosition(Vector2.Lerp(start, target, t));
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        isPerformingCombatMovement = false;
        lungeRoutine = null;
    }

    private void StopLunge()
    {
        if (lungeRoutine == null) return;

        StopCoroutine(lungeRoutine);
        isPerformingCombatMovement = false;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        lungeRoutine = null;
    }

    private AttackContext CreateAttackContext()
    {
        return new AttackContext(currentAttack, gameObject, GetAttackDirection(), currentComboStep);
    }
}
