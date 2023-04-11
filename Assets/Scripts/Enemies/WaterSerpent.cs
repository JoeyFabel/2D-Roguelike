using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSerpent : Boss
{
    private static readonly int AnimatorLungeTriggerID = Animator.StringToHash("Lunging");
    private static readonly int AnimatorBiteTriggerID = Animator.StringToHash("Bite");
    private static readonly int AnimatorToPlayerYID = Animator.StringToHash("To Player Y");
    
    public float lungeAngleOffset = 0f;
    public float lungeTime = 2f;
    public float lungeSpeed = 1f;

    private PlayerController player;

    public bool testLunge;

    protected override void Start()
    {
        base.Start();
        player = CharacterSelector.GetPlayerController();
    }

    private void Update()
    {
        if (testLunge)
        {
            testLunge = false;
            StartCoroutine(LungeAttack());
        }
    }

    private IEnumerator LungeAttack()
    {
        Vector2 towardPlayer = player.transform.position - transform.position;
        
        animator.SetFloat(AnimatorToPlayerYID, towardPlayer.y);
        animator.SetBool(AnimatorLungeTriggerID, true);

        sprite.flipX = towardPlayer.x < 0;
        
        // wait for windup to finish
        yield return new WaitForSeconds(0.625f);
        sprite.flipX = towardPlayer.x < 0;

        Vector2 lungeDirection;
        float angleToPlayer = Vector2.SignedAngle(Vector2.up, towardPlayer);
        
        // angle Z to face player
        transform.rotation = Quaternion.Euler(0, 0, angleToPlayer + lungeAngleOffset * (sprite.flipX ? -1: 1));

        // Move forward during the lunge
        float timestamp = Time.time;
        while (Time.time < timestamp + lungeTime)
        {
            Vector2 targetPosition = rigidbody.position + (Vector2)transform.up * (lungeSpeed * Time.fixedDeltaTime);
            rigidbody.MovePosition(targetPosition);
            
            yield return null;
        }

        // Stop moving and reset the rotation back to zero
        transform.rotation = Quaternion.identity;
        animator.SetBool(AnimatorLungeTriggerID, false);
    }

    private IEnumerator BiteAttack()
    {
        animator.SetTrigger(AnimatorBiteTriggerID);

        yield return null;
    }
}
