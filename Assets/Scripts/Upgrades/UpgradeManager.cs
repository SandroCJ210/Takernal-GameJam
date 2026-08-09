using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mantiene el registro de todas las mejoras y penalizaciones aplicadas al jugador durante la partida.
/// Escucha GameEvents.OnRewardApplied.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Registro de Mejoras")]
    [SerializeField] private List<StatBonus> upgradeHistory = new List<StatBonus>();

    [Header("Depuración")]
    [SerializeField] private bool showDebugLogs = true;

    public IReadOnlyList<StatBonus> UpgradeHistory => upgradeHistory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.OnRewardApplied += HandleRewardApplied;
    }

    private void OnDisable()
    {
        GameEvents.OnRewardApplied -= HandleRewardApplied;
    }

    private void HandleRewardApplied(StatBonus bonus)
    {
        upgradeHistory.Add(bonus);

        if (showDebugLogs)
        {
            bool isPositive = bonus.speedIncrease >= 0 && bonus.damageIncrease >= 0 && bonus.healthIncrease >= 0;
            string status = isPositive ? "Mejora (+) aplicada" : "Penalización (-) aplicada";

            Debug.Log($"[UpgradeManager] {status} | Vel: {bonus.speedIncrease:+0.0;-0.0;0}, Daño: {bonus.damageIncrease:+0.00;-0.00;0}, Vida: {bonus.healthIncrease:+0;-0;0}. Total registradas: {upgradeHistory.Count}");
        }
    }
}
