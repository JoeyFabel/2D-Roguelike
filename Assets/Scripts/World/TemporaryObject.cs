using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A <see cref="TemporaryObject"/> is an object that is loaded or destroyed after load depending on the save state
/// </summary>
public class TemporaryObject : SaveableObject
{
    private bool readyToBeDestroyed = false;

    public void DisableLoading()
    {
        readyToBeDestroyed = true;
    }

    public override WorldObjectSaveData GetSaveData()
    {
        TemporaryObjectSave data = new TemporaryObjectSave();

        data.destroyAfterLoad = readyToBeDestroyed;

        return data;
    }

    protected override void LoadData()
    {
        TemporaryObjectSave data = saveData as TemporaryObjectSave;

        if (data.destroyAfterLoad) Destroy(gameObject);

        isDoneLoading = true;
    }

    [System.Serializable]
    public class TemporaryObjectSave : WorldObjectSaveData
    {
        public bool destroyAfterLoad;
    }
}
