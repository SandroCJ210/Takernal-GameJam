using System;
using System.Collections.Generic;
using UnityEngine;


public class Hitbox : MonoBehaviour
{
    private const float MinColliderSize = 0.01f;

    private GameObject owner;
    private float damage;
    private float knockback;

    private readonly List<BoxCollider2D> boxPool = new List<BoxCollider2D>();
    private readonly List<CircleCollider2D> circlePool = new List<CircleCollider2D>();
    private readonly List<CapsuleCollider2D> capsulePool = new List<CapsuleCollider2D>();
    private readonly List<Collider2D> configuredColliders = new List<Collider2D>();
    private readonly HashSet<IDamageable> hitThisActivation = new HashSet<IDamageable>();

    public event Action<IDamageable, GameObject> OnHitLanded;

    private void Awake()
    {
        CacheExistingColliders();
        Deactivate();
    }

    public void Configure(GameObject owner, float damage, float knockback)
    {
        this.owner = owner;
        this.damage = damage;
        this.knockback = knockback;
    }

    public void ResetLocalRotation()
    {
        transform.localRotation = Quaternion.identity;
    }

    public void SetShapes(IReadOnlyList<HitboxShapeData> shapes)
    {
        DisableAllColliders();
        configuredColliders.Clear();

        if (shapes == null) return;

        int boxIndex = 0;
        int circleIndex = 0;
        int capsuleIndex = 0;

        for (int i = 0; i < shapes.Count; i++)
        {
            HitboxShapeData shape = shapes[i];
            if (shape == null) continue;

            switch (shape.shape)
            {
                case HitboxShapeType.Box:
                {
                    BoxCollider2D collider = GetBoxCollider(boxIndex++);
                    collider.offset = shape.offset;
                    collider.size = GetSafeSize(shape.size);
                    collider.enabled = false;
                    configuredColliders.Add(collider);
                    break;
                }

                case HitboxShapeType.Capsule:
                {
                    CapsuleCollider2D collider = GetCapsuleCollider(capsuleIndex++);
                    collider.offset = shape.offset;
                    collider.size = GetSafeSize(shape.size);
                    collider.direction = shape.capsuleDirection;
                    collider.enabled = false;
                    configuredColliders.Add(collider);
                    break;
                }

                default:
                {
                    CircleCollider2D collider = GetCircleCollider(circleIndex++);
                    collider.offset = shape.offset;
                    collider.radius = Mathf.Max(MinColliderSize, Mathf.Abs(shape.radius));
                    collider.enabled = false;
                    configuredColliders.Add(collider);
                    break;
                }
            }
        }
    }

    public void Activate(float duration)
    {
        CancelInvoke(nameof(Deactivate));
        hitThisActivation.Clear();

        for (int i = 0; i < configuredColliders.Count; i++)
            configuredColliders[i].enabled = true;

        Invoke(nameof(Deactivate), Mathf.Max(0f, duration));
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Deactivate));
        Deactivate();
    }

    private void Deactivate()
    {
        for (int i = 0; i < configuredColliders.Count; i++)
            configuredColliders[i].enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsOwnerCollider(other)) return;

        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null || !damageable.IsAlive || hitThisActivation.Contains(damageable)) return;

        hitThisActivation.Add(damageable);
        damageable.TakeDamage(damage);
        OnHitLanded?.Invoke(damageable, owner);

        var rb = other.attachedRigidbody;
        if (rb != null && owner != null)
        {
            Vector2 dir = (other.transform.position - owner.transform.position).normalized;
            if (dir == Vector2.zero)
                dir = owner.transform.right;

            rb.AddForce(dir * knockback, ForceMode2D.Impulse);
        }
    }

    private bool IsOwnerCollider(Collider2D other)
    {
        if (owner == null) return false;

        return other.gameObject == owner || other.transform.IsChildOf(owner.transform);
    }

    private void CacheExistingColliders()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].isTrigger = true;
            colliders[i].enabled = false;

            if (colliders[i] is BoxCollider2D box)
                boxPool.Add(box);
            else if (colliders[i] is CircleCollider2D circle)
                circlePool.Add(circle);
            else if (colliders[i] is CapsuleCollider2D capsule)
                capsulePool.Add(capsule);
        }
    }

    private BoxCollider2D GetBoxCollider(int index)
    {
        while (boxPool.Count <= index)
            boxPool.Add(CreateCollider<BoxCollider2D>());

        return boxPool[index];
    }

    private CircleCollider2D GetCircleCollider(int index)
    {
        while (circlePool.Count <= index)
            circlePool.Add(CreateCollider<CircleCollider2D>());

        return circlePool[index];
    }

    private CapsuleCollider2D GetCapsuleCollider(int index)
    {
        while (capsulePool.Count <= index)
            capsulePool.Add(CreateCollider<CapsuleCollider2D>());

        return capsulePool[index];
    }

    private T CreateCollider<T>() where T : Collider2D
    {
        T collider = gameObject.AddComponent<T>();
        collider.isTrigger = true;
        collider.enabled = false;
        return collider;
    }

    private Vector2 GetSafeSize(Vector2 size)
    {
        return new Vector2(
            Mathf.Max(MinColliderSize, Mathf.Abs(size.x)),
            Mathf.Max(MinColliderSize, Mathf.Abs(size.y))
        );
    }

    private void DisableAllColliders()
    {
        for (int i = 0; i < boxPool.Count; i++)
            boxPool[i].enabled = false;

        for (int i = 0; i < circlePool.Count; i++)
            circlePool[i].enabled = false;

        for (int i = 0; i < capsulePool.Count; i++)
            capsulePool[i].enabled = false;
    }
}
