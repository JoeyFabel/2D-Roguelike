using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaurianSkirmisherWeapon : WeaponController
{
    // Melee Offsets
    public CapsuleCollider2D northSouthMeleeHitBox;

    public AudioClip spearThrowSound;

    public GameObject spearPrefab;

    public int maxThrowingSpears = 5;
    private int numThrowingSpears;

    public float throwSpeed = 150f;

    public Text spearQuantityText;

    protected override void Start()
    {
        base.Start();

        northSouthMeleeHitBox.enabled = false;

        if (!loadedData) numThrowingSpears = maxThrowingSpears;

        spearQuantityText.text = "X " + numThrowingSpears;
    }

    public override void Attack(Vector2 lookDirection)
    {
        this.lookDirection = lookDirection;

        if (CanAttack()) Melee();
    }

    public override void SpecialAttack(Vector2 lookDirection)
    {
        this.lookDirection = lookDirection;

        if (CanAttack() && numThrowingSpears > 0)
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

    public override void DoMeleeHitCheck()
    {
        // S flip x offset: x * -1

        // north hitbox = south hitbox with x offset for flipped, -x offset for normal, y offset as specified by parameter
        // southeast: rotation 45, offset 0.1774674, -0.75
        // northeast = southeast with rotation of 130 around z
        // southeast default rotation = 45, offset of 0.1774674, -0.65
        // southwest = southeast with rotatino = 310, offset = -0.1774674, -0.75
        // norhtwest = southeast with rotation 235, offset of -0.1774674, -0.79

        CapsuleCollider2D overlapCollider;

        // Reposition Melee HitBox;
        if (lookDirection == Vector2.up)
        {
            print("melee attack north");

            var offset = northSouthMeleeHitBox.offset;
            offset.x = Mathf.Abs(offset.x) * (sprite.flipX ? 1 : -1f);
            offset.y = nMeleeYOffset;
            northSouthMeleeHitBox.offset = offset;

            overlapCollider = northSouthMeleeHitBox;
        }
        else if (lookDirection == Vector2.down)
        {
            print("melee attack south");

            var offset = northSouthMeleeHitBox.offset;
            offset.x = Mathf.Abs(offset.x) * (sprite.flipX ? -1f : 1f);
            offset.y = sMeleeYOffset;
            northSouthMeleeHitBox.offset = offset;

            overlapCollider = northSouthMeleeHitBox;
        }
        else if (lookDirection.y > 0)
        {
            if (lookDirection.x >= 0)
            {
                print("melee attack ne");

                var offset = new Vector2(0.1774674f, -0.7f);
                meleeHitBox.offset = offset;
                meleeHitBox.transform.rotation = Quaternion.Euler(0, 0, 130);

                overlapCollider = meleeHitBox;
            }
            else
            {
                print("melee attack nw");

                var offset = new Vector2(-0.1774674f, -0.79f);
                meleeHitBox.offset = offset;
                meleeHitBox.transform.rotation = Quaternion.Euler(0, 0, 235);

                overlapCollider = meleeHitBox;
            }
        }
        else
        {
            if (lookDirection.x >= 0)
            {
                print("melee attack se");

                var offset = new Vector2(0.1774674f, -0.65f);
                meleeHitBox.offset = offset;
                meleeHitBox.transform.rotation = Quaternion.Euler(0, 0, 45);

                overlapCollider = meleeHitBox;
            }
            else
            {
                print("melee attack sw");

                var offset = new Vector2(-0.1774674f, -0.75f);
                meleeHitBox.offset = offset;
                meleeHitBox.transform.rotation = Quaternion.Euler(0, 0, 310);

                overlapCollider = meleeHitBox;
            }
        }

        // The position of the hitbox is the position of the game object + the offset of the hitbox        
        overlapCollider.enabled = true;
        Vector2 hitboxPosition = new Vector2(overlapCollider.bounds.center.x, overlapCollider.bounds.center.y);
        overlapCollider.enabled = false;
        //hitboxPosition += overlapCollider.offset;

        //Debug.DrawRay(hitboxPosition, Vector2.right, Color.red, 3f);
        //Debug.DrawRay(hitboxPosition, Vector2.up, Color.red, 3f);                       

        Collider2D[] results = Physics2D.OverlapCapsuleAll(hitboxPosition, overlapCollider.size, overlapCollider.direction, overlapCollider.transform.rotation.eulerAngles.z, meleeLayerMask);
        //Collider2D results = Physics2D.OverlapBoxAll(hitbo)

        // Get all the damageables in the overlap capsule that should be hit
        List<Damageable> hitDamageables = new List<Damageable>();
        foreach (var collider in results)
        {
            print("hit " + collider);
            if (collider.TryGetComponent(out Damageable damageable)) hitDamageables.Add(damageable);
        }

        if (hitDamageables.Count == 0) PlaySound(meleeMissSound);
        else
        {
            HandleMeleeHits(hitDamageables); PlaySound(meleeMissSound);
        }
    }

    public override void ShootBullet()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector2 startPosition;
        Vector2 launchDirection = lookDirection;

        if (lookDirection == Vector2.up)
        {
            startPosition = new Vector2(0.0f, 1.6f);
        }
        else if (lookDirection == Vector2.down)
        {
            startPosition = new Vector2(0.7f, 0.7f);

            if (sprite.flipX) startPosition.x *= -1;
        }
        else if (lookDirection == Vector2.right || lookDirection == Vector2.left)
        {
            startPosition = new Vector2(0, 1f);
        }
        else if (lookDirection.y > 0)
        {
            if (!sprite.flipX)
            {
                startPosition = new Vector2(0.2f, 1.6f);
                //launchDirection = new Vector2(2, 1).normalized;
            }
            else
            {
                startPosition = new Vector2(-0.35f, 1.5f);
                //launchDirection = new Vector2(-2, 1).normalized;
            }            
        }
        else
        {
            if (!sprite.flipX)
            {
                startPosition = new Vector2(0.83f, 0.83f);
                //launchDirection = new Vector2(2, -1).normalized;
            }
            else
            {
                startPosition = new Vector2(-0.7f, 0.7f);
                //launchDirection = new Vector2(-2, -1).normalized;
            }
        }

        float angle = Mathf.Atan2(launchDirection.y, launchDirection.x) * Mathf.Rad2Deg - 90f;

        GameObject projectileObject = Instantiate(spearPrefab, rb.position + startPosition, Quaternion.AngleAxis(angle, Vector3.forward));

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        projectile.Launch(launchDirection, throwSpeed);

        // y start: -2.428767 : end -0.832

        // x start: 1.458176 : end 1.657

        PlaySound(spearThrowSound);

        numThrowingSpears--;
        spearQuantityText.text = "X " + numThrowingSpears;
    }

    public override void LoadPlayerData(PlayerLoadData data)
    {
        numThrowingSpears = data.numProjectiles;
        spearQuantityText.text = "X " + numThrowingSpears;

        loadedData = true;
    }

    public override PlayerLoadData GetPlayerLoadData()
    {
        PlayerLoadData data = new PlayerLoadData();

        data.numProjectiles = numThrowingSpears;
        data.currentMagic = 0;

        return data;
    }

    public override void RestAtShrine()
    {
        numThrowingSpears = maxThrowingSpears;
        spearQuantityText.text = "X " + numThrowingSpears;
    }
}
