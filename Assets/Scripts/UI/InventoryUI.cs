using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryCellPrefab;

    public GridLayoutGroup inventoryGrid;

    public Selectable firstSelectedOnOpen;
    public GameObject warningPanel;
    public Button warningCancelButton;
    public GameObject settingsPanel;

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
        returnToMenu = true;
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

    public IEnumerator DisplayItemGained(Item gainedItem, int quantity)
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

        itemGainedCanvasGroup.alpha = 0;
        itemGainedCanvasGroup.gameObject.SetActive(true);

        while (itemGainedCanvasGroup.alpha < 1)
        {
            itemGainedCanvasGroup.alpha += 1 / fadeTime * Time.unscaledDeltaTime;

            yield return null;
        }

        itemGainedCanvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(activeDuration);

        while (itemGainedCanvasGroup.alpha > 0.01f)
        {
            itemGainedCanvasGroup.alpha -= 1 / fadeTime * Time.unscaledDeltaTime;

            yield return null;
        }

        itemGainedCanvasGroup.alpha = 0;
        itemGainedCanvasGroup.gameObject.SetActive(false);
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

        // If the inventory is open
        if (gameObject.activeSelf)
        {
            if (warningPanel.activeSelf) // the main menu warning panel is opened
            {
                warningPanel.SetActive(false);                
                warningCancelButton.onClick.Invoke();
            }
            else // the main inventory is open
            {
                gameObject.SetActive(false);
                Time.timeScale = 1.0f;
            }
            player.EnableControlsAfterUI();
        }
        else // if the inventory is closed
        {
            gameObject.SetActive(true);
            Time.timeScale = 0f;

            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);                       
            firstSelectedOnOpen.Select();

            player.DisableControlsForUI();
        }
    }

    public void UpdateMoneyText(int money)
    {
        moneyText.text = money.ToString();
    }
}
