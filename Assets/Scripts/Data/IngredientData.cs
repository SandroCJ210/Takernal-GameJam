using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Takernal/Data/IngredientData")]
public class IngredientData : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject visualSidekick;

    [Header("UI de Tarjeta (Selección Post-Oleada)")]
    [TextArea(2, 3)]
    public string flavorText;
    public string abilityDescription;
    public IngredientRarity rarity;

    [Header("Bonus de Stats al Adquirirse")]
    public StatBonus statBonus;

    [Header("Habilidades al Adquirirse")]
    public AbilityDataSO[] grantedAbilities;
}
