using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctonidBoss : Boss
{
    [Header("Prefabs")]
    public GameObject laserBeamPrefab;

    [Header("Audio")]
    public AudioClip laserFireSound;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float minMoveTime = 0.5f;
    public float maxMoveTime = 2f;
    public ContactFilter2D contactFilter;
    public int minMovesInAction = 1;
    public int maxMovesInAction = 3;

    [Header("Laser Attack")]
    public int laserDamage = 2;
    public float laserLaunchSpeed = 250f;
    public float laserAttackChance = 0.5f;
    public float laserAttackCooldown = 1f;
    public float chanceForExtraLaserAttack = 0.1f;
    public float maxLaserAngleFromCenter = 60f;

    [Header("Eye Closed Parameters")]
    public float eyeClosedMinTime = 2f;
    public float eyeClosedMaxTime = 5f;

    [SerializeField]
    private int remainingMovesInAction = -1;

    private new CapsuleCollider2D collider;
    private PlayerController player;

    private bool justMoved;
    private bool isInvincible;

#if UNITY_EDITOR    
    private string currentAction;

    [SerializeField]
    private string actionDisplayText = "Current Action: none";

    private void OnDrawGizmosSelected()
    {
        actionDisplayText = "Current Action: " + currentAction;
    }
#endif

    protected override void Start()
    {
        base.Start();

        Debug.LogWarning("TODO - Figure out a way to handle player/octonid collisions without pushing the player through a wall"); 

        isInvincible = false;

        player = CharacterSelector.GetPlayerController();
        collider = GetComponent<CapsuleCollider2D>();

    //    DoMovement();
    }    

    public void RespondToPlayer()
    {        
        DoMovement();
    }

    #region Overridden Damage Methods
    public override void ApplyDamage(float amount)
    {
        if (isInvincible) return;

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

    protected override void Death()
    {
        // Trigger death animation and vfx, but wait to give rewards until after

        print("octonid died!");
        rigidbody.simulated = false;
        collider.enabled = false;
       // enabled = false;

        // This cancels whatever action the boss might have been doing
        StopAllCoroutines();

        StartCoroutine(PlayDeathVFX());
    }

    private IEnumerator PlayDeathVFX()
    {
        animator.SetTrigger("Death");
        audioSource.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)]);

        print("in death coroutine!");

        // Wait until the death animation is over
        yield return new WaitForSeconds(0.833f);

        DropXPAndMoney();

        BossRoomTriggerOnDeath();

        Destroy(gameObject);
    }
    #endregion

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out PlayerController player))
        {
            // Note - This boss does not have a specified damage type
            player.TakeDamage(damage);
        }
    }

    /// <summary>
    /// This method is the brain of the AI. It determines which course of action is the best move to take.
    /// </summary>
    private void ChooseAction()
    {
        if (justMoved)
        {
            if (Random.value <= laserAttackChance) StartCoroutine(DoLaserAttack());
            else StartCoroutine(CloseEyeAction());

            justMoved = false;
        }
        else
        {
            DoMovement();
            justMoved = true;
        }
    }

    /// <summary>
    /// A Primary Action for the Octonid boss, it causes the octonid to close its eye and become invincible before choosing its next action.
    /// </summary>
    private IEnumerator CloseEyeAction()
    {
#if UNITY_EDITOR
        currentAction = "Close Eye Action";
#endif

        animator.SetBool("Eye Closed", true);

        yield return new WaitForSeconds(0.5f);

        isInvincible = true;

        yield return new WaitForSeconds(Random.Range(eyeClosedMinTime - 0.5f, eyeClosedMaxTime - 0.5f));

        animator.SetBool("Eye Closed", false);

        yield return new WaitForSeconds(0.5f);

        // At the end, choose between the move action and the laser attack action, but the odds of a laser attack are increase.
        float laserAttackOdds = laserAttackChance + (1f - laserAttackChance) / 2;

        if (Random.value <= laserAttackOdds) StartCoroutine(DoLaserAttack());
        else ChooseAction();
    }

    /// <summary>
    /// A Primary Action for the Octonid boss, the octonid shoots a laser out of its eye.
    /// </summary>
    private IEnumerator DoLaserAttack()
    {
#if UNITY_EDITOR
        currentAction = "Laser Attack";
#endif

        // Make the octonid face towards the player
        Vector2 toPlayer = player.transform.position - transform.position;

        if (Mathf.Abs(toPlayer.x) >= Mathf.Abs(toPlayer.y)) animator.SetFloat("Movement X", toPlayer.x);
        else animator.SetFloat("Movement Y", toPlayer.y);

        // Trigger the animation
        animator.SetTrigger("Laser Attack");

        // Shoot the laser at the appropriate time in the animation
        yield return new WaitForSeconds(0.375f);

        ShootLaser(toPlayer.x, toPlayer.y);

        yield return new WaitForSeconds(0.625f);

        // The animation is now over, wait for the attack cooldown to end
        yield return new WaitForSeconds(laserAttackCooldown);

        // There is a small chance for a second laser to be fired, otherwise go back to movement
        if (Random.value <= chanceForExtraLaserAttack) StartCoroutine(DoLaserAttack());
        else ChooseAction();
    }

    private void ShootLaser(float facingX, float facingY)
    {
        // Fire the laser at the player, but clamp it within the Octonid's facing direction

        Vector2 towardsPlayer = player.GetComponent<Collider2D>().bounds.center - transform.position;
        towardsPlayer.Normalize();

        // Get the direction that the octonid is facing
        Vector2 facingDirection = Vector2.zero;

        if (Mathf.Abs(facingX) > Mathf.Abs(facingY)) facingDirection = facingX > 0 ? Vector2.right : Vector2.left;
        else facingDirection = facingY > 0 ? Vector2.up : Vector2.down;

        // The laser should now be pointing straight downward
        Projectile laser = Instantiate(laserBeamPrefab, rigidbody.position, Quaternion.identity).GetComponent<Projectile>();
        laser.damage = laserDamage;

        /*
        // Now clamp the laser's fire angle within its bounds
        if (Vector2.Angle(facingDirection, towardsPlayer) > maxLaserAngleFromCenter)
        {
            print("the player is at an angle of " + Vector2.Angle(facingDirection, towardsPlayer) + " from the facing direction");

            Vector2 clampedVector = Vector2.zero;

            float actualAngle = maxLaserAngleFromCenter;

            if (facingDirection == Vector2.left) actualAngle += 180f;
            else if (facingDirection == Vector2.up) actualAngle += 90f;
            else if (facingDirection == Vector2.down) actualAngle += 270f;
                
            // get the vector from the angle - This should be from facing straight rightward
            clampedVector = new Vector2(Mathf.Cos(maxLaserAngleFromCenter * Mathf.Deg2Rad), Mathf.Sin(maxLaserAngleFromCenter * Mathf.Deg2Rad));

            towardsPlayer = clampedVector.normalized;
        } */

        laser.Launch(towardsPlayer, laserLaunchSpeed);
    }

    /// <summary>
    /// A Primary Action for the Octonid boss, it triggers a random number of moves
    /// </summary>
    private void DoMovement()
    {
#if UNITY_EDITOR
        currentAction = "Movement Action";
#endif

        // Determine the length and direction of the movement
        float moveTime = Random.Range(minMoveTime, maxMoveTime);

        int moveDirection = Random.Range(0, 4);
        Vector2 movementVector;

        switch (moveDirection)
        {
            case 0:
                movementVector = Vector2.right;
                break;
            case 1:
                movementVector = Vector2.down;
                break;
            case 2:
                movementVector = Vector2.left;
                break;
            case 3:
                movementVector = Vector2.up;
                break;
            default:
                movementVector = Vector2.right;
                break;
        }

        if (remainingMovesInAction <= 0) remainingMovesInAction = Random.Range(minMovesInAction, maxMovesInAction + 1);

        StartCoroutine(Move(movementVector, moveTime));
    }

    /// <summary>
    /// Sub-Action, or helper method for DoMovement. Actual movement is handled here.
    /// </summary>
    /// <param name="direction">The direction for the Octonid to move in</param>
    /// <param name="moveTime">The length of time that the Octonid should move</param>    
    private IEnumerator Move(Vector2 direction, float moveTime)
    {
        float timestamp = Time.time;
        animator.SetBool("Moving", true);

        while (Time.time - timestamp <= moveTime)
        {
            // See if the movement will hit a wall, and if it does, change direction of movement

            List<RaycastHit2D> results = new List<RaycastHit2D>();

            //if (Physics2D.CapsuleCast(collider.bounds.center, collider.radius + 0.1f, direction, contactFilter, results, moveSpeed * Time.fixedDeltaTime) > 0)
            if (Physics2D.CapsuleCast(rigidbody.position, collider.size, collider.direction, 0, direction, contactFilter, results, moveSpeed * Time.fixedDeltaTime) > 0)
            {
                for (int i = results.Count - 1; i >= 0; i--)
                {
                    if (results[i].collider.Equals(collider)) results.RemoveAt(i);
                }

                if (results.Count > 0)
                {
                    List<Vector2> potentialDirections = new List<Vector2> { Vector2.right, Vector2.down, Vector2.left, Vector2.up };
                    potentialDirections.Remove(direction);

                    StartCoroutine(Move(potentialDirections[Random.Range(0, 3)], moveTime - (Time.time - timestamp)));

                    yield break;
                }
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

        remainingMovesInAction--;

        if (remainingMovesInAction > 0) DoMovement();
        else
        {
            yield return new WaitForSeconds(3f); // remove this

            ChooseAction();
        }
    }

}
