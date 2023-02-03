using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantTentacle : Enemy
{
    // flip x, negate x offset
    public CapsuleCollider2D meleeEastTrigger;
    public CapsuleCollider2D meleeSouthTrigger;

    public ContactFilter2D meleeHitLayers;
    public Transform tentacle;

    public float edgeMoveOffset = 0.75f;

    protected static readonly int AnimatorAttackEastID = Animator.StringToHash("Melee_E");
    protected static readonly int AnimatorAttackSouthID = Animator.StringToHash("Melee_S");

    protected override void Death()
    {
        animator.SetTrigger("Death");

        AudioClip deathClip = deathSounds[Random.Range(0, deathSounds.Length)];
        audioSource.PlayOneShot(deathClip);
        Destroy(gameObject, deathClip.length);
    }


    protected override void Start()
    {
        currentHealth = maxHealth;

        audioSource = tentacle.GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        sprite = tentacle.GetComponent<SpriteRenderer>();
        rigidbody = tentacle.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // move the tentacle to the closest position next to the player
            tentacle.position = GetComponent<Collider2D>().ClosestPoint(collision.transform.position);
            Vector3 towardsCenter = transform.position - tentacle.position;
            tentacle.position += towardsCenter * edgeMoveOffset;

            Vector3 toPlayer = collision.transform.position - tentacle.position;
            
            // East/West Melee attack
            if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y))
            {
                // start East/West attack animation
                animator.SetTrigger(AnimatorAttackEastID);

                // set trigger offset
                meleeEastTrigger.offset = new Vector2(Mathf.Abs(meleeEastTrigger.offset.x) * (toPlayer.x > 0 ? 1 : -1), meleeEastTrigger.offset.y);
            }
            else // South Melee Attack
            {
                animator.SetTrigger(AnimatorAttackSouthID);

                meleeSouthTrigger.offset = new Vector2(Mathf.Abs(meleeSouthTrigger.offset.x) * (toPlayer.x > 0 ? 1 : -1), meleeSouthTrigger.offset.y);
            }
            
            // Make the tentacle face towards the player
            sprite.flipX = toPlayer.x < 0;        
        }
    }

    private void MeleeEastHitCheck()
    {
        meleeEastTrigger.enabled = true;

        List<Collider2D> results = new List<Collider2D>();
        meleeEastTrigger.OverlapCollider(meleeHitLayers, results);
        
        meleeEastTrigger.enabled = false;

        if (results.Count > 0)
        {
            Collider2D player;
            if (player = results.Find((item) => item.CompareTag("Player")))
            {
                player.GetComponent<PlayerController>().TakeDamage(damage, damageType);
            }
            
        }
            
    }

    private void MeleeSouthHitCheck()
    {
        meleeSouthTrigger.enabled = true;

        List<Collider2D> results = new List<Collider2D>();
        meleeSouthTrigger.OverlapCollider(meleeHitLayers, results);
        
        meleeSouthTrigger.enabled = false;

        if (results.Count > 0)
        {
            Collider2D player;
            if (player = results.Find((item) => item.CompareTag("Player")))
            {
                player.GetComponent<PlayerController>().TakeDamage(damage, damageType);
            }

        }
    }
}
