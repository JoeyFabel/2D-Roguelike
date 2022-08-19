using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryCell : Button
{
    public Image itemDisplayImage;
    public GameObject additionalAmountGameObject;

    private Text additionalAmountText;
    private int itemID;

    private static float InitialDisplayDelay = 1f;
    
    public void Awake()
    {
        additionalAmountText = additionalAmountGameObject.GetComponentInChildren<Text>();

       // this.colors.normalColor = Color.red;
    }

    public void SetItem(Item item, int amount)
    {        
        itemDisplayImage.sprite = item.icon;
        itemID = item.itemID;

        if (amount > 1)
        {
            additionalAmountGameObject.SetActive(true);

            additionalAmountText.text = "x" + amount;
        }
        else additionalAmountGameObject.SetActive(false);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity == 1) additionalAmountGameObject.SetActive(false);
        else
        {
            print("updating quantity");
            additionalAmountGameObject.SetActive(true);

            additionalAmountText.text = "x" + newQuantity;
        }
    }

    public int GetItemID()
    {
        return itemID;
    }

    
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        
        StartCoroutine(DisplayItemName());
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);
        
        Inventory.TrySetQuickItem(itemID);
    }
    
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        
        StopAllCoroutines();     
    } 

    private IEnumerator DisplayItemName()
    {
        yield return new WaitForSecondsRealtime(InitialDisplayDelay);
        
        Inventory.DisplayItemName(transform.position, itemID);
    }
}
