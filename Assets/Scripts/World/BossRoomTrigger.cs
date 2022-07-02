using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossRoomTrigger : SaveableObject
{
    public Boss boss;

    /// <summary>
    /// This should include things like disabling colliders, spawning/enabling the boss, or playing an opening animation
    /// </summary>
    public UnityEvent OnBossRoomEntered;
    public UnityEvent OnBossDefeated;

    private bool bossDefeated = false;
    private bool bossRoomEntered = false;

    protected override void Awake()
    {
        base.Awake();

        boss.SetBossRoomTrigger(this);
    }

    protected override void Start()
    {
        bossRoomEntered = false;

        base.Start();
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

    public override WorldObjectSaveData GetSaveData()
    {
        BossSaveData saveData = new BossSaveData();
        saveData.bossDefeated = bossDefeated;

        return saveData;
    }

    protected override void LoadData()
    {
        var data = saveData as BossSaveData;

        bossDefeated = data.bossDefeated;

        isDoneLoading = true;
    }

    [System.Serializable]
    public class BossSaveData : WorldObjectSaveData
    {
        public bool bossDefeated;
    }
}
