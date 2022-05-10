using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantSlime : Boss
{
    private const float whiteFlashTime = 0.1875f;

    [Header("Giant Slime Stuff")]
    public float scaleIncreasePerHealth = 0.5f;

    public GameObject slimePrefab;

    public float slimeLaunchTime = 3f;

    public int minChildAttacksBeforeAbsorption = 1;
    public int maxChildAttacksBeforeAbsorption = 2;

    public float moveSpeed = 1f;
    public float moveSpeedDecreasePerHealth = 0.25f;
    public float launchDistance = 2f;

    public ContactFilter2D contactFilter;

    [Header("Attack Info")]
    [Tooltip("How long the slime pauses after an attack or random movement")]
    public float pauseTime = 2f;

    public float minRandomMoveTime = 1.5f;
    public float maxRandomMoveTime = 3f;

    public float minAttackTime = 2f;
    public float maxAttackTime = 4f;
    public int numMovesBeforeAttack = 2;

    [Header("Death VFX")]
    public GameObject deathVFX;
    public AudioClip vfxSound;

    private PlayerController player;

    private List<Slime> createdSlimes;

    new CapsuleCollider2D collider;
    Collider2D secondaryCollider;

    private float mainColliderOffset;
    private float secondaryColliderOffset;

    private int remainingMovesBeforeAttack;

    private bool hasAction;
    private bool isInvincible;

    protected override void Start()
    {
        base.Start();

        collider = GetComponent<CapsuleCollider2D>();
        secondaryCollider = transform.GetChild(0).GetComponent<Collider2D>();

        player = CharacterSelector.GetPlayerController();
        createdSlimes = new List<Slime>();

        Vector3 scale = Vector3.one + Vector3.one * scaleIncreasePerHealth * maxHealth;
        transform.localScale = scale;

        mainColliderOffset = collider.offset.x;
        secondaryColliderOffset = secondaryCollider.offset.x;

        remainingMovesBeforeAttack = numMovesBeforeAttack;

        isInvincible = true;
        hasAction = true;
        animator.SetBool("Hiding", true);
        collider.enabled = false;
        secondaryCollider.enabled = false;
    }

    public void ReactToPlayerArrival()
    {
        animator.SetBool("Hiding", false);

        StartCoroutine(WaitUntilRisen());
    }

    private void Update()
    {
        collider.offset = new Vector2(mainColliderOffset * (sprite.flipX ? -1 : 1), collider.offset.y);
        secondaryCollider.offset = new Vector2(secondaryColliderOffset * (sprite.flipX ? -1 : 1), secondaryCollider.offset.y);

        if (!hasAction)
        {
            if (remainingMovesBeforeAttack == 0)
            {
                remainingMovesBeforeAttack = numMovesBeforeAttack;
                StartCoroutine(DoAttack());
            }
            else
            {
                remainingMovesBeforeAttack--;
                StartCoroutine(DoMove(Vector2.zero));
            }
        }
    }

    protected override void Death()
    {
        rigidbody.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        enabled = false;

        // kill all the baby slimes :(
        for (int i = createdSlimes.Count - 1; i >= 0; i--) createdSlimes[i].ApplyDamage(createdSlimes[i].maxHealth);

        StartCoroutine(PlayDeathVFX());

        //StartCoroutine(DestroyAfterAudio());

        //DropXPAndMoney();
        //Debug.LogWarning("TODO -- defeat animation/vfx, give xp after effect ends");
    }

    public override void ApplyDamage(float amount)
    {
        if (isInvincible) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Death();
        }
        else
        {
            PlayDamagedSound();
            StartCoroutine(FlashRedOnDamage());
        
            Vector2 playerTowardsCenter = transform.position - player.transform.position;

            float jumpDistance = launchDistance * Mathf.Max(1f, amount);

            SpawnSlime(playerTowardsCenter.normalized * jumpDistance, slimeLaunchTime);
        }
    }

    public void LoseSlime(Slime deadSlime)
    {
        createdSlimes.Remove(deadSlime);
    }

    public void ReabsorbSlime(Slime slimeToAbsorb)
    {
        print(this + " absorbed " + slimeToAbsorb);

        createdSlimes.Remove(slimeToAbsorb);
        Destroy(slimeToAbsorb.gameObject);

        StartCoroutine(FlashWhiteOnHeal());

        currentHealth += 1;

        Vector3 newScale = transform.localScale;
        newScale += Vector3.one * scaleIncreasePerHealth;

        transform.localScale = newScale;

        moveSpeed -= moveSpeedDecreasePerHealth;
    }

    private void SpawnSlime(Vector2 jumpEndPosition, float slimeJumpSpeed)
    {
        Vector3 spawnPosition = collider.ClosestPoint((Vector2)collider.bounds.center + jumpEndPosition);

        Slime spawnedSlime = Instantiate(slimePrefab, spawnPosition, Quaternion.identity).GetComponent<Slime>();
        spawnedSlime.Start();

        spawnedSlime.SpawnFromGiantSlime(this, Random.Range(minChildAttacksBeforeAbsorption, maxChildAttacksBeforeAbsorption + 1), jumpEndPosition, slimeJumpSpeed);
        createdSlimes.Add(spawnedSlime);

        Vector3 newScale = transform.localScale;
        newScale -= Vector3.one * scaleIncreasePerHealth;

        transform.localScale = newScale;

        moveSpeed += moveSpeedDecreasePerHealth;
    }    

    private void DropXPAndMoney()
    {
        XPManager.GainXP(xpForDefeating, transform.position);
        if (Random.value <= moneyDropChance) Inventory.CreateMoneyDrop(moneyForDefeating, transform.position);
    }

    protected void OnCollisionStay2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player) player.TakeDamage(damage, damageType);     
    }

    int vfxToCreate = 5;

    private IEnumerator PlayDeathVFX()
    {
        float vfxCreateTime = 0.5f;

        float timestamp = Time.time;

        // first time to freeze animator
        animator.SetTrigger("Death");

        int remainingVfx = vfxToCreate;

        while (remainingVfx > 0)
        {
            if (Time.time >= timestamp + vfxCreateTime)
            {
                Vector3 vfxPosition = transform.position + (Vector3)Random.insideUnitCircle * collider.bounds.size.x;

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

    private IEnumerator WaitUntilRisen()
    {
        while (animator.GetBool("Rising Up")) yield return null;

        collider.enabled = true;
        secondaryCollider.enabled = true;

        hasAction = false;
        isInvincible = false;
    }

    /// <summary>
    /// Have the slime move in the specified direction. However, you can do a random direction if Vector2.zero is specified.
    /// </summary>
    /// <param name="moveDirection">The direction for the slime to move. Vector2.zero results in a random direction.</param>
    private IEnumerator DoMove(Vector2 moveDirection)
    {
        hasAction = true;

        // (1) Choose a random direction or use the specified direction
        Vector2 direction;

        if (moveDirection != Vector2.zero)
        {
            direction = moveDirection;
        }
        else
        {
            int xDir = Random.Range(-1, 2); // -1, 0, or 1
            int yDir = Random.Range(-1, 2); // -1, 0, or 1
            if (xDir == 0 && yDir == 0) xDir = 1; // cannot randomly not move

            direction = new Vector2(xDir, yDir).normalized;
        }

        // (2) Move in that direction for the randomly determined time
        float timestamp = Time.time;
        float moveTime = GetRandomMoveTime();
        animator.SetBool("Moving", true);

        sprite.flipX = direction.x < 0;

        while (Time.time - timestamp <= moveTime)
        {
            // (a) Make sure a wall will not be hit. If it will be hit, revert direction
            // Handle this with box cast

            List<RaycastHit2D> results = new List<RaycastHit2D>();

            //if (Physics2D.BoxCast(rigidbody.position, collider.size, 0, direction, contactFilter,  results, moveSpeed * Time.fixedDeltaTime) > 0)
            if (Physics2D.CapsuleCast(rigidbody.position, collider.size, collider.direction, 0, direction, contactFilter, results, moveSpeed * Time.fixedDeltaTime) > 0)
            {
                // ignore collisions with oneself
                for (int i = results.Count - 1; i >= 0; i--)
                {
                    if (results[i].collider == collider) results.RemoveAt(i);
                }

                if (results.Count > 0)
                {
                    // Get new move direction
                    // The new move direction needs to be in an opposite direction

                    int newX;
                    if (direction.x == 0) newX = Random.Range(-1, 2);
                    else newX = direction.x > 0 ? Random.Range(-1, 1) : Random.Range(0, 2);

                    int newY;
                    if (direction.y == 0) newY = Random.Range(-1, 2);
                    else newY = direction.y > 0 ? Random.Range(-1, 1) : Random.Range(0, 2);

                    Vector2 newMoveDirection = new Vector2(newX, newY).normalized;

                    // do a pause
                    yield return StartCoroutine(TakePause());

                    // Redo the movement
                    StartCoroutine(DoMove(newMoveDirection));

                    yield break;
                }
            }

            // (b) Move in the random direction
            rigidbody.MovePosition(rigidbody.position + direction * moveSpeed * Time.fixedDeltaTime);

            yield return null;
        }

        // Player is always detected
        hasAction = false;
    }

    private IEnumerator DoAttack()
    {
        hasAction = true;

        float attackTime = GetRandomAttackTime();

        animator.SetTrigger("Attack");
        animator.SetBool("Attacking", true);

        float timestamp = Time.time;
        isInvincible = true;

        bool needsDirection = true;
        float timeUntilNewDirection = 0;
        Vector2 direction = Vector2.zero;

        while (Time.time - timestamp <= attackTime)
        {
            timeUntilNewDirection -= Time.deltaTime;

            if (timeUntilNewDirection <= 0) needsDirection = true;
            else
            {
                List<RaycastHit2D> results = new List<RaycastHit2D>();

                if (Physics2D.CapsuleCast(rigidbody.position, collider.size, collider.direction, 0, direction, contactFilter, results, 4.5f * moveSpeed * Time.fixedDeltaTime) > 0)
                {
                    // ignore collisions with oneself
                    for (int i = results.Count - 1; i >= 0; i--)
                    {
                        if (results[i].collider == collider) results.RemoveAt(i);
                    }

                    if (results.Count > 0) needsDirection = true;

                }
            }

            if (needsDirection)
            {
                direction = (player.transform.position - transform.position).normalized;

                needsDirection = false;
                timeUntilNewDirection = attackTime / 3f;
            }

            // move towards player
            rigidbody.MovePosition(rigidbody.position + 4 * moveSpeed * Time.fixedDeltaTime * direction);

            yield return null;
        }

        isInvincible = false;
        animator.SetBool("Attacking", false);

        StartCoroutine(TakePause());
    }

    private IEnumerator TakePause()
    {
        animator.SetBool("Moving", false);

        float timestamp = Time.time;

        while (Time.time - timestamp <= pauseTime) yield return null;

        hasAction = false;
    }

    private IEnumerator FlashWhiteOnHeal()
    {
        float flashAmount = 0f;

        bool increasingValue = true;

        do
        {
            flashAmount += (1f / whiteFlashTime) * Time.deltaTime * (increasingValue ? 1 : -1);

            if (increasingValue && flashAmount >= 1f) increasingValue = false;

            sprite.material.SetFloat("_FlashAmount", flashAmount);
            
            yield return null;
        } while (flashAmount > 0.01f);

    }

    private float GetRandomMoveTime()
    {
        return Random.Range(minRandomMoveTime, maxRandomMoveTime);
    }

    private float GetRandomAttackTime()
    {
        return Random.Range(minAttackTime, maxAttackTime);
    }
}
