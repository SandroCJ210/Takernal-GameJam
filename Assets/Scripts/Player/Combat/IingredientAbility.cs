using UnityEngine;

// Contrato base: toda habilidad de ingrediente se identifica y sabe
// engancharse/desengancharse del jugador. Las interfaces de abajo son
// las que le dan capacidad real (segregacion de interfaces: cada
// ingrediente implementa solo las que necesita).
public interface IIngredientAbility
{
    string AbilityName { get; }
    void OnAcquired(PlayerCombat owner);
    void OnRemoved(PlayerCombat owner);
}

// Modifica el daño del combo basico.
public interface IAttackModifier : IIngredientAbility
{
    float ModifyDamage(float baseDamage);
}

// Se dispara cuando un golpe del combo basico conecta.
public interface IOnHitEffect : IIngredientAbility
{
    void OnHit(IDamageable target, GameObject source);
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