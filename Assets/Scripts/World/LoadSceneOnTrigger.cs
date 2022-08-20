using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneOnTrigger : MonoBehaviour
{
    [Header("Pick ONLY one")]
    [Tooltip("The name of the scene to load")]
    public string sceneName;
    public bool isShopScene;
    [Tooltip("Check this if this will load the player into the shrine")]
    public bool loadIntoShrine;
    [Tooltip("Check this if this is in the shrine loading the player into the last active scene")]
    public bool loadOutOfShrine;
    [Tooltip("Offset the player by this much when loading into the next scene")]
    public Vector2 playerOffset;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player was the one that entered the trigger, than load the scene
        if (collision.TryGetComponent(out PlayerController player))
        {
            if (loadIntoShrine) GameManager.LoadIntoShrine(collision.transform.position + Vector3.down, player.GetPlayerLoadData()); // subtracts 1 from y axis to load player below shrine
            else if (loadOutOfShrine) GameManager.LoadOutOfShrine();
            else if (isShopScene) GameManager.LoadIntoShop(collision.transform.position + Vector3.down, player.GetPlayerLoadData(), sceneName); 
            else GameManager.LoadScene(sceneName, playerOffset);
        }
    }
}
