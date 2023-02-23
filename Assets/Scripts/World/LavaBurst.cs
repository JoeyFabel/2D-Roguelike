using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaBurst : MonoBehaviour
{
    private static readonly int AnimatorBurstID = Animator.StringToHash("Burst");
    
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        animator.SetTrigger(AnimatorBurstID);
    }
}
