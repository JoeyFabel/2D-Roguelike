using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DwarfThundererWeapon : WeaponController
{
    // Melee Offsets
    // north x: 0.3
    // north y: 1
    // south x: 0.35
    // south y: 0.5

    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;
    public int maxAmmo = 5;
    private int ammo;
    public float shootSpeed = 300f;

    public UnityEngine.UI.Text ammoText;

    [Header("Audio")]
    public AudioClip gunFireSound;

    protected override void Start()
    {
        base.Start();

        if (!loadedData) ammo = maxAmmo;
        ammoText.text = "X " + ammo;
    }

    public override void Attack(Vector2 lookDirection)
    {
        this.lookDirection = lookDirection;

        if (CanAttack()) Melee();
    }

    public override void SpecialAttack(Vector2 lookDirection)
    {
        this.lookDirection = lookDirection;

        if (CanAttack() && ammo > 0)
        {
            animator.SetTrigger("Ranged Attack");
            
            StartCoroutine(DisableMovementDuringAttack());
        }
    }

    private void Melee()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        animator.SetTrigger("Melee Attack");

        StartCoroutine(DisableMovementDuringAttack());
    }
  
    public override void ShootBullet()
    {        
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // NE - x = .3, y = 1.05
        // N - x = .105 y = 1.25
        // SE - x= .5, y= .5
        // S - x = -.075, y = 0
        Vector2 startingOffset = Vector2.zero;
        MuzzleFlash.direction muzzleFlashDirection = MuzzleFlash.direction.North;
        
        if (lookDirection == Vector2.up) // N
        {
            startingOffset = new Vector2(0.105f, 1.25f);
            startingOffset.x *= sprite.flipX ? -1 : 1;
            muzzleFlashDirection = MuzzleFlash.direction.North;
        }
        else if (lookDirection == Vector2.down) // S
        {
            startingOffset = new Vector2(-0.075f, 0f);
            startingOffset.x *= sprite.flipX ? -1 : 1;
            muzzleFlashDirection = MuzzleFlash.direction.South;
        }
        else if (lookDirection == Vector2.right)
        {
            startingOffset = new Vector2(0.5f, 0.6f - 0.4f);
            muzzleFlashDirection = MuzzleFlash.direction.SouthEast;
        }
        else if (lookDirection == Vector2.left)
        {
            startingOffset = new Vector2(-0.5f, 0.6f - 0.4f);
            muzzleFlashDirection = MuzzleFlash.direction.SouthWest;
        }
        else if (lookDirection.x >= 0)
        {
            if (lookDirection.y > 0)
            {
                startingOffset = new Vector2(0.3f + 0.2f, 1.05f - 0.2f); // NE
                muzzleFlashDirection = MuzzleFlash.direction.NorthEast;
            }
            else
            {
                startingOffset = new Vector2(0.5f, 0.5f - 0.55f); // SE
                muzzleFlashDirection = MuzzleFlash.direction.SouthEast;
            }
        }
        else
        {
            if (lookDirection.y > 0)
            {
                startingOffset = new Vector2(-0.3f - 0.2f, 1.05f - 0.2f); // NW
                muzzleFlashDirection = MuzzleFlash.direction.NorthWest;
            }
            else
            {
                startingOffset = new Vector2(-0.5f, 0.5f - 0.55f); // SW
                muzzleFlashDirection = MuzzleFlash.direction.SouthWest;
            }
        }

        GameObject projectileObject = Instantiate(bulletPrefab, rb.position + startingOffset, Quaternion.identity);        

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        Vector2 launchDirection = lookDirection;
        //if (launchDirection == Vector2.right) launchDirection = (Vector2.right + Vector2.down).normalized;
        // if (launchDirection == Vector2.left) launchDirection = (Vector2.left + Vector2.down).normalized;

        projectile.Launch(launchDirection, shootSpeed);

        MuzzleFlash muzzleFlash = Instantiate(muzzleFlashPrefab, rb.position + startingOffset, Quaternion.identity).GetComponent<MuzzleFlash>();

        muzzleFlash.CreateMuzzleFlash(muzzleFlashDirection, sprite.flipX);

        PlaySound(gunFireSound);

        //ammo--;
        ammoText.text = "X " + ammo;
    }

    public override void LoadPlayerData(PlayerLoadData data)
    {
        ammo = data.numProjectiles;
        ammoText.text = "X " + ammo;

        loadedData = true;
    }

    public override PlayerLoadData GetPlayerLoadData()
    {
        PlayerLoadData data = new PlayerLoadData();
        data.numProjectiles = ammo;
        data.currentMagic = 0;

        return data;
    }

    public override void RestAtShrine()
    {
        ammo = maxAmmo;
        ammoText.text = "X " + ammo;
    }
}
