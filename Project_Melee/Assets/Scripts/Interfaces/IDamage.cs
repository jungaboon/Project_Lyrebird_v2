using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamage
{
    public void Damage(Vector3 attackDirection, float attackDamage = 1f, DamagePoint damagePoint = DamagePoint.High, bool heavyHit = false, bool knockback = false, Vector3 knockbackDirection = default);
}
