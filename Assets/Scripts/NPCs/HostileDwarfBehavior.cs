using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

public class HostileDwarfBehavior : HostileNpcBehavior
{
    private static readonly int AnimatorHostileTriggerID = Animator.StringToHash("Become Hostile");
    private static readonly int AnimatorAxeAttackID = Animator.StringToHash("Melee Axe");
    private static readonly int AnimatorHammerAttackID = Animator.StringToHash("Melee Hammer");

    private const float MeleeAxeAnimationLength = 0.9f;
    private const float MeleeHammerAnimationLength = 0.875f;
    
    [Header("Movement options")]
    public float moveSpeed = 2f;
    public float moveTime = 5f;
    [Range(0f, 1f)] public float zigZagMovementChange = 0.6f;
    public int minZigZagsPerAction = 3;
    public int maxZigZagsPerAction = 5;
    public float sqrMaxDistanceForCounterAttack = 1f;

    [Header("Desired Positioning")] 
    public float desiredCounterDistance = 0.5f;
    public float desiredAttackDistance = 0.2f;
    
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

    private bool isCountering = false;
    
    private Coroutine currentAction;
    private static readonly int AnimatorToPlayerYid = Animator.StringToHash("To Player Y");
    private static readonly int AnimatorMovementID = Animator.StringToHash("Movement");

    private new Rigidbody2D rigidbody;
    private Animator playerAnimator;
    private static readonly int AnimatorBlockingID = Animator.StringToHash("Blocking");

    public override void BecomeHostile()
    {
        base.Start();
        
        print("now hostile!");
        player = CharacterSelector.GetPlayerController();
        playerAnimator = player.GetComponent<Animator>();

        meleeAxeAttackXOffset = meleeAxeAttackCollider.offset.x;
        meleeHammerAttackXOffset = meleeHammerAttackCollider.offset.x;
        
        animator ??= GetComponent<Animator>();
        animator.SetTrigger(AnimatorHostileTriggerID);

        rigidbody = GetComponent<Rigidbody2D>();
        
        ChooseAction();
    }

    private void ChooseAction()
    {
       if ((player.transform.position - transform.position).magnitude <= 1.5f) currentAction = StartCoroutine(MeleeAttackAction());
       else currentAction = StartCoroutine(ZigZagToPlayerMovement());
       

       // Only attack if the player is within a certain distance.
       // If close enough, use MoveToDesiredAttackPosition first..
       
       // If the dwarf is moving and the player starts to do a melee attack, stop the current action and perform a brief dodge, followed by an attack
       // Types of movement:
       // 1) zig-zag towards player, then attack
       // 2) move forward/backward with player, some random side to side, trying to bait player
       // IF the player does an attack, and the enemy is close, try to dodge (certain chance)
       // IF the dodge is successful, perform a successful attack
    }
    
    private IEnumerator MeleeAttackAction()
    {
        print("doing melee attack");
        
        // Choose between hammer and axe attack
        bool hammerAttack = Random.value <= oddsMeleeAttackUsesHammer;

        Vector2 towardsPlayer = player.transform.position - transform.position;
        
        // Trigger the melee attack
        sprite.flipX = player.transform.position.x < transform.position.x;
        animator.SetFloat(AnimatorToPlayerYid, towardsPlayer.y);
        animator.SetTrigger(hammerAttack ? AnimatorHammerAttackID : AnimatorAxeAttackID);
        
        if (hammerAttack) yield return new WaitForSeconds(meleeHitCheckTime);
        else
        {
            float timestamp = Time.time;

            while (Time.time <= timestamp + MeleeAxeAnimationLength - meleeHitCheckTime)
            {
                rigidbody.MovePosition((Vector2)transform.position + moveSpeed / 2 * Time.fixedDeltaTime * towardsPlayer.normalized);
                yield return null;
            }
        }
        
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
        
        
        yield return new WaitForSeconds(1f);
        
        ChooseAction();
    }

