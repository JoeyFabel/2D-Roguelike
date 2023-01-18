using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ShopItem : MonoBehaviour, IInteractable
{
    public ShopKeeper shopOwner;

    public Item itemForSale;
    public int price;
    public bool requiresBottle;
    public bool usableIfShopkeeperDead = false;

    private IEnumerator Start()
    {
        // Let the shopkeeper be destroyed first before checking
        yield return null;
        
        if (shopOwner == null && !usableIfShopkeeperDead) Destroy(gameObject); 
    }

    public virtual void Interact()
    {
        if (shopOwner == null || !shopOwner.gameObject.activeInHierarchy)
        {
            // Potions should be able to be taken if shopkeeper is dead
            // Limited edition items should be able to be taken if the shopkeeper is dead
            // Items like bombs disappear either after being stolen, or upon reloading

            if (requiresBottle)
            {
                if (Inventory.PlayerHasEmptyBottle()) Inventory.LoseEmptyBottle();
                else
                {
                    StartCoroutine(DisplayNoBottleTheftDialog());
                    return;
                }
            }

            // Steal the item!
            Inventory.GainItem(itemForSale);
            if (!usableIfShopkeeperDead) Destroy(gameObject);
        }
        else shopOwner.DisplayPurchaseDialog(itemForSale, price, requiresBottle);
    }

    protected IEnumerator DisplayNoBottleTheftDialog()
    {
        var player = CharacterSelector.GetPlayerController();
        player.TryRemoveInteractable(this);
        
        Debug.LogWarning("Player does not have an empty bottle");

        DialogNode noBottleNode = new GameObject("No Bottle Node Helper").AddComponent<DialogNode>();
        noBottleNode.nextNode = null;
        noBottleNode.dialog = "I need an empty bottle for that!";

        DialogManager.SetSpeakerIcon(player.uiPortraitImage);
        DialogManager.DisplayDialog(noBottleNode, 20f);

        yield return new WaitForSecondsRealtime(2.5f);
        
        DialogManager.CloseDialog();
        Destroy(noBottleNode.gameObject);
    }
}
