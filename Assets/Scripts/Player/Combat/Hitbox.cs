using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
    private GameObject owner;
    private float damage;
    private float knockback;
    
    private readonly HashSet<IDamageable> hitThisActivation = new HashSet<IDamageable>();
    
    public event Action<IDamageable, GameObject> OnHitLanded;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        gameObject.SetActive(false);
    }

    public void Configure(GameObject owner, float damage, float knockback)
    {
        this.owner = owner;
        this.damage = damage;
        this.knockback = knockback;
    }

    public void Activate(float duration)
    {
        hitThisActivation.Clear();
        gameObject.SetActive(true);
        Invoke(nameof(Deactivate), duration);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && other.gameObject == owner) return;

        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null || hitThisActivation.Contains(damageable)) return;

        hitThisActivation.Add(damageable);
        damageable.TakeDamage(damage);
        OnHitLanded?.Invoke(damageable, owner);

        var rb = other.attachedRigidbody;
        if (rb != null && owner != null)
        {
            Vector2 dir = (other.transform.position - owner.transform.position).normalized;
            rb.AddForce(dir * knockback, ForceMode2D.Impulse);
        }
    }
}