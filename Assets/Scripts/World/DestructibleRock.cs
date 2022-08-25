using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DestructibleRock : MonoBehaviour, ISaveable
{
    [SerializeField]
    private int saveID = -1;
    
    private void Awake()
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
        DestructibleRockSaveData data = saveData as DestructibleRockSaveData;

        // Needs to be marked as done loading before the object gets destroyed
        DoneLoading = true;
        
        if (data.isDestroyed) Destroy(gameObject);
    }

    public int SaveIDNumber { get => saveID; set => saveID = value; }
    public bool DoneLoading { get; set; }

    [Serializable]
    public class DestructibleRockSaveData : WorldObjectSaveData
    {
        public bool isDestroyed;
    }

    private void OnDestroy()
    {
        SaveManager.UnRegisterSaveable(this);
    }
    
#if UNITY_EDITOR
    public void MarkAsDirty()
    {
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Changed saveID");
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }
#endif  
}
