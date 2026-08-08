using UnityEngine;

[System.Serializable]
public struct StatBonus
{
    public float speedIncrease;
    public float damageIncrease;
    public float healthIncrease;

    public StatBonus(float speedIncrease = 0f, float damageIncrease = 0f, float healthIncrease = 0f)
    {
        this.speedIncrease = speedIncrease;
        this.damageIncrease = damageIncrease;
        this.healthIncrease = healthIncrease;
    }
}
