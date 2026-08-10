using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CircleCollider2D))]
public class StunExplosion2D : MonoBehaviour
{
    [Header("Fallback para pruebas o usos directos")]
    [FormerlySerializedAs("radius")]
    [SerializeField] private float fallbackRadius = 1.25f;
    [FormerlySerializedAs("stunDuration")]
    [SerializeField] private float fallbackStunDuration = 2f;
    [FormerlySerializedAs("damage")]
    [SerializeField] private float fallbackDamage;
    [FormerlySerializedAs("targetLayers")]
    [SerializeField] private LayerMask fallbackTargetLayers = ~0;
    [SerializeField] private float lifetime = 0.15f;

    private readonly HashSet<IStunnable> stunnedThisExplosion = new HashSet<IStunnable>();
    private readonly HashSet<IDamageable> damagedThisExplosion = new HashSet<IDamageable>();
    private GameObject owner;
    private CircleCollider2D explosionCollider;
    private float radius;
    private float stunDuration;
    private float damage;
    private LayerMask targetLayers;

    private void Awake()
    {
        explosionCollider = GetComponent<CircleCollider2D>();
        explosionCollider.isTrigger = true;

        // Los SO de habilidades suelen sobreescribir esto con Configure().
        // Estos valores solo mantienen el prefab reusable si se prueba solo.
        ApplyConfiguration(fallbackRadius, fallbackStunDuration, fallbackDamage, fallbackTargetLayers, null);
    }

    public void Configure(float newRadius, float newStunDuration, float newDamage, LayerMask newTargetLayers, GameObject newOwner = null)
    {
        ApplyConfiguration(newRadius, newStunDuration, newDamage, newTargetLayers, newOwner);
    }

    private void ApplyConfiguration(float newRadius, float newStunDuration, float newDamage, LayerMask newTargetLayers, GameObject newOwner)
    {
        radius = Mathf.Max(0f, newRadius);
        stunDuration = Mathf.Max(0f, newStunDuration);
        damage = Mathf.Max(0f, newDamage);
        targetLayers = newTargetLayers;
        owner = newOwner;

        if (explosionCollider != null)
            explosionCollider.radius = radius;
    }

    private void Start()
    {
        ApplyCurrentOverlaps();
        Destroy(gameObject, Mathf.Max(0f, lifetime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ApplyToCollider(other);
    }

    private void ApplyCurrentOverlaps()
    {
        stunnedThisExplosion.Clear();
        damagedThisExplosion.Clear();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, targetLayers);

        for (int i = 0; i < hits.Length; i++)
            ApplyToCollider(hits[i]);
    }

    private void ApplyToCollider(Collider2D hit)
    {
        if (hit == null) return;
        if (IsOwnerCollider(hit)) return;
        if (!IsInLayerMask(hit.gameObject.layer, targetLayers)) return;

        IDamageable damageable = hit.GetComponentInParent<IDamageable>();
        if (damage > 0f && damageable != null && damageable.IsAlive && damagedThisExplosion.Add(damageable))
            damageable.TakeDamage(damage);

        IStunnable stunnable = hit.GetComponentInParent<IStunnable>();
        if (stunnable != null && stunnedThisExplosion.Add(stunnable))
            stunnable.ApplyStun(stunDuration);
    }

    private bool IsOwnerCollider(Collider2D other)
    {
        if (owner == null) return false;

        return other.gameObject == owner || other.transform.IsChildOf(owner.transform);
    }

    private static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float gizmoRadius = Application.isPlaying ? radius : fallbackRadius;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
    }
}
