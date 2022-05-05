using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantSlime : Boss
{
    private const float whiteFlashTime = 0.1875f;

    public float scaleIncreasePerHealth = 0.5f;

    public GameObject slimePrefab;

    public float slimeLaunchTime = 3f;

    public int minChildAttacksBeforeAbsorption = 1;
    public int maxChildAttacksBeforeAbsorption = 2;

    private PlayerController player;

    private List<Slime> createdSlimes;

    new Collider2D collider;


    protected override void Start()
    {
        base.Start();

        collider = GetComponent<Collider2D>();
        player = CharacterSelector.GetPlayerController();
        createdSlimes = new List<Slime>();

        Vector3 scale = Vector3.one + Vector3.one * scaleIncreasePerHealth * maxHealth;
        transform.localScale = scale;
    }

    protected override void Death()
    {
        animator.SetTrigger("Death");

        audioSource.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)]);
        rigidbody.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        enabled = false;

        StartCoroutine(DestroyAfterAudio());

        // kill all the baby slimes :(
        for (int i = createdSlimes.Count - 1; i >= 0; i--) createdSlimes[i].ApplyDamage(createdSlimes[i].maxHealth);

        Debug.LogWarning("TODO -- defeat animation/vfx, give xp after effect ends");
    }

    public float launchDistance = 2f;

    public override void ApplyDamage(float amount)
    {
        base.ApplyDamage(amount);

        if (currentHealth > 0)
        {
            Vector2 playerTowardsCenter = transform.position - player.transform.position;

            float jumpDistance = launchDistance * Mathf.Max(1f, amount);

            SpawnSlime(playerTowardsCenter.normalized * jumpDistance, slimeLaunchTime);
        }
    }

    public void LoseSlime(Slime deadSlime)
    {
        createdSlimes.Remove(deadSlime);
    }

    public void ReabsorbSlime(Slime slimeToAbsorb)
    {
        print(this + " absorbed " + slimeToAbsorb);

        createdSlimes.Remove(slimeToAbsorb);
        Destroy(slimeToAbsorb.gameObject);

        StartCoroutine(FlashWhiteOnHeal());

        currentHealth += 1;

        Vector3 newScale = transform.localScale;
        newScale += Vector3.one * scaleIncreasePerHealth;

        transform.localScale = newScale;
    }

    private void SpawnSlime(Vector2 jumpEndPosition, float slimeJumpSpeed)
    {
        Vector3 spawnPosition = collider.ClosestPoint((Vector2)collider.bounds.center + jumpEndPosition);

        Slime spawnedSlime = Instantiate(slimePrefab, spawnPosition, Quaternion.identity).GetComponent<Slime>();
        spawnedSlime.Start();

        spawnedSlime.SpawnFromGiantSlime(this, Random.Range(minChildAttacksBeforeAbsorption, maxChildAttacksBeforeAbsorption + 1), jumpEndPosition, slimeJumpSpeed);
        createdSlimes.Add(spawnedSlime);

        Vector3 newScale = transform.localScale;
        newScale -= Vector3.one * scaleIncreasePerHealth;

        transform.localScale = newScale;
    }    

    private IEnumerator FlashWhiteOnHeal()
    {
        float flashAmount = 0f;

        bool increasingValue = true;

        do
        {
            flashAmount += (1f / whiteFlashTime) * Time.deltaTime * (increasingValue ? 1 : -1);

            if (increasingValue && flashAmount >= 1f) increasingValue = false;

            sprite.material.SetFloat("_FlashAmount", flashAmount);
            
            yield return null;
        } while (flashAmount > 0.01f);

    }
}
