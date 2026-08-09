using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la visualización de un elemento de platillo en la lista del panel de entrega.
/// </summary>
public class DeliveryDishItemUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI tagsText;
    [SerializeField] private Image selectionBorder;
    [SerializeField] private Button selectButton;

    private DishData dishData;
    private Action<DeliveryDishItemUI> onSelectedCallback;
    private DraggableDishUI draggableComp;

    public DishData DishData => dishData;

    private void Awake()
    {
        draggableComp = GetComponent<DraggableDishUI>();
        if (draggableComp == null) draggableComp = gameObject.AddComponent<DraggableDishUI>();

        if (selectButton == null) selectButton = GetComponent<Button>();
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnClicked);
        }
    }

    public void Setup(DishData data, Canvas parentCanvas, Action<DeliveryDishItemUI> onSelected)
    {
        dishData = data;
        onSelectedCallback = onSelected;

        if (data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (iconImage != null)
        {
            if (data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        if (nameText != null)
        {
            nameText.text = data.displayName;
        }

        if (tagsText != null)
        {
            if (data.tags != null && data.tags.Count > 0)
            {
                tagsText.text = "Tags: " + string.Join(", ", data.tags);
            }
            else
            {
                tagsText.text = "Sin tags";
            }
        }

        if (draggableComp != null)
        {
            draggableComp.Setup(data, parentCanvas);
        }

        SetSelected(false);
    }

    private void OnClicked()
    {
        onSelectedCallback?.Invoke(this);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionBorder != null)
        {
            selectionBorder.gameObject.SetActive(isSelected);
        }
    }
}
