using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la interfaz visual de una tarjeta individual de cliente.
/// Muestra avatar, nombre, preferencias (gustos/disgustos) y la barra de paciencia con cambio de color.
/// </summary>
public class CustomerCardUI : MonoBehaviour
{
    [Header("Referencias de Información")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI likesText;
    [SerializeField] private TextMeshProUGUI dislikesText;

    [Header("Barra de Paciencia")]
    [SerializeField] private Slider patienceSlider;
    [SerializeField] private Image patienceFillImage;
    [SerializeField] private Gradient patienceGradient;

    [Header("Visuales y Animación")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject activeContainer;
    [SerializeField] private GameObject emptyContainer;

    private CustomerInstance currentInstance;

    private void Awake()
    {
        // Gradiente por defecto si no ha sido configurado en el Inspector (Verde -> Amarillo -> Rojo)
        if (patienceGradient == null || patienceGradient.colorKeys.Length == 0)
        {
            patienceGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(new Color(0.9f, 0.2f, 0.2f), 0.0f);  // Rojo al 0%
            colorKeys[1] = new GradientColorKey(new Color(0.95f, 0.8f, 0.1f), 0.5f); // Amarillo al 50%
            colorKeys[2] = new GradientColorKey(new Color(0.2f, 0.85f, 0.3f), 1.0f); // Verde al 100%

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);

            patienceGradient.SetKeys(colorKeys, alphaKeys);
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// Configura y muestra los datos del cliente asignado a la tarjeta.
    /// </summary>
    public void Setup(CustomerInstance instance)
    {
        currentInstance = instance;

        if (instance == null || instance.Data == null)
        {
            Clear();
            return;
        }

        if (activeContainer != null) activeContainer.SetActive(true);
        if (emptyContainer != null) emptyContainer.SetActive(false);

        // 1. Avatar
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

        // 2. Nombre
        if (nameText != null)
        {
            nameText.text = instance.Data.customerName;
        }

        // 3. Gustos
        if (likesText != null)
        {
            if (instance.Data.likedTags != null && instance.Data.likedTags.Count > 0)
            {
                likesText.text = "<color=#77DD77>Gusta: </color>" + string.Join(", ", instance.Data.likedTags);
                likesText.gameObject.SetActive(true);
            }
            else
            {
                likesText.gameObject.SetActive(false);
            }
        }

        // 4. Disgustos
        if (dislikesText != null)
        {
            if (instance.Data.dislikedTags != null && instance.Data.dislikedTags.Count > 0)
            {
                dislikesText.text = "<color=#FF6961>Odia: </color>" + string.Join(", ", instance.Data.dislikedTags);
                dislikesText.gameObject.SetActive(true);
            }
            else
            {
                dislikesText.gameObject.SetActive(false);
            }
        }

        // 5. Barra de paciencia inicial
        UpdatePatience(instance.CurrentPatience, instance.MaxPatience);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// Actualiza el valor del Slider de paciencia y cambia el color según el gradiente.
    /// </summary>
    public void UpdatePatience(float currentPatience, float maxPatience)
    {
        float normalized = maxPatience > 0f ? Mathf.Clamp01(currentPatience / maxPatience) : 0f;

        if (patienceSlider != null)
        {
            patienceSlider.value = normalized;
        }

        if (patienceFillImage != null)
        {
            patienceFillImage.color = patienceGradient.Evaluate(normalized);
        }
    }

    /// <summary>
    /// Limpia o desactiva la tarjeta cuando el slot está vacío.
    /// </summary>
    public void Clear()
    {
        currentInstance = null;

        if (activeContainer != null) activeContainer.SetActive(false);
        if (emptyContainer != null) emptyContainer.SetActive(true);

        // Si no se usa un contenedor de vacío estático, ocultar la tarjeta para que no quede un rectángulo oscuro
        if (emptyContainer == null)
        {
            gameObject.SetActive(false);
        }
    }
}
