using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleRock : MonoBehaviour, ISaveable
{
    public void Awake()
    {
        SaveManager.RegisterSaveable(this);
    }

    public void DestroyRock()
    {
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

    public WorldObjectSaveData GetSaveData()
    {
        // The rock is disabled until the save can be grabbed, then will be destroyed upon future loads
        DestructibleRockSaveData data = new DestructibleRockSaveData();
        data.isDestroyed = !gameObject.activeSelf;

        return data;
    }

    public void LoadData(WorldObjectSaveData saveData)
    {
        print("Loading data for " + gameObject.name);
        
        DestructibleRockSaveData data = saveData as DestructibleRockSaveData;

        // Needs to be marked as done loading before the object gets destroyed
        DoneLoading = true;
        
        if (data.isDestroyed) Destroy(gameObject);
    }

    public int SaveIDNumber { get; set; }
    public bool DoneLoading { get; set; }

    [Serializable]
    public class DestructibleRockSaveData : WorldObjectSaveData
    {
        public bool isDestroyed;
    }

    public void OnDestroy()
    {
        SaveManager.UnRegisterSaveable(this);
    }
    
    
}
