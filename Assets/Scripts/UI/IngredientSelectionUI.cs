using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class IngredientSelectionUI : StaticInstance<IngredientSelectionUI>
{
    [Header("Referencias de UI")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private IngredientCardUI[] cards;

    [Header("Layout Responsive")]
    [SerializeField] private bool useResponsiveLayout = true;
    [SerializeField] private Vector2 screenPadding = new Vector2(32f, 48f);
    [SerializeField] private float cardSpacing = 24f;
    [SerializeField] private float minCardWidth = 150f;
    [SerializeField] private float maxCardWidth = 260f;
    [SerializeField] private float minCardHeight = 220f;
    [SerializeField] private float maxCardHeight = 380f;
    [SerializeField] private float cardAspectRatio = 0.68f;

    private RectTransform selectionPanelRect;

    protected override void Awake()
    {
        base.Awake();
        EnsureEventSystem();
        CacheLayoutReferences();

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    public void ShowSelection(List<IngredientData> ingredients)
    {
        EnsureEventSystem();

        if (Instance == null)
        {
            // Asignar instancia si el GameObject estaba desactivado al inicio
            var field = typeof(StaticInstance<IngredientSelectionUI>).GetProperty("Instance");
            if (field != null) field.SetValue(null, this);
        }

        gameObject.SetActive(true);

        if (ingredients == null || ingredients.Count == 0)
        {
            Debug.LogWarning("[IngredientSelectionUI] Lista de ingredientes vacía o nula.");
            return;
        }

        Time.timeScale = 0f;

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
        }


        GameEvents.OnIngredientSelectionOpened?.Invoke();

        int maxCards = cards != null ? Mathf.Min(cards.Length, ingredients.Count) : 0;
        for (int i = 0; i < maxCards; i++)
        {
            if (cards[i] != null)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(ingredients[i], this);
            }
        }

        ApplyResponsiveLayout(maxCards);

        // Desactivar tarjetas excedentes si hay menos ingredientes que slots
        if (cards != null)
        {
            for (int i = maxCards; i < cards.Length; i++)
            {
                if (cards[i] != null)
                {
                    cards[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void OnCardSelected(IngredientData selectedIngredient)
    {
        if (selectedIngredient == null) return;

        Debug.Log($"[IngredientSelection] 🍕 Ingrediente Seleccionado: {selectedIngredient.displayName} (ID: {selectedIngredient.id})");
        Debug.Log($"[IngredientSelection] Stats otorgados -> Vida: +{selectedIngredient.statBonus.healthIncrease}, Daño: +{selectedIngredient.statBonus.damageIncrease}, Vel: +{selectedIngredient.statBonus.speedIncrease}");

        // Evento global principal (el compañero se suscribirá a este evento para sidekicks/recetas/habilidades)
        GameEvents.OnIngredientSelected?.Invoke(selectedIngredient);

        // Evento de bonificación para aplicar stats al DummyPlayer en pruebas
        if (selectedIngredient.statBonus.healthIncrease != 0 || 
            selectedIngredient.statBonus.damageIncrease != 0 || 
            selectedIngredient.statBonus.speedIncrease != 0)
        {
            GameEvents.OnRewardApplied?.Invoke(selectedIngredient.statBonus);
        }

        ClosePanel();
    }

    private void ClosePanel()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        GameEvents.OnIngredientSelectionClosed?.Invoke();
    }

    private void CacheLayoutReferences()
    {
        if (selectionPanel != null)
            selectionPanelRect = selectionPanel.GetComponent<RectTransform>();

        if (selectionPanelRect == null)
            selectionPanelRect = GetComponent<RectTransform>();
    }

    private void ApplyResponsiveLayout(int visibleCards)
    {
        if (!useResponsiveLayout || cards == null || visibleCards <= 0) return;

        CacheLayoutReferences();

        RectTransform cardsParent = GetCardsParent(visibleCards);
        RectTransform layoutArea = selectionPanelRect != null ? selectionPanelRect : cardsParent;
        if (layoutArea == null) return;

        if (selectionPanelRect != null)
        {
            selectionPanelRect.anchorMin = Vector2.zero;
            selectionPanelRect.anchorMax = Vector2.one;
            selectionPanelRect.offsetMin = Vector2.zero;
            selectionPanelRect.offsetMax = Vector2.zero;
        }

        Canvas.ForceUpdateCanvases();

        Rect areaRect = layoutArea.rect;
        float areaWidth = areaRect.width > 0f ? areaRect.width : Screen.width;
        float areaHeight = areaRect.height > 0f ? areaRect.height : Screen.height;

        float availableWidth = Mathf.Max(1f, areaWidth - screenPadding.x * 2f);
        float availableHeight = Mathf.Max(1f, areaHeight - screenPadding.y * 2f);
        float safeSpacing = Mathf.Max(0f, cardSpacing);
        float widthFromAvailableSpace = (availableWidth - safeSpacing * (visibleCards - 1)) / visibleCards;
        float cardWidth = Mathf.Clamp(widthFromAvailableSpace, minCardWidth, maxCardWidth);
        float cardHeight = Mathf.Clamp(cardWidth / Mathf.Max(0.01f, cardAspectRatio), minCardHeight, Mathf.Min(maxCardHeight, availableHeight));

        float totalWidth = cardWidth * visibleCards + safeSpacing * (visibleCards - 1);
        if (totalWidth > availableWidth)
        {
            cardWidth = Mathf.Max(1f, (availableWidth - safeSpacing * (visibleCards - 1)) / visibleCards);
            cardHeight = Mathf.Min(availableHeight, cardWidth / Mathf.Max(0.01f, cardAspectRatio));
            totalWidth = cardWidth * visibleCards + safeSpacing * (visibleCards - 1);
        }

        if (cardsParent != null)
        {
            cardsParent.anchorMin = new Vector2(0.5f, 0.5f);
            cardsParent.anchorMax = new Vector2(0.5f, 0.5f);
            cardsParent.pivot = new Vector2(0.5f, 0.5f);
            cardsParent.anchoredPosition = Vector2.zero;
            cardsParent.sizeDelta = new Vector2(totalWidth, cardHeight);
        }

        float startX = -totalWidth * 0.5f + cardWidth * 0.5f;
        for (int i = 0; i < visibleCards; i++)
        {
            if (cards[i] == null) continue;

            RectTransform cardRect = cards[i].GetComponent<RectTransform>();
            if (cardRect == null) continue;

            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
            cardRect.anchoredPosition = new Vector2(startX + i * (cardWidth + safeSpacing), 0f);
        }
    }

    private RectTransform GetCardsParent(int visibleCards)
    {
        for (int i = 0; i < visibleCards; i++)
        {
            if (cards[i] == null) continue;

            Transform parent = cards[i].transform.parent;
            if (parent != null && parent is RectTransform parentRect)
                return parentRect;
        }

        return selectionPanelRect;
    }

    private static void EnsureEventSystem()
    {
#if UNITY_2023_1_OR_NEWER
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
#else
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
#endif
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetAsLastSibling();
            return;
        }

        StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (legacyModule != null)
        {
            if (Application.isPlaying)
                Destroy(legacyModule);
            else
                DestroyImmediate(legacyModule);
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
    }
}
