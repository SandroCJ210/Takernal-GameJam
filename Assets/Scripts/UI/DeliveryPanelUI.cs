using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la interfaz principal de Entrega de Platillos.
/// Se abre/cierra al presionar TAB. Muestra inventario de platillos y clientes activos.
/// Soporta selección por clic y entregas por Drag & Drop.
/// </summary>
public class DeliveryPanelUI : MonoBehaviour
{
    public static DeliveryPanelUI Instance { get; private set; }

    [Header("Contenedores")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform dishesContainer;
    [SerializeField] private Transform customersContainer;

    [Header("Prefabs de Elementos UI")]
    [SerializeField] private DeliveryDishItemUI dishItemPrefab;
    [SerializeField] private DeliveryCustomerItemUI customerItemPrefab;

    [Header("UI de Confirmación y Preview")]
    [SerializeField] private TextMeshProUGUI previewText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button deliverButton;
    [SerializeField] private Button closeButton;

    [Header("Referencias")]
    [SerializeField] private PlayerFormController player;

    private Canvas parentCanvas;
    private List<DeliveryDishItemUI> spawnedDishItems = new List<DeliveryDishItemUI>();
    private List<DeliveryCustomerItemUI> spawnedCustomerItems = new List<DeliveryCustomerItemUI>();

    private DeliveryDishItemUI selectedDishItem;
    private DeliveryCustomerItemUI selectedCustomerItem;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        parentCanvas = GetComponentInParent<Canvas>();

        if (deliverButton != null)
        {
            deliverButton.onClick.AddListener(OnDeliverButtonClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerFormController>();
        }
    }

    /// <summary>
    /// Alterna el estado de apertura/cierre del panel.
    /// </summary>
    public void TogglePanel()
    {
        if (IsOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    /// <summary>
    /// Abre el panel de entrega, lo posiciona al frente del Canvas, pausa el juego y puebla la lista.
    /// </summary>
    public void OpenPanel()
    {
        // Posicionar al final de la jerarquía del Canvas para que se dibuje por encima de todo
        transform.SetAsLastSibling();

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        Time.timeScale = 0f;
        ClearSelection();
        RefreshLists();

        if (feedbackText != null) feedbackText.text = "";

        GameEvents.OnDeliveryPanelOpened?.Invoke();
    }

    /// <summary>
    /// Cierra el panel de entrega y reanuda el tiempo del juego.
    /// </summary>
    public void ClosePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        Time.timeScale = 1f;
        ClearSelection();

        GameEvents.OnDeliveryPanelClosed?.Invoke();
    }

    /// <summary>
    /// Refresca ambas listas (platillos y clientes).
    /// </summary>
    public void RefreshLists()
    {
        PopulateDishes();
        PopulateCustomers();
        UpdatePreview();
    }

    private List<DishData> GetPlayerDishes()
    {
        // La entrega final usa la forma activa del jugador real, no el DummyPlayer de pruebas.
        if (player == null) player = FindFirstObjectByType<PlayerFormController>();
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) player = playerObj.GetComponent<PlayerFormController>();
        }

        List<DishData> activeDish = new List<DishData>();
        if (player != null && player.IsDishFormActive && player.CurrentDish != null)
            activeDish.Add(player.CurrentDish);

        return activeDish;
    }

