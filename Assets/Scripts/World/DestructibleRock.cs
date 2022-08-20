using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleRock : SaveableObject
{
    public void DestroyRock()
    {
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

    public override WorldObjectSaveData GetSaveData()
    {
        // The rock is disabled until the save can be grabbed, then will be destroyed upon future loads
        DestructibleRockSaveData data = new DestructibleRockSaveData();
        data.isDestroyed = !gameObject.activeSelf;

        return data;
    }

    protected override void LoadData()
    {
        DestructibleRockSaveData data = saveData as DestructibleRockSaveData;

        // Needs to be marked as done loading before the object gets destroyed
        isDoneLoading = true;
        
        if (data.isDestroyed) Destroy(gameObject);
    }

    [Serializable]
    public class DestructibleRockSaveData : WorldObjectSaveData
    {
        public bool isDestroyed;
    }
}
