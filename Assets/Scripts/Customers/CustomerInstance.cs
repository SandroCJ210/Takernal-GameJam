using UnityEngine;

/// <summary>
/// Representa la instancia en tiempo de ejecución de un cliente en un slot activo.
/// Mantiene el estado de su paciencia individual y el temporizador.
/// </summary>
public class CustomerInstance
{
    public CustomerData Data { get; private set; }
    public int SlotIndex { get; private set; }
    public float CurrentPatience { get; private set; }
    public float MaxPatience { get; private set; }
    public bool IsExpired { get; private set; }

    /// <summary>
    /// Devuelve el porcentaje de paciencia restante entre 0.0 (0%) y 1.0 (100%).
    /// Útil para actualizar Sliders o barras de progreso en la UI.
    /// </summary>
    public float NormalizedPatience => MaxPatience > 0f ? Mathf.Clamp01(CurrentPatience / MaxPatience) : 0f;

    public CustomerInstance(CustomerData data, int slotIndex)
    {
        Data = data;
        SlotIndex = slotIndex;
        MaxPatience = data != null && data.maxPatience > 0f ? data.maxPatience : 30f;
        CurrentPatience = MaxPatience;
        IsExpired = false;
    }

    /// <summary>
    /// Decrementa la paciencia del cliente según el deltaTime transcurrido.
    /// </summary>
    /// <returns>True si el cliente acaba de expirar en este tick.</returns>
    public bool Tick(float deltaTime)
    {
        if (IsExpired) return false;

        CurrentPatience -= deltaTime;

        if (CurrentPatience <= 0f)
        {
            CurrentPatience = 0f;
            IsExpired = true;
            return true;
        }

        return false;
    }
}
