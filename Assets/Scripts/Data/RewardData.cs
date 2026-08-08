using UnityEngine;

[CreateAssetMenu(fileName = "NewReward", menuName = "Takernal/Data/RewardData")]
public class RewardData : ScriptableObject
{
    public string rewardName;
    public StatBonus bonus;
}
