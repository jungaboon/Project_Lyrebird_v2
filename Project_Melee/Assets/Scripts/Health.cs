using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamage
{
    [SerializeField] protected Animator animator;
    [SerializeField] protected UnityEvent onHit;

    public virtual void Damage(Vector3 attackDirection, float attackDamage = 1, DamagePoint damagePoint = DamagePoint.High, bool heavyHit = false, bool knockback = false, Vector3 knockbackDirection = default)
    {
        float hitX = Vector3.Dot(transform.right, attackDirection);
        float hitY = Vector3.Dot(transform.forward, attackDirection);
        animator.SetFloat("hitX", hitX);
        animator.SetFloat("hitY", hitY);
        animator.SetTrigger($"hit{(int)damagePoint}");
        onHit.Invoke();
    }
}
