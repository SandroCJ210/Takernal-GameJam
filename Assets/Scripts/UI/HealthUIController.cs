using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
public class HealthUIController : StaticInstance<HealthUIController>
{
    [SerializeField] private HealthComponent playerHealth;
    [SerializeField] private Animator characterHeadAnimator;
    [SerializeField] private Image healthBar;
    
    private static readonly int DeathTrigger = Animator.StringToHash("Death");

    private void Start()
    {
        playerHealth.OnHealthChanged += HandleHealthChanged;
        playerHealth.OnDeath += HandleDeath;
        
        HandleHealthChanged(playerHealth.Current, playerHealth.Max);
    }

    private void OnDestroy()
    {
        if (playerHealth == null) return;
        playerHealth.OnHealthChanged -= HandleHealthChanged;
        playerHealth.OnDeath -= HandleDeath;
    }

    private void HandleHealthChanged(float current, float max)
    {
        Debug.Log("Health UI");
        float normalized = max > 0f ? current / max : 0f;
        healthBar.fillAmount = normalized;
    }

    private void HandleDeath()
    {
        //characterHeadAnimationAnimator.SetTrigger(DeathTrigger);
    }
}