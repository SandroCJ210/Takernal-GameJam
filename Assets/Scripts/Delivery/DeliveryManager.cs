using UnityEngine;

/// <summary>
/// Gestor del Sistema de Entregas.
/// Evalúa la coincidencia entre los tags del platillo y los gustos/disgustos del cliente.
/// Procesa el éxito (premio), neutralidad (0 premio) o fracaso (penalización a los stats).
/// </summary>
public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    [Header("Depuración")]
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Procesa la entrega de un platillo a un cliente por parte del jugador.
    /// </summary>
    public DeliveryResult TryDeliver(DishData dish, CustomerInstance customer, PlayerFormController playerForm)
    {
        if (dish == null || customer == null || playerForm == null)
        {
            if (showDebugLogs) Debug.LogWarning("[DeliveryManager] No se pudo procesar la entrega: Referencias nulas.");
            return default;
        }

        if (customer.Data == null || CustomerManager.Instance == null)
        {
            if (showDebugLogs) Debug.LogWarning("[DeliveryManager] No se pudo procesar la entrega: falta data del cliente o CustomerManager.");
            return default;
        }

        if (!playerForm.IsDishFormActive || !playerForm.IsCurrentDish(dish))
        {
            if (showDebugLogs) Debug.LogWarning("[DeliveryManager] No se pudo procesar la entrega: el jugador no tiene activo ese platillo.");
            return default;
        }

        // 1. Calcular el score de satisfacción
        int score = CalculateSatisfactionScore(dish, customer.Data);

        // 2. Retirar el platillo del inventario del jugador
        if (!playerForm.ConsumeCurrentDishForDelivery(out DishData deliveredDish))
        {
            if (showDebugLogs) Debug.LogWarning("[DeliveryManager] No se pudo consumir el platillo activo del jugador.");
            return default;
        }

        dish = deliveredDish;

        // 3. Notificar evento de entrega realizada
        GameEvents.OnDishDelivered?.Invoke(dish);

        DeliveryResult result = new DeliveryResult(dish, customer, score);

        // 4. Aplicar resultados según el score
        if (score > 0)
        {
            // ÉXITO: Otorga una recompensa sorteada por peso al jugador
            RewardData chosenReward = RewardResolver.PickReward(customer.Data.possibleRewards);
            if (chosenReward != null)
            {
                GameEvents.OnRewardApplied?.Invoke(chosenReward.bonus);
                if (showDebugLogs)
                {
                    Debug.Log($"<color=green>[DeliveryManager] ¡ÉXITO! Cliente '{customer.Data.customerName}' encantado con '{dish.displayName}'. Score: +{score}. Recompensa obtenida: '{chosenReward.rewardName}'.</color>");
                }
            }
            else if (showDebugLogs)
            {
                Debug.Log($"<color=yellow>[DeliveryManager] ¡ÉXITO! Cliente '{customer.Data.customerName}' satisfecho, pero no tenía recompensas configuradas.</color>");
            }

            CustomerManager.Instance.RemoveCustomer(customer.SlotIndex, wasSatisfied: true);
        }
        else if (score == 0)
        {
            // NEUTRAL: Acepta la comida, pero no otorga recompensas ni penalizaciones
            if (showDebugLogs)
            {
                Debug.Log($"<color=yellow>[DeliveryManager] NEUTRAL. Cliente '{customer.Data.customerName}' recibió '{dish.displayName}'. Score: 0. Sin cambios en stats.</color>");
            }
            CustomerManager.Instance.RemoveCustomer(customer.SlotIndex, wasSatisfied: true);
        }
        else
        {
            // FRACASO: El cliente odia la comida y aplica un castigo sorteado
            PunishmentData chosenPunishment = RewardResolver.PickPunishment(customer.Data.possiblePunishments);
            if (chosenPunishment != null)
            {
                GameEvents.OnRewardApplied?.Invoke(chosenPunishment.penalty);
                if (showDebugLogs)
                {
                    Debug.LogWarning($"<color=red>[DeliveryManager] ¡DESASTRE! Cliente '{customer.Data.customerName}' odió '{dish.displayName}'. Score: {score}. Castigo recibido: '{chosenPunishment.punishmentName}'.</color>");
                }
            }
            else if (showDebugLogs)
            {
                Debug.LogWarning($"<color=red>[DeliveryManager] ¡DESASTRE! Cliente '{customer.Data.customerName}' odió '{dish.displayName}', pero no tenía castigos configurados.</color>");
            }

            CustomerManager.Instance.RemoveCustomer(customer.SlotIndex, wasSatisfied: false);
        }

        // 5. Notificar a la UI el resultado final
        GameEvents.OnDeliveryCompleted?.Invoke(result);

        return result;
    }

    /// <summary>
    /// Evalúa los tags del platillo contra los likedTags (+1) y dislikedTags (-1) del cliente.
    /// </summary>
    public int CalculateSatisfactionScore(DishData dish, CustomerData customer)
    {
        if (dish == null || customer == null) return 0;

        int score = 0;

        if (dish.tags != null)
        {
            foreach (string tag in dish.tags)
            {
                if (string.IsNullOrEmpty(tag)) continue;

                string cleanTag = tag.Trim().ToLower();

                if (customer.likedTags != null)
                {
                    foreach (string liked in customer.likedTags)
                    {
                        if (!string.IsNullOrEmpty(liked) && liked.Trim().ToLower() == cleanTag)
                        {
                            score++;
                        }
                    }
                }

                if (customer.dislikedTags != null)
                {
                    foreach (string disliked in customer.dislikedTags)
                    {
                        if (!string.IsNullOrEmpty(disliked) && disliked.Trim().ToLower() == cleanTag)
                        {
                            score--;
                        }
                    }
                }
            }
        }

        return score;
    }
}
