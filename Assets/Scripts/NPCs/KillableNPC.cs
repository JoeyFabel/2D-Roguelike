using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor.Animations;
using UnityEngine;

public class KillableNPC : Damageable
{
    private static readonly int AnimatorDeathTrigger = Animator.StringToHash("Death");

    [Header("Damaged Dialog Info")]
    public DialogNode damagedDialogNode;
    public DialogNode becameHostileDialogNode;
    public float dialogDisplayTime;

    public HostileNpcBehavior hostileBehavior;
    
    private bool isHostile;
    
    private Animator animator;
    private Sprite speakerIcon;

    private Coroutine displayDamageDialogRoutine;

    private PlayerController player;
    private DialogTree npcDialog;

    private bool started = false;

    public System.Action OnBecomeHostile;
    private static readonly int AnimatorHostileID = Animator.StringToHash("Hostile");

    protected override void Start()
    {
        if (started) return;
        
        base.Start();

        npcDialog = GetComponent<DialogTree>();
        speakerIcon = npcDialog.speakerIcon;
        animator = GetComponent<Animator>();
        player = CharacterSelector.GetPlayerController();
        
        started = true;
    }

    public override void ApplyDamage(float amount)
    {
        base.ApplyDamage(amount);
        
        PlayerController player = CharacterSelector.GetPlayerController();

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
        
        Destroy(gameObject, 1.125f);
    }

    private IEnumerator DisplayDamagedDialog(DialogNode dialogNode)
    {
        DialogManager.DisplayDialog(dialogNode);
        DialogManager.SetSpeakerIcon(speakerIcon);
        npcDialog.SetInteractable(false);
        
        yield return new WaitForSecondsRealtime(3f);
        
        DialogManager.FinishDialogLine();
        DialogManager.CloseDialog();
        if (!isHostile) npcDialog.SetInteractable(true);
    }

    protected virtual void ChooseAction()
    {
        
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

    private void BecomeHostile()
    {
        isHostile = true;

        OnBecomeHostile?.Invoke();
        animator.SetBool(AnimatorHostileID, true);

        hostileBehavior.BecomeHostile();
    }

    public bool IsHostile()
    {
        return isHostile;
    }
}