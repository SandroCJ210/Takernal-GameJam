using System.Collections.Generic;
using UnityEngine;

// Guarda TODAS las habilidades del jugador (dadas por ingredientes) y las
// clasifica por tipo al agregarlas. Asi PlayerCombat/Hitbox no preguntan
// "is IAttackModifier" en cada frame: ya estan separadas de antemano.
public class AbilityController : MonoBehaviour
{
    [SerializeField] private PlayerCombat combat;

    private readonly List<IIngredientAbility> all = new List<IIngredientAbility>();
    private readonly List<IAttackModifier> attackModifiers = new List<IAttackModifier>();
    private readonly List<IOnHitEffect> onHitEffects = new List<IOnHitEffect>();
    private readonly List<IActiveAbility> activeAbilities = new List<IActiveAbility>();
    private readonly List<IPassiveTick> passiveTicks = new List<IPassiveTick>();

    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < passiveTicks.Count; i++)
            passiveTicks[i].Tick(dt);
    }

    public void AddAbility(IIngredientAbility ability)
    {
        all.Add(ability);

        if (ability is IAttackModifier am) attackModifiers.Add(am);
        if (ability is IOnHitEffect ohe) onHitEffects.Add(ohe);
        if (ability is IActiveAbility aa) activeAbilities.Add(aa);
        if (ability is IPassiveTick pt) passiveTicks.Add(pt);

        ability.OnAcquired(combat);
    }

    public void RemoveAbility(IIngredientAbility ability)
    {
        all.Remove(ability);

        if (ability is IAttackModifier am) attackModifiers.Remove(am);
        if (ability is IOnHitEffect ohe) onHitEffects.Remove(ohe);
        if (ability is IActiveAbility aa) activeAbilities.Remove(aa);
        if (ability is IPassiveTick pt) passiveTicks.Remove(pt);

        ability.OnRemoved(combat);
    }

    public float ApplyDamageModifiers(float baseDamage)
    {
        for (int i = 0; i < attackModifiers.Count; i++)
            baseDamage = attackModifiers[i].ModifyDamage(baseDamage);
        return baseDamage;
    }

    public void TriggerOnHit(IDamageable target, GameObject source)
    {
        for (int i = 0; i < onHitEffects.Count; i++)
            onHitEffects[i].OnHit(target, source);
    }

    public void ActivateAbility(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < activeAbilities.Count)
            activeAbilities[slotIndex].Activate(combat);
    }
}