using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

[Obsolete("Saveable objects are no longer used. Use ISaveable interface instead", true)]
public abstract class SaveableObject : MonoBehaviour
{
    protected WorldObjectSaveData saveData;

    [SerializeField]
    private int saveIDNumber = - 1;
    protected bool isDoneLoading = false;

    protected bool started = false;

    public abstract WorldObjectSaveData GetSaveData();

    /// <summary>
    /// Load and initialize any values from the save data as needed.
    /// Make sure you set isDoneLoading to true once you are done
    /// </summary>
    protected abstract void LoadData();

    public string SaveID()
    {
        return SceneManager.GetActiveScene().name + ": " + saveIDNumber;
    }

    // Note that start should always run before the data is loaded. If there is no data, then start will run afterwards

    public void GrabSaveDataReference(Dictionary<string, WorldObjectSaveData> dataDictionary)
    {
        if (dataDictionary == null) return;

        if (dataDictionary.ContainsKey(SaveID()))
        {
            saveData = dataDictionary[SaveID()];
            isDoneLoading = false;
            if (!started) Start();
            LoadData();
        }
        else isDoneLoading = true;
    }

    public static bool IsLoading()
    {
        bool doneLoading = true;

        foreach (var saveable in instances)
        {
            doneLoading = doneLoading && saveable.isDoneLoading;
        }

        // print("is it done loading? " + doneLoading);
        return !doneLoading;
    }

    private static readonly HashSet<SaveableObject> instances = new HashSet<SaveableObject>();

    // public read-only access to the instances by only providing a clone
    // of the HashSet so nobody can remove items from the outside
    public static HashSet<SaveableObject> Instances => new HashSet<SaveableObject>(instances);

    protected virtual void Awake()
    {
        // simply register yourself to the existing instances
        instances.Add(this);
    }

    /// <summary>
    /// Note that this start method will run before the save data is loaded.
    /// If no save data is found, make sure any necessary values are set in this method
    /// </summary>
    protected virtual void Start()
    {
        started = true;
    }

    protected virtual void OnDestroy()
    {
        // don't forget to remove yourself at the end of your lifetime
        instances.Remove(this);
    }

    #region Editor Methods
#if UNITY_EDITOR

    [ContextMenu("Assign IDs to saveable world objects")]
    private void AssignIDs()
    {
        int nextID = 0;

        SaveableObject[] saveableWorldObjects = FindObjectsOfType<SaveableObject>();

        foreach (var saveable in saveableWorldObjects)
        {
            if (saveable.saveIDNumber >= nextID) nextID = saveable.saveIDNumber + 1;                       
        }


        foreach (var saveable in saveableWorldObjects)
        {
            if (saveable.saveIDNumber == -1) saveable.saveIDNumber = nextID++;
        }
    }
#endif
    #endregion
}

/*
public interface ISaveable
{
    public int SaveIdNumber
    {
        get;
        set;
    }

    public WorldObjectSaveData SaveData
    {
        get;
        set;
    }
}


public int SaveIdNumber
{ get; set; }

public WorldObjectSaveData SaveData
{
    get { return GetSaveData(); }
    set { saveData = value; }
}
    
#if UNITY_EDITOR
[ContextMenu("Set Save Interface ID")]
public void SetSaveInterfaceID()
{
    MonoBehaviour[] monoBehaviours = FindObjectsOfType<MonoBehaviour>();

    List<ISaveable> saveableInterfaces = new List<ISaveable>();
        
    foreach (var mono in monoBehaviours) saveableInterfaces.AddRange(mono.GetComponentsInChildren<ISaveable>());

    int currentSaveID = 0;

    foreach (var saveable in saveableInterfaces) saveable.SaveIdNumber = currentSaveID++;
        
    foreach (var saveable in saveableInterfaces) Debug.Log(saveable.ToString() + " has an id of " + saveable.SaveIdNumber);
}
#endif */