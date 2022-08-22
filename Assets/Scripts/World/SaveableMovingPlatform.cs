using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class SaveableMovingPlatform : MovingPlatform, ISaveable
{
    public bool savePosition = false;
    private bool loadedSave;

    private bool started = false;

    private AudioSource audioSource;

    [SerializeField]
    private int saveID = -1;
    public int SaveIDNumber
    {
        get => saveID;
        set => saveID = value;
    }
    
    public bool DoneLoading { get; set; }

    private void Awake()
    {
        SaveManager.RegisterSaveable(this);
    }

    private void OnDestroy()
    {
        SaveManager.UnRegisterSaveable(this);
    }

    protected override void Start()
    {
        if (started) return;
        
        if (loadedSave) return;
        
        base.Start();
        
        TryGetComponent<AudioSource>(out audioSource);
        
        started = true;
    }

    public override void StartMovement()
    {
        base.StartMovement();
        
        if (audioSource) audioSource.Play();
    }

    public override void EndMovement()
    {
        base.EndMovement();
        if (audioSource) audioSource.Stop();
    }
    
    public WorldObjectSaveData GetSaveData()
    {
        MovingPlatformSaveData saveData = new MovingPlatformSaveData();

        saveData.isActivated = isMoving;
        saveData.completedOneWay = completedOneWay;

        if (savePosition)
        {
            saveData.currentPosition = movingPlatform.transform.position;
            saveData.nextIndex = currentTargetIndex;
            saveData.movingForward = movingForward;
        }

        return saveData;
    }

    public void LoadData(WorldObjectSaveData saveData)
    {
        if (!started) Start();
        
        MovingPlatformSaveData data = saveData as MovingPlatformSaveData;

        if (data == null)
        {
            Debug.LogError("ERROR -- " + gameObject + " did not get a valid save data!", gameObject);
            return;
        }

        if (data.completedOneWay) // Good
        {
            currentTargetIndex = targetPoints.Length - 1;

            movingPlatform.transform.position = targetPoints[currentTargetIndex].position;
            completedOneWay = true;
            isMoving = false;
        }
        else if (savePosition) // Good
        {
            movingPlatform.transform.position = data.currentPosition;
            currentTargetIndex = data.nextIndex;
            movingForward = data.movingForward;

            isMoving = data.isActivated;
        }
        else if (data.isActivated) 
        {
            isMoving = true;
            movingForward = startMovingForward;
            currentTargetIndex = startTargetIndex;
            completedOneWay = false;

            lapStartTime = Time.time;
        }

        loadedSave = true;
        DoneLoading = true;
    }

    [System.Serializable]
    public class MovingPlatformSaveData : WorldObjectSaveData
    {
        public bool completedOneWay;
        public bool isActivated;
        
        // Only for platforms that have stopped moving
        public SerializableVector3 currentPosition;
        public int nextIndex;
        public bool movingForward;
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
