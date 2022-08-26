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
    public Dictionary<int, int> questSaves;
    public Dictionary<int, int> inventoryData;
    public int currentMoney;

    public XPSaveData xpData;
}

// NOTE - A quest save system could be implemented with a simple int saved.
// Each character has a quest-character script (Instead of TemporaryObject).
//       That quest-character script asks which stage of the quest this character is active in
//       If the quest-character's save-data quest step matches, then the object remains. otherwise destroy that object
// After that portion of the quest is completed, the quest state int is increased by 1 and saved (when game saves)
//      Note that if a character will be in the same place at different times, multiple characters could be put in the same place
//      Alternately, depending on how the script works, just that instance of the script could be destroyed and other quest-stage scripts left

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

[System.Serializable]
public class InputSaveData
{
    
}

/*
[System.Serializable]
public class QuestSaveData
{
    public int questPhase;
}
*/

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
