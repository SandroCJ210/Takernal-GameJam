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
}

