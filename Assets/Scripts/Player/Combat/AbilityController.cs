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
    private readonly List<IOnAttackStartedEffect> attackStartedEffects = new List<IOnAttackStartedEffect>();
    private readonly List<IOnAttackActivatedEffect> attackActivatedEffects = new List<IOnAttackActivatedEffect>();
    private readonly List<IDamageTakenModifier> damageTakenModifiers = new List<IDamageTakenModifier>();
    private readonly List<IOnDamageTakenEffect> damageTakenEffects = new List<IOnDamageTakenEffect>();
    private readonly Dictionary<IActiveAbility, float> activeCooldowns = new Dictionary<IActiveAbility, float>();

    private void Awake()
    {
        if (combat == null) combat = GetComponent<PlayerCombat>();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < passiveTicks.Count; i++)
            passiveTicks[i].Tick(dt);

        TickCooldowns(dt);
    }

    public void AddAbility(IIngredientAbility ability)
    {
        if (ability == null || all.Contains(ability)) return;
        if (combat == null) combat = GetComponent<PlayerCombat>();

        all.Add(ability);

        if (ability is IAttackModifier am) attackModifiers.Add(am);
        if (ability is IOnHitEffect ohe) onHitEffects.Add(ohe);
        if (ability is IActiveAbility aa)
        {
            activeAbilities.Add(aa);
            activeCooldowns[aa] = 0f;
        }
        if (ability is IPassiveTick pt) passiveTicks.Add(pt);
        if (ability is IOnAttackStartedEffect oase) attackStartedEffects.Add(oase);
        if (ability is IOnAttackActivatedEffect oaae) attackActivatedEffects.Add(oaae);
        if (ability is IDamageTakenModifier dtm) damageTakenModifiers.Add(dtm);
        if (ability is IOnDamageTakenEffect odte) damageTakenEffects.Add(odte);

        ability.OnAcquired(combat);
    }

    public void RemoveAbility(IIngredientAbility ability)
    {
        if (ability == null) return;
        if (!all.Remove(ability)) return;

        if (ability is IAttackModifier am) attackModifiers.Remove(am);
        if (ability is IOnHitEffect ohe) onHitEffects.Remove(ohe);
        if (ability is IActiveAbility aa)
        {
            activeAbilities.Remove(aa);
            activeCooldowns.Remove(aa);
        }
        if (ability is IPassiveTick pt) passiveTicks.Remove(pt);
        if (ability is IOnAttackStartedEffect oase) attackStartedEffects.Remove(oase);
        if (ability is IOnAttackActivatedEffect oaae) attackActivatedEffects.Remove(oaae);
        if (ability is IDamageTakenModifier dtm) damageTakenModifiers.Remove(dtm);
        if (ability is IOnDamageTakenEffect odte) damageTakenEffects.Remove(odte);

        ability.OnRemoved(combat);
    }

    public float ApplyDamageModifiers(float baseDamage, AttackContext context)
    {
        for (int i = 0; i < attackModifiers.Count; i++)
            baseDamage = attackModifiers[i].ModifyDamage(baseDamage, context);
        return baseDamage;
    }

    public void TriggerOnHit(IDamageable target, AttackContext context)
    {
        for (int i = 0; i < onHitEffects.Count; i++)
            onHitEffects[i].OnHit(target, context);
    }

    public void TriggerAttackStarted(AttackContext context)
    {
        for (int i = 0; i < attackStartedEffects.Count; i++)
            attackStartedEffects[i].OnAttackStarted(context);
    }

    public void TriggerAttackActivated(AttackContext context)
    {
        for (int i = 0; i < attackActivatedEffects.Count; i++)
            attackActivatedEffects[i].OnAttackActivated(context);
    }

    public float ApplyDamageTakenModifiers(float damage, DamageTakenContext context)
    {
        for (int i = 0; i < damageTakenModifiers.Count; i++)
        {
            DamageTakenContext currentContext = new DamageTakenContext(
                context.Target,
                context.Source,
                context.OriginalDamage,
                damage
            );
            damage = damageTakenModifiers[i].ModifyDamageTaken(damage, currentContext);
        }

        return Mathf.Max(0f, damage);
    }

    public void TriggerDamageTaken(DamageTakenContext context)
    {
        if (context.FinalDamage <= 0f) return;

        for (int i = 0; i < damageTakenEffects.Count; i++)
            damageTakenEffects[i].OnDamageTaken(context);
    }

    public void ActivateAbility(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= activeAbilities.Count) return;

        IActiveAbility ability = activeAbilities[slotIndex];
        if (GetCooldownRemaining(ability) > 0f) return;

        ability.Activate(combat);
        activeCooldowns[ability] = Mathf.Max(0f, ability.Cooldown);
    }

    public float GetCooldownRemaining(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= activeAbilities.Count) return 0f;

        return GetCooldownRemaining(activeAbilities[slotIndex]);
    }

    public float GetCooldownNormalized(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= activeAbilities.Count) return 0f;

        IActiveAbility ability = activeAbilities[slotIndex];
        float cooldown = Mathf.Max(0f, ability.Cooldown);
        if (Mathf.Approximately(cooldown, 0f)) return 0f;

        return GetCooldownRemaining(ability) / cooldown;
    }

    private float GetCooldownRemaining(IActiveAbility ability)
    {
        return activeCooldowns.TryGetValue(ability, out float remaining)
            ? Mathf.Max(0f, remaining)
            : 0f;
    }

    private void TickCooldowns(float deltaTime)
    {
        for (int i = 0; i < activeAbilities.Count; i++)
        {
            IActiveAbility ability = activeAbilities[i];
            if (!activeCooldowns.TryGetValue(ability, out float remaining) || remaining <= 0f)
                continue;

            activeCooldowns[ability] = Mathf.Max(0f, remaining - deltaTime);
        }
    }
}
