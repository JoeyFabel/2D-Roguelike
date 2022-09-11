using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Pdb;
using UnityEngine;
using Random = UnityEngine.Random;

public class HostilePeasantBehavior : HostileNpcBehavior
{
    public float maxAttackDistance = 1.5f;
    public float attackHitCheckTimestamp = 0.4f;
    public ContactFilter2D meleeContactFilter;
    
    public Collider2D meleeAttackCollider;
    private float meleeAttackXOffset;

    [Header("Damage info")] 
    public int damage;
    public Damageable.DamageTypes damageType;
    
    private Coroutine currentAction;
    
    private static readonly int Attack = Animator.StringToHash("Attack");
    private const float AttackAnimationLength = 0.625f;
    
    public override void BecomeHostile()
    {
        print("starting NPC enemy behavior");
        player = CharacterSelector.GetPlayerController();

        meleeAttackXOffset = meleeAttackCollider.offset.x;
        
        currentAction = StartCoroutine(AttackAction());
    }
    
    private IEnumerator AttackAction()
    {
        // Prevents errors if the npc loads hostile 
        yield return null;
        
        // Keep attacking until the npc dies
        while (enabled)
        {
            // Wait for the player to get within attack range
            while (Vector2.Distance(transform.position, player.transform.position) > maxAttackDistance)
            {
                sprite.flipX = player.transform.position.x < transform.position.x;
                
                yield return null;
            }
            
            // Do the attack
            animator.SetTrigger(Attack);

            yield return new WaitForSeconds(attackHitCheckTimestamp);
            
            // Check for a hit
            Vector2 colliderOffset = new Vector2(meleeAttackXOffset * (sprite.flipX ? -1 : 1), meleeAttackCollider.offset.y);
            
            meleeAttackCollider.offset = colliderOffset;

            List<Collider2D> results = new List<Collider2D>();
            if (Physics2D.OverlapCollider(meleeAttackCollider, meleeContactFilter, results) > 0)
            {
                if (results.Find((col) => col.TryGetComponent<PlayerController>(out var player)))
                {
                    player.TakeDamage(damage, damageType);
                }
            }
            
            yield return new WaitForSeconds(AttackAnimationLength - attackHitCheckTimestamp);
            
            // Get ready for the next attack!
            sprite.flipX = player.transform.position.x < transform.position.x;
            yield return null;
        }
    }

    public override void EndCurrentAction()
    {
        if (currentAction != null) StopCoroutine(currentAction);
    }
}
