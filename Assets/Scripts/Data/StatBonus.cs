using UnityEngine;

[System.Serializable]
public struct StatBonus
{
    [Header("Aumentos / Modificadores Planos")]
    public float speedIncrease;
    public float damageIncrease;
    public float healthIncrease;

    [Header("Aumentos / Modificadores Porcentuales (%)")]
    [Tooltip("Ejemplo: 0.1 = +10%, -0.15 = -15%")]
    public float speedPercent;
    public float damagePercent;
    public float healthPercent;

    public StatBonus(float speedIncrease = 0f, float damageIncrease = 0f, float healthIncrease = 0f,
                     float speedPercent = 0f, float damagePercent = 0f, float healthPercent = 0f)
    {
        this.speedIncrease = speedIncrease;
        this.damageIncrease = damageIncrease;
        this.healthIncrease = healthIncrease;
        this.speedPercent = speedPercent;
        this.damagePercent = damagePercent;
        this.healthPercent = healthPercent;
    }
}

