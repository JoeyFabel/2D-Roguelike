using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : Damageable
{
    public Damageable objectToDamage;
    public float damageMultiplier = 1f;

    public override void ApplyDamage(float amount)
    {        
        objectToDamage.ApplyDamage(amount * damageMultiplier);
    }

    public override void ApplyDamage(float amount, DamageTypes damageType)
    {
        objectToDamage.ApplyDamage(amount * damageMultiplier, damageType);
    }

    protected override void Death()
    {
        // do nothing
    }
}