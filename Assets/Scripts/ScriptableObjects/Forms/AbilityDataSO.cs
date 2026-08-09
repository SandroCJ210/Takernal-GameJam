using UnityEngine;

public abstract class AbilityDataSO : ScriptableObject, IIngredientAbility
{
    [SerializeField] private string abilityName;

    public string AbilityName => string.IsNullOrEmpty(abilityName) ? name : abilityName;

    public virtual void OnAcquired(PlayerCombat owner) { }
    public virtual void OnRemoved(PlayerCombat owner) { }
}
