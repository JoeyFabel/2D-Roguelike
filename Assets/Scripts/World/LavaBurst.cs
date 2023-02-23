using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaBurst : MonoBehaviour
{
    private static readonly int AnimatorBurstID = Animator.StringToHash("Burst");
    private const Damageable.DamageTypes DamageType = Damageable.DamageTypes.Fire;
    
    public CapsuleCollider2D flameBurstDamageCollider;
    public ContactFilter2D flameBurstContactFilter;

    public float damage = 2;
    
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) animator.SetTrigger(AnimatorBurstID);
    }

    private void CheckForFireDamage()
    {
        flameBurstDamageCollider.enabled = true;
        
        // Get the colliders hit by the flame burst
        List<Collider2D> results = new List<Collider2D>();
        flameBurstDamageCollider.OverlapCollider(flameBurstContactFilter, results);

        flameBurstDamageCollider.enabled = false;

        if (results.Count > 0)
        {
            Collider2D player = results.Find((item) => item.CompareTag("Player"));
            if (player )
            {
                player.GetComponent<PlayerController>().TakeDamage(damage, DamageType);
            }
        }
    }
}
