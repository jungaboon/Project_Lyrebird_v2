using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class EnemyHealth : Health
{
    [SerializeField] private Transform[] damagePoints;
    [SerializeField] private MMFeedbacks[] damageFeedbacks;

    public override void Damage(Vector3 attackDirection, float attackDamage = 1, DamagePoint damagePoint = DamagePoint.High, bool heavyHit = false, bool knockback = false, Vector3 knockbackDirection = default)
    {
        base.Damage(attackDirection, attackDamage, damagePoint, heavyHit, knockback, knockbackDirection);
        if (knockback && !animator.GetBool("knockedDown"))
        {
            attackDirection.y = 0f;
            transform.rotation = Quaternion.LookRotation(-attackDirection);
            animator.SetTrigger("knockback");
            damageFeedbacks[1].transform.position = damagePoints[(int)damagePoint].position;
            damageFeedbacks[1].PlayFeedbacks();
            SetKnockedDownStatus(1);
            return;
        }

        damageFeedbacks[0].transform.position = damagePoints[(int)damagePoint].position;
        damageFeedbacks[0].PlayFeedbacks();

        if (heavyHit)
        {
            animator.SetTrigger("heavyHit");
        }
    }

    public void SetKnockedDownStatus(int status)
    {
        animator.SetBool("knockedDown", status == 0 ? false : true);
    }
}
