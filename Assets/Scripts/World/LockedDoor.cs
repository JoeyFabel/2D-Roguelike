using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedDoor : SaveableObject, IInteractable
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

    protected override void Start()
    {
        if (started) return;

        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        if (saveData == null) isLocked = true;

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

    public override WorldObjectSaveData GetSaveData()
    {
        LockedDoorSaveData data = new LockedDoorSaveData();
        data.isLocked = isLocked;

        return data;
    }

    protected override void LoadData()
    {
        LockedDoorSaveData data = saveData as LockedDoorSaveData;

        isLocked = data.isLocked;

        isDoneLoading = true;

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
}
