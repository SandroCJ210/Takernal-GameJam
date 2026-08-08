using UnityEngine;

// Contrato base: toda habilidad de ingrediente se identifica y sabe
// engancharse/desengancharse del jugador. Las interfaces de abajo son
// las que le dan capacidad real; cada ingrediente implementa solo las
// que necesita.
public interface IIngredientAbility
{
    string AbilityName { get; }
    void OnAcquired(PlayerCombat owner);
    void OnRemoved(PlayerCombat owner);
}

// Modifica el dano del combo basico usando contexto del ataque actual.
// Tocino puede usarlo para criticos y Carne puede leer tags del Slam.
public interface IAttackModifier : IIngredientAbility
{
    float ModifyDamage(float baseDamage, AttackContext context);
}

// Se dispara cuando un golpe del combo basico conecta.
public interface IOnHitEffect : IIngredientAbility
{
    void OnHit(IDamageable target, AttackContext context);
}

// Habilidad nueva, activable con su propio boton/slot (dash, proyectil, etc).
public interface IActiveAbility : IIngredientAbility
{
    float Cooldown { get; }
    void Activate(PlayerCombat owner);
}

// Efecto pasivo que corre cada frame mientras el ingrediente esta en la build.
public interface IPassiveTick : IIngredientAbility
{
    void Tick(float deltaTime);
}

// Reacciona al inicio de un ataque sin meter casos especiales en PlayerCombat.
public interface IOnAttackStartedEffect : IIngredientAbility
{
    void OnAttackStarted(AttackContext context);
}

// Reacciona justo cuando el Animation Event activa la hitbox.
public interface IOnAttackActivatedEffect : IIngredientAbility
{
    void OnAttackActivated(AttackContext context);
}

// Modifica dano recibido antes de que llegue a HealthComponent.
public interface IDamageTakenModifier : IIngredientAbility
{
    float ModifyDamageTaken(float damage, DamageTakenContext context);
}

// Reacciona despues de recibir dano real. Cebolla/Lagrimas vive aca.
public interface IOnDamageTakenEffect : IIngredientAbility
{
    void OnDamageTaken(DamageTakenContext context);
}
