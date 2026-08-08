using UnityEngine;


public class PlayerStats : MonoBehaviour
{
    [SerializeField] private HealthComponent health;

    public float DamageMultiplier { get; private set; } = 1f;
    public float SpeedMultiplier { get; private set; } = 1f;
    public float UltimateDurationMultiplier { get; private set; } = 1f;
    public float RareIngredientChanceBonus { get; private set; } = 0f;

    public void AddDamageMultiplier(float amount) => DamageMultiplier += amount;
    public void AddSpeedMultiplier(float amount) => SpeedMultiplier += amount;
    public void AddUltimateDurationMultiplier(float amount) => UltimateDurationMultiplier += amount;
    public void AddRareIngredientChance(float amount) => RareIngredientChanceBonus += amount;
    public void AddMaxHealth(float amount) => health.IncreaseMaxHealth(amount);
}