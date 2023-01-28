using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantTentacle : Enemy
{
    // flip x, negate x offset
    public CapsuleCollider2D meleeEastTrigger;
    public CapsuleCollider2D meleeSouthTrigger;

    protected static readonly int AnimatorAttackEastID = Animator.StringToHash("Melee_E");
    protected static readonly int AnimatorAttackSouthID = Animator.StringToHash("Melee_S");

    protected override void Death()
    {
        animator.SetTrigger("Death");

        AudioClip deathClip = deathSounds[Random.Range(0, deathSounds.Length)];
        audioSource.PlayOneShot(deathClip);
        Destroy(gameObject, deathClip.length);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector3 toPlayer = collision.transform.position - transform.position;
            
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
}
