using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor.Animations;
using UnityEngine;

public class KillableNPC : Enemy
{
    private static readonly int AnimatorDeathTrigger = Animator.StringToHash("Death");
    private static readonly int AnimatorBoredTriggerID = Animator.StringToHash("Bored");

    /// <summary>
    /// How long does the npc need to be idle in order to play the bored animation?
    /// </summary>
    private const float BoredAnimDelay = 10f;
    private const float FlashTime = 0.1875f;

    [Header("Damaged Dialog Info")]
    public DialogNode damagedDialogNode;
    public DialogNode becameHostileDialogNode;
    public float dialogDisplayTime;

    public HostileNpcBehavior hostileBehavior;
    
    private bool isHostile;
    
    private Sprite speakerIcon;

    private Coroutine displayDamageDialogRoutine;
    private Coroutine playBoredAnimationRoutine;

    private PlayerController player;
    private DialogTree npcDialog;
    
    private bool started = false;

    private static readonly int AnimatorHostileID = Animator.StringToHash("Hostile");
    

    protected override void Start()
    {
        if (started) return;
        
        base.Start();

        npcDialog = GetComponent<DialogTree>();
        speakerIcon = npcDialog.speakerIcon;
        animator = GetComponent<Animator>();
        player = CharacterSelector.GetPlayerController();
        sprite = GetComponent<SpriteRenderer>();
        
        playBoredAnimationRoutine = StartCoroutine(PlayBoredAnimation());
        
        started = true;
    }

    public override void ApplyDamage(float amount)
    {
        // Damage the player, flash red, play sfx, and die if necessary
        base.ApplyDamage(amount);
        
        // Display dialog and/or become hostile
        if (!isHostile && currentHealth > 0)
        {
            if (displayDamageDialogRoutine != null)
            {
                DialogManager.FinishDialogLine();
                DialogManager.CloseDialog();
                StopCoroutine(displayDamageDialogRoutine);
            }

            DialogNode nodeToDisplay;
            if (currentHealth <= hostileBehavior?.minHealthBeforeHostile)
            {
                nodeToDisplay = becameHostileDialogNode;
                
                BecomeHostile();
            }
            else nodeToDisplay = damagedDialogNode;
            
            displayDamageDialogRoutine = StartCoroutine(DisplayDamagedDialog(nodeToDisplay));
        }
    }

    protected override void Death()
    {
        animator.SetTrigger(AnimatorDeathTrigger);

        if (displayDamageDialogRoutine != null)
        {
            DialogManager.FinishDialogLine();
            DialogManager.CloseDialog();
            StopCoroutine(displayDamageDialogRoutine);
        }
        
        hostileBehavior?.EndCurrentAction();
        
        // Cannot destroy npc until npc has had a chance to save
        //Destroy(gameObject, 1);
        
        // This effectively destroys the NPC for now, but keeps it in the saveable register so that it is saved before unloaded
        StartCoroutine(PseudoDestroyBeforeSave());
    }

    private IEnumerator PseudoDestroyBeforeSave()
    {
        // Disable collisions for the NCP if it has a rigidbody
        if (TryGetComponent(out Rigidbody2D rb)) rb.simulated = false;

        yield return new WaitForSeconds(1f);
        
        gameObject.SetActive(false);
    }

    private IEnumerator DisplayDamagedDialog(DialogNode dialogNode)
    {
        DialogManager.DisplayDialog(dialogNode, 25f);
        DialogManager.SetSpeakerIcon(speakerIcon);
        npcDialog.SetInteractable(false);
        
        yield return new WaitForSecondsRealtime(3f);
        
        DialogManager.FinishDialogLine();
        DialogManager.CloseDialog();
        if (!isHostile) npcDialog.SetInteractable(true);
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Set the amount of health hat the killable NPC has, and sets variables accordingly.
    /// Used upon loading health.
    /// </summary>
    /// <param name="health"></param>
    public void SetCurrentHealth(float health)
    {
        Start();
        
        print(this + " loaded with " + health + " health remaining");
        
        currentHealth = health;

        if (currentHealth <= hostileBehavior?.minHealthBeforeHostile)
        {
            print("loaded hostile");
            BecomeHostile();
        }
    }

    public void SetAsHostile()
    {
        Start();
        
        print(this + " loaded as hostile, getting full health as when just entered hostile");

        currentHealth = hostileBehavior.minHealthBeforeHostile;
        BecomeHostile();
    }

    private void BecomeHostile()
    {
        isHostile = true;

        if (playBoredAnimationRoutine != null) StopCoroutine(playBoredAnimationRoutine);

        animator.SetBool(AnimatorHostileID, true);
        
        npcDialog.SetFacePlayer(false);
        npcDialog.speechBubbleIcon.SetActive(false);
        npcDialog.SetInteractable(false);
        npcDialog.enabled = false;
        player?.TryRemoveInteractable(npcDialog);
        
        hostileBehavior.BecomeHostile();
    }

    public bool IsHostile()
    {
        return isHostile;
    }
    
    
    protected IEnumerator PlayBoredAnimation()
    {
        float timer = Time.time;
        
        // Wait for the specified delay time while being idle before playing the animation
        while (Time.time < timer + BoredAnimDelay)
        {
            // If the npc was talked to or noticed the player, the time needs to reset
            if (DialogManager.IsSpeechBubbleEnabled()) timer = Time.time;
            
            yield return null;
        }
        
        animator.SetTrigger(AnimatorBoredTriggerID);

        // Wait for the animation to end
        yield return new WaitForSeconds(1f);

        // Start the timer for the next bored animation
        if (!isHostile) playBoredAnimationRoutine = StartCoroutine(PlayBoredAnimation());
    }
}