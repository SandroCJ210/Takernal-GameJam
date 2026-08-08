using System;
using UnityEngine;


public class HealthComponent : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public bool IsAlive => currentHealth > 0f;
    public float Current => currentHealth;
    public float Max => maxHealth;

    public event Action<float, float> OnDamaged; // (cantidad, vidaActual)
    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged; // (vidaActual, vidaMaxima) - para UI

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("Player taking dmg");
        if (!IsAlive) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        OnDamaged?.Invoke(damage, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth += amount; 
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}