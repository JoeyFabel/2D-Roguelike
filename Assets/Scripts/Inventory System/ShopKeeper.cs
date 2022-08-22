using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopKeeper : DialogTree
{
    private const string ItemNameSignifier = "{itemName}";

    private const string ItemPriceSignifier = "{price}";

    public ForkingNode itemPurchaseDialogNode;
    public DialogNode notEnoughMoneyNode;
    public DialogNode missingBottleNode;

    [Header("Shop HUD")] 
    public GameObject shopHUDParent;
    public Text moneyAmountText;
    
    private string initialDialogText;

    private DialogNode currenShopNode;

    public UnityEvent<Item> OnItemBought;

    protected override void Start()
    {
        base.Start();
        
        initialDialogText = itemPurchaseDialogNode.dialog;
        shopHUDParent.SetActive(false);
    }

    public void DisplayPurchaseDialog(Item item, int itemPrice, bool requiresBottle)
    {
        // Set the dialog text
        itemPurchaseDialogNode.dialog = initialDialogText.Replace(ItemNameSignifier, item.itemName).Replace(ItemPriceSignifier, itemPrice.ToString());
        
        // If the dialog bubble is closed, open the forking node
        if (!DialogManager.IsSpeechBubbleEnabled())
        {
            DialogManager.SetSpeakerIcon(speakerIcon);
            DialogManager.DisplayDialog(itemPurchaseDialogNode);

            currenShopNode = itemPurchaseDialogNode;
            
            player.DisableControlsForDialog();
            
            moneyAmountText.text = Inventory.GetCurrentMoney().ToString();
            shopHUDParent.SetActive(true);
        }
        else if (DialogManager.isTyping)
        {
            DialogManager.FinishDialogLine();
        }
        else // Some dialog is already displayed
        {
            currenShopNode = currenShopNode.GetNextNode();

            // There is no next node to go to, close the dialog
            if (currenShopNode == null)
            {
                DialogManager.CloseDialog();

                shopHUDParent.SetActive(false);
                
                player.EnableControlsAfterUI();
            }
            else
            {
                // Did the player attempt to purchase the item?
                if (itemPurchaseDialogNode.nextNodes[itemPurchaseDialogNode.GetCurrentSelectedOption()].displayText
                    .Equals("Yes"))
                {
                    if (requiresBottle && !Inventory.PlayerHasEmptyBottle())
                    {
                        currenShopNode = missingBottleNode;
                        DialogManager.DisplayDialog(currenShopNode);
                        
                        return;
                    }
                    
                    if (Inventory.GetCurrentMoney() >= itemPrice)
                    {
                        // The item was purchased
                        Inventory.GainItem(item);

                        Inventory.LoseMoney(itemPrice);
                        if (requiresBottle) Inventory.LoseEmptyBottle();
                        
                        OnItemBought?.Invoke(item);

                        moneyAmountText.text = Inventory.GetCurrentMoney().ToString();
                    }
                    else
                    {
                        // Not enough money to purchase
                        currenShopNode = notEnoughMoneyNode;
                    }
                }

                DialogManager.DisplayDialog(currenShopNode);
            }

        }
    }
}
