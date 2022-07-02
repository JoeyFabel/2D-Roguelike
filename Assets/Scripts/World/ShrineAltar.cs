using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShrineAltar : MonoBehaviour, IInteractable
{
    public SpriteRenderer magicCircle;
    public Sprite normalCircleSprite;
    public Sprite litCircleSprite;

    public float glowTime = 2f;
    public float healVFXTime = 2f;

    public GameObject healVFX;

    public AudioClip restSFX;

    AudioSource audioSource;
    PlayerController player;

    //protected override void Start()
    private void Start()
    {
        //base.Start();

        audioSource = GetComponent<AudioSource>();
        player = CharacterSelector.GetPlayerController();

        magicCircle.sprite = normalCircleSprite;
    }

    public void Interact()
    {
        StartCoroutine(MakeMagicCircleGlow());
        player.RestAtShrine(glowTime);
        StartCoroutine(ShowHealVFX());

        audioSource.PlayOneShot(restSFX);
        GameManager.CreateSave();
    }

    private IEnumerator ShowHealVFX()
    {
        GameObject healVFXInstance = Instantiate(healVFX, player.transform);

        yield return new WaitForSeconds(healVFXTime);

        Destroy(healVFXInstance);
    }

    private IEnumerator MakeMagicCircleGlow()
    {
        magicCircle.sprite = litCircleSprite;

        yield return new WaitForSeconds(glowTime);

        magicCircle.sprite = normalCircleSprite;
    }
}
