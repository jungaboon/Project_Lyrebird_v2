using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class EnemyHealth : Health
{
    [SerializeField] private Transform[] damagePoints;
    [SerializeField] private MMFeedbacks damageFeedback;

    public override void Damage(Vector3 attackDirection, float attackDamage = 1, DamagePoint damagePoint = DamagePoint.High, bool knockback = false, Vector3 knockbackDirection = default)
    {
        base.Damage(attackDirection, attackDamage, damagePoint, knockback, knockbackDirection);
        damageFeedback.transform.position = damagePoints[(int)damagePoint].position;
        damageFeedback.PlayFeedbacks();
    }
}
