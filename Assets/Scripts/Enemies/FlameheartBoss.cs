using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameheartBoss : Boss
{
    public Collider2D flyingCollider;

    [Header("Fire Ball Attack")]
    public GameObject fireballPrefab;
    public AudioClip fireBreatheSFX;
    public float launchForce = 3f;
    public float maxAngleFromVertical = 45f;

    public Vector2 verticalFireballOffset;
    public Vector2 diagonalFireballOffset;
    
    public int minAnglesForAngledAnimation = 23;
    [Tooltip("How far into the animation is the fireball created?")]
    public float fireballInstantiateAnimPercentage = 0.25f;

    [Header("Melee Attack")]
    public CapsuleCollider2D meleeCollider;    
    public float hitCheckTimestamp = 3f / 8f;
    public float meleeAttackChance = 0.5f;
    public LayerMask meleeHitLayerMask;
    private float meleeColliderXOffset;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public BoxCollider2D arenaBounds;
    public int minMovesBetweenAttacks = 1;
    public int maxMovesBetweenAttacks = 3;

    private bool lastActionWasAttack;

    // Testing
    [Header("Testing")]
    public bool doFireballAttack;
    public bool doMeleeAttack;

    private bool hasAction;

    private PlayerController player;

    protected override void Start()
    {
        base.Start();

        player = CharacterSelector.GetPlayerController();

        flyingCollider.enabled = false;

        hasAction = false;

        meleeColliderXOffset = meleeCollider.offset.x;
        lastActionWasAttack = false;
    }

    /*
    private void Update()
    {
        if (hasAction) return;

        if (doFireballAttack)
        {
            doFireballAttack = false;
            StartCoroutine(FireballAttack());
        }   
        else if (doMeleeAttack)
        {
            doMeleeAttack = false;
            StartCoroutine(MeleeAttack());
        }
    }
    */
    private void ChooseAction()
    {
        if (hasAction) return;

        if (lastActionWasAttack) StartCoroutine(DoRandomMoves());
        else if (Random.value <= meleeAttackChance) StartCoroutine(MeleeAttack());
        else StartCoroutine(FireballAttack());
    }

    // for melee attack, fly and then try to move close to player
    // for fireball attack, fly and then try to move far from player
    // check how close to player the boss is, and then alter attack if within certain range and meets percent chance

    public void RespondToPlayer()
    {
        StartCoroutine(PlayerResponse());
    }

    private Vector3 GetRandomPositionInBounds()
    {
        float x = Random.Range(arenaBounds.bounds.min.x, arenaBounds.bounds.max.x);
        float y = Random.Range(arenaBounds.bounds.min.y, arenaBounds.bounds.max.y);

        return new Vector3(x, y, 0);
    }

    private Vector3 GetBestAttackPosition()
    {
        float x = Random.Range(arenaBounds.bounds.min.x, arenaBounds.bounds.max.x);
        float y = Mathf.Clamp(player.transform.position.y + 1f, arenaBounds.bounds.min.y, arenaBounds.bounds.max.y);

        return new Vector3(x, y, 0);
    }

    private IEnumerator DoRandomMoves()
    {
        print("doing random moves");

        hasAction = true;

        int numMoves = Random.Range(minMovesBetweenAttacks, maxMovesBetweenAttacks + 1);

        for (int i = 0;  i < numMoves; i++)
        {
            yield return StartCoroutine(MoveToPosition(GetRandomPositionInBounds()));

            yield return null;
        }

      //  yield return StartCoroutine(Land());

        yield return null;

        lastActionWasAttack = false;
        hasAction = false;

        ChooseAction();
    }

    private IEnumerator MoveToPosition(Vector3 position)
    {
        // If not flying already, take off
        if (!animator.GetBool("Flying"))
        {
            animator.SetBool("Flying", true);

            yield return null;

            while (!animator.GetBool("Can Move")) yield return null;
        }

        Vector3 towardsTarget = position - transform.position;

        while (Vector3.Distance(position, transform.position) >= 0.05f)
        {
            rigidbody.MovePosition(transform.position + Time.deltaTime * moveSpeed * towardsTarget.normalized);

            yield return null;
        }
    }

    private IEnumerator Land()
    {
        animator.SetBool("Flying", false);

        yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
    }

    private IEnumerator PlayerResponse()
    {
        hasAction = true;

        animator.SetTrigger("Spot Player");

        yield return null;

        while (animator.GetBool("Reacting")) yield return null;

        hasAction = false;

        if (player.health > 0) ChooseAction();
    }

    private IEnumerator MeleeAttack()
    {
        print("Doing melee attack");
        hasAction = true;

        // Move to attack position
        yield return StartCoroutine(MoveToPosition(GetBestAttackPosition()));
        print("    done moving");

        // Land before the attack

        yield return StartCoroutine(Land());
        print("    landed");

        // trigger the attack
        animator.SetTrigger("Melee Attack");

        sprite.flipX = player.transform.position.x < transform.position.x;

        // wait a frame for the animation to start
        yield return null;

        // do melee hit check somewhere in animation
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= hitCheckTimestamp) yield return null;

        meleeCollider.offset = new Vector2(meleeColliderXOffset * (sprite.flipX ? -1: 1), meleeCollider.offset.y);

        var hitCollider = Physics2D.OverlapCapsule(meleeCollider.bounds.center, meleeCollider.bounds.size, meleeCollider.direction, 0f, meleeHitLayerMask);

        if (hitCollider && hitCollider.gameObject.Equals(player.gameObject))
        {
            // dont hurt the player if the player is behind the flameheart
            if ((!sprite.flipX && player.transform.position.x >= transform.position.x) || (sprite.flipX && player.transform.transform.position.x <= transform.position.x)) player.TakeDamage(damage, damageType);
        }

        // wait for the animation to end
        while (animator.GetBool("Attacking")) yield return null;

        lastActionWasAttack = true;
        hasAction = false;

        if (player.health <= 0) animator.SetTrigger("Spot Player");
        else ChooseAction();
        
    }

    private IEnumerator FireballAttack()
    {
        print("Doing fireball attack");
        hasAction = true;

        // Move to attack position
        yield return StartCoroutine(MoveToPosition(GetBestAttackPosition()));
        print("    done moving");

        // Make sure the flameheart is not flying when this is triggered
        yield return StartCoroutine(Land());
        print("    landed");

        // Trigger the inhale animation
        animator.SetTrigger("Fire Attack");
        yield return null;

        // Wait for inhale to finish
        while (animator.GetBool("Inhaling")) yield return null;

        // Get the direction to the player
        Vector2 towardsPlayer = player.transform.position - transform.position;
        float anglesFromDown = Vector2.Angle(Vector2.down, towardsPlayer);

        sprite.flipX = towardsPlayer.x < 0;

        // If the player managed to get above the boss, the fireball can't go more than horizontal
        if (anglesFromDown >= 90f) anglesFromDown = 90;

        // If the angle is more than the minimum, do the angled animation
        animator.SetBool("Diagonal Fire", anglesFromDown >= minAnglesForAngledAnimation);

        // Wait for firebreathe animation to get to the desired point
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= fireballInstantiateAnimPercentage) yield return null;

        // then create the fireball
        audioSource.PlayOneShot(fireBreatheSFX);

        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.Euler(0, 0, 180 + anglesFromDown * (sprite.flipX ? -1 : 1)));
        if (animator.GetBool("Diagonal Fire"))
        {
            Vector3 fireballPos = fireball.transform.position;
            fireballPos.y += diagonalFireballOffset.y;
            fireballPos.x += diagonalFireballOffset.x * (sprite.flipX ? -1 : 1);


            fireball.transform.position = fireballPos;
        }
        else fireball.transform.position += (Vector3)verticalFireballOffset;

        // reget towards player and launch fireball
        towardsPlayer = player.GetComponent<Collider2D>().bounds.center - fireball.transform.position;
        towardsPlayer.Normalize();

        // if (towardsPlayer.y > 0) towardsPlayer.y = 0;
        if (Vector2.Angle(towardsPlayer, Vector2.up) < maxAngleFromVertical)
        {
            Vector2 clampedVector = towardsPlayer.normalized;

            clampedVector.x = Mathf.Cos(maxAngleFromVertical * Mathf.Deg2Rad);
            clampedVector.y = Mathf.Sin(maxAngleFromVertical * Mathf.Deg2Rad);

            towardsPlayer = clampedVector.normalized;
        }

        fireball.GetComponent<Fireball>().Launch(towardsPlayer, launchForce);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;

        yield return null;

        lastActionWasAttack = true;
        hasAction = false;

        if (player.health > 0) ChooseAction();
    }
}
