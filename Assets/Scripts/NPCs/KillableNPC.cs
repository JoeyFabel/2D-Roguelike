using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    private Rigidbody2D rigidbody;
    private Sprite speakerIcon;

    private Coroutine displayDamageDialogRoutine;
    private Coroutine currentAction;

    private PlayerController player;
    
    protected override void Start()
    {
        base.Start();

        speakerIcon = GetComponent<DialogTree>().speakerIcon;
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        player = CharacterSelector.GetPlayerController();
    }

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
                isHostile = true;
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
        
        StopCoroutine(currentAction);
        
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
}
