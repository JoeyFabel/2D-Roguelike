using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostileDwarfBehavior : HostileNpcBehavior
{
    private static readonly int AnimatorHostileTriggerID = Animator.StringToHash("Become Hostile");
    private static readonly int AnimatorAxeAttackID = Animator.StringToHash("Melee Axe");
    private static readonly int AnimatorHammerAttackID = Animator.StringToHash("Melee Hammer");

    private const float MeleeAxeAnimationLength = 0.9f;
    private const float MeleeHammerAnimationLength = 0.875f;
    
    [Header("Melee Options")]
    public Collider2D meleeAttackCollider;
    [Range(0f, 1f)]
    public float oddsMeleeAttackUsesHammer = 0.4f;
    public float meleeHitCheckTime = 0.4f;
    public ContactFilter2D meleeContactFilter;

    [Header("Damage Info")] 
    public float axeDamage = 1;
    public float hammerDamage = 1.5f;
    public Damageable.DamageTypes axeDamageType = Damageable.DamageTypes.Slash;
    public Damageable.DamageTypes hammerDamageType = Damageable.DamageTypes.Blunt;


    private float meleeAttackXOffset;

    private Coroutine currentAction;


    public override void BecomeHostile()
    {
        print("now hostile!");
        player = CharacterSelector.GetPlayerController();

        meleeAttackXOffset = meleeAttackCollider.offset.x;

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
        animator.SetTrigger(hammerAttack ? AnimatorHammerAttackID : AnimatorAxeAttackID);

        yield return new WaitForSeconds(meleeHitCheckTime);
        
        // Check for a melee hit
        Vector2 colliderOffset = new Vector2(meleeAttackXOffset * (sprite.flipX ? -1 : 1), meleeAttackCollider.offset.y);
        meleeAttackCollider.offset = colliderOffset;

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
        
        yield return new WaitForSeconds(5f);
        
        ChooseAction();
    }
}
