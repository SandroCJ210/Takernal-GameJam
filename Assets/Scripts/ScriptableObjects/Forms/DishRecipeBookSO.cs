using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDishRecipeBook", menuName = "Takernal/Player/Dish Recipe Book")]
public class DishRecipeBookSO : ScriptableObject
{
    [SerializeField] private List<DishFormDataSO> dishForms = new List<DishFormDataSO>();

    public IReadOnlyList<DishFormDataSO> DishForms => dishForms;

    public DishFormDataSO FindDishForm(DishData dish)
    {
        if (dish == null) return null;

        for (int i = 0; i < dishForms.Count; i++)
        {
            DishFormDataSO dishForm = dishForms[i];
            if (dishForm != null && IsSameDish(dishForm.Dish, dish))
                return dishForm;
        }

        return null;
    }

    public bool CanCraft(DishFormDataSO dishForm, PlayerIngredientInventory inventory)
    {
        return dishForm != null && inventory != null && inventory.HasIngredients(dishForm.RequiredIngredients);
    }

    public List<DishFormDataSO> GetCraftableDishes(PlayerIngredientInventory inventory)
    {
        List<DishFormDataSO> craftable = new List<DishFormDataSO>();
        if (inventory == null) return craftable;

        for (int i = 0; i < dishForms.Count; i++)
        {
            DishFormDataSO dishForm = dishForms[i];
            if (CanCraft(dishForm, inventory))
                craftable.Add(dishForm);
        }

        return craftable;
    }

    private static bool IsSameDish(DishData a, DishData b)
    {
        if (a == null || b == null) return false;
        if (a == b) return true;
        if (string.IsNullOrEmpty(a.id) || string.IsNullOrEmpty(b.id)) return false;

        return a.id == b.id;
    }
}
