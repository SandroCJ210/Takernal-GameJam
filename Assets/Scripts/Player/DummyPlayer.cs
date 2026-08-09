using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class DummyPlayer : MonoBehaviour
{
    [Header("Inventario de Platillos (Mock)")]
    [Tooltip("Platillos que porta el jugador para entregar. En el juego final provendrá de playerDish.currentRecipe")]
    public List<DishData> availableDishes = new List<DishData>();

    [Header("Pruebas: Generación Automática de Platillos")]
    [Tooltip("Activa la generación de platillos aleatorios cada N segundos para pruebas.")]
    public bool autoGenerateTestDishes = true;
    public float dishGenerationInterval = 8f;
    public List<DishData> testDishPool = new List<DishData>();
    private float testDishTimer = 0f;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackDamage = 15f;
    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;

    [Header("Depuración UI")]
    [Tooltip("Muestra una etiqueta con las estadísticas en pantalla para pruebas.")]
    public bool showDebugLabel = true;

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

        // Cargar pool de prueba si está vacío
        if (testDishPool == null || testDishPool.Count == 0)
        {
            testDishPool = new List<DishData>();
#if UNITY_EDITOR
            string[] dishGuids = UnityEditor.AssetDatabase.FindAssets("t:DishData");
            foreach (string g in dishGuids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                DishData d = UnityEditor.AssetDatabase.LoadAssetAtPath<DishData>(path);
                if (d != null && !testDishPool.Contains(d))
                {
                    testDishPool.Add(d);
                }
            }
            Debug.Log($"[DummyPlayer] Pool de prueba cargado con {testDishPool.Count} platillos desde Assets.");
#else
            DishData[] loadedDishes = Resources.LoadAll<DishData>("");
            if (loadedDishes != null) testDishPool.AddRange(loadedDishes);
#endif
        }
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

        // Generación periódica de platillos aleatorios para pruebas
        TickTestDishGeneration();
    }

    private void TickTestDishGeneration()
    {
        if (!autoGenerateTestDishes) return;

        testDishTimer += Time.deltaTime;
        if (testDishTimer >= dishGenerationInterval)
        {
            testDishTimer = 0f;
            AddRandomTestDish();
        }
    }

    public void AddRandomTestDish()
    {
        if (testDishPool == null || testDishPool.Count == 0)
        {
            testDishPool = new List<DishData>();
#if UNITY_EDITOR
            string[] dishGuids = UnityEditor.AssetDatabase.FindAssets("t:DishData");
            foreach (string g in dishGuids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                DishData d = UnityEditor.AssetDatabase.LoadAssetAtPath<DishData>(path);
                if (d != null && !testDishPool.Contains(d))
                {
                    testDishPool.Add(d);
                }
            }
#else
            DishData[] loadedDishes = Resources.LoadAll<DishData>("");
            if (loadedDishes != null) testDishPool.AddRange(loadedDishes);
#endif
        }

        if (testDishPool == null || testDishPool.Count == 0) return;

        DishData randomDish = testDishPool[Random.Range(0, testDishPool.Count)];
        if (randomDish != null)
        {
            if (availableDishes == null) availableDishes = new List<DishData>();
            availableDishes.Add(randomDish);
            Debug.Log($"<color=cyan>[DummyPlayer] 🍲 ¡Nuevo platillo recibido para probar: '{randomDish.displayName}'! (Total platillos: {availableDishes.Count})</color>");
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
                enemy.TakeDamage(attackDamage);
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

    public bool RemoveDish(DishData dish)
    {
        return availableDishes != null && availableDishes.Remove(dish);
    }

    public bool HasDishes()
    {
        return availableDishes != null && availableDishes.Count > 0;
    }

    public void ApplyUpgrade(StatBonus bonus)
    {
        // Modificadores planos
        moveSpeed += bonus.speedIncrease;
        attackDamage += bonus.damageIncrease;
        maxHealth += bonus.healthIncrease;

        // Modificadores porcentuales (%)
        if (bonus.speedPercent != 0f) moveSpeed += moveSpeed * bonus.speedPercent;
        if (bonus.damagePercent != 0f) attackDamage += attackDamage * bonus.damagePercent;
        if (bonus.healthPercent != 0f) maxHealth += maxHealth * bonus.healthPercent;

        // Protecciones y límites mínimos seguros
        moveSpeed = Mathf.Max(1f, moveSpeed);
        attackDamage = Mathf.Max(1f, attackDamage);
        maxHealth = Mathf.Max(10f, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth + (bonus.healthIncrease > 0f ? bonus.healthIncrease : 0f), 1f, maxHealth);

        Debug.Log($"[DummyPlayer] Stats Actualizados -> Velocidad: {moveSpeed:F2}, Daño: {attackDamage:F1}, Vida Max: {maxHealth:F1}, Vida Actual: {currentHealth:F1}");
    }

    private void OnGUI()
    {
        if (!showDebugLabel) return;

        Camera mainCam = Camera.main;
        Vector3 screenPos = mainCam != null ? mainCam.WorldToScreenPoint(transform.position + Vector3.up * 1.3f) : Vector3.zero;

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 12;
        boxStyle.alignment = TextAnchor.MiddleCenter;
        boxStyle.normal.textColor = Color.yellow;
        boxStyle.fontStyle = FontStyle.Bold;

        string labelText = $"❤️ Vida: {currentHealth:F0}/{maxHealth:F0}\n⚔️ Daño: {attackDamage:F1}\n⚡ Vel: {moveSpeed:F2}";

        float width = 180f;
        float height = 55f;

        if (screenPos != Vector3.zero && screenPos.z > 0)
        {
            Rect rect = new Rect(screenPos.x - width / 2f, Screen.height - screenPos.y - height / 2f, width, height);
            GUI.Box(rect, labelText, boxStyle);
        }
        else
        {
            Rect rect = new Rect(10, 10, width, height);
            GUI.Box(rect, labelText, boxStyle);
        }
    }
}
