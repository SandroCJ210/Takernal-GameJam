using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WeightedReward
{
    public RewardData reward;
    [Range(1, 100)] public int weight;
}

[System.Serializable]
public struct WeightedPunishment
{
    public PunishmentData punishment;
    [Range(1, 100)] public int weight;
}

[CreateAssetMenu(fileName = "NewCustomer", menuName = "Takernal/Data/CustomerData")]
public class CustomerData : ScriptableObject
{
    public string customerName;
    public Sprite avatar;
    public List<string> likedTags = new List<string>();
    public List<string> dislikedTags = new List<string>();
    public float maxPatience = 30f;

    [Header("Recompensas y Castigos (Con Pesos)")]
    public List<WeightedReward> possibleRewards = new List<WeightedReward>();
    public List<WeightedPunishment> possiblePunishments = new List<WeightedPunishment>();
}
