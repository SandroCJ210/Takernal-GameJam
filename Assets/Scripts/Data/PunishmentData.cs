using UnityEngine;

[CreateAssetMenu(fileName = "NewPunishment", menuName = "Takernal/Data/PunishmentData")]
public class PunishmentData : ScriptableObject
{
    public string punishmentName;
    public StatBonus penalty;
}
