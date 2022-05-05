using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{
    [Header("Slime Stuff")]
    public bool startHidden;

    [Header("Movement")]
    // Movement
    public float moveSpeed = 1f;
    public ContactFilter2D contactFilter;
    public float hiddenColliderHeight = 0.2f;
    public Vector2 hiddenColliderOffset;

    [Header("Time stuff")]
    [Tooltip("The slime will chase the player for this long, before pausing.")]
    public float playerChaseTime;
    [Tooltip("How long the slime pauses after an attack or random movement")]
    public float pauseTime = 2f;

    public float minRandomMoveTime = 1.5f;
    public float maxRandomMoveTime = 3f;

    public float minAttackTime = 2f;
    public float maxAttackTime = 4f;
    public int numMovesBeforeAttack = 2;

    [Header("Detection Values")]
    public float forwardDetectionDistance = 3f;
    public float rearDetectionDistance = 1f;
    public float minDistanceForDetectionLoss = 3f;
    public float hidingDetectionDistance = 1.5f;

    private bool isInvincible;
    private bool playerDetected;
    private bool hasAction;

    private float regularColliderHeight;
    private float mainColliderOffset;
    private float secondaryColliderOffset;

    private Vector2 regularColliderOffset;

    private int remainingMovesBeforeAttack;

    new CapsuleCollider2D collider;
    CapsuleCollider2D secondaryCollider;

    PlayerController player;

    private bool started = false;

    private GiantSlime parentSlime;
    private int attacksBeforeReabsorption = 0;

    public new void Start()
    {
        if (started) return;

        base.Start();

        player = CharacterSelector.GetPlayerController();
        collider = GetComponent<CapsuleCollider2D>();
        secondaryCollider = transform.GetChild(0).GetComponent<CapsuleCollider2D>();

        mainColliderOffset = collider.offset.x;
        secondaryColliderOffset = secondaryCollider.offset.x;

        regularColliderHeight = collider.bounds.size.y;
        regularColliderOffset = collider.offset;
        parentSlime = null;

        isInvincible = false;
        hasAction = false;

        remainingMovesBeforeAttack = numMovesBeforeAttack;

        if (startHidden)
        {
            StartCoroutine(HideFromPlayer());
        }

        started = true;
    }

    private void Update()
    {
        if (!parentSlime) CheckForPlayer();

        if (!hasAction)
        {
            if (parentSlime && attacksBeforeReabsorption == 0) StartCoroutine(TryToMergeWithParent());

            else if (playerDetected)
            {
                // move towards player or attack
                if (remainingMovesBeforeAttack > 0)
                {
                    Vector2 towardsPlayer = (player.transform.position - transform.position).normalized;

                    remainingMovesBeforeAttack--;

                    StartCoroutine(DoMove(towardsPlayer));
                }
                else
                {
                    attacksBeforeReabsorption--;
                    remainingMovesBeforeAttack = numMovesBeforeAttack;
                    StartCoroutine(DoAttack());
                }                
            }
            else
            {
                // move randomly, pause randomly
                StartCoroutine(DoMove(Vector2.zero));
            }
        }

        collider.offset = new Vector2(mainColliderOffset * (sprite.flipX ? -1 : 1), collider.offset.y);
        secondaryCollider.offset = new Vector2(secondaryColliderOffset * (sprite.flipX ? -1 : 1), secondaryCollider.offset.y);
    }

    private void CheckForPlayer()
    {
        if (!playerDetected)
        {

            float detectionDistance;

            if (player.transform.position.x >= transform.position.x) // player rightward of slime
            {
                if (sprite.flipX) detectionDistance = rearDetectionDistance;
                else detectionDistance = forwardDetectionDistance;
            }
            else // player leftward of slime
            {
                if (sprite.flipX) detectionDistance = forwardDetectionDistance;
                else detectionDistance = rearDetectionDistance;
            }

            playerDetected = (player.transform.position - transform.position).magnitude <= detectionDistance;

            if (playerDetected) print(this + " saw the player");
        }
        else
        {
            if ((player.transform.position - transform.position).magnitude > minDistanceForDetectionLoss)
            {
                playerDetected = false;
                print(this + " lost track of the player");
            }
        }
    }

    public override void ApplyDamage(float amount)
    {
        if (isInvincible) return;

        base.ApplyDamage(amount);
    }

    public void Jump(Vector2 jumpEndPosition, float jumpTime)
    {
        StartCoroutine(HandleJump(jumpEndPosition, jumpTime));
    }

    public void SpawnFromGiantSlime(GiantSlime parentSlime, int attacksBeforeReabsorption, Vector2 jumpEndPosition, float jumpTime)
    {
        this.parentSlime = parentSlime;
        this.attacksBeforeReabsorption = attacksBeforeReabsorption;

        playerDetected = true;

        Jump(jumpEndPosition, jumpTime);
    }

    protected override void Death()
    {
        animator.SetTrigger("Death");

        audioSource.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)]);
        rigidbody.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        enabled = false;

        if (parentSlime) parentSlime.LoseSlime(this);

        StartCoroutine(DestroyAfterAudio());
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player) player.TakeDamage(damage, damageType);
        else if (parentSlime && attacksBeforeReabsorption <= 0 && collision.gameObject.Equals(parentSlime.gameObject)) parentSlime.ReabsorbSlime(this);
    }

    private IEnumerator TryToMergeWithParent()
    {
        hasAction = true;

        while (enabled)
        {
            //  yield return StartCoroutine(DoMove(parentSlime.transform.position - transform.position));

            Vector2 towardsParent = (parentSlime.transform.position - transform.position).normalized;
            towardsParent *= moveSpeed * Time.fixedDeltaTime;

            rigidbody.MovePosition(rigidbody.position + towardsParent);

            sprite.flipX = towardsParent.x < 0;

            RaycastHit2D[] hit = Physics2D.BoxCastAll(rigidbody.position, collider.size, 0, towardsParent, moveSpeed * Time.fixedDeltaTime);
            if (hit.Length > 0)
            {
                foreach (var hitCollider in hit)
                {
                    if (hitCollider.collider.gameObject.Equals(parentSlime.gameObject))
                    {
                        parentSlime.ReabsorbSlime(this);
                        yield break;
                    }
                }
            }

            yield return null;
        }
    }

    private IEnumerator HandleJump(Vector2 jumpDirection, float jumpTime)
    {
        hasAction = true;
        animator.SetBool("Falling", true);
        animator.SetTrigger("Fall");
        collider.enabled = false;
        secondaryCollider.enabled = false;

        float jumpHeight = 0.75f;
                        
        Vector2 endPosition = rigidbody.position + jumpDirection;

        Vector2 firstVector = endPosition - rigidbody.position;
        firstVector.x /= 2;
        firstVector.y += jumpHeight;

        Vector2 secondVector = endPosition - firstVector;

        float jumpDistance = firstVector.magnitude + secondVector.magnitude;

        float jumpMovementSpeed = jumpDirection.magnitude / jumpTime * 2;

        bool hitMaxHeight = false;

        while ((endPosition - rigidbody.position).magnitude >= jumpMovementSpeed * Time.deltaTime)
        {
            Vector2 jumpMovement;
            if (!hitMaxHeight) {
                Vector2 towardsTarget = endPosition - rigidbody.position;

                towardsTarget.x /= 2;
                towardsTarget.y += jumpHeight;

                jumpMovement = towardsTarget.normalized * jumpMovementSpeed * Time.deltaTime;                
            }
            else jumpMovement = (endPosition - rigidbody.position).normalized * jumpMovementSpeed * Time.deltaTime;

            sprite.flipX = jumpMovement.x < 0;

            rigidbody.MovePosition(rigidbody.position + jumpMovement);

            yield return null;

            if (rigidbody.position.y >= endPosition.y + jumpHeight) hitMaxHeight = true;
        }
        
        animator.SetBool("Falling", false);
        hasAction = false;
        collider.enabled = true;
        secondaryCollider.enabled = true;
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
            if (Physics2D.CapsuleCast(rigidbody.position, collider.size, collider.direction, 0, direction, contactFilter,  results, moveSpeed * Time.fixedDeltaTime) > 0)
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

        // (3) Take a Pause if player is not detected
        if (!playerDetected) StartCoroutine(TakePause());
        else hasAction = false;
    }

    private IEnumerator TakePause()
    {
        animator.SetBool("Moving", false);

        float timestamp = Time.time;

        while (Time.time - timestamp <= pauseTime) yield return null;

        hasAction = false;
    }

    private IEnumerator HideFromPlayer()
    {
        animator.SetBool("Hiding", true);
        hasAction = true;

        collider.size = new Vector2(collider.size.x, hiddenColliderHeight);
        collider.offset = hiddenColliderOffset;

        while ((player.transform.position - transform.position).magnitude > hidingDetectionDistance) yield return null;

        print("done hiding");
        collider.size = new Vector2(collider.size.x, regularColliderHeight);
        collider.offset = regularColliderOffset;
        playerDetected = true;

        animator.SetBool("Hiding", false);

        yield return null;
        while (animator.GetBool("Rising Up")) yield return null;

        hasAction = false;
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
