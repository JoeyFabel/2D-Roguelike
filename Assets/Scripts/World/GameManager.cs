using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Constants
    private const string SaveFileName = "save.dat";
    #endregion

    public Image fadeImage;
    public CanvasGroup gameSavedImage;
    public float fadeTime = 1f;

    public Settings settings;

    private static GameManager instance;
    private CharacterSelector characterSelector;
    private Inventory playerInventory;

    private AudioSource backgroundMusicSource;

#if UNITY_EDITOR
    [SerializeField]
#endif
    private string lastSceneName;
    /// <summary>
    /// The player's last known position in the scene outside of the shrine
    /// </summary>
    private Vector3 playersLastPosition;

    private bool setPlayerPosition;
    
    private PlayerLoadData playerLoadData;
    private Dictionary<string, WorldObjectSaveData> saveableObjectDataDictionary;
    private Dictionary<int, int> questDataDictionary;

#if UNITY_EDITOR
    [Header("Testings Helpers")]
    public bool useCustomSpawnPoint;
    [Tooltip("Note - this ignores the save data location")]
    public Vector3 playerSpawnPosition;

#endif

    private void Awake()
    {
        Debug.LogWarning("TODO - Add some NPC quests");
        Debug.LogWarning("TODO - Add a  shop");
        Debug.LogWarning("TODO - Add 1x buy of water in a store");
        Debug.LogWarning("TODO - Add quick item HUD");
        Debug.LogWarning("TODO - Let projectiles destroy bombs early");
        Debug.LogWarning("TODO - Allow user to change keybindings from settings");
        Debug.LogWarning("TODO - Allow projectiles to pass over ground-to-water collisions (tile collision data exists, use it)");
        
        // Create the singleton or destroy the duplicate
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            characterSelector = GetComponent<CharacterSelector>();
            playerInventory = GetComponent<Inventory>();
            playerInventory.InitializeInventory();

            // make image black
            fadeImage.gameObject.SetActive(true);
            gameSavedImage.gameObject.SetActive(false);

            backgroundMusicSource = GetComponent<AudioSource>();

            playerLoadData = null;
            saveableObjectDataDictionary = new Dictionary<string, WorldObjectSaveData>();

            questDataDictionary = new Dictionary<int, int>();

            SceneManager.sceneLoaded += OnSceneLoaded;

            settings.LoadSettings();
            LoadSave();
            GetComponentInChildren<DialogManager>().InitializeDialogManager();
        }
        else Destroy(gameObject);
    }

    private void OnSceneLoaded(Scene loadedScene, LoadSceneMode loadMode)
    {
        HandleBGMusic();
        HandlePlayer();
        HandleSaveableObjects();

        StartCoroutine(FadeInScreen());
    }

    private void HandlePlayer()
    {
        characterSelector.SpawnCharacter();

        PlayerController player = CharacterSelector.GetPlayerController();
        
        if (setPlayerPosition) player.transform.position = playersLastPosition;

        if (playerLoadData != null) player.LoadPlayerData(playerLoadData);

#if UNITY_EDITOR
        if (useCustomSpawnPoint)
        {
            player.transform.position = playerSpawnPosition;
            useCustomSpawnPoint = false;
        }
#endif
    }

    private void HandleSaveableObjects()
    {
        foreach (var saveableObject in SaveableObject.Instances)
        {
            saveableObject.GrabSaveDataReference(instance.saveableObjectDataDictionary);
        }
    }

    private void HandleBGMusic()
    {
        AudioClip newBgSong = FindObjectOfType<BackgroundMusic>().backgroundSong;
        
        if (!newBgSong.name.Equals(backgroundMusicSource.clip?.name))
        {
            backgroundMusicSource.clip = newBgSong;
            backgroundMusicSource.Play();
        }
        //BackgroundMusic newBGMusic = FindObjectOfType<BackgroundMusic>();
        //newBGMusic.SetMusicToTime(oldBgName, oldBgTimestamp);
    }

    public static void LoadScene(string sceneName, Vector2 playerOffset)
    {
        instance.lastSceneName = SceneManager.GetActiveScene().name;

        PlayerController player = CharacterSelector.GetPlayerController();

        instance.playersLastPosition = player.transform.position + (Vector3)playerOffset;
        // Notice -- This should be used for transitions not involving the shrine (world-to-world)
        // This means that the trigger to load into one world should be positioned such that when entering it, you would not spawn inside the trigger in the new scene
        // E.g. Scene 1:                <->[trigger]   |
        //      Scene 2:       [trigger]<->            | No overlap between triggers       

        instance.setPlayerPosition = true;

        instance.playerLoadData = player.GetPlayerLoadData();
        instance.SaveWorldObjects();

        instance.StartCoroutine(LoadSceneAfterFade(sceneName));
    }

    public static void LoadIntoShrine(Vector3 playerPosition, PlayerLoadData playerData)
    {
        instance.lastSceneName = SceneManager.GetActiveScene().name;
        instance.playersLastPosition = playerPosition;
        instance.setPlayerPosition = false;
        instance.playerLoadData = playerData;
        instance.SaveWorldObjects();

        instance.StartCoroutine(LoadSceneAfterFade("Shrine"));
    }

    public static void LoadOutOfShrine()
    {
        instance.setPlayerPosition = true;

        // Dont remember the last scene name or player position if loading out of the shrine

        instance.playerLoadData = CharacterSelector.GetPlayerController().GetPlayerLoadData();

        instance.StartCoroutine(LoadSceneAfterFade(instance.lastSceneName));
    }

    /// <summary>
    /// Reloads you into the same scene, should be the shrine, and switches the player character on the load.
    /// </summary>
    /// <param name="newCharacterName">The name of the character to switch to</param>
    public static void SwitchToCharacter(string newCharacterName)
    {
        instance.characterSelector.SetCharacterName(newCharacterName);
        instance.playerLoadData = null;

        //LoadScene(SceneManager.GetActiveScene().name);
        //LoadIntoShrine(instance.playersLastPosition, null);

        instance.setPlayerPosition = false;
        instance.playerLoadData = null;

        instance.StartCoroutine(LoadSceneAfterFade("Shrine"));
    }

    public static string GetPlayerCharacterName()
    {
        return instance.characterSelector.GetCharacterName();
    }

    public static void CreateSave()
    {
        // Create a new save data with:
        // - the last active scene name (not the shrine, but whats outside the shrine)
        //- The current selected character

        SaveData data = new SaveData();
        data.lastActiveSceneName = instance.lastSceneName;
        data.playerCharacterName = instance.characterSelector.GetCharacterName();
        data.playersLastPosition = instance.playersLastPosition;

        data.xpData = XPManager.GetSaveData();

        instance.SaveWorldObjects();
        data.worldObjectSaves = instance.saveableObjectDataDictionary;

        data.questSaves = instance.questDataDictionary;
        
        data.inventoryData = instance.playerInventory.GetInventorySaveData();
        data.currentMoney = Inventory.GetCurrentMoney();

        // Create the file
        FileStream file = File.Create(Application.persistentDataPath + Path.AltDirectorySeparatorChar + SaveFileName);

        // Save it and encrypt it
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        
        // and close the file
        file.Close();

        print("Saved to file " + Application.persistentDataPath + Path.AltDirectorySeparatorChar + SaveFileName);

        instance.StartCoroutine(instance.ShowGameSavedMessage(2f));
    }

    /// <summary>
    /// Saves the world objects in this scene into the dictionary.
    /// Should be done before loading out of a scene to make sure the data is temporarily stored, to be saved when the game saves or lost if the player dies.
    /// </summary>
    private void SaveWorldObjects()
    {
        if (instance.saveableObjectDataDictionary == null)
        {
            // should already  be loaded
            Debug.LogError("ERROR -- The saveableObjectDictionary was non-existant before trying to save!");
            instance.saveableObjectDataDictionary = new Dictionary<string, WorldObjectSaveData>();
        }

        foreach (var saveable in SaveableObject.Instances)
        {
            // Overwrite an old key, or add a new key
            if (instance.saveableObjectDataDictionary.ContainsKey(saveable.SaveID())) instance.saveableObjectDataDictionary[saveable.SaveID()] = saveable.GetSaveData();
            else instance.saveableObjectDataDictionary.Add(saveable.SaveID(), saveable.GetSaveData());
        }
    }

    private void LoadSave()
    {
        //if (!Directory.Exists(Application.persistentDataPath)) Directory.CreateDirectory(Application.persistentDataPath);

        string filePath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + SaveFileName;

        if (File.Exists(filePath))
        {
            FileStream file = File.OpenRead(filePath);

            // Grab the save
            BinaryFormatter bf = new BinaryFormatter();
            SaveData saveData = bf.Deserialize(file) as SaveData;
            file.Close();

            // Now use its data

            characterSelector.SetCharacterName(saveData.playerCharacterName);
            lastSceneName = saveData.lastActiveSceneName;
            playersLastPosition = saveData.playersLastPosition;
            instance.saveableObjectDataDictionary = saveData.worldObjectSaves;
            instance.questDataDictionary = saveData.questSaves;
            instance.playerInventory.LoadInventoryFromData(saveData.inventoryData, saveData.currentMoney);
            XPManager.LoadXPData(saveData.xpData);
        }
        else
        {
            print("No save found!");
            instance.saveableObjectDataDictionary = new Dictionary<string, WorldObjectSaveData>();
            instance.questDataDictionary = new Dictionary<int, int>();
#if UNITY_EDITOR
            // instance.characterSelector.SetCharacterName("Dwarvish Thunderer");
            instance.characterSelector.UseDefaultCharacter();
#endif
        }
    }

    /// <summary>
    /// Get the current phase of the specified quest.
    /// Returns -1 if the quest does not exist in the save data yet.
    /// </summary>
    /// <param name="quest">The quest to check</param>
    /// <returns>The current phase that the quest is in</returns>
    public static int GetQuestPhase(int quest)
    {
        if (instance.questDataDictionary.ContainsKey(quest))
            return instance.questDataDictionary[quest];
        else return -1;
    }

    public static void SaveQuest(int quest, int questPhase)
    {
        if (instance.questDataDictionary.ContainsKey(quest)) instance.questDataDictionary[quest] = questPhase;
        else instance.questDataDictionary.Add(quest, questPhase);
    }
    
    private static IEnumerator LoadSceneAfterFade(string sceneName)
    {
        Time.timeScale = 0;

        yield return null;

        Color fadeColor = Color.black;
        fadeColor.a = instance.fadeImage.color.a;

        while (instance.fadeImage.color.a < 1f)
        {
            fadeColor.a += 1 / instance.fadeTime * Time.unscaledDeltaTime;

            instance.fadeImage.color = fadeColor;
            yield return null;
        }

        // Force to transparent in case error is too big
        fadeColor.a = 1;
        instance.fadeImage.color = fadeColor;

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeInScreen()
    {
        yield return StartCoroutine(WaitForLoadingToFinish());

        Color fadeColor = Color.black;
        fadeColor.a = fadeImage.color.a;

        while (fadeImage.color.a > 0.01f)
        {        
            fadeColor.a -= 0.75f / fadeTime * Time.unscaledDeltaTime;

            fadeImage.color = fadeColor;
            yield return null;
        }

        // Force to transparent in case error is too big
        fadeColor.a = 0;

        fadeImage.color = fadeColor;        

        Time.timeScale = 1;
    }

    private IEnumerator WaitForLoadingToFinish()
    {
        while (SaveableObject.IsLoading()) yield return null;
    }

    private IEnumerator ShowGameSavedMessage(float activeDuration)
    {
        gameSavedImage.alpha = 0;
        gameSavedImage.gameObject.SetActive(true);

        while (gameSavedImage.alpha < 1)
        {
            gameSavedImage.alpha += 1 / fadeTime * Time.deltaTime;

            yield return null;
        }

        gameSavedImage.alpha = 1f;

        yield return new WaitForSeconds(activeDuration);

        while (gameSavedImage.alpha > 0.01f)
        {
            gameSavedImage.alpha -= 1 / fadeTime * Time.deltaTime;

            yield return null;
        }

        gameSavedImage.alpha = 0;
        gameSavedImage.gameObject.SetActive(false);
    }

#if UNITY_EDITOR

    [UnityEditor.MenuItem("Save Management/Open Save File Location")]
    private static void OpenSaveFileLocation()
    {
        string itemPath = Application.persistentDataPath;
        itemPath = itemPath.Replace(@"/", @"\");

        System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
    }

    [UnityEditor.MenuItem("Save Management/Delete Save File")]
    private static void DeleteSaveFile()
    {
        if (UnityEditor.EditorUtility.DisplayDialog("Delete Save File?", "Are you sure you want to delete your save file?", "Yes", "No"))
        {
            string itemPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + SaveFileName;

            File.Delete(itemPath);
        }
    }
#endif
}
