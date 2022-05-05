using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : Damageable
{
    public GameObject[] createOnDestruction;    

    public override void ApplyDamage(float amount)
    {
        Death();
    }

    public override void ApplyDamage(float amount, DamageTypes damageType)
    {
        Death();
    }

    protected override void Death()
    {
        //AudioClip clipToPlay = deathSounds[Random.Range(0, deathSounds.Length - 1)];

        //audioSource.PlayOneShot(clipToPlay);

        // Doesnt need to play a sound as the player weapon will do the sound

        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        if (createOnDestruction.Length > 0)
        {
            foreach (var creation in createOnDestruction)
            {
                Instantiate(creation, rb.position, transform.rotation);
            }
        }

        GetComponent<SpriteRenderer>().enabled = false;

        Destroy(gameObject);
    }
}
