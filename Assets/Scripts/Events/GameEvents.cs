using System;
using UnityEngine;

public static class GameEvents
{
    // Ocurre cuando el enemigo muere o la oleada es superada
    public static Action OnWaveCompleted;
    
    // Ocurre cuando el jugador elige un ingrediente en la UI post-oleada
    public static Action<IngredientData> OnIngredientSelected;

    // Ocurre cuando el jugador entrega un platillo en la zona de entrega
    public static Action<DishData> OnDishDelivered;

    // Ocurre cuando se procesa la entrega con éxito y se aplica un buff
    public static Action<StatBonus> OnRewardApplied;

    // Señales de apertura y cierre de la UI de selección de ingredientes
    public static Action OnIngredientSelectionOpened;
    public static Action OnIngredientSelectionClosed;

    // Eventos del Sistema de Clientes
    public static Action<CustomerInstance, int> OnCustomerSpawned;
    public static Action<CustomerInstance, int, bool> OnCustomerRemoved; // (instancia, slotIndex, fueAtendido)
    public static Action<int, float, float> OnCustomerPatienceUpdated; // (slotIndex, currentPatience, maxPatience)
    public static Action<CustomerInstance, int> OnCustomerExpired;

    // Eventos del Sistema de Entrega
    public static Action OnDeliveryPanelOpened;
    public static Action OnDeliveryPanelClosed;
    public static Action<DeliveryResult> OnDeliveryCompleted;
}

