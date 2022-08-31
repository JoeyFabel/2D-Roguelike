using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PeasantNPC : DialogTree
{
    private int mushroomsGiven = 0;
    
    public override WorldObjectSaveData GetSaveData()
    {
        PeasantNPCSaveData data = new PeasantNPCSaveData();

        data.numMushroomsGiven = mushroomsGiven;
        data.dialogData = base.GetSaveData() as DialogTreeSaveData;

        return data;
    }

    public override void LoadData(WorldObjectSaveData saveData)
    {
        PeasantNPCSaveData data = saveData as PeasantNPCSaveData;

        if (data == null)
        {
            return;
        }
        
        mushroomsGiven = data.numMushroomsGiven;
        base.LoadData(data.dialogData);
    }

    [System.Serializable]
    public class PeasantNPCSaveData : WorldObjectSaveData
    {
        public int numMushroomsGiven;
        public DialogTreeSaveData dialogData;
    }

}
