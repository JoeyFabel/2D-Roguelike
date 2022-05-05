using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCollectible : MonoBehaviour
{
    public int healthAmount = 1;
    public bool pickupIfAtFullHealth = false;
    public GameObject vfxPrefab;

    public AudioClip collectedClip;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController controller = collision.GetComponent<PlayerController>();

        if (controller && (pickupIfAtFullHealth || !controller.AtMaxHealth()))
        {            
            controller.GainHealth(healthAmount);

            if (vfxPrefab) Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);

            controller.PlaySound(collectedClip);
        }
    }
}
