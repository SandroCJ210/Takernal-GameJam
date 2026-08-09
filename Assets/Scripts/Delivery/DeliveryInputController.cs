using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detecta la pulsación de la tecla TAB para abrir o cerrar la interfaz de entrega de platillos.
/// </summary>
public class DeliveryInputController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DeliveryPanelUI deliveryPanel;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleDeliveryPanel();
            Debug.Log("Se ha pulsado la tecla TAB. Se ha alternado la visibilidad del panel de entrega.");
        }
    }

    public void ToggleDeliveryPanel()
    {
        if (deliveryPanel == null)
        {
            deliveryPanel = DeliveryPanelUI.Instance;
        }

        if (deliveryPanel == null)
        {
            deliveryPanel = FindFirstObjectByType<DeliveryPanelUI>(FindObjectsInactive.Include);
        }

        if (deliveryPanel != null)
        {
            deliveryPanel.TogglePanel();
        }
        else
        {
            Debug.LogWarning("[DeliveryInputController] No se encontró DeliveryPanelUI en la escena.");
        }
    }
}
