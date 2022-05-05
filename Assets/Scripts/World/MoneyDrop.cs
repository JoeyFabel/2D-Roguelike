using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyDrop : MonoBehaviour
{
    [System.Serializable]
    public struct MoneyDropRateValue
    {
        public int minMoneyAmount;
        public Sprite sprite;
        public AudioClip gainSFX;
    }

    public MoneyDropRateValue[] moneyDropImages;    

    private int moneyAmount;
    private AudioClip moneyGainedSFX;

    SpriteRenderer sprite;

    public void SetMoney(int money)
    {
        sprite = GetComponent<SpriteRenderer>();

        moneyAmount = money;

        foreach (var pair in moneyDropImages)
        {
            if (money >= pair.minMoneyAmount)
            {
                sprite.sprite = pair.sprite;
                moneyGainedSFX = pair.gainSFX;
                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            Inventory.GainMoney(moneyAmount);

            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.PlayOneShot(moneyGainedSFX);
            sprite.enabled = false;
            GetComponent<Collider2D>().enabled = false;

            StartCoroutine(DestroyAfterAudio(audioSource));
        }
    }

    private IEnumerator DestroyAfterAudio(AudioSource audioSource)
    {
        while (audioSource.isPlaying) yield return null;

        Destroy(gameObject);
    }
}
