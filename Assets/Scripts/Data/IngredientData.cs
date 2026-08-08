using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Takernal/Data/IngredientData")]
public class IngredientData : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject visualSidekick;
}
