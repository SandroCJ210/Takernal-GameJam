using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour, IDamageable
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private HealthComponent health;

    [Header("Combo base")]
    [SerializeField] private AttackDataSO firstAttack;

    private AttackDataSO currentAttack;
    private bool isAttacking;
    private bool queuedNextAttack;
    private bool canQueueNextAttack; 

    [SerializeField] private AbilityController abilities;
    [SerializeField] private PlayerStats stats;

    public bool IsAlive => health.IsAlive;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        hitbox.OnHitLanded += (target, source) => abilities.TriggerOnHit(target, source);
    }

    private void Start()
    {
        InputHandler.Instance.OnAttackRecieved += HandleAttackInput;
        InputHandler.Instance.OnAbility1Recieved += HandleAbility1Input;
    }

    private void OnDestroy()
    {
        if (InputHandler.Instance == null) return;
        InputHandler.Instance.OnAttackRecieved -= HandleAttackInput;
        InputHandler.Instance.OnAbility1Recieved -= HandleAbility1Input;
    }

    private void HandleAbility1Input() => abilities.ActivateAbility(0);

    private void HandleAttackInput()
    {
        Debug.Log("Handling attack");
        if (!isAttacking)
        {
            Debug.Log("StartingAttack");
            StartAttack(firstAttack);
            return;
        }
        
        if (canQueueNextAttack && currentAttack.nextAttackInCombo != null)
        {
            queuedNextAttack = true;
        }
    }

    private void StartAttack(AttackDataSO attack)
    {
        if (attack == null) return;

        currentAttack = attack;
        isAttacking = true;
        queuedNextAttack = false;
        canQueueNextAttack = false;

        animator.SetTrigger(attack.animatorTrigger);

//        float finalDamage = abilities.ApplyDamageModifiers(attack.baseDamage) * stats.DamageMultiplier;
        Debug.Log("Attacked succesfully.");
//        hitbox.Configure(gameObject, finalDamage, attack.knockbackForce);
        Debug.Log("Attacked succesfully.2");
        if (attack.swingSfx != null)
            AudioSource.PlayClipAtPoint(attack.swingSfx, transform.position);
        
        Debug.Log("Attacked succesfully.3");
    }
    

    public void AE_OpenComboWindow() => canQueueNextAttack = true;

    public void AE_CloseComboWindow() => canQueueNextAttack = false;

    public void AE_ActivateHitbox() => hitbox.Activate(currentAttack.hitboxActiveTime);

    public void AE_AttackEnd()
    {
        if (queuedNextAttack && currentAttack.nextAttackInCombo != null)
        {
            StartAttack(currentAttack.nextAttackInCombo);
        }
        else
        {
            isAttacking = false;
            currentAttack = null;
        }
    }
    

    public void UseActiveAbility(int slotIndex) => abilities.ActivateAbility(slotIndex);
    

    public void TakeDamage(float damage)
    {
        health.TakeDamage(damage);
    }
}