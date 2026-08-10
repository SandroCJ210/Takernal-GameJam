using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(HealthComponent))]
public class ReturnToMenuOnPlayerDeath : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private float delayBeforeReturn = 1f;

    private HealthComponent health;
    private Coroutine returnRoutine;

    private void Awake()
    {
        health = GetComponent<HealthComponent>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (returnRoutine != null) return;

        returnRoutine = StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        float delay = Mathf.Max(0f, delayBeforeReturn);
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(menuSceneName))
        {
            Debug.LogWarning("[ReturnToMenuOnPlayerDeath] Falta asignar la escena del menu.");
            yield break;
        }

        SceneManager.LoadScene(menuSceneName);
    }
}
