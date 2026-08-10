using UnityEngine;


public class PlayerStats : MonoBehaviour
{
    [SerializeField] private HealthComponent health;
    [SerializeField] private bool listenToRewardEvents = true;

    public float FlatDamageBonus { get; private set; }
    public float FlatSpeedBonus { get; private set; }
    public float DamageMultiplier { get; private set; } = 1f;
    public float SpeedMultiplier { get; private set; } = 1f;
    public float UltimateDurationMultiplier { get; private set; } = 1f;

    private void Awake()
    {
        if (health == null) health = GetComponent<HealthComponent>();
    }

    private void OnEnable()
    {
        if (listenToRewardEvents)
            GameEvents.OnRewardApplied += ApplyStatBonus;
    }

    private void OnDisable()
    {
        if (listenToRewardEvents)
            GameEvents.OnRewardApplied -= ApplyStatBonus;
    }

    public void ApplyStatBonus(StatBonus bonus)
    {
        AddFlatDamage(bonus.damageIncrease);
        AddFlatSpeed(bonus.speedIncrease);
        AddDamageMultiplier(bonus.damagePercent);
        AddSpeedMultiplier(bonus.speedPercent);

        if (bonus.healthIncrease != 0f)
            AddMaxHealth(bonus.healthIncrease);

        if (bonus.healthPercent != 0f && health != null)
            AddMaxHealth(health.Max * bonus.healthPercent);
    }

    public void AddFlatDamage(float amount) => FlatDamageBonus += amount;
    public void AddFlatSpeed(float amount) => FlatSpeedBonus += amount;
    public void AddDamageMultiplier(float amount) => DamageMultiplier = Mathf.Max(0f, DamageMultiplier + amount);
    public void AddSpeedMultiplier(float amount) => SpeedMultiplier = Mathf.Max(0f, SpeedMultiplier + amount);
    public void AddUltimateDurationMultiplier(float amount) => UltimateDurationMultiplier = Mathf.Max(0f, UltimateDurationMultiplier + amount);

    public void AddMaxHealth(float amount)
    {
        if (health != null)
            health.IncreaseMaxHealth(amount);
    }
}
