using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientCardUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI flavorText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI abilityText;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private Button cardButton;

    private IngredientData data;
    private IngredientSelectionUI selectionManager;

    private void Awake()
    {
        if (cardButton == null)
            cardButton = GetComponent<Button>();

        if (cardButton != null)
            cardButton.onClick.AddListener(OnCardClicked);
    }

    public void Setup(IngredientData ingredient, IngredientSelectionUI manager)
    {
        data = ingredient;
        selectionManager = manager;

        if (data == null) return;

        ApplyTextDefaults();

        if (iconImage != null)
            iconImage.sprite = data.icon;

        if (nameText != null)
            nameText.text = data.displayName;

        if (flavorText != null)
            flavorText.text = data.flavorText;

        if (abilityText != null)
        {
            abilityText.text = string.IsNullOrEmpty(data.abilityDescription) 
                ? "Habilidad: Específica del ingrediente" 
                : data.abilityDescription;
        }

        if (statsText != null)
            statsText.text = BuildStatsText(data.statBonus);

        if (rarityBorder != null)
            rarityBorder.color = GetRarityColor(data.rarity);
    }

    private string BuildStatsText(StatBonus bonus)
    {
        List<string> parts = new List<string>();
        if (bonus.healthIncrease != 0) parts.Add($"<color=#FF6666>+ {bonus.healthIncrease} Vida</color>");
        if (bonus.damageIncrease != 0) parts.Add($"<color=#FFCC00>+ {bonus.damageIncrease} Daño</color>");
        if (bonus.speedIncrease != 0) parts.Add($"<color=#66CCFF>+ {bonus.speedIncrease} Velocidad</color>");

        return parts.Count > 0 ? string.Join("\n", parts) : "Sin cambio de stats";
    }


    private Color GetRarityColor(IngredientRarity rarity)
    {
        return rarity switch
        {
            IngredientRarity.Common => new Color(0.8f, 0.8f, 0.8f, 1f),
            IngredientRarity.Rare   => new Color(0.2f, 0.6f, 1f, 1f),
            IngredientRarity.Epic   => new Color(1f, 0.7f, 0f, 1f),
            _ => Color.white
        };
    }

    private void OnCardClicked()
    {
        if (selectionManager != null && data != null)
        {
            selectionManager.OnCardSelected(data);
        }
    }

    private void ApplyTextDefaults()
    {
        ConfigureText(nameText, 14f, 22f, TextOverflowModes.Ellipsis);
        ConfigureText(flavorText, 9f, 13f, TextOverflowModes.Ellipsis);
        ConfigureText(statsText, 10f, 16f, TextOverflowModes.Ellipsis);
        ConfigureText(abilityText, 9f, 13f, TextOverflowModes.Ellipsis);
    }

    private void ConfigureText(TextMeshProUGUI text, float minSize, float maxSize, TextOverflowModes overflowMode)
    {
        if (text == null) return;

        text.textWrappingMode = TextWrappingModes.Normal;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.overflowMode = overflowMode;
    }
}
