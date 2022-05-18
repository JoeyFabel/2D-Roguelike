using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainMole : Enemy
{
    [Header("Movement")] // these appear in flipped order
    [Header("Brain Mole Stuff")]

    public float moveSpeed = 1f;

    [Tooltip("How long the slime pauses after an attack or random movement")]
    public float pauseTime = 2f;

    public float minRandomMoveTime = 1.5f;
    public float maxRandomMoveTime = 3f;

    public ContactFilter2D contactFilter;

    [Header("Attack")]
    public AudioClip attackSound;
    public float maxDistanceForAttack = 0.3f;
    public float waitTimeAfterAttack = 0.5f;

    [Header("Detection Values")]
    public float forwardDetectionDistance = 3f;
    public float rearDetectionDistance = 1f;
    public float minDistanceForDetectionLoss = 3f;

    private bool hasAction;
    private bool playerDetected;
    private bool touchingPlayer;

    private new CapsuleCollider2D collider;
    private PlayerController player;

    protected override void Start()
    {
        base.Start();

        collider = GetComponent<CapsuleCollider2D>();
        player = CharacterSelector.GetPlayerController();

        hasAction = false;
        playerDetected = false;
        touchingPlayer = false;
    }

    private void Update()
    {
        CheckForPlayer();

        if (!hasAction)
        {
            if (playerDetected)
            {
                if (touchingPlayer || (player.transform.position - gameObject.transform.position).magnitude <= maxDistanceForAttack)
                {
                    StartCoroutine(Attack());
                }
                else StartCoroutine(MoveTowardsPlayer(GetRandomMoveTime()));
            }
            else
            {
                StartCoroutine(DoMove(Vector2.zero));
            }
        }
    }

    protected override void Death()
    {
        hasAction = true;
        StopAllCoroutines();

        animator.SetTrigger("Death");

        audioSource.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)]);

        //StartCoroutine(DestroyAfterAudio());
        Destroy(gameObject, 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {       
        if (collision.gameObject.Equals(player.gameObject)) touchingPlayer = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.Equals(player.gameObject)) touchingPlayer = false;
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
    
    private IEnumerator Attack()
    {
        if (animator.GetBool("Attacking")) yield break;
        
        hasAction = true;

        if (!animator.GetBool("Attacking")) animator.SetTrigger("Attack");

        yield return null;

        while (animator.GetBool("Attacking")) yield return null;

        if (touchingPlayer)
        {
            player.TakeDamage(damage, damageType);
        }

        yield return new WaitForSeconds(waitTimeAfterAttack);

        hasAction = false;
    }

    private IEnumerator MoveTowardsPlayer(float moveTime)
    {
        hasAction = true;

        float timestamp = Time.time;

        while (Time.time < timestamp + moveTime && !animator.GetBool("Attacking"))
        {
            if (!touchingPlayer)
            {
                Vector2 towardsPlayer = (player.transform.position - transform.position).normalized;

                sprite.flipX = towardsPlayer.x < 0;

                rigidbody.MovePosition(rigidbody.position + moveSpeed * Time.fixedDeltaTime * towardsPlayer);
            }
            else
            {
                StartCoroutine(Attack());
                yield break;
            }

            yield return null;
        }

        hasAction = false;
    }

    /// <summary>
    /// Have the Brain Mole move in the specified direction. However, you can do a random direction if Vector2.zero is specified.
    /// </summary>
    /// <param name="moveDirection">The direction for the Brain Mole to move. Vector2.zero results in a random direction.</param>
    private IEnumerator DoMove(Vector2 moveDirection)
    {
        hasAction = true;

        // (1) Choose a random direction or use the specified direction
        Vector2 direction;

        if (moveDirection != Vector2.zero) direction = moveDirection.normalized;
        else direction = Random.insideUnitCircle.normalized;


        // (2) Move in that direction for the randomly determined time
        float timestamp = Time.time;
        float moveTime = GetRandomMoveTime();
        //animator.SetBool("Moving", true);

        sprite.flipX = direction.x < 0;

        while (Time.time - timestamp <= moveTime)
        {
            // (a) Make sure a wall will not be hit. If it will be hit, revert direction
            // Handle this with box cast bc collider is kinematic

            List<RaycastHit2D> results = new List<RaycastHit2D>();

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
            rigidbody.MovePosition(rigidbody.position + moveSpeed * Time.fixedDeltaTime * direction);

            yield return null;
        }

        // (3) Take a Pause if player is not detected
        if (!playerDetected) StartCoroutine(TakePause());
        else hasAction = false;
    }

    private IEnumerator TakePause()
    {
        // animator.SetBool("Moving", false);

        float timestamp = Time.time;

        while (Time.time - timestamp <= pauseTime) yield return null;

        hasAction = false;
    }

    private float GetRandomMoveTime()
    {
        return Random.Range(minRandomMoveTime, maxRandomMoveTime);
    }
}
