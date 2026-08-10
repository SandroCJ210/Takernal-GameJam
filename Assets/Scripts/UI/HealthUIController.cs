using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthUIController : StaticInstance<HealthUIController>
{
    private enum HealthState
    {
        Healthy = 0,
        Injured = 1,
        Critical = 2
    }

    [SerializeField] private HealthComponent playerHealth;
    [SerializeField] private Animator characterHeadAnimator;
    [SerializeField] private Image healthBar;

    [Header("Character Head Animation")]
    [SerializeField] private string healthStateParameter = "HealthState";
    [SerializeField] private string headTriggerparameter = "HeadTrigger";
    [SerializeField] private string healthyStateName = "HappyFace";
    [SerializeField] private string injuredStateName = "InjuredFace";
    [SerializeField] private string criticalStateName = "CriticalFace";
    [SerializeField, Range(0f, 1f)] private float injuredHealthThreshold = 0.66f;
    [SerializeField, Range(0f, 1f)] private float criticalHealthThreshold = 0.33f;
    [SerializeField, Min(0f)] private float minimumAnimationInterval = 3f;
    [SerializeField, Min(0f)] private float maximumAnimationInterval = 6f;

    private Coroutine replayAnimationCoroutine;
    private HealthState currentHealthState;
    private bool isDead;
    

    private void Start()
    {
        if (playerHealth == null || characterHeadAnimator == null || healthBar == null)
        {
            Debug.LogWarning($"{nameof(HealthUIController)} necesita PlayerHealth, CharacterHeadAnimator y HealthBar asignados.", this);
            enabled = false;
            return;
        }

        playerHealth.OnHealthChanged += HandleHealthChanged;
        playerHealth.OnDeath += HandleDeath;

        HandleHealthChanged(playerHealth.Current, playerHealth.Max);
        replayAnimationCoroutine = StartCoroutine(ReplayCurrentHealthAnimation());
    }

    private void OnDestroy()
    {
        if (replayAnimationCoroutine != null)
        {
            StopCoroutine(replayAnimationCoroutine);
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
            playerHealth.OnDeath -= HandleDeath;
        }
    }

    private void HandleHealthChanged(float current, float max)
    {
        float normalized = max > 0f ? current / max : 0f;
        healthBar.fillAmount = normalized;
        currentHealthState = GetHealthState(normalized);
        PlayCurrentHealthAnimation();

        if (isDead && current > 0f)
        {
            isDead = false;
            replayAnimationCoroutine = StartCoroutine(ReplayCurrentHealthAnimation());
        }
    }

    private void HandleDeath()
    {
        isDead = true;

        if (replayAnimationCoroutine != null)
        {
            StopCoroutine(replayAnimationCoroutine);
            replayAnimationCoroutine = null;
        }
    }

    private HealthState GetHealthState(float normalizedHealth)
    {
        if (normalizedHealth <= criticalHealthThreshold)
        {
            return HealthState.Critical;
        }

        if (normalizedHealth <= injuredHealthThreshold)
        {
            return HealthState.Injured;
        }

        return HealthState.Healthy;
    }

    private void PlayCurrentHealthAnimation()
    {
        characterHeadAnimator.SetFloat(healthStateParameter, (int)currentHealthState);
        characterHeadAnimator.SetTrigger(headTriggerparameter);
    }
    

    private IEnumerator ReplayCurrentHealthAnimation()
    {
        while (true)
        {
            float interval = Random.Range(
                Mathf.Min(minimumAnimationInterval, maximumAnimationInterval),
                Mathf.Max(minimumAnimationInterval, maximumAnimationInterval));

            yield return new WaitForSeconds(interval);
            PlayCurrentHealthAnimation();
        }
    }
}
