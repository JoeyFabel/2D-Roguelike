using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Exactly the same as a <see cref="Damageable"/>, with no additions.
/// All it does is allow enemies to be distinguished from other damageables
/// </summary>
public abstract class Enemy : Damageable
{
    // And now its adds a spot for XP

    protected new Rigidbody2D rigidbody;
    protected SpriteRenderer sprite;
    protected Animator animator;

    [Header("Damage")]
    public int damage;
    public DamageTypes damageType;

    [Header("Experience")]
    public int xpForDefeating = 0;

    public int moneyForDefeating = 10;
    public float moneyDropChance = 0f;

    private const float flashTime = 0.1875f;
    
    protected override void Start()
    {
        base.Start();

        rigidbody = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public override void ApplyDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            XPManager.GainXP(xpForDefeating, transform.position);
            if (Random.value <= moneyDropChance) Inventory.CreateMoneyDrop(moneyForDefeating, transform.position);

            Death();
        }
        else
        {
            PlayDamagedSound();
            StartCoroutine(FlashRedOnDamage());
        }
    }

    protected IEnumerator FlashRedOnDamage()
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

    protected IEnumerator DestroyAfterAudio()
    {
        while (audioSource.isPlaying) yield return null;

        Destroy(gameObject);
    }
}
