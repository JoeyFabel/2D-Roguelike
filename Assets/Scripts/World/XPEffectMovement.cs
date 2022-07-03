using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPEffectMovement : MonoBehaviour
{
    public float upwardMoveSpeed = 2f;
    public float horizontalMovementSpeed = 2f;
    public float horizontalOscillationTime = 0.5f;
    public float lifetime = 2f;

    float lastOscillateTime = 0f;

    private void Start()
    {
        StartCoroutine(DestroyAfterLifetime());
    }

    private IEnumerator DestroyAfterLifetime()
    {
        float enterTime = Time.unscaledTime;

        while (Time.unscaledTime - enterTime < lifetime) yield return null;

        Destroy(gameObject);
    }

    void Update()
    {
        Vector3 movement = new Vector3();

        float rightPercentage = Mathf.Sin(2 * Mathf.PI * (lastOscillateTime / horizontalOscillationTime));
        
        movement += Time.unscaledDeltaTime * upwardMoveSpeed * Vector3.up;
        movement += rightPercentage * horizontalMovementSpeed * Time.unscaledDeltaTime * Vector3.right;


        lastOscillateTime += Time.unscaledDeltaTime;
        /*if (lastOscillateTime >= horizontalOscillationTime)
        {
            lastOscillateTime = 0f;
            movingRight = !movingRight;
        }*/

        transform.position += movement;
    }

}
