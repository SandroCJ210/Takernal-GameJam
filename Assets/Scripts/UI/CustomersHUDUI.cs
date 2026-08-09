using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador principal del HUD de Clientes (Top-Left Panel).
/// Escucha los eventos globales de GameEvents para instanciar, actualizar y remover las tarjetas
/// de clientes dinámicamente en una fila horizontal.
/// </summary>
public class CustomersHUDUI : MonoBehaviour
{
    [Header("Contenedor y Prefab")]
    [Tooltip("Transform contenedor que posee el HorizontalLayoutGroup.")]
    [SerializeField] private Transform cardsContainer;

    [Tooltip("Prefab de la tarjeta de cliente individual.")]
    [SerializeField] private CustomerCardUI cardPrefab;

    [Tooltip("Lista opcional de tarjetas pre-creadas en la jerarquía (si se prefiere estático).")]
    [SerializeField] private List<CustomerCardUI> preCreatedCards = new List<CustomerCardUI>();

    private Dictionary<int, CustomerCardUI> slotToCardMap = new Dictionary<int, CustomerCardUI>();

    private void Awake()
    {
        FixEventSystemInputModule();

        if (cardsContainer == null)
        {
            cardsContainer = transform;
        }

        EnsureHorizontalLayoutGroup();

        // Registrar tarjetas pre-creadas si existen en la escena
        for (int i = 0; i < preCreatedCards.Count; i++)
        {
            if (preCreatedCards[i] != null)
            {
                slotToCardMap[i] = preCreatedCards[i];
                preCreatedCards[i].Clear();
            }
        }
    }

    private void EnsureHorizontalLayoutGroup()
    {
        if (cardsContainer == null) return;

        UnityEngine.UI.HorizontalLayoutGroup layout = cardsContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = cardsContainer.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        }

        layout.spacing = 15f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childScaleWidth = false;
        layout.childScaleHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private void FixEventSystemInputModule()
    {
#if UNITY_2023_1_OR_NEWER
        UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
#else
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
#endif
        if (eventSystem != null)
        {
            var legacyModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (legacyModule != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(legacyModule);
                }
                else
                {
                    DestroyImmediate(legacyModule);
                }
            }

            var inputModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputModule == null)
            {
                eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }
    }

    private void OnEnable()
    {
        GameEvents.OnCustomerSpawned += HandleCustomerSpawned;
        GameEvents.OnCustomerRemoved += HandleCustomerRemoved;
        GameEvents.OnCustomerPatienceUpdated += HandleCustomerPatienceUpdated;
    }

    private void OnDisable()
    {
        GameEvents.OnCustomerSpawned -= HandleCustomerSpawned;
        GameEvents.OnCustomerRemoved -= HandleCustomerRemoved;
        GameEvents.OnCustomerPatienceUpdated -= HandleCustomerPatienceUpdated;
    }

    private void Start()
    {
        // Sincronizar con el CustomerManager si ya tiene clientes activos
        SyncWithCustomerManager();
    }

    /// <summary>
    /// Sincroniza las tarjetas si el CustomerManager ya ha instanciado clientes antes de habilitar la UI.
    /// </summary>
    public void SyncWithCustomerManager()
    {
        if (CustomerManager.Instance == null) return;

        CustomerInstance[] activeSlots = CustomerManager.Instance.ActiveSlots;
        if (activeSlots == null) return;

        for (int i = 0; i < activeSlots.Length; i++)
        {
            if (activeSlots[i] != null)
            {
                HandleCustomerSpawned(activeSlots[i], i);
            }
            else
            {
                HandleCustomerRemoved(null, i, false);
            }
        }
    }

    private void HandleCustomerSpawned(CustomerInstance instance, int slotIndex)
    {
        CustomerCardUI card = GetOrCreateCardForSlot(slotIndex);
        if (card != null)
        {
            card.gameObject.SetActive(true);
            card.Setup(instance);
        }
    }

    private void HandleCustomerRemoved(CustomerInstance instance, int slotIndex, bool wasSatisfied)
    {
        if (slotToCardMap.TryGetValue(slotIndex, out CustomerCardUI card) && card != null)
        {
            card.Clear();
        }
    }

    private void HandleCustomerPatienceUpdated(int slotIndex, float currentPatience, float maxPatience)
    {
        if (slotToCardMap.TryGetValue(slotIndex, out CustomerCardUI card) && card != null)
        {
            card.UpdatePatience(currentPatience, maxPatience);
        }
    }

    private CustomerCardUI GetOrCreateCardForSlot(int slotIndex)
    {
        if (slotToCardMap.TryGetValue(slotIndex, out CustomerCardUI existingCard) && existingCard != null)
        {
            return existingCard;
        }

        if (cardPrefab == null)
        {
            // Intentar cargar fallback desde Resources si no está asignado
            cardPrefab = Resources.Load<CustomerCardUI>("UI/CustomerCard");
        }

        if (cardPrefab == null)
        {
            Debug.LogWarning("[CustomersHUDUI] No se pudo crear tarjeta para el slot " + slotIndex + ": cardPrefab no está asignado en Inspector.");
            return null;
        }

        CustomerCardUI newCard = Instantiate(cardPrefab, cardsContainer);
        newCard.name = $"CustomerCard_Slot_{slotIndex}";
        slotToCardMap[slotIndex] = newCard;
        return newCard;
    }
}
