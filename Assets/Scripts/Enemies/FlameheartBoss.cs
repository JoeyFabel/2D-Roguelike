using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameheartBoss : Boss
{
    public Collider2D flyingCollider;

    [Header("Fire Ball Attack")]
    public GameObject fireballPrefab;
    public AudioClip fireBreatheSFX;

    public Vector2 verticalFireballOffset;
    public Vector2 diagonalFireballOffset;
    
    public int minAnglesForAngledAnimation = 23;
    [Tooltip("How far into the animation is the fireball created?")]
    public float fireballInstantiateAnimPercentage = 0.25f;

    // Testing
    public bool doFireballAttack;
    public bool doMeleeAttack;

    private bool hasAction;

    private PlayerController player;

    protected override void Start()
    {
        base.Start();

        player = CharacterSelector.GetPlayerController();

        flyingCollider.enabled = false;

        hasAction = false;
    }

    private void Update()
    {
        if (hasAction) return;

        if (doFireballAttack)
        {
            doFireballAttack = false;
            StartCoroutine(FireballAttack());
        }   
        else if (doMeleeAttack)
        {
            doMeleeAttack = false;
            StartCoroutine(MeleeAttack());
        }
    }

    public void RespondToPlayer()
    {
        StartCoroutine(PlayerResponse());
    }

    private IEnumerator PlayerResponse()
    {
        hasAction = true;

        animator.SetTrigger("Spot Player");

        yield return null;

        while (animator.GetBool("Reacting")) yield return null;

        hasAction = false;
    }

    private IEnumerator MeleeAttack()
    {
        yield return null;
    }

    private IEnumerator FireballAttack()
    {
        hasAction = true;

        // Trigger the inhale animation
        animator.SetTrigger("Fire Attack");
        yield return null;

        // Wait for inhale to finish
        while (animator.GetBool("Inhaling")) yield return null;

        // Get the direction to the player
        Vector2 towardsPlayer = player.transform.position - transform.position;
        float anglesFromDown = Vector2.Angle(Vector2.down, towardsPlayer);

        print("angles from down: " + anglesFromDown);

        // If the player managed to get above the boss, the fireball can't go more than horizontal
        if (anglesFromDown >= 90f) anglesFromDown = 90;

        // If the angle is more than the minimum, do the angled animation
        animator.SetBool("Diagonal Fire", anglesFromDown >= minAnglesForAngledAnimation);

        // Wait for firebreathe animation to get to the desired point
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= fireballInstantiateAnimPercentage) yield return null;

        // then create the fireball
        audioSource.PlayOneShot(fireBreatheSFX);

        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.Euler(0, 0, 180 + anglesFromDown * (sprite.flipX ? -1 : 1)));
        if (animator.GetBool("Diagonal Fire"))
        {
            Vector3 fireballPos = fireball.transform.position;
            fireballPos.y += diagonalFireballOffset.y;
            fireballPos.x += diagonalFireballOffset.x * (sprite.flipX ? -1 : 1);

            fireball.transform.position = fireballPos;
        }
        else fireball.transform.position += (Vector3)verticalFireballOffset;

        hasAction = false;
    }
}
