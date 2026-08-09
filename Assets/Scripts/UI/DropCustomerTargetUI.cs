using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Detecta cuando un DraggableDishUI es soltado sobre la tarjeta de un cliente en el panel de entrega.
/// </summary>
public class DropCustomerTargetUI : MonoBehaviour, IDropHandler
{
    private CustomerInstance customerInstance;
    private DeliveryPanelUI deliveryPanel;

    public void Setup(CustomerInstance customer, DeliveryPanelUI panel)
    {
        customerInstance = customer;
        deliveryPanel = panel;
    }

    public CustomerInstance CustomerInstance => customerInstance;

    public void OnDrop(PointerEventData eventData)
    {
        if (customerInstance == null || deliveryPanel == null) return;

        DraggableDishUI draggedDish = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<DraggableDishUI>() : null;
        if (draggedDish != null && draggedDish.DishData != null)
        {
            deliveryPanel.DeliverDishToCustomer(draggedDish.DishData, customerInstance);
        }
    }
}
