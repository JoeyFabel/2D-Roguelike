using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A <see cref="TemporaryObject"/> is an object that is loaded or destroyed after load depending on the save state
/// </summary>
public class TemporaryObject : MonoBehaviour, ISaveable
{
    private bool readyToBeDestroyed = false;

    [SerializeField]
    private int saveID = -1;
    public int SaveIDNumber
    {
        get => saveID;
        set => saveID = value;
    }
    
    public bool DoneLoading { get; set; }

    private void Awake()
    {
        SaveManager.RegisterSaveable(this);
    }

    private void OnDestroy()
    {
        SaveManager.UnRegisterSaveable(this);
    }

    public void DisableLoading()
    {
        readyToBeDestroyed = true;
    }

    public WorldObjectSaveData GetSaveData()
    {
        TemporaryObjectSave data = new TemporaryObjectSave();

        data.destroyAfterLoad = readyToBeDestroyed;

        return data;
    }

    public void LoadData(WorldObjectSaveData saveData)
    {
        TemporaryObjectSave data = saveData as TemporaryObjectSave;

        if (data.destroyAfterLoad) Destroy(gameObject);

        DoneLoading = true;
    }

    [System.Serializable]
    public class TemporaryObjectSave : WorldObjectSaveData
    {
        public bool destroyAfterLoad;
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
