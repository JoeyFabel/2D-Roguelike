using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LimitedShopItem : ShopItem, ISaveable
{
    public int amountInStock;

    [SerializeField]
    private int saveID = -1;
    public int SaveIDNumber
    {
        get => saveID;
        set => saveID = value;
    }

    public bool DoneLoading { get; set; }
    
    private void Awake()
    {
        SaveManager.RegisterSaveable(this);
    }

    private void OnDestroy()
    {
        SaveManager.UnRegisterSaveable(this);
    }

    private void Start()
    {
        shopOwner.OnItemBought.AddListener(CheckIfItemWasBought);
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
    
    public WorldObjectSaveData GetSaveData()
    {
        ShopItemSaveData data = new ShopItemSaveData();
        data.remainingStock = amountInStock;

        return data;
    }

    public void LoadData(WorldObjectSaveData saveData)
    {
        ShopItemSaveData data = saveData as ShopItemSaveData;

        amountInStock = data.remainingStock;

        if (amountInStock <= 0) Destroy(gameObject);
        
        DoneLoading = true;
    }

    [System.Serializable]
    public class ShopItemSaveData : WorldObjectSaveData
    {
        public int remainingStock;
    }
    
#if UNITY_EDITOR
    public void MarkAsDirty()
    {
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Changed saveID");
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }
#endif  
}
