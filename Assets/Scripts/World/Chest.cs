using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable, ISaveable
{
    public Sprite chestClosedSprite;
    public Sprite chestOpenSprite;

    public Item itemToGain;
    [Min(1)]
    public int quantityToGain = 1;

    public int moneyToGain = 0;

    private SpriteRenderer spriteRenderer;

    private bool isChestOpened;
    private AudioSource audioSource;

    private bool started = false;
    private bool hasSaveData = false;
    
    private void Start()
    {
        if (started) return;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = chestClosedSprite;

        audioSource = GetComponent<AudioSource>();

        if (!hasSaveData)
        {
            isChestOpened = false;

            spriteRenderer.sprite = chestClosedSprite;
        }

        started = true;
    }

    public void Interact()
    {
        if (!isChestOpened)
        {
            spriteRenderer.sprite = chestOpenSprite;
            audioSource.Play();

            if (itemToGain) Inventory.GainItem(itemToGain, quantityToGain);
            Inventory.GainMoney(moneyToGain);

            isChestOpened = true;

            foreach (var collider in GetComponents<Collider2D>()) if (collider.isTrigger) collider.enabled = false;
        }
    }

    public WorldObjectSaveData GetSaveData()
    {
        ChestSaveData data = new ChestSaveData();

        data.isOpened = isChestOpened;

        print("Saved a " + SaveManager.GetSaveID(this) + " (chest)");

        return data;
    }

    public void LoadData(WorldObjectSaveData saveData)
    {
        // Make sure this knows that there is a save data
        hasSaveData = true;
        
        // Start if this hasn't started yet
        if (!started) Start();
        
        // Now this is ready to load the data
        ChestSaveData data = saveData as ChestSaveData;

        isChestOpened = data.isOpened;

        if (isChestOpened)
        {
            spriteRenderer.sprite = chestOpenSprite;

            foreach (var collider in GetComponents<Collider2D>()) if (collider.isTrigger) collider.enabled = false;
        }

        DoneLoading = true;
    }

    public int SaveIDNumber { get; set; }
    public bool DoneLoading { get; set; }
    
    public void Awake()
    {
        SaveManager.RegisterSaveable(this);
    }

    public void OnDestroy()
    {
        SaveManager.UnRegisterSaveable(this);
    }

    [System.Serializable]
    public class ChestSaveData : WorldObjectSaveData
    {
        public bool isOpened;
    }
}
