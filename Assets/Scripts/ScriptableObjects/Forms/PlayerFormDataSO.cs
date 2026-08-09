using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerForm", menuName = "Takernal/Player/Form Data")]
public class PlayerFormDataSO : ScriptableObject
{
    [Header("Identidad")]
    [SerializeField] private string formId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("Visual")]
    [SerializeField] private GameObject formPrefab;

    [Header("Combate")]
    [SerializeField] private AttackDataSO firstAttack;
    [SerializeField] private AbilityDataSO[] formAbilities;

    [Header("Movimiento opcional")]
    [SerializeField] private bool overrideMovement;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float maxSpeed = 7.5f;
    [Range(0f, 1f)]
    [SerializeField] private float drag = 0.4f;

    public string FormId => formId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject FormPrefab => formPrefab;
    public AttackDataSO FirstAttack => firstAttack;
    public IReadOnlyList<AbilityDataSO> FormAbilities => formAbilities;
    public bool OverrideMovement => overrideMovement;
    public float Acceleration => acceleration;
    public float MaxSpeed => maxSpeed;
    public float Drag => drag;
    public virtual bool IsDishForm => false;
}
