using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la visualización de un cliente en la lista del panel de entrega.
/// </summary>
public class DeliveryCustomerItemUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI likesText;
    [SerializeField] private TextMeshProUGUI dislikesText;
    [SerializeField] private Slider patienceSlider;
    [SerializeField] private Image selectionBorder;
    [SerializeField] private Button selectButton;

    private CustomerInstance customerInstance;
    private Action<DeliveryCustomerItemUI> onSelectedCallback;
    private DropCustomerTargetUI dropTargetComp;

    public CustomerInstance CustomerInstance => customerInstance;

    private void Awake()
    {
        dropTargetComp = GetComponent<DropCustomerTargetUI>();
        if (dropTargetComp == null) dropTargetComp = gameObject.AddComponent<DropCustomerTargetUI>();

        if (selectButton == null) selectButton = GetComponent<Button>();
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnClicked);
        }
    }

    public void Setup(CustomerInstance instance, DeliveryPanelUI deliveryPanel, Action<DeliveryCustomerItemUI> onSelected)
    {
        customerInstance = instance;
        onSelectedCallback = onSelected;

        if (instance == null || instance.Data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (avatarImage != null)
        {
            if (instance.Data.avatar != null)
            {
                avatarImage.sprite = instance.Data.avatar;
                avatarImage.gameObject.SetActive(true);
            }
            else
            {
                avatarImage.gameObject.SetActive(false);
            }
        }

        if (nameText != null)
        {
            nameText.text = instance.Data.customerName;
        }

        if (likesText != null)
        {
            if (instance.Data.likedTags != null && instance.Data.likedTags.Count > 0)
            {
                likesText.text = "<color=#77DD77>Gusta: </color>" + string.Join(", ", instance.Data.likedTags);
            }
            else
            {
                likesText.text = "<color=#77DD77>Gusta: </color>-";
            }
        }

        if (dislikesText != null)
        {
            if (instance.Data.dislikedTags != null && instance.Data.dislikedTags.Count > 0)
            {
                dislikesText.text = "<color=#FF6961>Odia: </color>" + string.Join(", ", instance.Data.dislikedTags);
            }
            else
            {
                dislikesText.text = "<color=#FF6961>Odia: </color>-";
            }
        }

        if (patienceSlider != null)
        {
            patienceSlider.value = instance.NormalizedPatience;
        }

        if (dropTargetComp != null)
        {
            dropTargetComp.Setup(instance, deliveryPanel);
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
