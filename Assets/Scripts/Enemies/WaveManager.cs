using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DifficultyPreset { Easy, Normal, Hard }

public class WaveManager : MonoBehaviour
{
    [Header("Enemy Pool")]
    [Tooltip("List of enemy prefabs that can spawn in this level")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public Transform[] spawnPoints;
    
    [Header("Scene Organization")]
    [Tooltip("Transform (e.g. empty GameObject 'Enemies') to parent instantiated enemies")]
    public Transform enemiesParent;

    [Header("Difficulty Configuration")]
    public DifficultyPreset difficultyPreset = DifficultyPreset.Normal;
    
    [Header("Base Settings")]
    public float timeBetweenWaves = 3f;
    public float spawnInterval = 1f;

    [Header("Quantity Scaling")]
    public int baseEnemyCount = 1;
    public float countMultiplier = 2.0f;
    public int countLinearThreshold = 5;
    public int countLinearIncrement = 5;

    [Header("Stats Scaling")]
    public float statMultiplierPerWave = 0.5f;
    public int statLinearThreshold = 5;
    public float statLinearIncrement = 20f;

    [Header("Current State (Debug)")]
    public int currentWaveNumber = 1; // 1-indexed
    public int activeEnemiesCount = 0;
    public bool isWaveActive = false;
    public bool isLevelComplete = false;

    private void OnEnable()
    {
        ChaserEnemy.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        ChaserEnemy.OnEnemyDied -= HandleEnemyDied;
    }

    private void Start()
    {
        if (enemyPrefabs.Count > 0)
        {
            StartCoroutine(StartNextWaveRoutine());
        }
        else
        {
            Debug.LogWarning("[WaveManager] No hay prefabs de enemigos configurados.");
        }
    }

    private IEnumerator StartNextWaveRoutine()
    {
        if (isLevelComplete) yield break;

        Debug.Log($"[WaveManager] Iniciando Oleada {currentWaveNumber}");
        isWaveActive = true;
        activeEnemiesCount = 0;

        int totalEnemies = CalculateEnemyCountForWave(currentWaveNumber);
        
        int spawnedCount = 0;
        while (spawnedCount < totalEnemies)
        {
            if (isLevelComplete) break;
            
            SpawnEnemy();
            spawnedCount++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private int CalculateEnemyCountForWave(int waveNum)
    {
        float diffMult = GetDifficultyMultiplier(difficultyPreset);
        int count = 0;

        if (waveNum <= countLinearThreshold)
        {
            // Crecimiento exponencial
            count = Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(countMultiplier, waveNum - 1) * diffMult);
        }
        else
        {
            // Crecimiento lineal a partir del umbral
            int thresholdCount = Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(countMultiplier, countLinearThreshold - 1));
            count = Mathf.RoundToInt((thresholdCount + (waveNum - countLinearThreshold) * countLinearIncrement) * diffMult);
        }

        return Mathf.Max(1, count); // Al menos 1
    }

    private float CalculateStatFactorForWave(int waveNum)
    {
        float diffMult = GetDifficultyMultiplier(difficultyPreset);
        float factor = 1f;

        if (waveNum <= statLinearThreshold)
        {
            factor = 1f + (waveNum - 1) * statMultiplierPerWave;
        }
        else
        {
            float thresholdFactor = 1f + (statLinearThreshold - 1) * statMultiplierPerWave;
            // statLinearIncrement es un valor absoluto extra, lo convertimos a factor sumando (incremento / 100)
            // Para mantenerlo simple, solo sumaremos un incremento lineal al multiplicador
            factor = thresholdFactor + (waveNum - statLinearThreshold) * (statLinearIncrement / 100f); 
        }

        return factor * diffMult;
    }

    private float GetDifficultyMultiplier(DifficultyPreset diff)
    {
        switch (diff)
        {
            case DifficultyPreset.Easy: return 0.5f; // Mitad de enemigos y stats (ejemplo)
            case DifficultyPreset.Normal: return 1.0f;
            case DifficultyPreset.Hard: return 2.0f;
            default: return 1.0f;
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0) return;

        Vector3 spawnPosition = Vector3.zero;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            spawnPosition = randomPoint.position;
        }
        else
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * 8f;
            spawnPosition = new Vector3(randomCircle.x, randomCircle.y, 0f);
        }

        GameObject selectedPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        GameObject enemyObj = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        
        if (enemiesParent != null)
        {
            enemyObj.transform.SetParent(enemiesParent);
        }

        ChaserEnemy chaser = enemyObj.GetComponent<ChaserEnemy>();
        if (chaser != null)
        {
            // Leer base stats del script (ya sea de data si está configurado en prefab, o los defaults)
            // El componente ChaserEnemy ya inicializó los stats en su Awake/Start (desde data o Inspector), 
            // pero vamos a sobreescribirlos escalándolos basados en los valores base de su EnemyData.
            
            float baseHealth = chaser.data != null ? chaser.data.maxHealth : 30f;
            float baseSpeed = chaser.data != null ? chaser.data.moveSpeed : 3f;
            float baseDamage = chaser.data != null ? chaser.data.damage : 10f;

            float statFactor = CalculateStatFactorForWave(currentWaveNumber);
            
            chaser.InitializeWithScaledStats(
                baseHealth * statFactor, 
                baseSpeed * (1f + (statFactor-1f)*0.2f), // Velocidad escala menos para no ser incontrolable
                baseDamage * statFactor
            );
        }

        activeEnemiesCount++;
    }

    private void HandleEnemyDied(ChaserEnemy enemy)
    {
        activeEnemiesCount--;

        if (activeEnemiesCount <= 0 && isWaveActive && !isLevelComplete)
        {
            isWaveActive = false;
            Debug.Log($"[WaveManager] ¡Oleada {currentWaveNumber} Completada!");
            
            GameEvents.OnWaveCompleted?.Invoke();

            currentWaveNumber++;
            Invoke(nameof(TriggerNextWave), timeBetweenWaves);
        }
    }

    private void TriggerNextWave()
    {
        if (!isLevelComplete)
        {
            StartCoroutine(StartNextWaveRoutine());
        }
    }

    public void SetLevelComplete()
    {
        isLevelComplete = true;
        isWaveActive = false;
        Debug.Log("[WaveManager] Nivel completado, se detienen las oleadas.");
    }
}
