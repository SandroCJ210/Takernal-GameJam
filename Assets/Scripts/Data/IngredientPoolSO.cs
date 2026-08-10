using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredientPool", menuName = "Takernal/Data/IngredientPool")]
public class IngredientPoolSO : ScriptableObject
{
    public List<IngredientData> allIngredients = new List<IngredientData>();

    // TODO(Jam): Esto es un shortcut temporal para probar Pizza rapido.
    // Despues de la entrega, reemplazarlo por reglas reales de loot/recetas/clientes.
    [SerializeField] private List<IngredientData> guaranteedIngredients = new List<IngredientData>();

    public List<IngredientData> GetRandomIngredients(int count)
    {
        List<IngredientData> result = new List<IngredientData>();
        if ((allIngredients == null || allIngredients.Count == 0) && (guaranteedIngredients == null || guaranteedIngredients.Count == 0))
        {
            Debug.LogWarning("[IngredientPoolSO] No hay ingredientes en el pool global.");
            return result;
        }

        AddGuaranteedIngredients(result, count);

        List<IngredientData> copy = BuildRandomPool(result);
        int amountToPick = Mathf.Min(count - result.Count, copy.Count);

        for (int i = 0; i < amountToPick; i++)
        {
            int randomIndex = Random.Range(0, copy.Count);
            result.Add(copy[randomIndex]);
            copy.RemoveAt(randomIndex);
        }

        return result;
    }

    private void AddGuaranteedIngredients(List<IngredientData> result, int maxCount)
    {
        if (guaranteedIngredients == null) return;

        for (int i = 0; i < guaranteedIngredients.Count && result.Count < maxCount; i++)
        {
            IngredientData ingredient = guaranteedIngredients[i];
            if (ingredient == null || ContainsIngredient(result, ingredient)) continue;

            result.Add(ingredient);
        }
    }

    private List<IngredientData> BuildRandomPool(List<IngredientData> alreadyPicked)
    {
        List<IngredientData> copy = new List<IngredientData>();
        if (allIngredients == null) return copy;

        for (int i = 0; i < allIngredients.Count; i++)
        {
            IngredientData ingredient = allIngredients[i];
            if (ingredient == null || ContainsIngredient(alreadyPicked, ingredient)) continue;

            copy.Add(ingredient);
        }

        return copy;
    }

    private static bool ContainsIngredient(List<IngredientData> ingredients, IngredientData target)
    {
        if (ingredients == null || target == null) return false;

        for (int i = 0; i < ingredients.Count; i++)
        {
            IngredientData ingredient = ingredients[i];
            if (ingredient == null) continue;
            if (ingredient == target) return true;
            if (!string.IsNullOrEmpty(ingredient.id) && ingredient.id == target.id) return true;
        }

        return false;
    }
}
