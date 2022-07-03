using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameheartBoss : Boss
{
    public Collider2D flyingCollider;
    private Collider2D landedCollider;

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
    public float rotationSpeed = 0.5f;
    private float circleRadius = 2f;

    private bool lastActionWasAttack;

    [Header("Audio")]
    public AudioClip wingFlapSfx;
    public AudioClip meleeAttackSfx;
    public AudioClip firebreathSfx;

    [Header("Death")]
    public int numVfxToCreate = 5;
    public GameObject deathVFX;
    public AudioClip vfxSound;

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
        landedCollider = GetComponent<Collider2D>();

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

        if (lastActionWasAttack) DoMovementAction();
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

    protected override void Death()
    {
        // Play the death animation & effects
        print("died!");

        rigidbody.simulated = false;
        landedCollider.enabled = false;
        flyingCollider.enabled = false;

        enabled = false;

        StopAllCoroutines();
        StartCoroutine(PlayDeathVFX());
    }

    #region Death Helper Methods

    private IEnumerator PlayDeathVFX()
    {        
        // Land first
        yield return StartCoroutine(Land());

        float vfxCreateTime = 0.4f;

        float timestamp = Time.time;

        // first time to freeze animator
        animator.SetTrigger("Death");

        int remainingVfx = numVfxToCreate;

        while (remainingVfx > 0)
        {
            if (Time.time >= timestamp + vfxCreateTime)
            {
                Vector3 vfxPosition = transform.position + (Vector3)Random.insideUnitCircle * landedCollider.bounds.size.x;

                // should destroy itself
                Instantiate(deathVFX, vfxPosition, Quaternion.identity).transform.localScale = Vector3.one * 0.5f;

                audioSource.PlayOneShot(vfxSound);

                timestamp = Time.time;
                remainingVfx--;
            }

            yield return null;
        }

        // second time for animation
        animator.SetTrigger("Death");
        audioSource.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)]);

        yield return new WaitForSeconds(1f);

        DropXPAndMoney();

        BossRoomTriggerOnDeath();

        Destroy(gameObject);
    }

    private void DropXPAndMoney()
    {
        XPManager.GainXP(xpForDefeating, transform.position);
        if (Random.value <= moneyDropChance) Inventory.CreateMoneyDrop(moneyForDefeating, transform.position);
    }

    #endregion

    // Prevents XP and money from being dropped too soon
    public override void ApplyDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            //XPManager.GainXP(xpForDefeating, transform.position);
            //if (Random.value <= moneyDropChance) Inventory.CreateMoneyDrop(moneyForDefeating, transform.position);

            Death();
        }
        else
        {
            PlayDamagedSound();
            StartCoroutine(FlashRedOnDamage());
        }
    }

    private Vector3 GetRandomPositionInBounds()
    {
        float x = Random.Range(arenaBounds.bounds.min.x + 1, arenaBounds.bounds.max.x - 1);
        float y = Random.Range(arenaBounds.bounds.min.y, arenaBounds.bounds.max.y);

        return new Vector3(x, y, 0);
    }

    private Vector3 GetBestAttackPosition()
    {
        float x = Random.Range(arenaBounds.bounds.min.x + 1, arenaBounds.bounds.max.x - 1);
        float y = Mathf.Clamp(player.transform.position.y + 1f, arenaBounds.bounds.min.y, arenaBounds.bounds.max.y);

        return new Vector3(x, y, 0);
    }

    private void DoMovementAction()
    {
        if (Random.value <= 0.5f) StartCoroutine(DoRandomMoves());
        else StartCoroutine(CirclePlayer());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        print(collision.collider.name + " hit " + collision.otherCollider.name);
    }

    private void PlayWingFlapSFX()
    {
        audioSource.PlayOneShot(wingFlapSfx);
    }

    /// <summary>
    /// Sub-action, run during another action, that has the Flameheart takeoff and start flying
    /// </summary>
    /// <returns></returns>
    private IEnumerator Takeoff()
    {
        // If not flying already, take off
        if (!animator.GetBool("Flying"))
        {
            animator.SetBool("Flying", true);

            yield return null;

            while (!animator.GetBool("Can Move")) yield return null;

            landedCollider.enabled = false;
            flyingCollider.enabled = true;
        }
    }

    /// <summary>
    /// Sub-action, run during another action, that has the Flameheart land on the ground
    /// </summary>
    /// <returns></returns>
    private IEnumerator Land()
    {
        animator.SetBool("Flying", false);

        yield return null;

        flyingCollider.enabled = false;
        landedCollider.enabled = true;

        while (animator.GetBool("Landing")) yield return null;        
    }

    /// <summary>
    /// Primary action that has the Flameheart move to a few random positions before choosing the next action
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Sub-action, run during another action, that moves the Flameheart to the specified position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private IEnumerator MoveToPosition(Vector3 position)
    {
        // If not flying already, take off
        yield return StartCoroutine(Takeoff());

        Vector3 towardsTarget = position - transform.position;

        while (Vector3.Distance(position, transform.position) >= 0.05f)
        {
            Vector2 movePosition = rigidbody.position;
            movePosition += Time.fixedDeltaTime * moveSpeed * (Vector2)towardsTarget.normalized;

            //rigidbody.MovePosition(transform.position + Time.deltaTime * moveSpeed * towardsTarget.normalized);
            rigidbody.MovePosition(movePosition);

            sprite.flipX = towardsTarget.x < 0;

            yield return null;
        }
    }

    /// <summary>
    /// Sub-action, run during another action, that moves the Flameheart to a position next to the player
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToPlayer()
    {
        float timeout = 4f;
        float speedMultiplier = 1f;

        // If not flying already, take off
        yield return StartCoroutine(Takeoff());

        while (Vector3.Distance(player.transform.position, transform.position) >= 1f)
        {
            Vector2 movePosition = rigidbody.position;
            movePosition += Time.fixedDeltaTime * moveSpeed * speedMultiplier * (Vector2)(player.transform.position - transform.position).normalized;

            rigidbody.MovePosition(movePosition);

            timeout -= Time.fixedDeltaTime;
            sprite.flipX = player.transform.position.x < transform.position.x;

            if (timeout <= 0)
            {
                speedMultiplier += 0.5f;
                timeout = 2f;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Primary action that has the Flameheart circle the player a few times before choosing the next action
    /// </summary>
    /// <returns></returns>
    private IEnumerator CirclePlayer()
    {
        // If not flying already, take off
        yield return StartCoroutine(Takeoff());

        // Move to the player before circling
        yield return StartCoroutine(MoveToPlayer());

        /*
         * To circle the player, two things are needed: The players position, and a circle function
         *      - The players position is straightforward
         *      - The circle function is radius * (cos(2?/timePerCircle), sin(2?/timePerCircle)
         */

        // timePerCircle is the distance per circle divided by the move speed
        //float timePerCircle = Mathf.PI * 2 * circleRadius / moveSpeed;

        float time = Random.Range((float)minMovesBetweenAttacks, maxMovesBetweenAttacks + 1) * Mathf.PI / 2;

        Vector2 circlePosition;
        float angle = 0f;

        while (time >= 0)
        {
            circlePosition = new Vector2(Mathf.Cos(angle) * circleRadius, Mathf.Sin(angle) * circleRadius);
          
            rigidbody.MovePosition((Vector2)player.transform.position + circlePosition);
            angle += Time.deltaTime * rotationSpeed * 2 * Mathf.PI;

            sprite.flipX = player.transform.position.x < transform.position.x;

            time -= Time.deltaTime;

            yield return null;
        }

        lastActionWasAttack = false;
        hasAction = false;

        ChooseAction();
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
      //  print("Doing melee attack");
        hasAction = true;

        // Move to attack position
        //yield return StartCoroutine(MoveToPosition(GetBestAttackPosition()));
        yield return StartCoroutine(MoveToPlayer());
      //  print("    done moving");

        // Land before the attack

        yield return StartCoroutine(Land());
      //  print("    landed");

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

        audioSource.PlayOneShot(meleeAttackSfx);

        // wait for the animation to end
        while (animator.GetBool("Attacking")) yield return null;

        lastActionWasAttack = true;
        hasAction = false;

        if (player.health <= 0) animator.SetTrigger("Spot Player");
        else ChooseAction();
        
    }

    private IEnumerator FireballAttack()
    {
      // print("Doing fireball attack");
        hasAction = true;

        // Move to attack position
        yield return StartCoroutine(MoveToPosition(GetBestAttackPosition()));
      //  print("    done moving");

        // Make sure the flameheart is not flying when this is triggered
        yield return StartCoroutine(Land());
      //  print("    landed");

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
        audioSource.PlayOneShot(firebreathSfx);

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
