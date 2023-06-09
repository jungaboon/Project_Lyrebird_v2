using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public enum DamagePoint
{
    Low,
    Mid,
    High
}

[CreateAssetMenu(fileName = "Attack Parameter", menuName = "Scriptable Objects/Create Attack Parameter")]
public class ScriptableAttack : ScriptableObject
{
    public Vector3 attackDirection = Vector3.forward;
    public float attackAngle = 45f;
    public float attackRange = 1.5f;
    public float attackDamage = 1f;
    public DamagePoint damagePoint;
    public bool heavyHit = false;
    [HideIf("heavyHit")] public bool knockBack = false;
    [ShowIf("knockBack")]public Vector3 knockbackDirection = Vector3.up;
}
