using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HostileNpcBehavior : MonoBehaviour
{
    [Header("Hostility Info")] public int minHealthBeforeHostile = 3;

    protected Animator animator;
    protected SpriteRenderer sprite;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public abstract void BecomeHostile();

    public virtual void EndCurrentAction()
    {
        
    }
    
    /*
     private IEnumerator RetreatAction()
    {
        for (int i = 0; i < Random.Range(minMovesPerAction, maxMovesPerAction); i++)
        {
            Vector2 desiredPosition = rigidbody.position + (rigidbody.position - (Vector2)player.transform.position).normalized * 2;
            yield return StartCoroutine(MoveToPosition(desiredPosition));
        }
        
        ChooseAction();
    }

    private IEnumerator MovementAction()
    {
        for (int i = 0; i < Random.Range(minMovesPerAction, maxMovesPerAction); i++)
        {
            Vector2 desiredPosition = rigidbody.position + Random.insideUnitCircle * 2f;
            yield return StartCoroutine(MoveToPosition(desiredPosition));
        }
        
        ChooseAction();
    }      
    
        private IEnumerator MoveToPosition(Vector2 position)
    {
        // Move until within an acceptable error
        while (Vector2.Distance(rigidbody.position, position) > 0.1f)
        {
            Vector2 movementVector = (position - rigidbody.position).normalized * (moveSpeed * Time.fixedDeltaTime);
            
            rigidbody.MovePosition(rigidbody.position + movementVector);
            
            yield return null;
        }
    }
     */
}
