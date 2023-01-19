using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendableDrawBridge : MonoBehaviour
{
    public float endLength;
    public float moveSpeed = 1f;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private BoxCollider2D boxCollider;

    private bool isExtended = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void ExtendDrawbridge()
    {
        if (!isExtended)
        {
            StartCoroutine(MoveDrawbridge());
            isExtended = true;
        }
    }

    private IEnumerator MoveDrawbridge()
    {
        while (boxCollider.size.y < endLength - moveSpeed * Time.deltaTime)
        {
            // Increase the drawbridge's size
            Vector2 size = boxCollider.size;
            size.y += moveSpeed * Time.deltaTime;

            boxCollider.size = size;

            // Increase the drawbridge's offset
            Vector2 offset = boxCollider.offset;
            offset.y = -boxCollider.size.y / 2;

            boxCollider.offset = offset;

            // Set the tiling height
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, boxCollider.size.y);

            yield return null;
        }

        // Put the drawbridge at the end state
        boxCollider.size = new Vector2(boxCollider.size.x, endLength);
        boxCollider.offset = new Vector2(boxCollider.offset.x, -endLength / 2);
        spriteRenderer.size = new Vector2(spriteRenderer.size.x, endLength);
    }

}
