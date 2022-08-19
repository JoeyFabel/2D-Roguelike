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

    public void Interact()
    {
        shopOwner.DisplayPurchaseDialog(itemForSale, price, requiresBottle);
    }
}
