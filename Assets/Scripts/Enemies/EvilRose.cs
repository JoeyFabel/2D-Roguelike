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
        Projectile[] launchedThorns = new Projectile[3];
        Vector3 launchPosition = transform.position;
        float startingRotation = 0f;
        
        switch (direction)
        {
            // Rotations on Z-Axis go counter-clockwise as the angle increases
            case ShootDirection.Up:
                launchPosition.y += 1.15f;

                startingRotation = 0f;
                
                break;
            case ShootDirection.Left:
                launchPosition.y += 0.55f;
                launchPosition.x -= 0.3f;

                startingRotation = 90f;
                
                break;
            case ShootDirection.Down:
                launchPosition.y += 0.8f;

                startingRotation = 180f;
                
                break;
            case ShootDirection.Right:
                launchPosition.y += 0.55f;
                launchPosition.x += 0.3f;

                startingRotation = 270f;
                
                break;
        }
        
        launchedThorns[0] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, startingRotation))
            .GetComponent<Projectile>();

        launchedThorns[1] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, startingRotation + launchSpreadAngle))
            .GetComponent<Projectile>();

        launchedThorns[2] = Instantiate(thornProjectilePrefab, launchPosition, Quaternion.Euler(0f, 0f, startingRotation - launchSpreadAngle))
            .GetComponent<Projectile>();
        
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
