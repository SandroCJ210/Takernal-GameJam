using UnityEngine;

[CreateAssetMenu(fileName = "CheeseMozzarellaBallAbility", menuName = "Abilities/Cheese/Mozzarella Ball")]
public class CheeseMozzarellaBallAbilitySO : AbilityDataSO, IActiveAbility
{
    [Header("Proyectil")]
    [SerializeField] private CheeseProjectile projectilePrefab;
    [SerializeField] private StunExplosion2D explosionPrefab;
    [SerializeField] private float cooldown = 6f;
    [SerializeField] private float spawnDistance = 0.65f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 1.25f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float explosionDamage;
    [SerializeField] private LayerMask targetLayers = ~0;

    public float Cooldown => Mathf.Max(0f, cooldown);

    public bool Activate(PlayerCombat owner)
    {
        if (owner == null || projectilePrefab == null) return false;

        Vector2 direction = owner.FacingDirection;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
            direction = owner.transform.right;

        direction.Normalize();
        Vector3 spawnPosition = owner.transform.position + (Vector3)(direction * Mathf.Max(0f, spawnDistance));

        CheeseProjectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        projectile.ConfigureExplosion(explosionPrefab, explosionRadius, stunDuration, explosionDamage, targetLayers);
        projectile.Launch(owner.gameObject, direction);
        return true;
    }
}
