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

    public virtual void Interact()
    {
        if (shopOwner == null)
        {
            Debug.LogWarning("TODO - Handle shop items if shop owner is dead");
            
            // Potions should be able to be taken if shopkeeper is dead
            // Limited edition items should be able to be taken if the shopkeeper is dead
            // Items like bombs should maybe disappear?
            
            return;
        }
        shopOwner.DisplayPurchaseDialog(itemForSale, price, requiresBottle);
    }
}