    private IEnumerator ZigZagToPlayerMovement()
    {
        animator.SetFloat(AnimatorMovementID, 1f);
        print("zig zagging towards player");

        // The number of remaining zigZags, reduce by 1 every zig/zag
        int zigZagCount = minZigZagsPerAction;//Random.Range(minZigZagsPerAction, maxZigZagsPerAction + 1);
        
        bool leftwardZigZag = true; // Is the current zig zag relatively to the left of the player or to the right?
        bool firstZigZag = true; // First zig zag is half length

        while (zigZagCount > 0 && !isCountering)
        {
            Vector2 currentPosition = transform.position;
            Vector2 playerPosition = player.transform.position;
            
            // Move
            // Step 1: Get the toPlayer Vector
            Vector2 towardsPlayer = playerPosition - currentPosition;
            
            print("still in move    ");
            
            // Step 2: Pick the next movement vector 
            Vector2 endPosition = towardsPlayer / zigZagCount + (Vector2.Perpendicular(towardsPlayer) * (leftwardZigZag ? 1 : -1)).normalized;
            // This is not normalized!
            
            // Calculate this now as it is a constant movement vector, so it can be reused
            Vector2 moveVector = endPosition.normalized;
            
            // Set animator values
            animator.SetFloat(AnimatorToPlayerYid, moveVector.y);
            sprite.flipX = moveVector.x < 0;
            
            // Step 3: move towards the end position until it is in reach.
            while (!isCountering && (currentPosition + endPosition - (Vector2)transform.position).magnitude >
                   moveSpeed * Time.fixedDeltaTime)
            {
                // See if player decided to attack and the dwarf is in counter attack range
                if (IsAbleToCounterAttack(towardsPlayer))
                {
                    StopCoroutine(nameof(ZigZagToPlayerMovement));
                    currentAction = StartCoroutine(TryToCounterAttack());

                    yield break;
                }

                // Move towards point
                rigidbody.MovePosition((Vector2)transform.position + moveSpeed * Time.fixedDeltaTime * moveVector);

                yield return null;
            }

            // Decrease the number of remaining zig zags and go again in the other direction
            leftwardZigZag = !leftwardZigZag;
            zigZagCount--;
        }
        
        animator.SetFloat(AnimatorMovementID, 0f);
        ChooseAction();
    }

    private bool IsAbleToCounterAttack(Vector2 towardsPlayer)
    {
       return playerAnimator.GetBool("Attacking") && towardsPlayer.sqrMagnitude <= sqrMaxDistanceForCounterAttack;
    }
    
    private IEnumerator BaitPlayerMovement()
    {
        print("Baiting player");
        
        yield return null;
    }

    private IEnumerator TryToCounterAttack()
    {
        print("doing counter attack!");
        rigidbody.velocity = Vector2.zero;
        isCountering = true;

        animator.SetFloat(AnimatorMovementID, 0);
        animator.SetTrigger("Defend");

        isInvincible = true;

        yield return null;
        while (animator.GetBool(AnimatorBlockingID)) yield return null;

        isInvincible = false;

        // Step 4) Attack!
        print("attack!");
        currentAction = StartCoroutine(MeleeAttackAction());
        isCountering = false;
    }

    private IEnumerator MoveToDesiredAttackPosition()
    {
        animator.SetFloat(AnimatorMovementID, 1f);

        Vector2 desiredAttackPosition = (transform.position - player.transform.position.normalized) * desiredAttackDistance + player.transform.position;
        Vector2 moveVector = desiredAttackPosition - (Vector2)transform.position;
        
        animator.SetFloat(AnimatorToPlayerYid, moveVector.y);
        sprite.flipX = moveVector.x < 0;
        
        while ((desiredAttackPosition - (Vector2)transform.position).magnitude > moveSpeed * Time.fixedDeltaTime)
        {
            rigidbody.MovePosition((Vector2)transform.position + moveSpeed * Time.fixedDeltaTime * moveVector);

            yield return null;
        }
        
        animator.SetFloat(AnimatorMovementID, 0);
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

/*
 
    public void MarkHostileState(bool isHostile)
    {
        this.isHostile = isHostile;
    }
    
    public override WorldObjectSaveData GetSaveData()
    {
        ShopKeeperSaveData saveData = new ShopKeeperSaveData();

        saveData.baseSaveData = base.GetSaveData() as DialogTreeSaveData;
        saveData.isHostile = isHostile;

        return saveData;
    }

    public override void LoadData(WorldObjectSaveData saveData)
    {
        ShopKeeperSaveData data = saveData as ShopKeeperSaveData;

        if (data == null) return;

        base.LoadData(saveData);
    }

    [System.Serializable]
    public class ShopKeeperSaveData : WorldObjectSaveData
    {
        public DialogTreeSaveData baseSaveData;

        public bool isHostile;
    }
 */
