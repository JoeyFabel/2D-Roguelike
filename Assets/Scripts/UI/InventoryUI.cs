using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryCellPrefab;

    public GridLayoutGroup inventoryGrid;

    public Selectable firstSelectedOnOpen;
    public GameObject warningPanel;
    public Button warningCancelButton;
    public GameObject settingsPanel;
    public Button settingsBackButton;
    public Button keybindingBackButton;
    
    public Text moneyText;

    [Header("Item Gained Display")]
    public Image gainedItemImage;
    public Text gainedItemText;
    public Text gainOrLossText;
    public CanvasGroup itemGainedCanvasGroup;
    public float fadeTime = 0.75f;
    public float activeDuration = 2f;
    public Color itemGainedTextColor = Color.green;
    public Color itemLostTextColor = Color.red;

    [Header("Gold Gained Display")] public CanvasGroup goldGainedCanvasGroup;
    public Text goldGainedHeaderText;
    public Text goldAmountText;
    public Color goldGainedTextColor = Color.yellow;
    public Color goldLostTextColor = Color.red;
    public int itemGainedHeight = 200;
    private RectTransform goldGainedRect;

    [Header("Quick Item Display")] 
    public Image quickItemImage;

    [Header("Item Name Display")] 
    public RectTransform itemNameTextHolderParent;
    private Text itemNameText;
    
    private List<InventoryCell> inventoryCells;
    
    private PlayerController player;

    private bool returnToMenu;
    
    public void InitializeUI()
    {
        for (var i = inventoryGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(inventoryGrid.transform.GetChild(i).gameObject);
        }

        inventoryCells = new List<InventoryCell>();

        gameObject.SetActive(false);
        settingsPanel.SetActive(false);

        itemGainedCanvasGroup.gameObject.SetActive(false);
        goldGainedCanvasGroup.gameObject.SetActive(false);

        goldGainedRect = goldGainedCanvasGroup.GetComponent<RectTransform>();
        goldGainedRect.anchoredPosition = Vector2.zero;

        itemNameText = itemNameTextHolderParent.GetComponentInChildren<Text>();
        itemNameTextHolderParent.gameObject.SetActive(false);
        
        UpdateQuickItemDisplay(null);

        returnToMenu = true;
    }

    /*
    #if UNITY_EDITOR
    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null) return;
       Debug.Log("Current selected: " + EventSystem.current.currentSelectedGameObject.name, EventSystem.current.currentSelectedGameObject);
    }
#endif
    */
    
    public void UpdateQuickItemDisplay(Item quickItem)
    {
        if (quickItem == null) quickItemImage.transform.parent.gameObject.SetActive(false);
        else
        {
            quickItemImage.transform.parent.gameObject.SetActive(true);
            quickItemImage.sprite = quickItem.icon;
        }
    }
    
    public void UpdateItemUI(Item item, int newQuantity)
    {
        InventoryCell cellToModify = inventoryCells.Find((InventoryCell cell) => cell.GetItemID() == item.itemID);

        if (cellToModify)
        {
            if (newQuantity > 0) cellToModify.UpdateQuantity(newQuantity);
            else
            {
                inventoryCells.Remove(cellToModify);
                Destroy(cellToModify.gameObject);
            }
        }
        else
        {
            cellToModify = Instantiate(inventoryCellPrefab, inventoryGrid.transform).GetComponent<InventoryCell>();

            cellToModify.Awake();
            cellToModify.SetItem(item, newQuantity);

            inventoryCells.Add(cellToModify);
        }        
    }

    public void DisplayItemName(string itemName, Vector2 itemCellPosition)
    {
        itemNameText.text = itemName;

        itemNameTextHolderParent.position = itemCellPosition + new Vector2(-100, 150);
        
        itemNameTextHolderParent.gameObject.SetActive(true);
    }

    public void HideItemName()
    {
        itemNameTextHolderParent.gameObject.SetActive(false);
    }

    public IEnumerator DisplayGoldGained(int amount, MonoBehaviour coroutineParent)
    {
        if (amount > 0)
        {
            // Gold was gained
            goldGainedHeaderText.text = "Gold Gained";
            goldGainedHeaderText.color = goldGainedTextColor;

            goldAmountText.text = amount + "     Gained";
            goldAmountText.color = goldGainedTextColor;
        }
        else
        {
            // Gold was lost
            goldGainedHeaderText.text = "Gold Lost";
            goldGainedHeaderText.color = goldLostTextColor;

            goldAmountText.text = Mathf.Abs(amount) + "        Lost";
            goldAmountText.color = goldLostTextColor;
        }

        yield return coroutineParent.StartCoroutine(FadeCanvasInAndOut(goldGainedCanvasGroup));
        //yield return coroutineParent.StartCoroutine(ControlGoldGainedHUDPosition());
    }

    public IEnumerator DisplayItemGained(Item gainedItem, int quantity, MonoBehaviour coroutineParent)
    {        
        if (quantity > 0)
        {
            gainOrLossText.text = "Item Gained:";

            gainOrLossText.color = itemGainedTextColor;
            gainedItemText.color = itemGainedTextColor;
        }
        else
        {
            gainOrLossText.text = "Item Lost:";

            gainOrLossText.color = itemLostTextColor;
            gainedItemText.color = itemLostTextColor;
        }

        gainedItemImage.sprite = gainedItem.icon;
        gainedItemText.text = gainedItem.itemName;

        if (quantity > 1) gainedItemText.text += " (" + quantity + "x)";
        else if (quantity < -1) gainedItemText.text += "(" + (-quantity) + "x)";

        goldGainedRect.anchoredPosition = new Vector2(0f, -itemGainedHeight);
        yield return coroutineParent.StartCoroutine(FadeCanvasInAndOut(itemGainedCanvasGroup));
        goldGainedRect.anchoredPosition = Vector2.zero;
    }

    private IEnumerator FadeCanvasInAndOut(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(true);

        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += 1 / fadeTime * Time.unscaledDeltaTime;

            yield return null;
        }

        canvasGroup.alpha = 1;

        yield return new WaitForSecondsRealtime(activeDuration);

        while (canvasGroup.alpha > 0.01f)
        {
            canvasGroup.alpha -= 1 / fadeTime * Time.unscaledDeltaTime;

            yield return null;
        }

        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
    }
    
    public void CloseInventoryUI()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1.0f;
        
        player.EnableControlsAfterUI();
    }

    public void EnableQuitToMenu()
    {
        returnToMenu = true;
    }

    public void EnableQuitToDesktop()
    {
        returnToMenu = false;
    }
    
    public void QuitGame()
    {
        if (returnToMenu)
        {
            
#if UNITY_EDITOR
        Debug.Log("TODO - Exit to main menu");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
        else
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public void OpenSettingsMenu()
    {
        Debug.LogWarning("TODO -- implement settings menu");
    }

    public void ReturnToMainMenu()
    {
        Debug.LogWarning("TODO -- implement return to main menu");
    }

    /// <summary>
    /// Toggles the Inventory UI on or off, and returns the UI's state.
    /// </summary>
    /// <returns>true if the UI is now active, false if the UI is now disabled</returns>
    public void Toggle()
    {
        if (player == null) player = CharacterSelector.GetPlayerController();

        HideItemName();
        
        // If the inventory is open
        if (gameObject.activeSelf)
        {
            if (warningPanel.activeInHierarchy) // the main menu warning panel is opened
            {
                warningPanel.SetActive(false);                
                warningCancelButton.onClick.Invoke();
            }
            else if (keybindingBackButton.gameObject.activeInHierarchy)
            {
                keybindingBackButton.onClick.Invoke();
            }
            else if (settingsPanel.activeInHierarchy) // The settings panel is opend
            {
                settingsBackButton.onClick.Invoke();
            }
            else // the main inventory is open
            {
                gameObject.SetActive(false);
                
                // Display the quick item when the inventory closes
                UpdateQuickItemDisplay(Inventory.GetQuickItem());
                
                Time.timeScale = 1.0f;
            }
            player.EnableControlsAfterUI();
        }
        else // if the inventory is closed
        {
            gameObject.SetActive(true);
            Time.timeScale = 0f;

            EventSystem.current.SetSelectedGameObject(null);                       
            firstSelectedOnOpen.Select();

            // Hide the quick item when the inventory is open
            UpdateQuickItemDisplay(null);
            
            player.DisableControlsForUI();
        }
    }

    public void UpdateMoneyText(int money)
    {
        moneyText.text = money.ToString();
    }
}
