using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryCellPrefab;

    public GridLayoutGroup inventoryGrid;

    public Selectable firstSelectedOnOpen;
    public GameObject warningPanel;
    public Button warningCancelButton;

    public Text moneyText;

    private List<InventoryCell> inventoryCells;

    private PlayerController player;

    public void InitializeUI()
    {
        for (var i = inventoryGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(inventoryGrid.transform.GetChild(i).gameObject);
        }

        inventoryCells = new List<InventoryCell>();

        gameObject.SetActive(false);
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

    public void CloseInventoryUI()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1.0f;

        player.EnableControlsAfterUI();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
