using System.Collections.Generic;
using UnityEngine;

public class IngredientSelectionUI : StaticInstance<IngredientSelectionUI>
{
    [Header("Referencias de UI")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private IngredientCardUI[] cards;

    protected override void Awake()
    {
        base.Awake();
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    public void ShowSelection(List<IngredientData> ingredients)
    {
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
}
