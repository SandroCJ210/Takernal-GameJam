using UnityEngine;

public class WaveRewardManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private RewardChest sceneRewardChest;
    [SerializeField] private GameObject chestPrefab;

    [Header("Configuracion")]
    [SerializeField] private float spawnDelay = 0.5f;
    [SerializeField] private bool preferSceneRewardChest = true;

    private void OnEnable()
    {
        GameEvents.OnWaveCompleted += HandleWaveCompleted;
    }

    private void OnDisable()
    {
        GameEvents.OnWaveCompleted -= HandleWaveCompleted;
        CancelInvoke(nameof(ActivateReward));
    }

    private void Start()
    {
        if (waveManager == null)
            waveManager = FindFirstObjectByType<WaveManager>();

        if (sceneRewardChest == null)
            sceneRewardChest = FindFirstObjectByType<RewardChest>(FindObjectsInactive.Include);

        if (preferSceneRewardChest && sceneRewardChest != null)
        {
            sceneRewardChest.SetDestroyAfterOpen(false);
            sceneRewardChest.SetInteractable(false);
        }
    }

    private void HandleWaveCompleted()
    {
        Invoke(nameof(ActivateReward), spawnDelay);
    }

    private void ActivateReward()
    {
        if (preferSceneRewardChest && sceneRewardChest != null)
        {
            sceneRewardChest.SetInteractable(true);
            Debug.Log("[WaveRewardManager] Pot de recompensa activado.");
            return;
        }

        SpawnChest();
    }

    private void SpawnChest()
    {
        if (chestPrefab == null)
        {
            Debug.LogWarning("[WaveRewardManager] No se ha asignado un Pot de escena ni un prefab de cofre.");
            return;
        }

        Vector3 spawnPos = Vector3.zero;
        if (waveManager != null)
            spawnPos = waveManager.lastEnemyDeathPosition;

        Instantiate(chestPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"[WaveRewardManager] Cofre generado en la posicion {spawnPos}");
    }
}
