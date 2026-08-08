using UnityEngine;

public struct AttackContext
{
    public AttackDataSO Attack { get; }
    public GameObject Source { get; }
    public Vector2 Direction { get; }
    public int ComboStep { get; }

    public AttackContext(AttackDataSO attack, GameObject source, Vector2 direction, int comboStep)
    {
        Attack = attack;
        Source = source;
        Direction = direction;
        ComboStep = comboStep;
    }

    public string AttackId => Attack != null ? Attack.attackId : string.Empty;
    public bool IsComboFinisher => Attack != null && Attack.nextAttackInCombo == null;

    public bool HasTag(AttackTag tag)
    {
        return Attack != null && Attack.HasTag(tag);
    }
}

public struct DamageTakenContext
{
    public PlayerCombat Target { get; }
    public GameObject Source { get; }
    public float OriginalDamage { get; }
    public float FinalDamage { get; }

    public DamageTakenContext(PlayerCombat target, GameObject source, float originalDamage, float finalDamage)
    {
        Target = target;
        Source = source;
        OriginalDamage = originalDamage;
        FinalDamage = finalDamage;
    }
}
