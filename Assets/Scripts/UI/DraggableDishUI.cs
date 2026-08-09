using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Permite arrastrar una tarjeta de platillo con el ratón o touch y soltarla sobre un cliente.
/// </summary>
public class DraggableDishUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private DishData dishData;
    private Canvas parentCanvas;
    private GameObject dragIconObject;
    private RectTransform dragIconRect;
    private CanvasGroup canvasGroup;

    public void Setup(DishData data, Canvas canvas)
    {
        dishData = data;
        parentCanvas = canvas;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public DishData DishData => dishData;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dishData == null) return;

        // Crear ícono flotante de arrastre
        dragIconObject = new GameObject("DishDragPreview");
        dragIconObject.transform.SetParent(parentCanvas != null ? parentCanvas.transform : transform.root, false);
        dragIconObject.transform.SetAsLastSibling();

        Image img = dragIconObject.AddComponent<Image>();
        if (dishData.icon != null) img.sprite = dishData.icon;
        img.raycastTarget = false; // Importante para que no bloquee el evento OnDrop del cliente

        dragIconRect = dragIconObject.GetComponent<RectTransform>();
        dragIconRect.sizeDelta = new Vector2(64f, 64f);

        canvasGroup.alpha = 0.5f;

        UpdateDragPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconObject != null)
        {
            UpdateDragPosition(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CleanupDragPreview();
    }

    private void OnDisable()
    {
        CleanupDragPreview();
    }

    private void OnDestroy()
    {
        CleanupDragPreview();
    }

    public void CleanupDragPreview()
    {
        if (dragIconObject != null)
        {
            Destroy(dragIconObject);
            dragIconObject = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    private void UpdateDragPosition(PointerEventData eventData)
    {
        if (dragIconRect == null) return;

        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas != null ? (RectTransform)parentCanvas.transform : (RectTransform)dragIconRect.parent,
            eventData.position,
            parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? parentCanvas.worldCamera : null,
            out mousePos
        );
        dragIconRect.anchoredPosition = mousePos;
    }
}
