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
    public int minMovesInAction = 1;
    public int maxMovesInAction = 3;

    private int remainingMovesInAction = -1;

    private new CircleCollider2D collider;

    protected override void Start()
    {
        base.Start();

        collider = GetComponent<CircleCollider2D>();

        DoMovement();
    }
    
    /// <summary>
    /// This method is the brain of the AI. It determines which course of action is the best move to take.
    /// </summary>
    private void ChooseAction()
    {
        DoMovement();
    }

    /// <summary>
    /// A Primary Action for the Octonid boss, it triggers a random number of moves
    /// </summary>
    private void DoMovement()
    {
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

        if (remainingMovesInAction <= 0) remainingMovesInAction = Random.Range(minMovesInAction, maxMovesInAction + 1);

        StartCoroutine(Move(movementVector, moveTime));
    }

    private IEnumerator Move(Vector2 direction, float moveTime)
    {
        float timestamp = Time.time;
        animator.SetBool("Moving", true);

        List<RaycastHit2D> results = new List<RaycastHit2D>();
        
        while (Time.time - timestamp <= moveTime)
        {
            // See if the movement will hit a wall, and if it does, change direction of movement

            results.Clear();
            Physics2D.CircleCast(collider.bounds.center, collider.radius + 0.1f, direction, contactFilter, results, moveSpeed * Time.fixedDeltaTime);

            for (int i = results.Count - 1; i>= 0; i--) if (results[i].collider.Equals(collider))
                {
                    results.RemoveAt(i);
                    break;
                }

            if (results.Count > 0)
            {
                print(results.Count + ": " + results[0].collider.name);

                List<Vector2> potentialDirections = new List<Vector2> { Vector2.right, Vector2.down, Vector2.left, Vector2.up };
                potentialDirections.Remove(direction);

                ContinueMovement(potentialDirections[Random.Range(0, 3)], moveTime - (Time.time - timestamp));

                yield break;
            }

            // Do the movement
            rigidbody.MovePosition(rigidbody.position + moveSpeed * Time.fixedDeltaTime * direction);
            animator.SetFloat("Movement X", direction.x);
            animator.SetFloat("Movement Y", direction.y);

            // yield for the next frame and decrease the timer
           // moveTime -= Time.fixedDeltaTime;
            yield return null;
        }

        animator.SetBool("Moving", false);

        if (remainingMovesInAction > 0) DoMovement();
        else
        {
            yield return new WaitForSeconds(3f); // remove this

            ChooseAction();
        }
    }

    private void ContinueMovement(Vector2 direction, float moveTime)
    {
        StartCoroutine(Move(direction, moveTime));
    }
}
