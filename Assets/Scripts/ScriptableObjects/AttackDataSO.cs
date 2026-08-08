using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Combat/Attack Data")]
public class AttackDataSO : ScriptableObject
{
    [Header("Identidad")]
    public string attackName;
    public string animatorTrigger;

    [Header("Stats")]
    public float baseDamage = 10f;
    public float knockbackForce = 5f;
    public float hitboxActiveTime = 0.15f;

    [Header("Combo")]
    [Tooltip("Si el jugador presiona ataque durante la ventana de combo, se encadena este golpe")]
    public AttackDataSO nextAttackInCombo;

    [Header("FX (opcional)")]
    public GameObject hitVfxPrefab;
    public AudioClip swingSfx;
}