using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PeasantNPC : DialogTree
{
    public Item mushroomItem;

    [SerializeField]
    private int mushroomsGiven = 0;

    public MushroomReward[] possibleRewards;

    public DialogNode rewardGivenParentNode;

    private bool readyForReward = false;

    private KillableNPC healthManager;
    
    [SerializeField]
    private Queue<Item> rewardsAvailable = new Queue<Item>();
    
    protected override void Start()
    {
        if (started) return;
        
        base.Start();

        healthManager ??= GetComponent<KillableNPC>();
        //healthManager.OnBecomeHostile += OnBecomeHostile;
    }

    public void OnBecomeHostile()
    {
        faceTowardsPlayer = false;
        speechBubbleIcon.SetActive(false);

        interactable = false;
        
        print("becoming hostile!");
        
        enabled = false;
        
        player?.TryRemoveInteractable(this);
    }

    public void GivePeasantMushroom()
    {
        Inventory.LoseItem(mushroomItem);

        mushroomsGiven++;
        
        CheckForRewards();
    }

    public void GivePeasantAllMushrooms()
    {
        // Lose all mushrooms and give them to the NPC
        while (Inventory.PlayerHasItem(mushroomItem))
        {
            Inventory.LoseItem(mushroomItem);
            mushroomsGiven++;
        }
        
        CheckForRewards();
    }

    private void CheckForRewards()
    {
        bool rewardGiven = false;

        // Determine which rewards were earned
        for (int i = 0; i < possibleRewards.Length; i++)
        {
            if (possibleRewards[i].alreadyGained) continue;

            // Gain the reward
            if (mushroomsGiven >= possibleRewards[i].reqMushrooms)
            {
                // Using the queue allows multiple rewards to be gained if enough mushrooms were given, without accidentally missing some
                rewardsAvailable.Enqueue(possibleRewards[i].itemToGain);
                
                if (!rewardGiven) StartCoroutine(GiveRewardWhenReady());
                
                possibleRewards[i].alreadyGained = true;

                rewardGiven = true;
            }
        }
        
        if (rewardGiven) ForceSetCurrentNode(rewardGivenParentNode);
    }

    private IEnumerator GiveRewardWhenReady()
    {
        while (!readyForReward) yield return null;
        
        Item reward = rewardsAvailable.Dequeue();
        Inventory.GainItem(reward);
        
        readyForReward = false;
        
        // If another reward was earned at the same time, go again
        if (rewardsAvailable.Count > 0)
        {
            ForceSetCurrentNode(rewardGivenParentNode);

            StartCoroutine(GiveRewardWhenReady());
        }
    }
    
    public void GiveReward()
    {
        readyForReward = true;
    }

    public override WorldObjectSaveData GetSaveData()
    {
        PeasantNPCSaveData data = new PeasantNPCSaveData();

        data.numMushroomsGiven = mushroomsGiven;
        data.dialogData = base.GetSaveData() as DialogTreeSaveData;

        //data.currentHealth = healthManager.GetCurrentHealth();
        data.isHostile = healthManager.IsHostile();
        
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

        for (int i = 0; i < possibleRewards.Length; i++) if (mushroomsGiven > possibleRewards[i].reqMushrooms) possibleRewards[i].alreadyGained = true;
        
        //        healthManager.SetCurrentHealth(data.currentHealth);
        if (data.isHostile)
        {
            if (!started) healthManager ??= GetComponent<KillableNPC>();
            healthManager.SetAsHostile();
        }
       
        base.LoadData(data.dialogData);
    }

    private void OnDisable()
    {
        player?.TryRemoveInteractable(this);
    }

    [System.Serializable]
    public class PeasantNPCSaveData : WorldObjectSaveData
    {
        public int numMushroomsGiven;
        public DialogTreeSaveData dialogData;

        public bool isHostile;
    }

    [System.Serializable]
    public struct MushroomReward
    {
        public int reqMushrooms;
        public Item itemToGain;

        public bool alreadyGained;
    }
}
