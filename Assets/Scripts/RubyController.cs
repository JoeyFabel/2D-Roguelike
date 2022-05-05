using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class RubyController : MonoBehaviour
{
    public int maxHealth = 5;
    public float speed = 3.0f;

    public float timeInvincible = 2.0f;

    public int health { get {return currentHealth; } }

    public GameObject projectilePrefab;
    public GameObject hitVfxPrefab;

    public AudioClip damagedClip;
    public AudioClip cogThrowClip;

    // Input stuff
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction attackAction;
    InputAction interactAction;

    // Stored Inputs
    Vector2 movementVector = Vector2.zero;

    // Miscellaneus
    new Rigidbody2D rigidbody2D;
    AudioSource audioSource;
    
    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);

    private int currentHealth;

    private bool isInvincible;
    float invincibleTimer;

    private void Awake()
    {
        //playerControls = GetComponent<PlayerControls>();             
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Movement"];
        attackAction = playerInput.actions["Attack"];
        interactAction = playerInput.actions["Interact"];
    }

    /*
    private void OnEnable()
    {
        
    } */

    private void OnDisable()
    {
        // Unsubscribe from things here

        moveAction.performed -= Move;
        moveAction.canceled -= Move;

        attackAction.performed -= Attack;

        interactAction.performed -= Interact;
    }


    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Subscribe to input action events here. make sure to unsubscribe in onDisable or onDestroy
        moveAction.performed += Move;
        moveAction.canceled += Move;
        attackAction.performed += Attack;
        interactAction.performed += Interact;

        currentHealth = maxHealth;

        // TODO - Make it so that you can fall in water, remove colliders from water tiles
        //Debug.LogError("Make player able to fall in water");
    }

    void FixedUpdate()
    {
        if (isInvincible)
        {
            invincibleTimer -= Time.fixedDeltaTime;
            isInvincible = invincibleTimer >= 0;
        }


        Vector2 position = rigidbody2D.position;
        
        if (!Mathf.Approximately(movementVector.x, 0.0f) || !Mathf.Approximately(movementVector.y, 0.0f))
        {
            lookDirection.Set(movementVector.x, movementVector.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", movementVector.magnitude);

        position += speed * Time.fixedDeltaTime * movementVector;

        rigidbody2D.MovePosition(position);
    }

    public void GainHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetHealthValue(currentHealth / (float)maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;

        animator.SetTrigger("Hit");

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);

        isInvincible = true;
        invincibleTimer = timeInvincible;

        Instantiate(hitVfxPrefab, transform.position, transform.rotation, transform);

        UIHealthBar.instance.SetHealthValue(currentHealth / (float)maxHealth);

        PlaySound(damagedClip);
    }

    public bool AtMaxHealth()
    {
        return currentHealth == maxHealth;
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    // Input Methods here
    public void Move(InputAction.CallbackContext context)
    {
        movementVector = context.ReadValue<Vector2>();
    }

    public void Attack(InputAction.CallbackContext context)
    {
        Launch();
    }

    public void Melee(InputAction.CallbackContext context)
    {
        animator.SetTrigger("Melee Attack");
    }

    public void Interact(InputAction.CallbackContext context)
    {
        RaycastHit2D hit = Physics2D.Raycast(rigidbody2D.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));

        if (hit.collider)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                NonPlayerCharacter npc = hit.collider.GetComponent<NonPlayerCharacter>();

                npc.DisplayDialogue();
            }
        }
    }

    // delete later
    private void Launch()
    {        
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2D.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");

        PlaySound(cogThrowClip);
    }
}
