using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An interface that allows data to be saved.
/// Warning - Make sure you add an instance of your class to the SaveManager on awake,
/// and make sure you remove it OnDestroy
/// </summary>
public interface ISaveable
{
    public WorldObjectSaveData GetSaveData();
    
    public void LoadData(WorldObjectSaveData saveData);

    public int SaveIDNumber
    {
        get;
        set;
    }

    public bool DoneLoading
    {
        get;
        set;
    }

    // Force you to implement these
    public void Awake();

    public void OnDestroy();
}
