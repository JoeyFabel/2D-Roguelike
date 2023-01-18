using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An intermediary class that allows communication between ISaveables and the GameManager
/// </summary>
public class SaveManager : MonoBehaviour
{
    public WorldObjectSaveData GetSaveData(string saveID)
    {
        return null;
    }

    public static string GetSaveID(ISaveable saveable)
    {
        return GameManager.GetCurrentSceneName() + ": " + saveable.SaveIDNumber;
    }

    public static void DistributeSaveData(Dictionary<string, WorldObjectSaveData> dataDictionary)
    {
        if (dataDictionary == null) return;

        // Give the save data to each object that needs it
        foreach (var saveable in Instances)
        {
            string saveID = GetSaveID(saveable);

            if (dataDictionary.ContainsKey(saveID))
            {
                // If the ISaveable has a saveData, load it
                saveable.DoneLoading = false;

                WorldObjectSaveData saveData = dataDictionary[saveID];

                saveable.LoadData(saveData);
            } // Otherwise, the ISaveable is done loading
            else saveable.DoneLoading = true;
        }
    }
    
    public static bool IsLoading()
    {
        // See if any saveable is still loading and return that value
        bool doneLoading = true;

        foreach (var saveable in instances) doneLoading = doneLoading && saveable.DoneLoading;
        
        return !doneLoading;
    }

    // A HashSet that stores the saveable instances in the scene
    private static readonly HashSet<ISaveable> instances = new HashSet<ISaveable>();

    public static void RegisterSaveable(ISaveable saveable)
    {
        instances.Add(saveable);
    }

    public static void UnRegisterSaveable(ISaveable saveable)
    {
        instances.Remove(saveable);
    }
    
    // The copy of the hashset the can be accessed
    // This prevents anyone from removing items
    public static HashSet<ISaveable> Instances => new HashSet<ISaveable>(instances);

    #region Editor Methods
    #if UNITY_EDITOR
    [ContextMenu("Assign IDs to ISaveables"), UnityEditor.MenuItem("Save Management/Assign IDs to Saveables")]
    private static void AssignIDs()
    {
        // Get the saveables
        MonoBehaviour[] monoBehaviours = FindObjectsOfType<MonoBehaviour>(true);

        List<ISaveable> saveableInterfaces = new List<ISaveable>();
        
        foreach (var mono in monoBehaviours) saveableInterfaces.AddRange(mono.GetComponentsInChildren<ISaveable>());

        int currentSaveID = 0;

        // Get the highest use saveID
        foreach (var saveable in saveableInterfaces)
        {
            if (saveable.SaveIDNumber >= 0 && saveable.SaveIDNumber >= currentSaveID)
                currentSaveID = saveable.SaveIDNumber + 1;
        }

        // Set each unset saveable saveID to the next number
        foreach (var saveable in saveableInterfaces)
            if (saveable.SaveIDNumber == -1)
            {
                saveable.SaveIDNumber = currentSaveID++;
                saveable.MarkAsDirty();
            }
        
        // Prevent duplicate IDs
        PreventDuplicateIDs(saveableInterfaces);
        foreach (var saveable in saveableInterfaces) Debug.Log(saveable.ToString() + " has an id of " + saveable.SaveIDNumber);
    }

    
    private static void PreventDuplicateIDs(List<ISaveable> saveableInterfaces)
    {
        if (saveableInterfaces.Count < 2) return;
        
        List<ISaveable> problematicSaveables = new List<ISaveable>();

        // get the next lowest unused id number
        int nextIDNumber = -1;
        foreach (var saveable in saveableInterfaces)
            if (saveable.SaveIDNumber >= nextIDNumber)
                nextIDNumber = saveable.SaveIDNumber + 1;

        for (int i = 0; i < saveableInterfaces.Count; i++)
        {
            // If a saveable is already known to be problematic, don't check it again as it would flag the first saveable (it should keep its id)
            if (problematicSaveables.Contains(saveableInterfaces[i])) continue;
            
            for (int j = 0; j < saveableInterfaces.Count; j++)
            {
                if (i == j) continue; // Don't compare a saveable to itself

                // If two saveables have the same ID, add the SECOND one to the problematic list (the first one keeps the id)
                if (saveableInterfaces[i].SaveIDNumber == saveableInterfaces[j].SaveIDNumber) problematicSaveables.Add(saveableInterfaces[j]);
            }
        }

        // Fix any duplicates
        if (problematicSaveables.Count > 0)
        {
            // Fix the problematic save IDs
            foreach (var problematicSaveable in problematicSaveables) problematicSaveable.SaveIDNumber = nextIDNumber++;
        }
    }
#endif
#endregion
}
