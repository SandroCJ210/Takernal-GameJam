using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDishForm", menuName = "Takernal/Player/Dish Form Data")]
public class DishFormDataSO : PlayerFormDataSO
{
    [Header("Platillo")]
    [SerializeField] private DishData dish;
    [SerializeField] private float duration = 20f;

    public DishData Dish => dish;
    public float Duration => Mathf.Max(0f, duration);
    public IReadOnlyList<IngredientData> RequiredIngredients => dish != null ? dish.requiredIngredients : null;
    public override bool IsDishForm => true;
}
