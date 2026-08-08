using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ChaserEnemy : MonoBehaviour
{
    [Header("Data Configuration")]
    public EnemyData data;

    [Header("Runtime Stats (Debug)")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float damage = 10f;

    [Header("Attack Settings")]
    public float attackCooldown = 1f;
    private float nextAttackTime = 0f;

    public static event Action<ChaserEnemy> OnEnemyDied;

    private Transform targetPlayer;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InitializeFromData();
        FindTargetPlayer();
    }

    public void InitializeFromData()
    {
        if (data != null)
        {
            currentHealth = data.maxHealth;
            moveSpeed = data.moveSpeed;
            damage = data.damage;

            if (spriteRenderer != null)
            {
                if (data.sprite != null)
                {
                    spriteRenderer.sprite = data.sprite;
                }
                spriteRenderer.color = data.debugColor;
            }
        }
    }

    public void InitializeWithScaledStats(float health, float speed, float dmg)
    {
        currentHealth = health;
        moveSpeed = speed;
        damage = dmg;
    }

    private void FindTargetPlayer()
    {
        // Search for DummyPlayer first, or fallback to object with tag "Player"
        DummyPlayer playerScript = FindFirstObjectByType<DummyPlayer>();
        if (playerScript != null)
        {
            targetPlayer = playerScript.transform;
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
        }
    }

    private void FixedUpdate()
    {
        if (targetPlayer == null)
        {
            FindTargetPlayer();
            return;
        }

        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"[ChaserEnemy] Recibió {amount} de daño. Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"[ChaserEnemy] {gameObject.name} ha sido derrotado.");
        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        TryDamagePlayer(collider.gameObject);
    }

    private void TryDamagePlayer(GameObject targetObj)
    {
        if (Time.time < nextAttackTime) return;

        DummyPlayer dummyPlayer = targetObj.GetComponent<DummyPlayer>();
        if (dummyPlayer != null)
        {
            dummyPlayer.TakeDamage(damage);
            nextAttackTime = Time.time + attackCooldown;
        }
    }
}
