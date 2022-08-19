using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> itemDatabase;

    public GameObject moneyDrop;

    [Header("Item Prefabs")] public GameObject bombPrefab;

    /// <summary>
    /// A dictionary representing the player's inventory.
    /// The Key represents the item in the inventory, so there can only be one item of each kind displayed at a time.
    /// The Value represents the number of this item in the inventory.
    /// </summary>
    [SerializeField]
    private Dictionary<Item, int> inventory;

    private static Inventory instance;

    private static InventoryUI inventoryUI;

    private Coroutine itemGainedHUDRoutine;
    private Coroutine goldGainedHUDRoutine;

    private int money;

    public void InitializeInventory()
    {
        instance = this;
        inventory = new Dictionary<Item, int>();
        inventoryUI = GetComponentInChildren<InventoryUI>();

        inventoryUI.InitializeUI();
    }

    /// <summary>
    /// Adds an <see cref="Item"/> to the player's inventory.
    /// </summary>
    /// <param name="itemToGain">The item to be gained</param>
    public static void GainItem(Item itemToGain)
    {
        GainItem(itemToGain, 1);
    }

    public static void GainItem(Item itemToGain, int quantity)
    {
        if (instance.inventory.ContainsKey(itemToGain)) instance.inventory[itemToGain] += quantity;
        else instance.inventory.Add(itemToGain, quantity);

        inventoryUI.UpdateItemUI(itemToGain, instance.inventory[itemToGain]);

        if (instance.itemGainedHUDRoutine != null) instance.StopCoroutine(instance.itemGainedHUDRoutine);
        instance.itemGainedHUDRoutine = instance.StartCoroutine(inventoryUI.DisplayItemGained(itemToGain, quantity, instance));
    }

    /// <summary>
    /// Removes a <see cref="Item"/> from the player's inventory.
    /// </summary>
    /// <param name="itemToLose">The item to be lost</param>
    public static void LoseItem(Item itemToLose)
    {
        if (!instance.inventory.ContainsKey(itemToLose))
        {
            Debug.LogError("The item " + itemToLose.itemName + " was not found in the inventory, not losing one!");
            
            return;
        }
        
        if (instance.inventory[itemToLose] > 1)
        {
            instance.inventory[itemToLose]--;
            inventoryUI.UpdateItemUI(itemToLose, instance.inventory[itemToLose]);
        }
        else
        {
            instance.inventory.Remove(itemToLose);
            inventoryUI.UpdateItemUI(itemToLose, 0);
        }

        if (instance.itemGainedHUDRoutine != null) instance.StopCoroutine(instance.itemGainedHUDRoutine);
        instance.itemGainedHUDRoutine = instance.StartCoroutine(inventoryUI.DisplayItemGained(itemToLose, -1, instance));
    }

    public static bool PlayerHasItem(Item itemToCheck)
    {
        if (itemToCheck == null) return true;

        return instance.inventory.ContainsKey(itemToCheck);
    }

    public static void GainMoney(int amount)
    {
        if (amount == 0) return;
        
        instance.money += amount;

        inventoryUI.UpdateMoneyText(instance.money);
        
        if (instance.goldGainedHUDRoutine != null) instance.StopCoroutine(instance.goldGainedHUDRoutine);
        instance.goldGainedHUDRoutine = instance.StartCoroutine(inventoryUI.DisplayGoldGained(amount, instance));
    }

    public static void LoseMoney(int amount)
    {
        if (amount == 0) return;
        
        instance.money -= amount;

        inventoryUI.UpdateMoneyText(instance.money);
        
        if (instance.goldGainedHUDRoutine != null) instance.StopCoroutine(instance.goldGainedHUDRoutine);
        instance.goldGainedHUDRoutine = instance.StartCoroutine(inventoryUI.DisplayGoldGained(-amount, instance));
    }

    public static int GetCurrentMoney()
    {
        return instance.money;
    }

    public static void CreateMoneyDrop(int money, Vector3 position)
    {
        if (money <= 0) return;

        MoneyDrop moneyDrop = Instantiate(instance.moneyDrop, position, Quaternion.identity).GetComponent<MoneyDrop>();

        moneyDrop.SetMoney(money);
    }

    public static Item GetQuickItem()
    {
        // TODO - implement quick item HUD
        return instance.itemDatabase.Find(item => item.itemName.Equals("Bomb"));
    }

    public void LoadInventoryFromData(Dictionary<int, int> inventoryData, int currentMoney)
    {
        inventory = new Dictionary<Item, int>();

        money = currentMoney;
        inventoryUI.UpdateMoneyText(money);

        if (inventoryData == null) return;

        // load the inventory from the data
        foreach (KeyValuePair<int, int> dataPair in inventoryData)
        {
            inventory.Add(GetItemFromID(dataPair.Key), dataPair.Value);            
        }

        // display the inventory in the ui
        foreach (KeyValuePair<Item, int> dataPair in inventory)
        {
            inventoryUI.UpdateItemUI(dataPair.Key, dataPair.Value);
        }
    }

    public Dictionary<int, int> GetInventorySaveData()
    {
        Dictionary<int, int> data = new Dictionary<int, int>();

        foreach (KeyValuePair<Item, int> dataPair in inventory)
        {
            data.Add(dataPair.Key.itemID, dataPair.Value);
        }

        return data;
    }

    private Item GetItemFromID(int itemID)
    {
        foreach (var item in itemDatabase) if (item.itemID == itemID) return item;

        return null;
    }

    /// <summary>
    /// Toggle the Inventory UI on or off.
    /// </summary>
    /// <returns>True if the UI is now enabled, false if it is now disabled</returns>
    public static void ToggleInventoryUI()
    {
        if (!XPManager.gamePausedForLevelUp) inventoryUI.Toggle();
    }

#if UNITY_EDITOR
    [ContextMenu("Populate Item Database")]
    private void PopulateItemDB()
    {
        itemDatabase = new List<Item>();

        itemDatabase.AddRange(Resources.FindObjectsOfTypeAll<Item>());
    }
#endif
}
