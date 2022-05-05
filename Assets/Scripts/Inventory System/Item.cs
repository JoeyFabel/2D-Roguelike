using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public Sprite icon;
    public string itemName;

    public int itemID = -1;

#if UNITY_EDITOR
    [ContextMenu("Assign IDs to items")]
    private void AssignIDs()
    {
        Item[] items = Resources.FindObjectsOfTypeAll<Item>();

        int nextID = 0;

        foreach (var item in items) if (item.itemID >= nextID) nextID = item.itemID + 1;

        foreach (var item in items) if (item.itemID == -1) item.itemID = nextID++;
    }
#endif
}
