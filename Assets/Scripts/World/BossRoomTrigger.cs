using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

public class BossRoomTrigger : MonoBehaviour, ISaveable
{
    public Boss boss;

    /// <summary>
    /// This should include things like disabling colliders, spawning/enabling the boss, or playing an opening animation
    /// </summary>
    public UnityEvent OnBossRoomEntered;
    public UnityEvent OnBossDefeated;

    private bool bossDefeated = false;
    private bool bossRoomEntered = false;

    [SerializeField]
    private int saveID = -1;

    public bool DoneLoading { get; set; }
    public int SaveIDNumber { get => saveID; set => saveID = value; }

    private void Awake()
    {
        SaveManager.RegisterSaveable(this);

        boss.SetBossRoomTrigger(this);
    }

    private void OnDestroy()
    {
        SaveManager.UnRegisterSaveable(this);
    }

    private void Start()
    {
        bossRoomEntered = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!bossRoomEntered && collision.TryGetComponent<PlayerController>(out var player))
        {
            OnBossRoomEntered.Invoke();
            bossRoomEntered = true;
        }
    }

    public void OnBossDeath()
    {
        bossDefeated = true;
        OnBossDefeated.Invoke();
    }

    public WorldObjectSaveData GetSaveData()
    {
        BossSaveData saveData = new BossSaveData();
        saveData.bossDefeated = bossDefeated;

        Debug.Log("saved a boss room trigger " + gameObject.name, gameObject);
        
        return saveData;
    }

    public void LoadData(WorldObjectSaveData saveData)
    {
        var data = saveData as BossSaveData;

        bossDefeated = data.bossDefeated;

        DoneLoading = true;
    }
    
    [System.Serializable]
    public class BossSaveData : WorldObjectSaveData
    {
        public bool bossDefeated;
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
