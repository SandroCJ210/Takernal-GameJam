using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ChaserEnemy : MonoBehaviour, IDamageable, IStunnable
{
    private const float DefaultHealth = 30f;

    [Header("Data Configuration")]
    public EnemyData data;

    [Header("Runtime Stats (Debug)")]
    [SerializeField] private float currentHealth = DefaultHealth;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float damage = 10f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 0.9f;
    [SerializeField] private float attackRadius = 0.75f;
    [SerializeField] private float attackWindup = 0.2f;
    [SerializeField] private float attackRecovery = 0.35f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private LayerMask attackLayers = ~0;

    [Header("Hit Reaction")]
    [SerializeField] private float hitStunDuration = 0.25f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string xDirectionParameter = "xInput";
    [SerializeField] private string yDirectionParameter = "yInput";
    [SerializeField] private string speedParameter = "speed";
    [SerializeField] private string attackingParameter = "isAttacking";
    [SerializeField] private string stunnedParameter = "isStunned";

    [Header("Animation State (Debug)")]
    [SerializeField] private Vector2 facingDirection = Vector2.down;
    [SerializeField] private Vector2 movementDirection;
    [SerializeField] private int facingX;
    [SerializeField] private int facingY = -1;
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool isStunned;

    private float nextAttackTime = 0f;

    public static event Action<ChaserEnemy> OnEnemyDied;
    public bool IsAlive => !hasDied && currentHealth > 0f;
    public bool IsAttacking => isAttacking;
    public bool IsStunned => isStunned;
    public Vector2 FacingDirection => facingDirection;
    public Vector2 MovementDirection => movementDirection;
    public int FacingX => facingX;
    public int FacingY => facingY;

    private Transform targetPlayer;
    private IDamageable targetDamageable;
    private PlayerCombat targetCombat;
    private DummyPlayer targetDummyPlayer;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool statsInitialized;
    private bool hasDied;
    private float hitStunEndsAt;
    private Coroutine attackRoutine;
    private Coroutine hitStunRoutine;
    private int xDirectionHash;
    private int yDirectionHash;
    private int speedHash;
    private int attackingHash;
    private int stunnedHash;
    private bool hasXDirectionParameter;
    private bool hasYDirectionParameter;
    private bool hasSpeedParameter;
    private bool hasAttackingParameter;
    private bool hasStunnedParameter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        xDirectionHash = Animator.StringToHash(xDirectionParameter);
        yDirectionHash = Animator.StringToHash(yDirectionParameter);
        speedHash = Animator.StringToHash(speedParameter);
        attackingHash = Animator.StringToHash(attackingParameter);
        stunnedHash = Animator.StringToHash(stunnedParameter);

        CacheAnimatorParameters();
    }

    private void Start()
    {
        ApplyVisualsFromData();

        if (!statsInitialized)
            InitializeStatsFromData();

        FindTargetPlayer();
    }

    public void InitializeFromData()
    {
        InitializeStatsFromData();
        ApplyVisualsFromData();
    }

    public void InitializeWithScaledStats(float health, float speed, float dmg)
    {
        currentHealth = Mathf.Max(1f, health);
        moveSpeed = Mathf.Max(0f, speed);
        damage = Mathf.Max(0f, dmg);
        statsInitialized = true;
    }

    private void InitializeStatsFromData()
    {
        if (data != null)
        {
            currentHealth = data.maxHealth;
            moveSpeed = data.moveSpeed;
            damage = data.damage;
        }
        else if (currentHealth <= 0f)
        {
            currentHealth = DefaultHealth;
        }

        statsInitialized = true;
    }

    private void ApplyVisualsFromData()
    {
        if (data == null || spriteRenderer == null) return;

        if (data.sprite != null)
            spriteRenderer.sprite = data.sprite;

        spriteRenderer.color = data.debugColor;
    }

    private void FindTargetPlayer()
    {
        targetPlayer = null;
        targetDamageable = null;
        targetCombat = FindFirstObjectByType<PlayerCombat>();
        targetDummyPlayer = null;

        if (targetCombat != null && targetCombat.IsAlive)
        {
            targetPlayer = targetCombat.transform;
            targetDamageable = targetCombat;
            return;
        }

        targetDummyPlayer = FindFirstObjectByType<DummyPlayer>();
        if (targetDummyPlayer != null)
        {
            targetPlayer = targetDummyPlayer.transform;
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        targetPlayer = playerObj.transform;
        targetCombat = playerObj.GetComponentInParent<PlayerCombat>();

        if (targetCombat != null)
        {
            targetDamageable = targetCombat;
            return;
        }

        targetDamageable = playerObj.GetComponentInParent<IDamageable>();
    }

    private void FixedUpdate()
    {
        if (!IsAlive) return;

        if (isStunned)
        {
            movementDirection = Vector2.zero;
            UpdateAnimator(0f);
            return;
        }

        if (targetPlayer == null || !IsTargetAlive())
        {
            FindTargetPlayer();
            if (targetPlayer == null) return;
        }

        Vector2 toTarget = targetPlayer.position - transform.position;
        float distanceToTarget = toTarget.magnitude;
        Vector2 direction = distanceToTarget > Mathf.Epsilon ? toTarget / distanceToTarget : facingDirection;

        SetFacingFromDirection(direction);

        if (isAttacking)
        {
            StopMovement();
            UpdateAnimator(0f);
            return;
        }

        if (distanceToTarget <= attackRange)
        {
            StopMovement();
            UpdateAnimator(0f);

            if (Time.time >= nextAttackTime)
                attackRoutine = StartCoroutine(AttackRoutine());

            return;
        }

        movementDirection = direction;
        rb.MovePosition(rb.position + movementDirection * moveSpeed * Time.fixedDeltaTime);
        UpdateAnimator(moveSpeed);
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        Debug.Log($"[ChaserEnemy] Recibio {amount} de dano. Vida restante: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        StartHitStun();
    }

    public void ApplyStun(float duration)
    {
        if (!IsAlive || duration <= 0f) return;

        StartHitStun(duration);
    }

    private void Die()
    {
        if (hasDied) return;

        hasDied = true;
        Debug.Log($"[ChaserEnemy] {gameObject.name} ha sido derrotado.");
        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        StopMovement();
        UpdateAnimator(0f);

        yield return new WaitForSeconds(attackWindup);

        PerformAttackHit();

        yield return new WaitForSeconds(attackRecovery);

        isAttacking = false;
        attackRoutine = null;
        UpdateAnimator(0f);
    }

    private void StartHitStun()
    {
        StartHitStun(hitStunDuration);
    }

    private void StartHitStun(float duration)
    {
        if (duration <= 0f) return;

        float newHitStunEndsAt = Time.time + duration;

        CancelAttack();
        nextAttackTime = Mathf.Max(nextAttackTime, newHitStunEndsAt);

        // Evita que un hit-stun corto de dano pise un stun largo aplicado por una habilidad.
        if (hitStunRoutine != null && newHitStunEndsAt <= hitStunEndsAt)
            return;

        if (hitStunRoutine != null)
            StopCoroutine(hitStunRoutine);

        hitStunEndsAt = newHitStunEndsAt;
        hitStunRoutine = StartCoroutine(HitStunRoutine(duration));
    }

    private IEnumerator HitStunRoutine(float duration)
    {
        isStunned = true;
        movementDirection = Vector2.zero;
        UpdateAnimator(0f);

        yield return new WaitForSeconds(duration);

        isStunned = false;
        hitStunEndsAt = 0f;
        hitStunRoutine = null;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        UpdateAnimator(0f);
    }

    private void CancelAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        isAttacking = false;
    }

    private void PerformAttackHit()
    {
        if (!IsAlive || targetPlayer == null || !IsTargetAlive()) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, attackLayers);

        foreach (Collider2D hit in hits)
        {
            if (hit == null || !IsCurrentTarget(hit.gameObject))
                continue;

            if (targetCombat != null && targetCombat.IsAlive)
            {
                targetCombat.TakeDamage(damage, gameObject);
                return;
            }

            if (targetDamageable != null && targetDamageable.IsAlive)
            {
                targetDamageable.TakeDamage(damage);
                return;
            }

            if (targetDummyPlayer != null)
            {
                targetDummyPlayer.TakeDamage(damage);
                return;
            }
        }
    }

    private void SetFacingFromDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon) return;

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            facingDirection = direction.x < 0f ? Vector2.left : Vector2.right;
        }
        else
        {
            facingDirection = direction.y < 0f ? Vector2.down : Vector2.up;
        }

        facingX = Mathf.RoundToInt(facingDirection.x);
        facingY = Mathf.RoundToInt(facingDirection.y);
    }

    private void StopMovement()
    {
        movementDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    private void UpdateAnimator(float currentSpeed)
    {
        if (spriteRenderer != null && facingX != 0)
        {
            spriteRenderer.flipX = (facingX > 0);
        }

        if (animator == null) return;

        if (hasXDirectionParameter)
            animator.SetFloat(xDirectionHash, facingX);

        if (hasYDirectionParameter)
            animator.SetFloat(yDirectionHash, facingY);

        if (hasSpeedParameter)
            animator.SetFloat(speedHash, currentSpeed);

        if (hasAttackingParameter)
            animator.SetBool(attackingHash, isAttacking);

        if (hasStunnedParameter)
            animator.SetBool(stunnedHash, isStunned);
    }

    private void CacheAnimatorParameters()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == xDirectionHash && parameter.type == AnimatorControllerParameterType.Float)
                hasXDirectionParameter = true;

            if (parameter.nameHash == yDirectionHash && parameter.type == AnimatorControllerParameterType.Float)
                hasYDirectionParameter = true;

            if (parameter.nameHash == speedHash && parameter.type == AnimatorControllerParameterType.Float)
                hasSpeedParameter = true;

            if (parameter.nameHash == attackingHash && parameter.type == AnimatorControllerParameterType.Bool)
                hasAttackingParameter = true;

            if (parameter.nameHash == stunnedHash && parameter.type == AnimatorControllerParameterType.Bool)
                hasStunnedParameter = true;
        }
    }

    private bool IsCurrentTarget(GameObject targetObj)
    {
        if (targetPlayer == null)
            FindTargetPlayer();

        if (targetPlayer == null || targetObj == null) return false;

        Transform targetTransform = targetObj.transform;
        return targetTransform == targetPlayer
            || targetTransform.IsChildOf(targetPlayer)
            || targetPlayer.IsChildOf(targetTransform);
    }

    private bool IsTargetAlive()
    {
        if (targetCombat != null)
            return targetCombat.IsAlive;

        if (targetDamageable != null)
            return targetDamageable.IsAlive;

        return targetDummyPlayer != null;
    }

    private void OnDisable()
    {
        CancelAttack();

        if (hitStunRoutine != null)
        {
            StopCoroutine(hitStunRoutine);
            hitStunRoutine = null;
        }

        isStunned = false;
        isAttacking = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
