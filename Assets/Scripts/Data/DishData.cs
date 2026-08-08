using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDish", menuName = "Takernal/Data/DishData")]
public class DishData : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public List<IngredientData> requiredIngredients = new List<IngredientData>();
    public List<string> tags = new List<string>();
}
