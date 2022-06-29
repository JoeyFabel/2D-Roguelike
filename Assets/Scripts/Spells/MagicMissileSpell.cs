using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissileSpell : Projectile
{
    public float enemyCheckRadius = 5f;
    public LayerMask enemyCheckLayerMask;

    public float moveSpeed = 2f;
    public float launchForceIfNoTarget = 150f;
    [Tooltip("The percentage of homing that is applied on every move, with 0.0 being no homing and 1.0 being pure homing")]
    public float homingPercentage = 1.0f;

    private float birthTime;

    //private Collider2D target;
    private Collider2D target;
    
    private Vector2 lookDirection;

    // WARNING - If homing doesnt work, make sure rotateWithVelocity is disabled

    public override void OnCast(Vector2 lookDirection, GameObject caster)
    {
        this.lookDirection = lookDirection;

        birthTime = Time.time;

        FindTarget(caster.transform.position);

        StartCoroutine(DetonateAfterLifetime());
        if (target) StartCoroutine(SeekTarget());
        else
        {
            rotateWithVelocity = true;
            Launch(lookDirection, launchForceIfNoTarget);
        }
    }


    private void FindTarget(Vector3 currentPosition)
    {
        Collider2D bestTarget = null;
        float closestDistanceSqr = 100f;        

        // TODO - prioritize enemies over damageable objects later
        Collider2D[] collidersInRange = Physics2D.OverlapCircleAll(transform.position, enemyCheckRadius, enemyCheckLayerMask);

        List<Damageable> damageables = new List<Damageable>();

        foreach (var collider in collidersInRange)
        {
            if (collider.TryGetComponent(out Damageable foundDamageable)) damageables.Add(foundDamageable);
        }

        // If the target is more than 90 in either direction away from look direction, don't count it.

        if (damageables.Count == 0)
        {
            target = null;
            return;
        }

        List<Enemy> enemyTargets = new List<Enemy>();

        foreach (var potentialTarget in damageables)
        {
            // See if any of the damageables are Enemies. If they are, add them to this list.
            if (potentialTarget is Enemy) enemyTargets.Add(potentialTarget as Enemy);
        }

        // TODO - sort out enemies and damageables behind player
        Debug.LogWarning("TODO - Sort out enemies and damageables behind the player");

        if (enemyTargets.Count > 0)
        {
            foreach (var potentialTarget in enemyTargets)
            {
                Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;

                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget.GetComponent<Collider2D>();
                }
            }
        }
        else // Find the closest damageable if no enemies are available
        {
            foreach (var potentialTarget in damageables)
            {
                Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;

                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget.GetComponent<Collider2D>();
                }
            }
        }

        target = bestTarget;
    }

    public float rotationSpeed = 1f;

    private IEnumerator SeekTarget()
    {
        print("Missile seeking " + target.name);
        Vector2 playerToTarget = (Vector2)target.bounds.center - rb.position;
        rb.SetRotation(Mathf.Atan2(playerToTarget.y, playerToTarget.x) * Mathf.Rad2Deg);
        //rb.position += spellLaunchSpeed * Time.fixedDeltaTime * playerToTarget.normalized;


        while (!impacted)
        {
            //Vector2 direction = (Vector2)target.transform.position - rb.position;
            Vector2 direction = (Vector2)target.bounds.center - rb.position;
            direction.Normalize();

            float rotateAmount = Vector3.Cross(direction, transform.up).z;
            //print("rotation amount: " + rotateAmount);

            rb.angularVelocity = -rotateAmount * rotationSpeed;
            rb.velocity = transform.up * moveSpeed;
            yield return null;
        }
    }

    private IEnumerator DetonateAfterLifetime()
    {
        yield return null;

        while (!impacted)
        {
            if (Time.time >= birthTime + lifetime)
            {
                StopAllCoroutines();

                audioSource.PlayOneShot(defaultImpactSound);

                StartCoroutine(MissileImpact());
            }
            
            yield return null;
        }
    }

    protected override void OnImpact(AudioClip impactSound)
    {
        impacted = true;

        audioSource.PlayOneShot(impactSound);

        StartCoroutine(MissileImpact());
    }

    private IEnumerator MissileImpact()
    {
        impacted = true;
        rb.simulated = false;

        var animator = GetComponent<Animator>();

        animator.SetTrigger("Explode");        

        while (audioSource.isPlaying || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f) yield return null;

        Destroy(gameObject);
    }
}
