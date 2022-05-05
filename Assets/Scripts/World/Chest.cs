using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : SaveableObject, IInteractable
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

    protected override void Start()
    {
        if (started) return;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = chestClosedSprite;

        audioSource = GetComponent<AudioSource>();

        if (saveData == null)
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

    public override WorldObjectSaveData GetSaveData()
    {
        ChestSaveData data = new ChestSaveData();

        data.isOpened = isChestOpened;

        print("Saved a " + SaveID() + " (chest)");

        return data;
    }

    protected override void LoadData()
    {
        ChestSaveData data = saveData as ChestSaveData;

        isChestOpened = data.isOpened;

        if (isChestOpened)
        {
            spriteRenderer.sprite = chestOpenSprite;

            foreach (var collider in GetComponents<Collider2D>()) if (collider.isTrigger) collider.enabled = false;
        }

        isDoneLoading = true;
    }

    [System.Serializable]
    public class ChestSaveData : WorldObjectSaveData
    {
        public bool isOpened;
    }
}
