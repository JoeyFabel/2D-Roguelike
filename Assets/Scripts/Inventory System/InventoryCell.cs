using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCell : MonoBehaviour
{
    public Image itemDisplayImage;
    public GameObject additionalAmountGameObject;

    private Text additionalAmountText;
    private int itemID;

    public void Awake()
    {
        additionalAmountText = additionalAmountGameObject.GetComponentInChildren<Text>();
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
            print("updating quatnity");
            additionalAmountGameObject.SetActive(true);

            additionalAmountText.text = "x" + newQuantity;
        }
    }

    public int GetItemID()
    {
        return itemID;
    }
}
