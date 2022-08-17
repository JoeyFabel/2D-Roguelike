using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilRose : Enemy
{
    public float hideInShellTime = 3f;
    
    public GameObject thornProjectilePrefab;
    public float launchSpreadAngle;
    public float launchSpeed;
    private const int NumProjectilesPerLaunch = 3;
    
    private static readonly int AnimatorDamageTriggerID = Animator.StringToHash("Death");

    private bool hidingInShell = false;
    private bool isInvincible = false;
    
    public override void ApplyDamage(float amount)
    {
        if (isInvincible) return;

        if (!hidingInShell) StartCoroutine(HideInShell());
        else Death();
    }

    private IEnumerator HideInShell()
    {
        // Set the hiding trigger
        animator.SetTrigger(AnimatorDamageTriggerID);

        // Start hiding, but be invincible while hiding animation plays
        isInvincible = true;
        
        yield return null;
        
        print("transitioning into retract anim");
        
        // transitioning into Rose-Retract state
        while (animator.IsInTransition(0)) yield return null;

        isInvincible = false;
        hidingInShell = true;

        print("now retracted");
        
        // Wait until the transition to the sprout animation is complete
        while (!animator.IsInTransition(0)) yield return null;

        print("un-retracting");
        
        // The rose is now sprouting
        hidingInShell = false;
    }

    protected override void Death()
    {
        // animator.SetTrigger(AnimatorDamageTriggerID);
        
        Destroy(gameObject);
        
        // Maybe sprout back up later
    }

    public void FireThornMissiles(ShootDirection direction)
    {
        print("launching thorns in direction " + direction);
        
        Projectile[] launchedThorns = new Projectile[3];
        Vector3 launchPosition;
        
        // Rotations on Z-Axis go counter-clockwise as the angle increases
        if (direction == ShootDirection.Up)
        {
            launchPosition = transform.position;
            launchPosition.y += 1.15f;
                
            // change this to the launch position
            launchedThorns[0] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.identity)
                .GetComponent<Projectile>();

            launchedThorns[1] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, launchSpreadAngle))
                    .GetComponent<Projectile>();

            launchedThorns[2] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, -launchSpreadAngle))
                .GetComponent<Projectile>();
        }
        else if (direction == ShootDirection.Left)
        {
            launchPosition = transform.position;
            launchPosition.y += 0.55f;
            launchPosition.x -= 0.3f;
            
            // change this to the launch position
            launchedThorns[0] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, 90f))
                .GetComponent<Projectile>();

            launchedThorns[1] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, 90f + launchSpreadAngle))
                .GetComponent<Projectile>();

            launchedThorns[2] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, 90f - launchSpreadAngle))
                .GetComponent<Projectile>();
        }
        else if (direction == ShootDirection.Down)
        {
            launchPosition = transform.position;
            launchPosition.y += 0.8f;
            
            // change this to the launch position
            launchedThorns[0] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, 180f))
                .GetComponent<Projectile>();

            launchedThorns[1] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, 180f + launchSpreadAngle))
                .GetComponent<Projectile>();

            launchedThorns[2] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, 180f - launchSpreadAngle))
                .GetComponent<Projectile>();
        }
        else if (direction == ShootDirection.Right)
        {
            launchPosition = transform.position;
            launchPosition.y += 0.55f;
            launchPosition.x += 0.3f;
            
            // change this to the launch position
            launchedThorns[0] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, 270f))
                .GetComponent<Projectile>();

            launchedThorns[1] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, 270f + launchSpreadAngle))
                .GetComponent<Projectile>();

            launchedThorns[2] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, 270f - launchSpreadAngle))
                .GetComponent<Projectile>();
        }
        
        foreach (var projectile in launchedThorns) projectile.Launch(projectile.transform.up, launchSpeed);
    }

    public enum ShootDirection
    {
        Up,
        Right,
        Down,
        Left
    }
}
