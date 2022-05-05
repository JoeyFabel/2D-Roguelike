using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    Animator animator;

    public enum direction
    {
        NorthEast,
        North,
        NorthWest,
        SouthEast,
        South,
        SouthWest
    }

    public void CreateMuzzleFlash(direction shootDirection, bool invertX)
    {
        GetComponent<SpriteRenderer>().flipX = invertX;

        animator = GetComponent<Animator>();

        if (shootDirection == direction.North) animator.Play("MuzzleFlashN");
        else if (shootDirection == direction.NorthEast || shootDirection == direction.NorthWest) animator.Play("MuzzleFlashNE");
        else if (shootDirection == direction.SouthEast || shootDirection == direction.SouthWest) animator.Play("MuzzleFlashSE");
        else animator.Play("MuzzleFlashS");

        StartCoroutine(DestroyAfterAnimation());
    }

    IEnumerator DestroyAfterAnimation()
    {
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) yield return null;

        Destroy(gameObject);
    }
}
