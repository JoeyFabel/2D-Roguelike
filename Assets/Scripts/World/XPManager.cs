using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XPManager : MonoBehaviour
{
    private static XPManager instance;
    public static bool gamePausedForLevelUp;

    public static System.Action OnLevelUp;

    public Text xpText;
    public Text levelText;

    public GameObject levelUpPanel;
    public Selectable firstSelectedButton;

    public GameObject xpGainedEffectPrefab;

    private const int MAX_LEVEL = 99;

    private int currentXP;
    private int currentLevel;

    private int hpLevelUpIncreases = 0;
    private int attackLevelUpIncreases = 0;
    private int mpLevelUpIncreases = 0;

    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(this);

        levelUpPanel.SetActive(false);
    }

    public static void GainXP(int experience, Vector3 worldLocation)
    {
        instance.currentXP += experience;

        GameObject levelUpVFX = Instantiate(instance.xpGainedEffectPrefab, worldLocation, Quaternion.identity);

        levelUpVFX.GetComponentInChildren<Text>().text = experience.ToString();

        if (instance.currentXP >= instance.GetXPToNextLevel(instance.currentLevel + 1))
        {
            // Level Up!
            instance.LevelUp();

            //instance.StartCoroutine(instance.LevelUpAfterVFX(levelUpVFX, 2f));            
        }

        instance.UpdateTexts();
    }

    private IEnumerator LevelUpAfterVFX(GameObject vfxObject, float vfxTime)
    {
        float enterTime = Time.time;

        while (Time.time - enterTime < vfxTime)
        {
            yield return null;
        }

        LevelUp();
    }

    private void LevelUp()
    {
        currentXP -= GetXPToNextLevel(currentLevel + 1);
        
        currentLevel++;

        instance.UpdateTexts();

        CharacterSelector.GetPlayerController().DisableControlsForUI();

        levelUpPanel.SetActive(true);

        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        firstSelectedButton.Select();

        StartCoroutine(PauseForLevelUpSelection());

        // Level up -- choose more hp, more attack, or more magic (if magic user)
        // Different levels for each character?
    }

    private IEnumerator PauseForLevelUpSelection()
    {
        gamePausedForLevelUp = true;
        Time.timeScale = 0;
        while (levelUpPanel.activeSelf)
        {
            yield return null;
        }

        gamePausedForLevelUp = false;
        OnLevelUp?.Invoke();
        Time.timeScale = 1;
        CharacterSelector.GetPlayerController().EnableControlsAfterUI();
    }

    public void ChooseExtraHP()
    {
        hpLevelUpIncreases++;

        levelUpPanel.SetActive(false);        
    }

    public void ChooseExtraMP()
    {
        mpLevelUpIncreases++;

        levelUpPanel.SetActive(false);
    }

    public void ChooseExtraAttack()
    {
        attackLevelUpIncreases++;

        levelUpPanel.SetActive(false);
    }

    /// <summary>
    /// Get the amount of experience required for the next level up. This is the experience difference, not the total required xp.
    /// </summary>
    /// <param name="level">The next level for the player to reach</param>
    /// <returns></returns>
    private int GetXPToNextLevel(int level)
    {
        // no more leveling up
        if (level > MAX_LEVEL) return 0;

        return (int)Mathf.Floor(level - 1 + (300f * Mathf.Pow(2f,(level - 1)/7f))) / 4;
    }

    public static int GetCurrentXP()
    {
        return instance.currentXP;
    }

    public static int GetCurrentLevel()
    {
        return instance.currentXP;
    }

    private void UpdateTexts()
    {
        levelText.text = "Level " + currentLevel;
        xpText.text = (instance.GetXPToNextLevel(currentLevel + 1) - currentXP) + " EXP to next level.";
    }

    public static int GetHPLevelUps()
    {
        return instance.hpLevelUpIncreases;
    }

    public static int GetAttackLevelUps()
    {
        return instance.attackLevelUpIncreases;
    }

    public static int GetMPLevelUps()
    {
        return instance.mpLevelUpIncreases;
    }

    public static XPSaveData GetSaveData()
    {
        XPSaveData data = new XPSaveData();

        data.currentExp = instance.currentXP;
        data.currentLevel = instance.currentLevel;

        data.attackLevelUps = instance.attackLevelUpIncreases;
        data.hpLevelUps = instance.hpLevelUpIncreases;
        data.mpLevelUps = instance.mpLevelUpIncreases;

        return data;
    }

    public static void LoadXPData(XPSaveData data)
    {
        if (data == null)
        {
            instance.currentLevel = 1;
            instance.currentXP = 0;

            instance.attackLevelUpIncreases = 0;
            instance.hpLevelUpIncreases = 0;
            instance.mpLevelUpIncreases = 0;

            instance.UpdateTexts();
            return;
        }

        instance.currentXP = data.currentExp;
        instance.currentLevel = data.currentLevel;

        instance.attackLevelUpIncreases = data.attackLevelUps;
        instance.hpLevelUpIncreases = data.hpLevelUps;
        instance.mpLevelUpIncreases = data.mpLevelUps;

        instance.UpdateTexts();
    }
}
