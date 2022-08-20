using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LimitedShopItem : SaveableObject, IInteractable
{
    public int amountInStock;

    public ShopKeeper shopOwner;

    public Item itemForSale;
    public int price;
    public bool requiresBottle;

    protected override void Start()
    {
        base.Start();

        shopOwner.OnItemBought.AddListener(CheckIfItemWasBought);
    }

    public void Interact()
    {
        print("Interacting with limited shop item");
        
        shopOwner.DisplayPurchaseDialog(itemForSale, price, requiresBottle);
    }
    
    private void CheckIfItemWasBought(Item boughtItem)
    {
        if (boughtItem.itemID == itemForSale.itemID)
        {
            amountInStock--;

            if (amountInStock <= 0)
            {
                // The item was bought
                shopOwner.OnItemBought.RemoveListener(CheckIfItemWasBought);

                // Dont destroy until after the dialog is closed or you will be stuck in dialog
                StartCoroutine(DestroyAfterDialog());
            }
        }
    }

    private IEnumerator DestroyAfterDialog()
    {
        while (DialogManager.IsSpeechBubbleEnabled()) yield return null;
        
        Destroy(gameObject);
    }
    
    public override WorldObjectSaveData GetSaveData()
    {
        ShopItemSaveData data = new ShopItemSaveData();
        data.remainingStock = amountInStock;

        return data;
    }

    protected override void LoadData()
    {
        ShopItemSaveData data = saveData as ShopItemSaveData;

        amountInStock = data.remainingStock;

        if (amountInStock <= 0) Destroy(gameObject);
        
        isDoneLoading = true;
    }

    [System.Serializable]
    public class ShopItemSaveData : WorldObjectSaveData
    {
        public int remainingStock;
    }
}
