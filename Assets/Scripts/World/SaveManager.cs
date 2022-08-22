using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
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
    [ContextMenu("Assign IDs to ISaveables")]
    private void AssignIDs()
    {
        MonoBehaviour[] monoBehaviours = FindObjectsOfType<MonoBehaviour>(true);

        List<ISaveable> saveableInterfaces = new List<ISaveable>();
        
        foreach (var mono in monoBehaviours) saveableInterfaces.AddRange(mono.GetComponentsInChildren<ISaveable>());

        int currentSaveID = 0;

        foreach (var saveable in saveableInterfaces)
            if (saveable.SaveIDNumber == -1)
            {
                saveable.SaveIDNumber = currentSaveID++;
                saveable.MarkAsDirty();
            }
        
        
        foreach (var saveable in saveableInterfaces) Debug.Log(saveable.ToString() + " has an id of " + saveable.SaveIDNumber);
    }
    #endif
    #endregion
}
