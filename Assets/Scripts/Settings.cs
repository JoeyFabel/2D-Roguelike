using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.IO;

public class Settings : MonoBehaviour
{
    private const int minDecibels = -50;
    private const int maxDecibels = 0;
    private const string SettingsFileName = "Settings.json";

    public Text masterVolumePercentageText;
    public Text musicVolumePercentageText;
    public Text soundFXVolumePercentageText;

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundFXVolumeSlider;

    public AudioMixer audioMixer;

    class SettingsSaveData
    {
        public float masterVolume;
        public float musicVolume;
        public float soundFXVolume;
    }

    // Data saving variable
    private string persistentPath = "";

    // Volume Settings
    private float masterVolumePercentage = 1f;
    private float musicVolumePercentage = 1f;
    private float soundFXVolumePercentage = 1f;

    public void LoadSettings()
    {
        persistentPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + SettingsFileName;

        // errors if this cant find the file

        if (File.Exists(persistentPath)) 
        {
            using StreamReader reader = new StreamReader(persistentPath);

            string json = reader.ReadToEnd();        

            SettingsSaveData data = JsonUtility.FromJson<SettingsSaveData>(json);

            if (data != null)
            {
                masterVolumePercentage = data.masterVolume;
                musicVolumePercentage = data.musicVolume;
                soundFXVolumePercentage = data.soundFXVolume;

                masterVolumeSlider.value = masterVolumePercentage;
                musicVolumeSlider.value = musicVolumePercentage;
                soundFXVolumeSlider.value = soundFXVolumePercentage;
            }
        }        

        StartCoroutine(LoadVolumeSettingsIntoMixer());
    }

    private IEnumerator LoadVolumeSettingsIntoMixer()
    {
        yield return null;

        // This object has to be enabled for this to work and cannot be done in awake, one frame must be waited first
        audioMixer.SetFloat("Master Volume", Mathf.Log(masterVolumePercentage) * 20);
        audioMixer.SetFloat("Music Volume", Mathf.Log(musicVolumePercentage) * 20);
        audioMixer.SetFloat("SFX Volume", Mathf.Log(soundFXVolumePercentage) * 20);
    }

    public void SaveSettings()
    {
        SettingsSaveData settingsData = new SettingsSaveData();
        settingsData.masterVolume = masterVolumePercentage;
        settingsData.musicVolume = musicVolumePercentage;
        settingsData.soundFXVolume = soundFXVolumePercentage;

        string savePath = persistentPath;

        string json = JsonUtility.ToJson(settingsData);

        using StreamWriter writer = new StreamWriter(savePath);
        writer.Write(json);

    }

    public void ChangeMasterVolume(float volumePercentage)
    {
        // Note -- the slider should go from 0 to 1
        masterVolumePercentageText.text = (int)(volumePercentage * 100) + " %";

        masterVolumePercentage = volumePercentage;

        audioMixer.SetFloat("Master Volume", Mathf.Log(volumePercentage) * 20);
    }

    public void ChangeMusicVolume(float volumePercentage)
    {
        musicVolumePercentageText.text = (int)(volumePercentage * 100) + " %";

        musicVolumePercentage = volumePercentage;

        audioMixer.SetFloat("Music Volume", Mathf.Log(volumePercentage) * 20);
    }

    public void ChangeSoundFXVolume(float volumePercentage)
    {
        soundFXVolumePercentageText.text = (int)(volumePercentage * 100) + " %";

        soundFXVolumePercentage = volumePercentage;

        audioMixer.SetFloat("SFX Volume", Mathf.Log(volumePercentage) * 20);
    }
}
