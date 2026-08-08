using UnityEngine;

public class WaveRewardManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private GameObject chestPrefab;

    [Header("Configuración")]
    [SerializeField] private float spawnDelay = 0.5f;

    private void OnEnable()
    {
        GameEvents.OnWaveCompleted += HandleWaveCompleted;
    }

    private void OnDisable()
    {
        GameEvents.OnWaveCompleted -= HandleWaveCompleted;
    }

    private void Start()
    {
        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
        }
    }

    private void HandleWaveCompleted()
    {
        Invoke(nameof(SpawnChest), spawnDelay);
    }

    private void SpawnChest()
    {
        if (chestPrefab == null)
        {
            Debug.LogWarning("[WaveRewardManager] No se ha asignado el prefab del cofre.");
            return;
        }

        Vector3 spawnPos = Vector3.zero;
        if (waveManager != null)
        {
            spawnPos = waveManager.lastEnemyDeathPosition;
        }

        Instantiate(chestPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"[WaveRewardManager] Cofre generado en la posición {spawnPos}");
    }
}
