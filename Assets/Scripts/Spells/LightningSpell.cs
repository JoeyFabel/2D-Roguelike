using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningSpell : Spell
{
    [Tooltip("how far can the lightning go away from the caster?")]
    public float lightningRadius = 5f;

    public float lightningBoltDamage = 3f;

    public GameObject lightningBoltEffect;
    public int numLightningEffects = 3;
    public float lightningBoltLifetime = 0.4f;

    public LayerMask lightningStrikeLayerMask;

    public AudioClip failedSound;
    public AudioClip successfulSound;

    private AudioSource audioSource;    

    public override void OnCast(Vector2 lookDirection, GameObject caster)
    {
        audioSource = GetComponent<AudioSource>();

        List<Damageable> hittableObjects = new List<Damageable>();

        var hitColliders = Physics2D.OverlapCircleAll(caster.transform.position, lightningRadius, lightningStrikeLayerMask);

        foreach (var collider in hitColliders)
        {
            if (collider.TryGetComponent(out Damageable damageable)) hittableObjects.Add(damageable);
        }

        if (hittableObjects.Count > 0)
        {
            foreach (var damageable in hittableObjects)
            {
                damageable.ApplyDamage(lightningBoltDamage);

                CreateLightningStrikeEffect(damageable.transform.position);
                print(damageable + " struck by lightning");
            }

            audioSource.PlayOneShot(successfulSound);
        }
        else audioSource.PlayOneShot(failedSound);

        Destroy(gameObject, 3f);
    }

    private void CreateLightningStrikeEffect(Vector3 strikePosition)
    {
        Animator lightningAnimator = Instantiate(lightningBoltEffect, strikePosition, Quaternion.identity).GetComponent<Animator>();

        int seed = Random.Range(0, numLightningEffects + 1);
        lightningAnimator.SetFloat("Seed", seed);

        Destroy(lightningAnimator.gameObject, lightningBoltLifetime);
    }
}
