using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    private BossRoomTrigger bossTrigger;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Death()
    {
        bossTrigger.OnBossDeath();
    }

    protected void BossRoomTriggerOnDeath()
    {
        bossTrigger.OnBossDeath();
    }

    public void SetBossRoomTrigger(BossRoomTrigger trigger)
    {
        bossTrigger = trigger;
    }
}
