using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // The shrine will always be loaded into, but this is what is loaded outside of the shrine
    public string lastActiveSceneName;
    public string playerCharacterName;

    public SerializableVector3 playersLastPosition;
    public Dictionary<string, WorldObjectSaveData> worldObjectSaves;
    public Dictionary<int, int> inventoryData;
    public int currentMoney;

    public XPSaveData xpData;
}

[System.Serializable]
public class SceneData
{
    public string sceneName;
    // store things like open gates and unlocked doors
}

/// <summary>
/// This class stores information for the player to carry over between scenes, but is not saved to the save file.
/// </summary>
public class PlayerLoadData
{
    public int currentHealth;
    public int numProjectiles; // throwing spears, bullets, etc.
    public int currentMagic;
}

[System.Serializable]
public class XPSaveData
{
    public int currentExp;
    public int currentLevel;

    public int hpLevelUps;
    public int mpLevelUps;
    public int attackLevelUps;
}

[System.Serializable]
public abstract class WorldObjectSaveData
{
    public int saveID;
}

#region Serializable Data Structs
[System.Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float xValue, float yValue, float zValue)
    {
        x = xValue;
        y = yValue;
        z = zValue;        
    }

    public static implicit operator SerializableVector3(Vector3 vector)
    {
        return new SerializableVector3(vector.x, vector.y, vector.z);
    }

    public static implicit operator Vector3(SerializableVector3 vector)
    {
        return new Vector3(vector.x, vector.y, vector.z);
    }
    #endregion
}
