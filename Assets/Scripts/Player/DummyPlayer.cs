using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class DummyPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Combat & Stat Multipliers")]
    public float damageMultiplier = 1f;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float baseDamage = 15f;
    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        GameEvents.OnRewardApplied += ApplyUpgrade;
    }

    private void OnDisable()
    {
        GameEvents.OnRewardApplied -= ApplyUpgrade;
    }

    private void Update()
    {
        Vector2 input = Vector2.zero;

        // Soporte para Teclado (WASD / Flechas)
        if (Keyboard.current != null)
        {
            float moveX = 0f;
            float moveY = 0f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;

            input = new Vector2(moveX, moveY);
        }

        // Soporte para Mando (Joystick izquierdo)
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.magnitude > 0.1f)
            {
                input = stick;
            }
        }

        movementInput = input.normalized;

        // Ataque (Espacio o Clic Izquierdo del Mouse)
        bool attackPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            attackPressed = true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            attackPressed = true;
        }

        if (attackPressed && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void Attack()
    {
        Debug.Log("[DummyPlayer] ¡Ataque en área ejecutado!");
        
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            ChaserEnemy enemy = hitCollider.GetComponent<ChaserEnemy>();
            if (enemy != null)
            {
                float calculatedDamage = baseDamage * damageMultiplier;
                enemy.TakeDamage(calculatedDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"[DummyPlayer] Recibió {amount} de daño. Vida restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //eliminar al player dummy de la escena
        Debug.Log("[DummyPlayer] ¡El DummyPlayer ha sido derrotado!");
        Destroy(gameObject);
    }

    public void ApplyUpgrade(StatBonus bonus)
    {
        moveSpeed += bonus.speedIncrease;
        damageMultiplier += bonus.damageIncrease;
        maxHealth += bonus.healthIncrease;
        currentHealth = Mathf.Min(currentHealth + bonus.healthIncrease, maxHealth);

        Debug.Log($"[DummyPlayer] Stats Actualizados -> Velocidad: {moveSpeed}, Daño: x{damageMultiplier}, Vida Max: {maxHealth}");
    }
}
