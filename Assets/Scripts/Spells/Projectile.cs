using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Spell
{
    public float lifetime = 4f;
    public float damage;
    public float spellLaunchSpeed = 200f;
    [Tooltip("Set the to 0 or less for no AoE to be used")]
    public float damageRadius = 0f;

    public bool rotateWithVelocity = false;

    public Damageable.DamageTypes damageType = Damageable.DamageTypes.Fire;

    public AudioClip defaultImpactSound;

    [Tooltip("Set to true for basic projectiles like arrows, but leave on false for things with special effects on detonation, like fireballs")]
    public bool destroyAfterLifetime = false;

    protected Rigidbody2D rb;
    protected AudioSource audioSource;

    protected bool impacted;

    private MapManager mapManager;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        mapManager = FindObjectOfType<MapManager>();

        impacted = false;

        transform.parent = null;

        if (rotateWithVelocity) StartCoroutine(RotateWithVelocity());
    }

    public override void OnCast(Vector2 lookDirection, GameObject player)
    {
        Launch(lookDirection, spellLaunchSpeed);
    }

    public void Launch(Vector2 direction, float force)
    {
        rb.AddForce(direction * force);


        if (rotateWithVelocity)
        {
            Vector2 velocity = direction.normalized;
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        if (destroyAfterLifetime) Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        AudioClip impactSound = defaultImpactSound;


        if (damageRadius <= 0)
        {
            if (collision.gameObject.TryGetComponent(out Damageable damageable))
            {
                //print("hit a " + damageable.gameObject);
                damageable.ApplyDamage(damage, damageType);
                impactSound = damageable.GetWeaponHitSound(damageType);
            }
            else if (collision.gameObject.TryGetComponent(out PlayerController player))
            {
                player.TakeDamage(damage, damageType);
                impactSound = defaultImpactSound;
            }
        }
        else
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(rb.position, damageRadius);

            List<Damageable> hitDamageables = new List<Damageable>();

            foreach (var hitCollider in hitColliders) if (hitCollider.TryGetComponent(out Damageable damagedObject)) hitDamageables.Add(damagedObject);

            foreach (var damageObject in hitDamageables) damageObject.ApplyDamage(damage, damageType);

            if (hitDamageables.Count > 0) impactSound = hitDamageables[0].GetWeaponHitSound(damageType);
        }

        /*
         else
        {
            // check for a height difference collision
            MapManager.TileCollisionData collisionData = mapManager.getTileCollisionData(rb.position);

            if (collisionData != null)
            {
                /*
                 * If moving upward, bottom side collision counts but not top side collision
                 * If moving downard, only top side collisions count
                 * If moving rightward, only left side collisions count
                 *  If moving leftward, only right side collisions count
                 */
        /*
                print("hit a " + collision.gameObject + ", " + collision.relativeVelocity);
                print(collisionData.ToString());

                bool hitSomething = false;

                Vector2 projectileVelocity = collision.relativeVelocity;
                projectileVelocity.x *= -1;
                projectileVelocity.y *= -1;

                // if moving rightward, check for rightSideCollision
                if (projectileVelocity.x > 0 && collisionData.rightSideCollision) hitSomething = true;
                if (projectileVelocity.x < 0 && collisionData.leftSideCollisions) hitSomething = true;
                if (projectileVelocity.y > 0 && collisionData.bottomSideCollisions) hitSomething = true;
                if (projectileVelocity.y < 0 && collisionData.topSideCollisions) hitSomething = true;

                if (!hitSomething || collisionData.noCollisions)
                {
                    print(this + " hit " + collision.gameObject.name + ", but the collision was ignored due to height differences");
                    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
                    hitHeightObject = collision.collider;

                    rb.isKinematic = true;

                    rb.velocity = projectileVelocity;

                    StartCoroutine(WaitForHeightColliderExit());
                    return;
                }
            }
        }
        */

        OnImpact(impactSound);
    }

    private IEnumerator WaitForHeightColliderExit()
    {
        // Phase 1 is while inside the height collider. ends when the other edge is reached, but not crossed
        while (mapManager.getTileCollisionData(rb.position) == null) yield return null;
        // Phase 2 continues until that tile is left, meaning a tile with no collision data is found
        while (mapManager.getTileCollisionData(rb.position) != null) yield return null;

        print("no longer in " + hitHeightObject.name);
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), hitHeightObject, false);

    }

    Collider2D hitHeightObject;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.Equals(hitHeightObject))
        {
            print("exited collision with " + collision.gameObject.name);
            GetComponent<Collider2D>().isTrigger = false;
        }
    }

    protected IEnumerator RotateWithVelocity()
    {
        while (!impacted)
        {
            // Rotate in direction of force
            Vector2 velocity = rb.velocity;
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            yield return null;
        }
    }

    protected virtual void OnImpact(AudioClip impactSound)
    {
        if (impactSound) audioSource.PlayOneShot(impactSound);
        
        StartCoroutine(DestroyAfterAudio());
    }

    protected IEnumerator DestroyAfterAudio()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        rb.simulated = false;

        while (audioSource.isPlaying) yield return null;

        Destroy(gameObject);
    }
}
