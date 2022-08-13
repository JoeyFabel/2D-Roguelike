using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Damageable
{
    public Damageable.DamageTypes damageType = Damageable.DamageTypes.Fire;

    public float lifetime = 3f;

    [Header("Explosion")]
    public GameObject explosionVFX;
    public AudioClip explosionSFX;
    public float explosionRadius = 2f;
    public LayerMask explosionFilter;
    public int damage = 1;
    
    [Header("Flashing Info")] 
    public float maxLifetimeFlashesPerSecond = 1f;
    public float minLifetimeFlashesPerSecond = 3f;

    private float spawnedTimestamp;
    
    private SpriteRenderer sprite;
    
    protected override void Start()
    {
        base.Start();
        
        sprite = GetComponent<SpriteRenderer>();
        
        spawnedTimestamp = Time.time;

        StartCoroutine(FlashWithLifetime());
    }

    protected override void Death()
    {
        Explode();
    }

    private void Explode()
    {
        StopAllCoroutines();
        sprite.enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
        
        Instantiate(explosionVFX, transform.position, Quaternion.identity);
        
        CheckForDamageables();
        
        audioSource.PlayOneShot(explosionSFX);

        StartCoroutine(DestroyAfterAudio());
    }

    private void CheckForDamageables()
    {
        var hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, explosionFilter);

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Damageable damageable)) damageable.ApplyDamage(damage, damageType);
            else if (hit.TryGetComponent(out PlayerController player)) player.TakeDamage(damage, damageType);
            else if (hit.TryGetComponent(out DestructibleRock rock)) rock.DestroyRock();
        }
    }
    
    private IEnumerator DestroyAfterAudio()
    {
        while (audioSource.isPlaying) yield return null;

        Destroy(gameObject);
    }
    
    private IEnumerator FlashWithLifetime()
    {
        float flashesPerSecond = maxLifetimeFlashesPerSecond;
        
        Coroutine flashRoutine = null;

        Color originalColor = sprite.color;

        while (Time.time < spawnedTimestamp + lifetime)
        {
            flashesPerSecond = Mathf.Lerp(maxLifetimeFlashesPerSecond, minLifetimeFlashesPerSecond,  (Time.time - spawnedTimestamp) / lifetime);
            // flash time (sec) = numFlashes / flashes/second = 1/flashesPerSecond 
            
            flashRoutine = StartCoroutine(DoSingleFlash(1f / flashesPerSecond));

            yield return flashRoutine;
        }
        
        StopCoroutine(flashRoutine);

        sprite.color = originalColor;
        
        Explode();
    }

    private IEnumerator DoSingleFlash(float flashTime)
    {
        float timestamp = Time.time;
        Color originalColor = sprite.color;
        Color flashColor = sprite.color;

        // Flash into full red
        while (Time.time - timestamp <= flashTime / 2f)
        {
            flashColor = Color.Lerp(originalColor, Color.red, (Time.time - timestamp) / (flashTime / 2f));

            sprite.color = flashColor;
            
            yield return null;
        }

        sprite.color = Color.red;

        // Flash back to original color
        timestamp = Time.time;
        while (Time.time - timestamp <= flashTime / 2f)
        {
            flashColor = Color.Lerp(Color.red, originalColor, (Time.time - timestamp) / (flashTime / 2f));

            sprite.color = flashColor;

            yield return null;
        }

        sprite.color = originalColor;
    }
}
