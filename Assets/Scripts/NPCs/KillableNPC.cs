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

    [Header("Hostility Info")] 
    public int minHealthBeforeHostile = 3;

    public float moveSpeed = 2f;
    [Range(0f, 1f)] public float attackChance = 0.5f;
    [Range(0f, 1f)] public float retreatChance = 0.25f;
    public int minMovesPerAction = 2;
    public int maxMovesPerAction = 4;

    private bool isHostile;
    
    private Animator animator;
    private Sprite speakerIcon;

    private Coroutine displayDamageDialogRoutine;
    private Coroutine currentAction;

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
            if (currentHealth > minHealthBeforeHostile) nodeToDisplay = damagedDialogNode;
            else
            {
                nodeToDisplay = becameHostileDialogNode;
                
                BecomeHostile();
            }
            
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

        if (currentAction != null) StopCoroutine(currentAction);
        
        Destroy(gameObject, 1.125f);
    }

    private IEnumerator DisplayDamagedDialog(DialogNode dialogNode)
    {
        DialogManager.DisplayDialog(dialogNode);
        DialogManager.SetSpeakerIcon(speakerIcon);

        
        yield return new WaitForSecondsRealtime(3f);
        
        DialogManager.FinishDialogLine();
        DialogManager.CloseDialog();
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

        if (currentHealth <= minHealthBeforeHostile)
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

        ChooseAction();
    }

    public bool IsHostile()
    {
        return isHostile;
    }
}

/*
    private void ChooseAction()
    {
        float value = Random.value;
        if (value <= attackChance) currentAction = StartCoroutine(AttackAction());
        else if (value <= attackChance + retreatChance) currentAction = StartCoroutine(RetreatAction());
        else currentAction = StartCoroutine(MovementAction());
    }

    private IEnumerator RetreatAction()
    {
        for (int i = 0; i < Random.Range(minMovesPerAction, maxMovesPerAction); i++)
        {
            Vector2 desiredPosition = rigidbody.position + (rigidbody.position - (Vector2)player.transform.position).normalized * 2;
            yield return StartCoroutine(MoveToPosition(desiredPosition));
        }
        
        ChooseAction();
    }

    private IEnumerator MovementAction()
    {
        for (int i = 0; i < Random.Range(minMovesPerAction, maxMovesPerAction); i++)
        {
            Vector2 desiredPosition = rigidbody.position + Random.insideUnitCircle * 2f;
            yield return StartCoroutine(MoveToPosition(desiredPosition));
        }
        
        ChooseAction();
    }

    private IEnumerator MoveToPosition(Vector2 position)
    {
        // Move until within an acceptable error
        while (Vector2.Distance(rigidbody.position, position) > 0.1f)
        {
            Vector2 movementVector = (position - rigidbody.position).normalized * (moveSpeed * Time.fixedDeltaTime);
            
            rigidbody.MovePosition(rigidbody.position + movementVector);
            
            yield return null;
        }
    }

    private IEnumerator AttackAction()
    {
        print("Attack!");

        yield return null;
        
        ChooseAction();
    }
*/