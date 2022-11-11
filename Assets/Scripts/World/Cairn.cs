using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Cairn : Damageable, ISaveable
{
    [SerializeField] 
    private int saveID = -1;

    public int SaveIDNumber
    {
        get => saveID;
        set => saveID = value;
    }

    public bool DoneLoading { get; set; }
    
    public Sprite destroyedCairnSprite;
    public int minHealthToRubble = 2;

    private bool isDestroyed;
    private SpriteRenderer sprite;
    
    protected override void Start()
    {
        base.Start();
        sprite = GetComponent<SpriteRenderer>();
    }

    public override void ApplyDamage(float amount)
    {
        // When the cairn is hit, it takes damage
        // If health is too low, then it becomes rubble, and finally is destroyed
        currentHealth -= amount;

        if (currentHealth <= 0) Death();
        if (currentHealth <= minHealthToRubble) sprite.sprite = destroyedCairnSprite;
    }

    protected override void Death()
    {
        // Destroy the Cairn and increase the quest stage by 1
        audioSource.PlayOneShot(deathSounds[0]);
        sprite.sprite = destroyedCairnSprite;
        
        print("Cairn destroyed");

        int currentQuestStage = GameManager.GetQuestPhase((int)QuestStage.Quests.Cairns);
        GameManager.SaveQuest((int)QuestStage.Quests.Cairns, currentQuestStage + 1);

        sprite.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        isDestroyed = true;
        
        StartCoroutine(DisableAfterAudio());
    }

    private IEnumerator DisableAfterAudio()
    {
        while (audioSource.isPlaying) yield return null;
        
        gameObject.SetActive(false);
    }
    
    public WorldObjectSaveData GetSaveData()
    {
        DestructibleRock.DestructibleRockSaveData saveData = new DestructibleRock.DestructibleRockSaveData();
        saveData.isDestroyed = isDestroyed;

        if (isDestroyed)print("The cairn is destroyed");
        else print("The cairn is not destroyed");
        
        return saveData;
    }

    public void LoadData(WorldObjectSaveData saveData)
    {
        DestructibleRock.DestructibleRockSaveData data = saveData as DestructibleRock.DestructibleRockSaveData;

        DoneLoading = true;

        if (data.isDestroyed)
        {
            Destroy(gameObject);
        }
    }
    
    private void Awake()
    {
        SaveManager.RegisterSaveable(this);
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
