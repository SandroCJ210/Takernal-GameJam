using UnityEngine;

[System.Serializable]
public class HitboxShapeData
{
    public HitboxShapeType shape = HitboxShapeType.Box;
    public Vector2 offset;
    public Vector2 size = Vector2.one;
    public float radius = 0.5f;
    public CapsuleDirection2D capsuleDirection = CapsuleDirection2D.Vertical;
}

public enum HitboxShapeType
{
    Box,
    Circle,
    Capsule
}

public enum AttackDirection
{
    Right,
    Left,
    Up,
    Down
}

[System.Serializable]
public class DirectionalHitboxData
{
    public HitboxShapeData[] right;
    public HitboxShapeData[] left;
    public HitboxShapeData[] up;
    public HitboxShapeData[] down;

    public HitboxShapeData[] GetShapes(AttackDirection direction)
    {
        switch (direction)
        {
            case AttackDirection.Left:
                return left;
            case AttackDirection.Up:
                return up;
            case AttackDirection.Down:
                return down;
            default:
                return right;
        }
    }
}

public enum AttackTag
{
    Melee,
    Ranged,
    Projectile,
    Area,
    ComboStarter,
    ComboMiddle,
    ComboFinisher,
    Ability
}

[CreateAssetMenu(fileName = "NewAttack", menuName = "Combat/Attack Data")]
public class AttackDataSO : ScriptableObject
{
    [Header("Identidad")]
    [Tooltip("Id estable para logica de habilidades. Ej: tomato_headbutt, tomato_uppercut, tomato_slam.")]
    public string attackId;
    public string animatorTrigger;
    public AttackTag[] tags;

    [Header("Stats")]
    public float baseDamage = 10f;
    public float knockbackForce = 5f;
    public float hitboxActiveTime = 0.15f;

    [Header("Movimiento del ataque")]
    [Tooltip("Distancia que avanza el jugador cuando la animacion llama AE_LungeForward. 0 desactiva el avance.")]
    public float lungeDistance;
    [Tooltip("Duracion del avance en segundos. Valores muy bajos hacen que el golpe se sienta mas explosivo.")]
    public float lungeDuration = 0.08f;

    [Header("Hitbox fallback")]
    [Tooltip("Fallback opcional. Se usa si la direccion actual no tiene una hitbox por dirección configurada.")]
    public HitboxShapeData[] hitboxShapes;

    [Header("Hitboxes por direccion")]
    [Tooltip("Hitboxes especificas por direccion.")]
    public DirectionalHitboxData directionalHitboxes = new DirectionalHitboxData();

    [Header("Combo")]
    [Tooltip("Siguiente ataque del combo")]
    public AttackDataSO nextAttackInCombo;

    [Header("FX (opcional)")]
    public GameObject hitVfxPrefab;
    public AudioClip swingSfx;

    public bool HasTag(AttackTag tag)
    {
        if (tags == null) return false;

        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i] == tag)
                return true;
        }

        return false;
    }

    public HitboxShapeData[] GetHitboxShapes(AttackDirection direction)
    {
        HitboxShapeData[] directionalShapes = directionalHitboxes != null
            ? directionalHitboxes.GetShapes(direction)
            : null;

        return HasShapes(directionalShapes) ? directionalShapes : hitboxShapes;
    }

    private static bool HasShapes(HitboxShapeData[] shapes)
    {
        return shapes != null && shapes.Length > 0;
    }
}
