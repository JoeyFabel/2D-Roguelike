using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HostileDwarfBehavior : HostileNpcBehavior
{
    private static readonly int AnimatorHostileTriggerID = Animator.StringToHash("Become Hostile");
    private static readonly int AnimatorAxeAttackID = Animator.StringToHash("Melee Axe");
    private static readonly int AnimatorHammerAttackID = Animator.StringToHash("Melee Hammer");

    private const float MeleeAxeAnimationLength = 0.9f;
    private const float MeleeHammerAnimationLength = 0.875f;
    
    [Header("Melee Options")]
    public Collider2D meleeAxeAttackCollider;
    public Collider2D meleeHammerAttackCollider;
    [Range(0f, 1f)]
    public float oddsMeleeAttackUsesHammer = 0.4f;
    public float meleeHitCheckTime = 0.4f;
    public ContactFilter2D meleeContactFilter;

    [Header("Damage Info")] 
    public float axeDamage = 1;
    public float hammerDamage = 1.5f;
    public Damageable.DamageTypes axeDamageType = Damageable.DamageTypes.Slash;
    public Damageable.DamageTypes hammerDamageType = Damageable.DamageTypes.Blunt;

    [Header("Collider Offset Info")]
    public float hammerSouthColliderYOffset;
    public float hammerNorthColliderYOffset;
    public float axeSouthColliderYOffset;
    public float axeNorthColliderYOffset;

    private float meleeAxeAttackXOffset;
    private float meleeHammerAttackXOffset;

    private Coroutine currentAction;
    private static readonly int AnimatorToPlayerYid = Animator.StringToHash("To Player Y");


    public override void BecomeHostile()
    {
        print("now hostile!");
        player = CharacterSelector.GetPlayerController();

        meleeAxeAttackXOffset = meleeAxeAttackCollider.offset.x;
        meleeHammerAttackXOffset = meleeHammerAttackCollider.offset.x;

        animator ??= GetComponent<Animator>();
        animator.SetTrigger(AnimatorHostileTriggerID);

        ChooseAction();
    }

    private void ChooseAction()
    {
        currentAction = StartCoroutine(MeleeAttackAction());
    }
    
    private IEnumerator MeleeAttackAction()
    {
        // Choose between hammer and axe attack
        bool hammerAttack = Random.value <= oddsMeleeAttackUsesHammer;
        
        // Trigger the melee attack
        sprite.flipX = player.transform.position.x < transform.position.x;
        animator.SetFloat(AnimatorToPlayerYid, player.transform.position.y - transform.position.y);
        animator.SetTrigger(hammerAttack ? AnimatorHammerAttackID : AnimatorAxeAttackID);
        
        yield return new WaitForSeconds(meleeHitCheckTime);
        
        // Check for a melee hit
        var meleeAttackCollider = SetAttackColliderOffsets(hammerAttack, player.transform.position.y > transform.position.y);

        float damage = hammerAttack ? hammerDamage : axeDamage;
        Damageable.DamageTypes damageType = hammerAttack ? hammerDamageType : axeDamageType;

        List<Collider2D> results = new List<Collider2D>();
        if (Physics2D.OverlapCollider(meleeAttackCollider, meleeContactFilter, results) > 0)
        {
            // Damage the player if the player was hit
            if (results.Find((col) => col.TryGetComponent<PlayerController>(out var player)))
            {
                player.TakeDamage(damage, damageType);
            }

            // Damage any damageables that were hit (boxes, bombs, etc)
            foreach (var collider in results)
            {
                if (collider.TryGetComponent(out Damageable damageable)) damageable.ApplyDamage(damage, damageType);
            }
        }
        
        // Wait for the animation to end
        if (hammerAttack) yield return new WaitForSeconds(MeleeHammerAnimationLength - meleeHitCheckTime);
        else yield return new WaitForSeconds(MeleeAxeAnimationLength - meleeHitCheckTime);
        
        yield return new WaitForSeconds(2f);
        
        ChooseAction();
    }

    private Collider2D SetAttackColliderOffsets(bool hammerAttack, bool facingNorth)
    {
        if (hammerAttack) // set hammer attack offsets
        {
            Vector2 colliderOffset = new Vector2(meleeHammerAttackXOffset * (sprite.flipX ? -1 : 1), 
                (facingNorth ? hammerNorthColliderYOffset : hammerSouthColliderYOffset));
            
            meleeHammerAttackCollider.offset = colliderOffset;

            return meleeHammerAttackCollider;
        }
        else // Set axe collider offset
        {
            Vector2 colliderOffset = new Vector2(meleeAxeAttackXOffset * (sprite.flipX ? -1 : 1), 
                (facingNorth ? axeNorthColliderYOffset : axeSouthColliderYOffset));
            
            meleeAxeAttackCollider.offset = colliderOffset;

            return meleeAxeAttackCollider;
        }
    }
}
