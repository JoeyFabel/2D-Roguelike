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

            Vector2 desiredPosition = obCollider.ClosestPoint((Vector2)player.transform.position + validGroundDirection);
            if (validGroundDirection.x > 0) desiredPosition.x += playerCollider.bounds.extents.x;
            else if (validGroundDirection.x < 0) desiredPosition.x -= playerCollider.bounds.extents.x;
            if (validGroundDirection.y > 0) desiredPosition.y += playerCollider.bounds.extents.y;
            else if (validGroundDirection.y < 0) desiredPosition.y -= playerCollider.bounds.extents.y;

            print("Player is OB");
            player.transform.position = desiredPosition;
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
