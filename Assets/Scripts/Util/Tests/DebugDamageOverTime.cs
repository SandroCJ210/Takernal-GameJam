using UnityEngine;

public class DebugDamageOverTime : MonoBehaviour
{
    [SerializeField] private HealthComponent target; 
    [SerializeField] private float damage = 10f;
    [SerializeField] private float interval = 2f;


    private void Start()
    {
        InvokeRepeating(nameof(DealDamage), interval, interval);
    }

    private void DealDamage()
    {
        Debug.Log("Damage");
        target?.TakeDamage(damage);
    }
}