    private void PopulateDishes()
    {
        // Limpiar items anteriores
        foreach (var item in spawnedDishItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        spawnedDishItems.Clear();

        if (dishesContainer == null) return;

        List<DishData> dishesToDisplay = GetPlayerDishes();
        foreach (var dish in dishesToDisplay)
        {
            if (dish == null) continue;

            DeliveryDishItemUI itemUI = Instantiate(dishItemPrefab, dishesContainer);
            itemUI.Setup(dish, parentCanvas, OnDishItemSelected);
            spawnedDishItems.Add(itemUI);
        }
    }

    private void PopulateCustomers()
    {
        // Limpiar items anteriores
        foreach (var item in spawnedCustomerItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        spawnedCustomerItems.Clear();

        if (customersContainer == null || CustomerManager.Instance == null) return;

        CustomerInstance[] activeSlots = CustomerManager.Instance.ActiveSlots;
        if (activeSlots == null) return;

        for (int i = 0; i < activeSlots.Length; i++)
        {
            CustomerInstance customer = activeSlots[i];
            if (customer == null) continue;

            DeliveryCustomerItemUI itemUI = Instantiate(customerItemPrefab, customersContainer);
            itemUI.Setup(customer, this, OnCustomerItemSelected);
            spawnedCustomerItems.Add(itemUI);
        }
    }

    private void OnDishItemSelected(DeliveryDishItemUI item)
    {
        if (selectedDishItem != null) selectedDishItem.SetSelected(false);
        selectedDishItem = item;

        if (selectedDishItem != null) selectedDishItem.SetSelected(true);

        UpdatePreview();
    }

    private void OnCustomerItemSelected(DeliveryCustomerItemUI item)
    {
        if (selectedCustomerItem != null) selectedCustomerItem.SetSelected(false);
        selectedCustomerItem = item;

        if (selectedCustomerItem != null) selectedCustomerItem.SetSelected(true);

        UpdatePreview();
    }

    private void ClearSelection()
    {
        if (selectedDishItem != null) selectedDishItem.SetSelected(false);
        if (selectedCustomerItem != null) selectedCustomerItem.SetSelected(false);

        selectedDishItem = null;
        selectedCustomerItem = null;

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (deliverButton != null)
        {
            deliverButton.interactable = (selectedDishItem != null && selectedCustomerItem != null);
        }

        if (previewText == null) return;

        if (selectedDishItem == null || selectedCustomerItem == null)
        {
            previewText.text = "Selecciona un platillo y un cliente (o arrastra el platillo hacia el cliente).";
            return;
        }

        DishData dish = selectedDishItem.DishData;
        CustomerInstance customer = selectedCustomerItem.CustomerInstance;

        if (dish == null || customer == null || customer.Data == null)
        {
            previewText.text = "";
            return;
        }

        int score = DeliveryManager.Instance != null ? DeliveryManager.Instance.CalculateSatisfactionScore(dish, customer.Data) : 0;

        string resultLabel;
        if (score > 0) resultLabel = "<color=#77DD77>Éxito (+) ¡Le gustará!</color>";
        else if (score == 0) resultLabel = "<color=#FDFD96>Neutral (0) Indiferente</color>";
        else resultLabel = "<color=#FF6961>Desastre (-) ¡Lo odiará!</color>";

        previewText.text = $"<b>Entregar:</b> {dish.displayName}  ➡  {customer.Data.customerName}\n<b>Pronóstico:</b> {resultLabel} (Score: {score})";
    }

    private void OnDeliverButtonClicked()
    {
        if (selectedDishItem == null || selectedCustomerItem == null) return;

        DeliverDishToCustomer(selectedDishItem.DishData, selectedCustomerItem.CustomerInstance);
    }

    /// <summary>
    /// Ejecuta la entrega del platillo al cliente especificado (llamado por el botón o por Drag & Drop).
    /// </summary>
    public void DeliverDishToCustomer(DishData dish, CustomerInstance customer)
    {
        if (dish == null || customer == null) return;
        if (player == null) player = FindFirstObjectByType<PlayerFormController>();

        if (DeliveryManager.Instance == null)
        {
            Debug.LogWarning("[DeliveryPanelUI] No se encontró DeliveryManager en la escena.");
            return;
        }

        DeliveryResult result = DeliveryManager.Instance.TryDeliver(dish, customer, player);

        if (!result.IsValid)
        {
            if (feedbackText != null)
                feedbackText.text = "<color=#FF6961>No hay un platillo activo para entregar.</color>";

            ClearSelection();
            RefreshLists();
            return;
        }

        // Feedback al usuario
        if (feedbackText != null)
        {
            if (result.Score > 0)
            {
                feedbackText.text = $"<color=#77DD77>¡Entrega Exitosa! {customer.Data.customerName} disfrutó {dish.displayName}. (+Stats)</color>";
            }
            else if (result.Score == 0)
            {
                feedbackText.text = $"<color=#FDFD96>Entrega Neutral. {customer.Data.customerName} recibió {dish.displayName}. (Sin cambios)</color>";
            }
            else
            {
                feedbackText.text = $"<color=#FF6961>¡Mala Entrega! A {customer.Data.customerName} no le gustó {dish.displayName}. (-Stats)</color>";
            }
        }

        ClearSelection();
        RefreshLists();
    }
}
