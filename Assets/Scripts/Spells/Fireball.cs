using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : Projectile
{
    private float birthTime;

    public override void OnCast(Vector2 lookDirection, GameObject player)
    {
        base.OnCast(lookDirection, player);

        birthTime = Time.time;
        
        StartCoroutine(DetonateAfterLifetime());
    }

    protected override void OnImpact(AudioClip impactSound)
    {
        impacted = true;

        // transform.rotation = Quaternion.identity;

        audioSource.PlayOneShot(impactSound);

        StartCoroutine(FireballImpact());
    }

    private IEnumerator FireballImpact()
    {
        rb.simulated = false;

        var animator = GetComponent<Animator>();

        animator.SetTrigger("Explode");

        //print("fireball impact");

        while (audioSource.isPlaying || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f) yield return null;

        Destroy(gameObject);
    }

    private IEnumerator DetonateAfterLifetime()
    {
        while (!impacted)
        {
            if (Time.time >= birthTime + lifetime)
            {
                StopAllCoroutines();

                audioSource.PlayOneShot(defaultImpactSound);

                StartCoroutine(FireballImpact());
            }

            yield return null;
        }
    }
}