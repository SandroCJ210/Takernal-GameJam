using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIngredientInventory : MonoBehaviour
{
    [SerializeField] private int maxIngredients;
    [SerializeField] private bool listenToIngredientSelectionEvents = true;
    [SerializeField] private AbilityController abilityController;
    [SerializeField] private List<IngredientData> ingredients = new List<IngredientData>();

    private readonly Dictionary<AbilityDataSO, int> grantedAbilityCounts = new Dictionary<AbilityDataSO, int>();

    public IReadOnlyList<IngredientData> Ingredients => ingredients;
    public int IngredientsCount => ingredients.Count;

    public event Action<IngredientData> OnIngredientAdded;
    public event Action<IngredientData> OnIngredientRemoved;
    public event Action OnIngredientsChanged;

    private void Awake()
    {
        if (abilityController == null) abilityController = GetComponent<AbilityController>();
    }

    private void Start()
    {
        RebuildGrantedAbilities();
    }

    private void OnEnable()
    {
        if (listenToIngredientSelectionEvents)
            GameEvents.OnIngredientSelected += AddIngredientFromSelection;
    }

    private void OnDisable()
    {
        if (listenToIngredientSelectionEvents)
            GameEvents.OnIngredientSelected -= AddIngredientFromSelection;
    }

    public bool AddIngredient(IngredientData ingredient)
    {
        if (ingredient == null) return false;
        if (maxIngredients > 0 && ingredients.Count >= maxIngredients) return false;

        ingredients.Add(ingredient);
        AddGrantedAbilities(ingredient);
        OnIngredientAdded?.Invoke(ingredient);
        OnIngredientsChanged?.Invoke();
        return true;
    }

    public bool RemoveIngredient(IngredientData ingredient)
    {
        int index = FindIngredientIndex(ingredient);
        if (index < 0) return false;

        IngredientData removed = ingredients[index];
        ingredients.RemoveAt(index);
        RemoveGrantedAbilities(removed);
        OnIngredientRemoved?.Invoke(removed);
        OnIngredientsChanged?.Invoke();
        return true;
    }

    public bool HasIngredients(IReadOnlyList<IngredientData> requiredIngredients)
    {
        if (requiredIngredients == null || requiredIngredients.Count == 0) return true;
        if (ingredients.Count < requiredIngredients.Count) return false;

        bool[] used = new bool[ingredients.Count];
        for (int i = 0; i < requiredIngredients.Count; i++)
        {
            int index = FindIngredientIndex(requiredIngredients[i], used);
            if (index < 0) return false;

            used[index] = true;
        }

        return true;
    }

    public bool ConsumeIngredients(IReadOnlyList<IngredientData> requiredIngredients)
    {
        if (!HasIngredients(requiredIngredients)) return false;
        if (requiredIngredients == null || requiredIngredients.Count == 0) return true;

        for (int i = 0; i < requiredIngredients.Count; i++)
        {
            int index = FindIngredientIndex(requiredIngredients[i]);
            if (index < 0) return false;

            IngredientData removed = ingredients[index];
            ingredients.RemoveAt(index);
            RemoveGrantedAbilities(removed);
            OnIngredientRemoved?.Invoke(removed);
        }

        OnIngredientsChanged?.Invoke();
        return true;
    }

    public void Clear()
    {
        if (ingredients.Count == 0) return;

        ClearGrantedAbilities();
        ingredients.Clear();
        OnIngredientsChanged?.Invoke();
    }

    public int CountIngredient(IngredientData ingredient)
    {
        int count = 0;
        for (int i = 0; i < ingredients.Count; i++)
        {
            if (IsSameIngredient(ingredients[i], ingredient))
                count++;
        }

        return count;
    }

    private int FindIngredientIndex(IngredientData ingredient, bool[] used = null)
    {
        if (ingredient == null) return -1;

        for (int i = 0; i < ingredients.Count; i++)
        {
            if (used != null && used[i]) continue;
            if (IsSameIngredient(ingredients[i], ingredient))
                return i;
        }

        return -1;
    }

    private static bool IsSameIngredient(IngredientData a, IngredientData b)
    {
        if (a == null || b == null) return false;
        if (a == b) return true;
        if (string.IsNullOrEmpty(a.id) || string.IsNullOrEmpty(b.id)) return false;

        return a.id == b.id;
    }

    private void AddIngredientFromSelection(IngredientData ingredient)
    {
        AddIngredient(ingredient);
    }

    private void AddGrantedAbilities(IngredientData ingredient)
    {
        if (ingredient == null || ingredient.grantedAbilities == null) return;

        for (int i = 0; i < ingredient.grantedAbilities.Length; i++)
        {
            AbilityDataSO ability = ingredient.grantedAbilities[i];
            if (ability == null) continue;

            if (grantedAbilityCounts.TryGetValue(ability, out int count))
            {
                grantedAbilityCounts[ability] = count + 1;
                continue;
            }

            grantedAbilityCounts[ability] = 1;
            if (abilityController != null)
                abilityController.AddAbility(ability);
        }
    }

    private void RemoveGrantedAbilities(IngredientData ingredient)
    {
        if (ingredient == null || ingredient.grantedAbilities == null) return;

        for (int i = 0; i < ingredient.grantedAbilities.Length; i++)
        {
            AbilityDataSO ability = ingredient.grantedAbilities[i];
            if (ability == null) continue;
            if (!grantedAbilityCounts.TryGetValue(ability, out int count)) continue;

            count--;
            if (count > 0)
            {
                grantedAbilityCounts[ability] = count;
                continue;
            }

            grantedAbilityCounts.Remove(ability);
            if (abilityController != null)
                abilityController.RemoveAbility(ability);
        }
    }

    private void ClearGrantedAbilities()
    {
        if (abilityController != null)
        {
            foreach (AbilityDataSO ability in grantedAbilityCounts.Keys)
                abilityController.RemoveAbility(ability);
        }

        grantedAbilityCounts.Clear();
    }

    private void RebuildGrantedAbilities()
    {
        ClearGrantedAbilities();

        for (int i = 0; i < ingredients.Count; i++)
            AddGrantedAbilities(ingredients[i]);
    }
}
