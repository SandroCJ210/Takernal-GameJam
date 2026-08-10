using UnityEngine;

[CreateAssetMenu(fileName = "PizzaSpinDashAbility", menuName = "Abilities/Pizza/Spin Dash")]
public class PizzaSpinDashAbilitySO : AbilityDataSO, IActiveAbility
{
    [Header("Movimiento")]
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private float forwardDistance = 3f;
    [SerializeField] private float forwardDuration = 0.35f;
    [SerializeField] private float returnDuration = 0.25f;

    [Header("Danio")]
    [SerializeField] private float hitRadius = 1f;
    [SerializeField, Min(0f)] private float hitboxActiveDelay = 0f;
    [SerializeField] private float hitboxActiveDuration = -1f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float knockbackForce = 4f;
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField, Min(1)] private int maxHits = 16;

    [Header("Animacion")]
    [SerializeField] private string spinTriggerParameter = "PizzaSpin";
    [SerializeField] private string spinningBoolParameter = "isSpinning";

    public float Cooldown => Mathf.Max(0f, cooldown);

    public bool Activate(PlayerCombat owner)
    {
        if (owner == null || owner.IsMovementLocked) return false;

        PizzaSpinDashRunner runner = owner.GetComponent<PizzaSpinDashRunner>();
        if (runner == null)
            runner = owner.gameObject.AddComponent<PizzaSpinDashRunner>();

        return runner.TryStart(
            owner,
            owner.FacingDirection,
            forwardDistance,
            forwardDuration,
            returnDuration,
            hitRadius,
            hitboxActiveDelay,
            hitboxActiveDuration,
            damage,
            knockbackForce,
            targetLayers,
            maxHits,
            spinTriggerParameter,
            spinningBoolParameter
        );
    }
}
