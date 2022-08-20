using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponController : MonoBehaviour
{
    public float damageIncreasePerLeveulUp = 0.5f;

    [Tooltip("The hitbox for the melee attack")]
    public CapsuleCollider2D meleeHitBox;
    public LayerMask meleeLayerMask;

    public Damageable.DamageTypes damageType;
    public int baseDamage = 1;
    private float damage;

    [Header("Melee Hitbox Offsets")]
    public float nMeleeYOffset;
    public float sMeleeYOffset;

    [Header("Melee Hitbox out of bounds offsets")]
    public float allowableXOffsetNorth;
    public float allowableYOffsetNorth;
    public float allowableXOffsetSouth;
    public float allowableYOffsetSouth;

    [Header("Audio")]
    public AudioClip meleeMissSound;

    protected Animator animator;
    protected Rigidbody2D rb;
    protected SpriteRenderer sprite;

    protected Vector2 lookDirection;

    private AudioSource audioSource;

    protected bool loadedData = false;

    protected static readonly int AnimatorAttackingID = Animator.StringToHash("Attacking");

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        animator.SetBool("Can Move", true);

        CalculateAttackDamageFromLevel();
        XPManager.OnLevelUp += CalculateAttackDamageFromLevel;

        meleeHitBox.enabled = false;

        UIHealthBar.instance.DisableMagicBar();
        SpellHUD.instance.PlayerCantUseMagic();
    }

    private void OnDestroy()
    {
        XPManager.OnLevelUp -= CalculateAttackDamageFromLevel;
    }

    protected void CalculateAttackDamageFromLevel()
    {
        damage = baseDamage + damageIncreasePerLeveulUp * XPManager.GetAttackLevelUps();
    }

    public abstract void Attack(Vector2 lookDirection);

    public abstract void SpecialAttack(Vector2 lookDirection);

    public virtual void DoMeleeHitCheck()
    {
        RepositionMeleeHitBox();

        // The position of the hitbox is the position of the game object + the offset of the hitbox        
        Vector2 hitboxPosition = new Vector2(meleeHitBox.transform.position.x, meleeHitBox.transform.position.y);
        hitboxPosition += meleeHitBox.offset;

        // Get all colliders that overlap the hitbox
        Collider2D[] results = Physics2D.OverlapCapsuleAll(hitboxPosition, meleeHitBox.size, meleeHitBox.direction, 0f, meleeLayerMask);

        // Get all the damageables in the overlap capsule that should be hit
        List<Damageable> hitDamageables = new List<Damageable>();
        foreach (var collider in results)
        {
            if (collider.TryGetComponent(out Damageable damageable))
            {
                // Check to make sure hit damageable is not out of bounds
                if (lookDirection.x >= 0)
                {
                    if (lookDirection.y > 0) // NE 
                    {
                     //   print("NE Attack");

                        Debug.DrawLine(transform.position + Vector3.up * allowableYOffsetNorth - Vector3.right, transform.position + Vector3.up * allowableYOffsetNorth + Vector3.right, Color.red, 5f); //horizontal line
                        Debug.DrawLine(transform.position + Vector3.right * allowableXOffsetNorth, transform.position + Vector3.right * allowableXOffsetNorth + Vector3.up * 2f, Color.red, 5f); // vertical line

                        // Get the ne corner of the hit damageable
                        Vector2 neCorner = collider.bounds.center;
                        neCorner.y += collider.bounds.extents.y;
                        neCorner.x += collider.bounds.extents.x;

                        // and the ne corner of the out of bounds box
                        Vector2 offsetIntersection = transform.position;
                        offsetIntersection.x += allowableXOffsetNorth;
                        offsetIntersection.y += allowableYOffsetNorth; ;

                        // and check if the hit damageable is fully inside the out of bounds box
                        if (neCorner.x < offsetIntersection.x && neCorner.y < offsetIntersection.y) { print("attack missed"); continue; }
                    }
                    else // SE
                    {
                     //   print("SE Attack");

                        Debug.DrawLine(transform.position + Vector3.up * allowableYOffsetSouth - Vector3.right, transform.position + Vector3.up * allowableYOffsetSouth + Vector3.right, Color.red, 5f);
                        Debug.DrawLine(transform.position + Vector3.right * allowableXOffsetSouth, transform.position + Vector3.right * allowableXOffsetSouth + Vector3.up * 2f, Color.red, 5f);

                        Vector2 seCorner = collider.bounds.center;
                        seCorner.y -= collider.bounds.extents.y;
                        seCorner.x += collider.bounds.extents.x;

                        Vector2 offsetIntersection = transform.position;
                        offsetIntersection.x += allowableXOffsetSouth;
                        offsetIntersection.y += allowableYOffsetSouth;

                        if (seCorner.x < offsetIntersection.x && seCorner.y > offsetIntersection.y) { print("attack missed"); continue; }
                    }
                }
                else
                {
                    if (lookDirection.y > 0) // NW
                    {
                      //  print("NW attack");

                        Debug.DrawLine(transform.position + Vector3.up * allowableYOffsetNorth - Vector3.right, transform.position + Vector3.up * allowableYOffsetNorth + Vector3.right, Color.red, 5f);
                        Debug.DrawLine(transform.position + Vector3.right * -allowableXOffsetNorth, transform.position + Vector3.right * -allowableXOffsetNorth + Vector3.up * 2f, Color.red, 5f);

                        Vector2 nwCorner = collider.bounds.center;
                        nwCorner.x -= collider.bounds.extents.x;
                        nwCorner.y += collider.bounds.extents.y;

                        Vector2 offsetIntersection = transform.position;
                        offsetIntersection.x -= allowableXOffsetNorth;
                        offsetIntersection.y += allowableYOffsetNorth;

                        if (nwCorner.x > offsetIntersection.x && nwCorner.y < offsetIntersection.y) { print("attack missed"); continue; }
                    }
                    else // SW
                    {
                       // print("SW attack");

                        Debug.DrawLine(transform.position + Vector3.up * allowableYOffsetSouth - Vector3.right, transform.position + Vector3.up * allowableYOffsetSouth + Vector3.right, Color.red, 5f);
                        Debug.DrawLine(transform.position + Vector3.right * -allowableXOffsetSouth, transform.position + Vector3.right * -allowableXOffsetSouth + Vector3.up * 2f, Color.red, 5f);

                        Vector2 swCorner = collider.bounds.center;
                        swCorner.x -= collider.bounds.extents.x;
                        swCorner.y -= collider.bounds.extents.y;

                        Vector2 offsetIntersection = transform.position;
                        offsetIntersection.x -= allowableXOffsetSouth;
                        offsetIntersection.y += allowableYOffsetSouth;

                        if (swCorner.x > offsetIntersection.x && swCorner.y > offsetIntersection.y) { print("attack missed"); continue; }
                    }
                }

                // Add the damageable to the list if it was a good hit
                hitDamageables.Add(damageable);
            }
        }

        if (hitDamageables.Count == 0) PlaySound(meleeMissSound);
        else { HandleMeleeHits(hitDamageables); PlaySound(meleeMissSound); }
    }

    public virtual void ShootBullet()
    {
        // An empty function in case a unit has no ranged weapon, but here for the animator behavior
    }

    public virtual bool CanAttack()
    {
        return !animator.GetBool(AnimatorAttackingID);
    }

    protected void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    protected void HandleMeleeHits(List<Damageable> hitDamageables)
    {        
        foreach (var damageable in hitDamageables)
        {
            //print("melee hit " + damageable.gameObject);

            // Prevent a damage receiver from being hit by the same swing as the actual damageable
            if (damageable is DamageReceiver)
            {
                if (!hitDamageables.Contains((damageable as DamageReceiver).objectToDamage)) damageable.ApplyDamage(damage, damageType);
            }
            else damageable.ApplyDamage(damage, damageType);

            if (damageable.GetWeaponHitSound(damageType) == null) Debug.Log(damageable.name + " has no hit sound for damage type of " + damageType, damageable);

            // Will play multiple sounds if multiple enemies hit - see if this sounds good or if only last hit enemy should play sound
            PlaySound(damageable.GetWeaponHitSound(damageType));
        }
    }

    protected void RepositionMeleeHitBox()
    {
        Vector2 offset = meleeHitBox.offset;
        offset.x = Mathf.Abs(offset.x) * (sprite.flipX ? -1f : 1f); // Flip the x-offset to the left side if sprite faces left

        // Calculate the new y-offset of the hitbox based on facing north/south
        if (lookDirection.y > 0.01f) offset.y = nMeleeYOffset;
        else offset.y = sMeleeYOffset;

        // Set the new offset to the hitbox
        meleeHitBox.offset = offset;
    }

    protected IEnumerator DisableMovementDuringAttack()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        while (animator.GetBool("Attacking")) yield return null;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public abstract void LoadPlayerData(PlayerLoadData data);

    public abstract PlayerLoadData GetPlayerLoadData();

    public abstract void RestAtShrine();
}
