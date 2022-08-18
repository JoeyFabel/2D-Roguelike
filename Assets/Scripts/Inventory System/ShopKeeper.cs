public class ShopKeeper : DialogTree
{
    private const string ItemNameSignifier = "{itemName}";

    private const string ItemPriceSignifier = "{price}";

    public ForkingNode itemPurchaseDialogNode;
    public DialogNode notEnoughMoneyNode;

    private string initialDialogText;

    private DialogNode currenShopNode;

    protected override void Start()
    {
        base.Start();
        
        initialDialogText = itemPurchaseDialogNode.dialog;
    }

    public void DisplayPurchaseDialog(Item item, int itemPrice)
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
        }
        else if (DialogManager.isTyping)
        {
            DialogManager.FinishDialogLine();
        }
        else
        {
            // Purchase option!

            currenShopNode = currenShopNode.GetNextNode();

            if (currenShopNode == null)
            {
                DialogManager.CloseDialog();

                player.EnableControlsAfterUI();
            }
            else
            {
                // Did the player attempt to purchase the item?
                if (itemPurchaseDialogNode.nextNodes[itemPurchaseDialogNode.GetCurrentSelectedOption()].displayText
                    .Equals("Yes"))
                {
                    if (Inventory.GetCurrentMoney() >= itemPrice)
                    {
                        // The item was purchased
                        Inventory.GainItem(item);

                        Inventory.LoseMoney(itemPrice);
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
