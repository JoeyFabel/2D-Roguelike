using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctonidBoss : Boss
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float minMoveTime = 0.5f;
    public float maxMoveTime = 2f;
    public ContactFilter2D contactFilter;

    private bool hasAction;

    private new CircleCollider2D collider;

    protected override void Start()
    {
        base.Start();

        collider = GetComponent<CircleCollider2D>();

        DoMovement();
    }

    public void DoMovement()
    {
        hasAction = true;

        // Determine the length and direction of the movement
        float moveTime = Random.Range(minMoveTime, maxMoveTime);

        int moveDirection = Random.Range(0, 4);
        Vector2 movementVector;

        switch (moveDirection)
        {
            case 0: movementVector = Vector2.right;
                break;
            case 1: movementVector = Vector2.down;
                break;
            case 2: movementVector = Vector2.left; 
                break;
            case 3: movementVector = Vector2.up; 
                break;
            default: movementVector = Vector2.right;
                break;
        }

        StartCoroutine(Move(movementVector, moveTime));
    }

    private IEnumerator Move(Vector2 direction, float moveTime)
    {
        float timestamp = Time.time;
        animator.SetBool("Moving", true);

        while (Time.time - timestamp <= moveTime)
        {
            // See if the movement will hit a wall
            List<RaycastHit2D> results = new List<RaycastHit2D>();

            /*

            // If there are going to be any collisions, check and see what is going to be hit
            if (Physics2D.CircleCast(collider.bounds.center, collider.radius + 0.1f, direction, contactFilter, results, moveSpeed * Time.fixedDeltaTime) > 0) // Stack overflow here?
            {
                // ignore collisions with oneself
                for (int i = results.Count - 1; i >= 0; i--)
                {
                    if (results[i].collider == collider) results.RemoveAt(i);
                }

                // If there are collisions with another object, move in a different direction for the remaining time
                if (results.Count > 0)
                {
                    // Get a list with the three movement directions that are not being used
                    List<Vector2> potentialDirections = new List<Vector2> { Vector2.right, Vector2.down, Vector2.left, Vector2.up };
                    potentialDirections.Remove(direction);

                    // Move in one of those directions, randomly picked, for the remaining time
                    ContinueMovement(potentialDirections[Random.Range(0, 3)], moveTime - (Time.time - timestamp)));

                    // Stop the current movement instance
                    yield break;
                }
            }

            */

            // Do the movement
            rigidbody.MovePosition(rigidbody.position + moveSpeed * Time.fixedDeltaTime * direction);
            animator.SetFloat("Movement X", direction.x);
            animator.SetFloat("Movement Y", direction.y);

            // yield for the next frame and decrease the timer
           // moveTime -= Time.fixedDeltaTime;
            yield return null;
        }

        animator.SetBool("Moving", false);

        yield return new WaitForSeconds(3f); // remove this

        hasAction = false;

        DoMovement();
    }

    private void ContinueMovement(Vector2 direction, float moveTime)
    {
        StartCoroutine(Move(direction, moveTime));
    }
}
