using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CheeseProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float maxLifetime = 3f;
    [SerializeField] private LayerMask impactLayers = ~0;
    [SerializeField] private StunExplosion2D explosionPrefab;
    [SerializeField] private float explosionRadius = 1.25f;
    [SerializeField] private float explosionStunDuration = 2f;
    [SerializeField] private float explosionDamage;
    [SerializeField] private LayerMask explosionTargetLayers = ~0;

    private GameObject owner;
    private Vector2 direction = Vector2.right;
    private bool hasExploded;

    public void Launch(GameObject newOwner, Vector2 newDirection)
    {
        owner = newOwner;

        if (newDirection.sqrMagnitude > Mathf.Epsilon)
            direction = newDirection.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void ConfigureExplosion(StunExplosion2D newExplosionPrefab, float radius, float stunDuration, float damage, LayerMask targetLayers)
    {
        explosionPrefab = newExplosionPrefab;
        explosionRadius = Mathf.Max(0f, radius);
        explosionStunDuration = Mathf.Max(0f, stunDuration);
        explosionDamage = Mathf.Max(0f, damage);
        explosionTargetLayers = targetLayers;
    }

    private void Awake()
    {
        Collider2D projectileCollider = GetComponent<Collider2D>();
        projectileCollider.isTrigger = true;
    }

    private void Start()
    {
        Destroy(gameObject, Mathf.Max(0.05f, maxLifetime));
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded || other == null) return;
        if (IsOwnerCollider(other)) return;
        if (!IsInLayerMask(other.gameObject.layer, impactLayers)) return;

        Explode();
    }

    private void Explode()
    {
        hasExploded = true;

        if (explosionPrefab != null)
        {
            StunExplosion2D explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.Configure(explosionRadius, explosionStunDuration, explosionDamage, explosionTargetLayers, owner);
        }

        Destroy(gameObject);
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
}
