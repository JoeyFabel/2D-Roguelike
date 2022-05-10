using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainMole : Enemy
{
    [Header("Brain Mole Stuff")]

    [Header("Movement")]
    public float moveSpeed = 1f;

    [Tooltip("How long the slime pauses after an attack or random movement")]
    public float pauseTime = 2f;

    public float minRandomMoveTime = 1.5f;
    public float maxRandomMoveTime = 3f;

    public ContactFilter2D contactFilter;

    private bool hasAction;
    private bool playerDetected;

    private new BoxCollider2D collider;

    protected override void Start()
    {
        base.Start();

        collider = GetComponent<BoxCollider2D>();

        hasAction = false;
        playerDetected = false;
    }

    private void Update()
    {
        if (!hasAction)
        {
            if (playerDetected)
            {

            }
            else
            {
                StartCoroutine(DoMove(Vector2.zero));
            }
        }
    }

    protected override void Death()
    {
        animator.SetTrigger("Death");

        audioSource.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)]);

        StartCoroutine(DestroyAfterAudio());
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

            if (Physics2D.BoxCast(rigidbody.position, collider.size, 0, direction, contactFilter,  results, moveSpeed * Time.fixedDeltaTime) > 0)

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

    private float GetRandomMoveTime()
    {
        return Random.Range(minRandomMoveTime, maxRandomMoveTime);
    }
}
