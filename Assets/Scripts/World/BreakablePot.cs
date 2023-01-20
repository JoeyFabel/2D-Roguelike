using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakablePot : Damageable
{
    [Header("For Money Drops")]
    public MoneyDrop moneyDrop;
    public int moneyAmount;

    [Header("For Item Drops")]
    public ItemPickup itemToDrop;

    public float dropChance = 0.25f;

    protected override void Death()
    {
        if (Random.value <= dropChance)
        {
            if (itemToDrop) Instantiate(itemToDrop, transform.position, transform.rotation);
            else if (moneyAmount > 0) Instantiate(moneyDrop, transform.position, transform.rotation).SetMoney(moneyAmount);
        }
        Destroy(gameObject);
    }
}