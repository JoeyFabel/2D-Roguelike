using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class QuestStage : MonoBehaviour
{
     public Quests quest;
     
     [Tooltip("This object will be destroyed if the quest for this character is not in this stage. Note that this may need to be -1")]
     public int activeQuestStageNumber = 0;
     
     [Tooltip("Anything that should be performed in addition to saving the quest data.")]
     public UnityEvent OnQuestStageCompleted;

     public Item[] itemsToGain;
     
     
     private void Start()
     {
          if (activeQuestStageNumber != GameManager.GetQuestPhase((int)quest)) Destroy(gameObject);
     }

     public void MarkStageAsComplete()
     {
          GameManager.SaveQuest((int)quest, activeQuestStageNumber + 1);
          
          OnQuestStageCompleted?.Invoke();

          foreach (var item in itemsToGain) Inventory.GainItem(item);
     }

     public enum Quests
     {
          Skeleton,
     }
}
