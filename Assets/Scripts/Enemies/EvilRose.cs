using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilRose : Enemy
{
    public float hideInShellTime = 3f;

    public float maxDistanceToShootThorns = 6f;
    public float thornFireRechargeTime = 3f;
    
    public GameObject thornProjectilePrefab;
    public float launchSpreadAngle;
    public float launchSpeed;
    private const int NumProjectilesPerLaunch = 3;
    
    private static readonly int AnimatorDamageTriggerID = Animator.StringToHash("Death");
    private static readonly int AnimatorShootTriggerID = Animator.StringToHash("Shoot Thorn");
    private static readonly int AnimatorToPlayerXID = Animator.StringToHash("To Player X");
    private static readonly int AnimatorToPlayerYID = Animator.StringToHash("To Player Y");
    
    private bool hidingInShell = false;
    private bool isInvincible = false;
    [SerializeField]
    private bool canShootThorns = true;
    
    private PlayerController player;

    protected override void Start()
    {
        base.Start();

        player = CharacterSelector.GetPlayerController();

        canShootThorns = true;
    }

    private void Update()
    {
        if (canShootThorns && Vector3.Distance(player.transform.position, transform.position) <= maxDistanceToShootThorns) ShootThorns();
    }

    /// <summary>
    /// Triggers the animator to shoot thorns towards the player.
    /// </summary>
    private void ShootThorns()
    {
        // Get the direction towards the player for the animator's use
        Vector2 toPlayer = (player.transform.position - transform.position);
        if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y)) toPlayer.y = 0;
        else toPlayer.x = 0;

        animator.SetFloat(AnimatorToPlayerXID, toPlayer.x);
        animator.SetFloat(AnimatorToPlayerYID, toPlayer.y);

        animator.SetTrigger(AnimatorShootTriggerID);

        StartCoroutine(RechargeThornAttack());
    }

    private IEnumerator RechargeThornAttack()
    {
        canShootThorns = false;

        yield return new WaitForSeconds(thornFireRechargeTime);

        canShootThorns = true;
    }

    public override void ApplyDamage(float amount)
    {
        if (isInvincible) return;

        if (!hidingInShell)
        {
            audioSource.PlayOneShot(damageSounds[0]);
            
            StartCoroutine(HideInShell());
        }
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
        
        audioSource.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)]);

        XPManager.GainXP(xpForDefeating, transform.position);
        if (Random.value <= moneyDropChance) Inventory.CreateMoneyDrop(moneyForDefeating, transform.position);
        
        audioSource.PlayOneShot(deathSounds[0]);

        sprite.enabled = false;
        enabled = false;
        
        Destroy(gameObject, 0.422f);
    }

    /// <summary>
    /// Creates and launches three thorn projectiles that fire in the specified direction.
    /// Called from an animator event.
    /// </summary>
    /// <param name="direction">The direction that the thorns should be fired in</param>
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
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDistanceToShootThorns);
    }
#endif
}
