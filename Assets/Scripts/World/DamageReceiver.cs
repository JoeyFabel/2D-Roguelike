using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : Damageable
{
    public Damageable objectToDamage;
    public float damageMultiplier = 1f;

    protected override void Start()
    {
        // TODO - Fix errors when projectile radius hits damage receiver and base damageable
        base.Start();

        fireballImpactSound = objectToDamage.fireballImpactSound;
        slashImpactSound = objectToDamage.slashImpactSound;
        thrustImpactSound = objectToDamage.thrustImpactSound;
        bluntImpactSound = objectToDamage.bluntImpactSound;
        magicImpactSound = objectToDamage.magicImpactSound;
    }

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