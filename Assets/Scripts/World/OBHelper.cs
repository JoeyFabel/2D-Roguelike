using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBHelper : MonoBehaviour
{
    public Vector2 validGroundDirection;

    BoxCollider2D obCollider;

    private void Start()
    {
        obCollider = GetComponent<BoxCollider2D>();

        obCollider.isTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            // move the player back into bounds
            Collider2D playerCollider = player.GetComponent<Collider2D>();

            Vector2 desiredPosition = Vector2.zero;

            if (validGroundDirection.x > 0)
            {
                desiredPosition.y = player.transform.position.y;
                desiredPosition.x = obCollider.bounds.max.x + playerCollider.bounds.size.x;
            }
            else if (validGroundDirection.x < 0)
            {
                desiredPosition.y = player.transform.position.y;
                desiredPosition.x = obCollider.bounds.min.x - playerCollider.bounds.size.x;
            }
            if (validGroundDirection.y > 0)
            {
                desiredPosition.x = player.transform.position.x;
                desiredPosition.y = obCollider.bounds.max.y + playerCollider.bounds.size.y;
            }
            else if (validGroundDirection.y < 0)
            {
                desiredPosition.x = player.transform.position.x;
                desiredPosition.y = obCollider.bounds.min.y - playerCollider.bounds.size.y;
                print("bounds min: " + obCollider.bounds.min.y + ", player bounds size: " + playerCollider.bounds.size.y);
            }

            //print("Player is OB - moving player to " + desiredPosition);
            player.GetComponent<Rigidbody2D>().position = desiredPosition;
        }
        else if (collision.TryGetComponent(out Enemy enemy))
        {
            if (enemy is Boss) return;

            // Move the enemy back into bounds
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();

            Vector2 desiredPosition = obCollider.ClosestPoint((Vector2)enemy.transform.position + validGroundDirection);
            if (validGroundDirection.x > 0) desiredPosition.x += enemyCollider.bounds.extents.x * 1.5f;
            else if (validGroundDirection.x < 0) desiredPosition.x -= enemyCollider.bounds.extents.x * 1.5f;
            if (validGroundDirection.y > 0) desiredPosition.y += enemyCollider.bounds.extents.y * 1.5f;
            else if (validGroundDirection.y < 0) desiredPosition.y -= enemyCollider.bounds.extents.y * 1.5f;

            enemy.transform.position = desiredPosition;
        }
    }
}
