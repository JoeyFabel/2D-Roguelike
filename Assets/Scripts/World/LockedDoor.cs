using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractable, ISaveable
{
    public Item requiredKey;

    public Sprite doorOpenedSprite;

    public AudioClip openSound;
    public AudioClip failedOpenSound;

    public bool slidingDoor;

    private bool isLocked;

    private AudioSource audioSource;
    private Animator animator;

    public GameObject associatedCameraCollider;

    private bool started = false;
    private bool hasSaveData = false;
    
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

    private void Start()
    {
        if (started) return;

        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        if (!hasSaveData) isLocked = true;

        if (associatedCameraCollider) associatedCameraCollider.SetActive(!isLocked);

        started = true;
    }
        
    public void OpenDoor()
    {
        animator.SetTrigger("Open");

        if (associatedCameraCollider) associatedCameraCollider?.SetActive(true);

        StartCoroutine(WaitForDoorToOpen());
    }

    private IEnumerator WaitForDoorToOpen()
    {
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
            print(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }

        foreach (var collider in GetComponents<Collider2D>()) collider.enabled = false;
    }

    public WorldObjectSaveData GetSaveData()
    {
        LockedDoorSaveData data = new LockedDoorSaveData();
        data.isLocked = isLocked;

        return data;
    }

    public void LoadData(WorldObjectSaveData saveData)
    {
        hasSaveData = true;
        if (!started) Start();
        
        LockedDoorSaveData data = saveData as LockedDoorSaveData;

        isLocked = data.isLocked;

        DoneLoading = true;

        if (!isLocked)
        {
            animator.enabled = false;
            GetComponent<SpriteRenderer>().sprite = doorOpenedSprite;

            foreach (var collider in GetComponents<Collider2D>()) collider.enabled = false;

            if (slidingDoor) gameObject.SetActive(false);
        }
    }

    public void Interact()
    {
        if (Inventory.PlayerHasItem(requiredKey))
        {
            audioSource.PlayOneShot(openSound);

            isLocked = false;

            OpenDoor();

            Inventory.LoseItem(requiredKey);
        }
        else audioSource.PlayOneShot(failedOpenSound);
    }

    [System.Serializable]
    public class LockedDoorSaveData : WorldObjectSaveData
    {
        public bool isLocked;
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
