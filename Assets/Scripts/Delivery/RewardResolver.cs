using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase auxiliar para resolver el sorteo ponderado (Weighted Random Choice)
/// de recompensas y castigos según los pesos asignados.
/// </summary>
public static class RewardResolver
{
    public static RewardData PickReward(List<WeightedReward> rewards)
    {
        if (rewards == null || rewards.Count == 0) return null;

        int totalWeight = 0;
        foreach (var item in rewards)
        {
            if (item.reward != null && item.weight > 0)
            {
                totalWeight += item.weight;
            }
        }

        if (totalWeight <= 0) return null;

        int randomPoint = Random.Range(0, totalWeight);
        int currentWeightSum = 0;

        foreach (var item in rewards)
        {
            if (item.reward == null || item.weight <= 0) continue;

            currentWeightSum += item.weight;
            if (randomPoint < currentWeightSum)
            {
                return item.reward;
            }
        }

        return rewards[0].reward;
    }

    public static PunishmentData PickPunishment(List<WeightedPunishment> punishments)
    {
        if (punishments == null || punishments.Count == 0) return null;

        int totalWeight = 0;
        foreach (var item in punishments)
        {
            if (item.punishment != null && item.weight > 0)
            {
                totalWeight += item.weight;
            }
        }

        if (totalWeight <= 0) return null;

        int randomPoint = Random.Range(0, totalWeight);
        int currentWeightSum = 0;

        foreach (var item in punishments)
        {
            if (item.punishment == null || item.weight <= 0) continue;

            currentWeightSum += item.weight;
            if (randomPoint < currentWeightSum)
            {
                return item.punishment;
            }
        }

        return punishments[0].punishment;
    }
}
