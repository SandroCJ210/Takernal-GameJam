using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredientPool", menuName = "Takernal/Data/IngredientPool")]
public class IngredientPoolSO : ScriptableObject
{
    public List<IngredientData> allIngredients = new List<IngredientData>();

    public List<IngredientData> GetRandomIngredients(int count)
    {
        List<IngredientData> result = new List<IngredientData>();
        if (allIngredients == null || allIngredients.Count == 0)
        {
            Debug.LogWarning("[IngredientPoolSO] No hay ingredientes en el pool global.");
            return result;
        }

        List<IngredientData> copy = new List<IngredientData>(allIngredients);
        int amountToPick = Mathf.Min(count, copy.Count);

        for (int i = 0; i < amountToPick; i++)
        {
            int randomIndex = Random.Range(0, copy.Count);
            result.Add(copy[randomIndex]);
            copy.RemoveAt(randomIndex);
        }

        return result;
    }
}
