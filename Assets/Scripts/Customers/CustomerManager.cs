using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestor principal del Sistema de Clientes.
/// Administra la cola activa de clientes en slots, la paciencia individual de cada uno
/// y el temporizador de generación periódica (cada N segundos).
/// </summary>
public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("Pool & Limites")]
    [Tooltip("Pool de plantillas ScriptableObject de clientes disponibles.")]
    [SerializeField] private List<CustomerData> customerPool = new List<CustomerData>();

    [Tooltip("Número máximo de clientes simultáneos permitidos en la cola activa.")]
    [SerializeField] private int maxActiveCustomers = 3;

    [Header("Configuración de Spawning")]
    [Tooltip("Tiempo en segundos (N) entre cada intento de spawn de un cliente nuevo.")]
    [SerializeField] private float spawnInterval = 5f;

    [Tooltip("Inicia el temporizador de spawn automáticamente al arrancar la escena.")]
    [SerializeField] private bool autoStartSpawning = true;

    [Header("Depuración")]
    [SerializeField] private bool showDebugLogs = true;

    // Estado Runtime
    private CustomerInstance[] activeSlots;
    private float spawnTimer = 0f;
    private bool isSpawningActive = false;

    public int MaxActiveCustomers => maxActiveCustomers;
    public int ActiveCustomerCount { get; private set; }
    public CustomerInstance[] ActiveSlots => activeSlots;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeSlots();
    }

    private void Start()
    {
        if (autoStartSpawning)
        {
            StartSpawning();
        }
    }

    private void Update()
    {
        // 1. Manejo del Spawning periódico cada N segundos
        if (isSpawningActive)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                TrySpawnCustomer();
            }
        }

        // 2. Manejo y actualización de paciencia de clientes activos
        TickActiveCustomers();
    }

    /// <summary>
    /// Inicializa la estructura interna de slots fijos según maxActiveCustomers.
    /// </summary>
    public void InitializeSlots()
    {
        activeSlots = new CustomerInstance[maxActiveCustomers];
        ActiveCustomerCount = 0;
    }

    /// <summary>
    /// Inicia el ciclo continuo de generación de clientes cada spawnInterval segundos.
    /// Spawnea inmediatamente el primer cliente si la cola está vacía.
    /// </summary>
    public void StartSpawning()
    {
        isSpawningActive = true;
        spawnTimer = 0f;

        if (showDebugLogs)
        {
            Debug.Log($"[CustomerManager] Spawning iniciado. Intervalo: {spawnInterval}s, Máximo activos: {maxActiveCustomers}");
        }

        // Intento de spawn inmediato al iniciar si hay espacio
        TrySpawnCustomer();
    }

    /// <summary>
    /// Detiene temporalmente la generación de nuevos clientes.
    /// </summary>
    public void StopSpawning()
    {
        isSpawningActive = false;
        if (showDebugLogs)
        {
            Debug.Log("[CustomerManager] Spawning detenido.");
        }
    }

    /// <summary>
    /// Intenta colocar un nuevo cliente aleatorio del pool en el primer slot disponible.
    /// </summary>
    /// <returns>True si se pudo spawnear con éxito un cliente.</returns>
    public bool TrySpawnCustomer()
    {
        if (activeSlots == null)
        {
            InitializeSlots();
        }

        if (ActiveCustomerCount >= maxActiveCustomers)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[CustomerManager] No se pudo spawnear: Cola llena ({ActiveCustomerCount}/{maxActiveCustomers}).");
            }
            return false;
        }

        List<CustomerData> validPool = customerPool != null ? customerPool.FindAll(c => c != null) : null;
        if (validPool == null || validPool.Count == 0)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("[CustomerManager] No hay clientes asignados en el Customer Pool.");
            }
            return false;
        }

        int freeSlotIndex = GetFirstFreeSlotIndex();
        if (freeSlotIndex == -1) return false;

        // Selección aleatoria del pool válido
        CustomerData selectedData = validPool[Random.Range(0, validPool.Count)];
        CustomerInstance newInstance = new CustomerInstance(selectedData, freeSlotIndex);

        activeSlots[freeSlotIndex] = newInstance;
        ActiveCustomerCount++;

        string customerName = selectedData != null && !string.IsNullOrEmpty(selectedData.customerName) ? selectedData.customerName : "Cliente Desconocido";
        float maxPatience = selectedData != null ? selectedData.maxPatience : 30f;

        if (showDebugLogs)
        {
            Debug.Log($"<color=green>[CustomerManager] Cliente '{customerName}' spawnearon en slot [{freeSlotIndex}]. Paciencia: {maxPatience}s. (Activos: {ActiveCustomerCount}/{maxActiveCustomers})</color>");
        }

        GameEvents.OnCustomerSpawned?.Invoke(newInstance, freeSlotIndex);
        return true;
    }

    /// <summary>
    /// Remueve al cliente de un slot específico (por entrega realizada o porque expiró su paciencia).
    /// </summary>
    public void RemoveCustomer(int slotIndex, bool wasSatisfied)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Length) return;

        CustomerInstance instance = activeSlots[slotIndex];
        if (instance == null) return;

        activeSlots[slotIndex] = null;
        ActiveCustomerCount = Mathf.Max(0, ActiveCustomerCount - 1);

        if (showDebugLogs)
        {
            string reason = wasSatisfied ? "Atendido con éxito" : "Expiró paciencia";
            Debug.Log($"<color=yellow>[CustomerManager] Cliente '{instance.Data.customerName}' removido del slot [{slotIndex}]. Razón: {reason}. (Activos: {ActiveCustomerCount}/{maxActiveCustomers})</color>");
        }

        GameEvents.OnCustomerRemoved?.Invoke(instance, slotIndex, wasSatisfied);
    }

    /// <summary>
    /// Actualiza frame a frame la paciencia de todos los clientes activos.
    /// </summary>
    private void TickActiveCustomers()
    {
        if (activeSlots == null) return;

        for (int i = 0; i < activeSlots.Length; i++)
        {
            CustomerInstance customer = activeSlots[i];
            if (customer == null) continue;

            bool expiredNow = customer.Tick(Time.deltaTime);

            // Notifica la actualización de paciencia a la UI
            GameEvents.OnCustomerPatienceUpdated?.Invoke(i, customer.CurrentPatience, customer.MaxPatience);

            if (expiredNow)
            {
                HandleCustomerExpired(customer);
            }
        }
    }

    /// <summary>
    /// Maneja el momento exacto en que la paciencia de un cliente llega a cero.
    /// Sortea un castigo de sus possiblePunishments y lo aplica al jugador.
    /// </summary>
    private void HandleCustomerExpired(CustomerInstance customer)
    {
        PunishmentData chosenPunishment = RewardResolver.PickPunishment(customer.Data.possiblePunishments);
        if (chosenPunishment != null)
        {
            GameEvents.OnRewardApplied?.Invoke(chosenPunishment.penalty);
            if (showDebugLogs)
            {
                Debug.LogWarning($"<color=red>[CustomerManager] ¡Paciencia agotada! El cliente '{customer.Data.customerName}' en slot [{customer.SlotIndex}] se fue descontento. Castigo aplicado: '{chosenPunishment.punishmentName}'.</color>");
            }
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning($"<color=red>[CustomerManager] ¡Paciencia agotada! El cliente '{customer.Data.customerName}' en slot [{customer.SlotIndex}] se fue descontento (sin castigos configurados).</color>");
        }

        GameEvents.OnCustomerExpired?.Invoke(customer, customer.SlotIndex);
        RemoveCustomer(customer.SlotIndex, wasSatisfied: false);
    }

    private int GetFirstFreeSlotIndex()
    {
        if (activeSlots == null) return -1;

        for (int i = 0; i < activeSlots.Length; i++)
        {
            if (activeSlots[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Permite ajustar dinámicamente en tiempo de ejecución el límite máximo de clientes activos.
    /// </summary>
    public void SetMaxActiveCustomers(int newMax)
    {
        if (newMax <= 0) return;

        int oldMax = maxActiveCustomers;
        maxActiveCustomers = newMax;
        System.Array.Resize(ref activeSlots, maxActiveCustomers);

        if (showDebugLogs)
        {
            Debug.Log($"[CustomerManager] Capacidad de clientes cambiada de {oldMax} a {maxActiveCustomers}.");
        }
    }

    /// <summary>
    /// Obtiene la instancia activa de cliente en un determinado slot index (o null si está vacío).
    /// </summary>
    public CustomerInstance GetCustomerInSlot(int slotIndex)
    {
        if (activeSlots == null || slotIndex < 0 || slotIndex >= activeSlots.Length) return null;
        return activeSlots[slotIndex];
    }
}
