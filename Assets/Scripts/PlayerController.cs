using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

/*
 * Note to self:
 * 
 * Have Scriptable Object for different characters. Include references to:
 *      - Character prefab
 *      - Character name
 */


public class PlayerController : MonoBehaviour
{
    public int baseMaxHealth = 5;
    public int hpIncreasePerLevelUp = 1;
    
    /*public int maxStamina = 20;
    public float staminaRegenRate = 4.0f; // Stamina per second */
    
    public float speed = 3.0f;

    public float timeInvincible = 2.0f;

    public int health { get { return currentHealth; } }
    /*public int stamina { get { return (int)currentStamina; } }*/

    [Header("Resistances")]
    public Damageable.Resistances resistances = new Damageable.Resistances(0f, 0f, 0f, 0f, 0f);


    public GameObject hitVfxPrefab;
    public GameObject waterSplashVfxPrefab;

    public Sprite uiPortraitImage;
    public GameObject interactCanvas;

    [Header("Audio")]
    public AudioClip[] damagedClips;
    public AudioClip[] deathClips;
    public AudioClip splashSound;

    // Input stuff
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction regularAttackAction;
    private InputAction specialAttackAction;
    private InputAction interactAction;
    private InputAction cycleSpellAction;
    private InputAction inventoryToggleAction;
    private InputAction useQuickItemAction;

    // Stored Inputs
    Vector2 movementVector = Vector2.zero;

    // Miscellaneus
    Rigidbody2D rb;
    AudioSource audioSource;
    SpriteRenderer sprite;
    Animator animator;

    WeaponController playerWeapon;

    Vector2 lookDirection = new Vector2(1, 0);

    [SerializeField]
    private int maxHealth;
    private int currentHealth;
    private float currentStamina;

    private bool isInvincible;
    private bool isInvincibleAfterDamage;
    private bool isTouchingWall;
    float invincibleTimer;

    private bool loadedData = false;

    private IInteractable currentInteractable;

    private new Collider2D collider;

    private MapManager mapManager;
    private Vector3 respawnPosition;

    private void Awake()
    {
        //playerControls = GetComponent<PlayerControls>();             
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Movement"];
        regularAttackAction = playerInput.actions["Attack"];
        specialAttackAction = playerInput.actions["Special Attack"];
        interactAction = playerInput.actions["Interact"];
        cycleSpellAction = playerInput.actions["Cycle Spell"];
        inventoryToggleAction = playerInput.actions["Open Inventory"];
        useQuickItemAction = playerInput.actions["Use Quick Item"];

        playerWeapon = GetComponent<WeaponController>();

        GameObject.FindGameObjectWithTag("VCam").GetComponent<CinemachineVirtualCamera>().Follow = transform;
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

        regularAttackAction.performed -= AttemptAttack;
        specialAttackAction.performed -= AttemptSpecialAttack;

        interactAction.performed -= Interact;

        cycleSpellAction.performed -= AttemptSpellCycle;

        inventoryToggleAction.performed -= ToggleInventory;

        useQuickItemAction.performed -= UseQuickItem;

        XPManager.OnLevelUp -= GetMaxHPFromLevel;
    }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        sprite = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        mapManager = FindObjectOfType<MapManager>();

        heightTestPlayer = collider.bounds.extents.y + 0.05f;

        // Subscribe to input action events here. make sure to unsubscribe in onDisable or onDestroy
        moveAction.performed += Move;
        moveAction.canceled += Move;
        regularAttackAction.performed += AttemptAttack;
        specialAttackAction.performed += AttemptSpecialAttack;
        interactAction.performed += Interact;
        cycleSpellAction.performed += AttemptSpellCycle;
        inventoryToggleAction.performed += ToggleInventory;
        useQuickItemAction.performed += UseQuickItem;
        XPManager.OnLevelUp += GetMaxHPFromLevel;

        GetMaxHPFromLevel();

        if (!loadedData) currentHealth = maxHealth;
        UIHealthBar.instance.UpdatePortrait(uiPortraitImage);

        isInvincible = false;
        isInvincibleAfterDamage = false;

        DisableInteractSymbol();
        // TODO - Make it so that you can fall in water, remove colliders from water tiles
        //Debug.LogError("Make player able to fall in water");
    }

    void FixedUpdate()
    {
        if (isInvincibleAfterDamage)
        {
            invincibleTimer -= Time.fixedDeltaTime;
            isInvincibleAfterDamage = invincibleTimer >= 0;
        }

        if (animator.GetBool("Can Move")) HandleMovement();
        else HandleMovingGround(); // attacking on moving platform, for example

        /*if (!animator.GetBool("Attacking"))
        {
            currentStamina += staminaRegenRate * Time.fixedDeltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        } */
    }

    float heightTestPlayer;
    Collider2D currentGround = null;
    Vector3 currentGroundPosition;
    public LayerMask groundMask;

    private void HandleMovement()
    {
        Vector2 position = rb.position;

        if (!Mathf.Approximately(movementVector.x, 0.0f) || !Mathf.Approximately(movementVector.y, 0.0f))
        {
            lookDirection.Set(movementVector.x, movementVector.y);
            lookDirection.Normalize();
        }

        if (movementVector.x < 0) sprite.flipX = true;
        else if (movementVector.x > 0.01f) sprite.flipX = false;

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", movementVector.magnitude);

        // point cast
        RaycastHit2D hit = Physics2D.Raycast(collider.bounds.center, Vector2.down, heightTestPlayer, groundMask);
        bool isGrounded = hit.collider != null;

        Vector2 groundMoved = Vector2.zero;

        if (hit.collider != currentGround)
        {
            respawnPosition = transform.position - (Vector3)lookDirection;
            currentGround = hit.collider;
            if (hit.collider != null) currentGroundPosition = hit.collider.transform.position;
        }
        else
        {
            if (currentGround)
            {
                groundMoved = hit.collider.transform.position - currentGroundPosition;                

                currentGroundPosition = hit.collider.transform.position;

                rb.MovePosition(rb.position + groundMoved);

                //   positionDelta.x += groundMoved.x;
                //   positionDelta.y += groundMoved.y;
                //    movementVector.x += groundMoved.x;
                //   movementVector.y += groundMoved.y;
            }
            else respawnPosition = transform.position - (Vector3)lookDirection;
        }

        //if (mapManager.getMovementMultiplier(position) < 1f) print(mapManager.getMovementMultiplier(position));
        position += mapManager.getMovementMultiplier(position) * speed * Time.fixedDeltaTime * movementVector + groundMoved;

        rb.MovePosition(position);
        // rb.velocity = mapManager.getMovementMultiplier(position) * speed * movementVector;
    }

    private void HandleMovingGround()
    {
        // point cast
        RaycastHit2D hit = Physics2D.Raycast(collider.bounds.center, Vector2.down, heightTestPlayer, groundMask);
        bool isGrounded = hit.collider != null;

        Vector2 groundMoved = Vector2.zero;

        if (hit.collider != currentGround)
        {
            currentGround = hit.collider;
            if (hit.collider != null) currentGroundPosition = hit.collider.transform.position;
        }
        else
        {
            if (currentGround)
            {
                print("ground!");
                groundMoved = hit.collider.transform.position - currentGroundPosition;
                currentGroundPosition = hit.collider.transform.position;

                rb.MovePosition(rb.position + groundMoved);

                //   positionDelta.x += groundMoved.x;
                //   positionDelta.y += groundMoved.y;
                //    movementVector.x += groundMoved.x;
                //   movementVector.y += groundMoved.y;
            }
        }
    }

    public void GainHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetHealthValue(currentHealth / (float)maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (isInvincibleAfterDamage || isInvincible) return;

        //print("Player taking " + amount + " damage");
        currentHealth -= amount;

        if (currentHealth > 0)
        {
            //animator.SetTrigger("Hit");

            isInvincibleAfterDamage = true;
            invincibleTimer = timeInvincible;
            StartCoroutine(FlashWhileInvincible(0.1f, timeInvincible));

            Instantiate(hitVfxPrefab, transform.position, transform.rotation, transform);

            UIHealthBar.instance.SetHealthValue(currentHealth / (float)maxHealth);

            var damagedClip = damagedClips[Random.Range(0, damagedClips.Length - 1)];
            PlaySound(damagedClip);
        }
        else Death();
    }

    public void TakeDamage(float amount, Damageable.DamageTypes damageType)
    {
        float damage = amount - (amount * resistances.GetResistanceValue(damageType));

        // Right now, technically could heal if resistances > 100%, but not implemented, maybe never will be
        if (damage >= 0) TakeDamage((int)damage);
    }

    private IEnumerator FlashWhileInvincible(float flashTime, float invincibleTime)
    {
        float startTime = Time.time;
        float visibilityTimer = flashTime;

        bool spriteVisible = false;
        sprite.enabled = false;

        while (Time.time - startTime <= invincibleTime)
        {
            if (visibilityTimer >= 0) visibilityTimer -= Time.deltaTime;
            else
            {
                spriteVisible = !spriteVisible;
                sprite.enabled = spriteVisible;

                visibilityTimer = flashTime;
            }
    
            yield return null;
        }

        sprite.enabled = true;
    }

    private void Death()
    {
        rb.simulated = false;

        playerInput.DeactivateInput();

        animator.SetTrigger("Death");

        UIHealthBar.instance.SetHealthValue(0f);

        var deathClip = deathClips[Random.Range(0, deathClips.Length - 1)];
        PlaySound(deathClip);

        sprite.sortingOrder--;

        enabled = false;
    }

    public bool AtMaxHealth()
    {
        return currentHealth == maxHealth;
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void EnableInteractSymbol()
    {
        interactCanvas.SetActive(true);
    }

    public void DisableInteractSymbol()
    {
        interactCanvas.SetActive(false);
    }

    #region Input Methods
    
    private void Move(InputAction.CallbackContext context)
    {
        movementVector = context.ReadValue<Vector2>();
    }

    private void Interact(InputAction.CallbackContext context)
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();

            if (currentInteractable is not DialogTree && currentInteractable is not ShopItem) currentInteractable = null;
            DisableInteractSymbol();
        }
    }

    private void AttemptAttack(InputAction.CallbackContext context)
    {
        playerWeapon.Attack(lookDirection);
    }

    private void AttemptSpecialAttack(InputAction.CallbackContext context)
    {
        playerWeapon.SpecialAttack(lookDirection);
    }

    private void AttemptSpellCycle(InputAction.CallbackContext context)
    {
        SpellHUD.instance?.CycleSelectedSpell();
    }

    private void ToggleInventory(InputAction.CallbackContext context)
    {
        Inventory.ToggleInventoryUI();
    }

    private void UseQuickItem(InputAction.CallbackContext context)
    {
        Consumable quickItem = Inventory.GetQuickItem() as Consumable;

        if (quickItem == null) return;

        print("using " + quickItem.itemName);
        
        // Inventory.LoseItem(quickItem);
        
        if (quickItem.objectToSpawn)
        {
            GameObject spawnedObject = Instantiate(quickItem.objectToSpawn, rb.position, Quaternion.identity);
            
            if (quickItem.throwObject) ThrowItem(spawnedObject);
        }   
        
        if (quickItem.healthToGain > 0) GainHealth(quickItem.healthToGain);
        if (quickItem.manaToGain > 0 && playerWeapon is SaurianAugurWeapon augurWeapon)
        {
            augurWeapon.GainMana(quickItem.manaToGain);
        }
    }
#endregion

    /// <summary>
    /// Applies movement to the object, simulating it being thrown by the character
    /// </summary>
    /// <param name="objectToThrow">The object being thrown</param>
    public void ThrowItem(GameObject objectToThrow)
    {
        
    }

    public void DisableControlsForUI()
    {
        playerInput.DeactivateInput();
        inventoryToggleAction.Enable();
    }

    public void DisableControlsForDialog()
    {
        playerInput.DeactivateInput();
        interactAction.Enable();
    }
    
    public void EnableControlsAfterUI()
    {
        playerInput.ActivateInput();
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        //if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        //{
        //    FellInWater(other.GetComponentInParent<Grid>());
        //}
        if (other.CompareTag("Hole") && InsideCol(collider, other))
        {
            RaycastHit2D hit = Physics2D.Raycast(collider.bounds.center, Vector2.down, heightTestPlayer, groundMask);
            bool isGrounded = hit.collider != null;

            if (hit.collider == null) FellInHole();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            currentInteractable = interactable;
            EnableInteractSymbol();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractable>(out var interactable))
        {
            currentInteractable = null;
            DisableInteractSymbol();
        }
    }

    public static bool InsideCol(Collider2D mycol, Collider2D other)
    {
        if (other.bounds.Contains(mycol.bounds.min)
             && other.bounds.Contains(mycol.bounds.max))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private float shrinkRate = 5f;
    private float fallMoveRate = 1f;    

    private void FellInWater(Grid grid)
    {
        var startPos = transform.position - (Vector3)(lookDirection);
        StartCoroutine(DisableWhileInWater(2f, startPos));

        StartCoroutine(ShrinkPlayer(shrinkRate));
      //  StartCoroutine(MovePlayerToCellCenter(grid));
    }

    private void FellInHole()
    {
        StopAllCoroutines();
        sprite.enabled = true;
        invincibleTimer = 0;
        isInvincibleAfterDamage = false;

        var startPos = transform.position - (Vector3)lookDirection;


        StartCoroutine(DisableWhileInHole(2f, 0.5f, 1));
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isTouchingWall = true;            
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isTouchingWall = false;
        }
    }

    public bool IsTouchingWall()
    {
        return isTouchingWall;

        /*
        List<ContactPoint2D> contactPoints = new List<ContactPoint2D>();

        rb.GetContacts(contactPoints);

        if (collider.IsTouchingLayers())

        foreach (var contact in contactPoints)
        {
                if (contact.collider.CompareTag("Ground")) return true;
        }

        return false;
        */
    }

    private IEnumerator DisableWhileInHole(float timeInHole, float movementTime, int fallDamage)
    {
        var oldMovement = lookDirection;

        animator.SetBool("Falling", true);
        animator.SetBool("Can Move", false);

        playerInput.DeactivateInput();
        collider.enabled = false;

        float timestamp = Time.time;
        StartCoroutine(ShrinkPlayer(movementTime));

        //yield return new WaitForSeconds(movementTime);

        while (Time.time < timestamp + movementTime)
        {
            Vector2 position = rb.position;

            position += speed * Time.fixedDeltaTime * oldMovement;

            rb.MovePosition(position);

            yield return null;
        }

        // Let the player move in the same direction briefly while falling for better appearance
        movementVector = Vector3.zero;
        rb.velocity = Vector2.zero;
        sprite.enabled = false;
        //TakeDamage(fallDamage);

        yield return new WaitForSeconds(timeInHole - movementTime);
        //currentHealth -= fallDamage;
        //UIHealthBar.instance.SetHealthValue(currentHealth / (float)maxHealth);
        TakeDamage(fallDamage);
        animator.SetBool("Falling", false);
        animator.SetBool("Can Move", true);

        collider.enabled = true;
        transform.position = respawnPosition;

        playerInput.ActivateInput();
        sprite.enabled = true;
        transform.localScale = Vector3.one;

        if (currentHealth <= 0) Death();
    }

    private IEnumerator DisableWhileInWater(float timeInWater, Vector3 respawnPosition)
    {
        var oldMovement = movementVector;

        playerInput.DeactivateInput();
        movementVector = oldMovement;
        
        yield return new WaitForSeconds(timeInWater);

        transform.position = respawnPosition;

        transform.localScale = Vector3.one;
        rb.simulated = true;

        playerInput.ActivateInput();
        sprite.enabled = true;
    }

    private IEnumerator ShrinkPlayer(float shrinkSpeed)
    {        
        while (transform.localScale.magnitude > 0.5f)
        {
            transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

            yield return null;
        }

        //Instantiate(waterSplashVfxPrefab, transform.position, waterSplashVfxPrefab.transform.rotation);
        movementVector = Vector2.zero; 
        //sprite.enabled = false;
    }

    private IEnumerator MovePlayerToCellCenter(Grid hitGrid)
    {
        Vector2 movementFactor = Vector2.zero;

        if (Mathf.Abs(lookDirection.x) > Mathf.Abs(lookDirection.y))
        {
            if (lookDirection.x > 0.01f) movementFactor = Vector2.right;
            else if (lookDirection.x < -0.01f) movementFactor = Vector2.left;
        }
        else
        {
            if (lookDirection.y > 0.01f) movementFactor = Vector2.up;
            else if (lookDirection.y < -0.01f) movementFactor = Vector2.down;
        }

        var hitCell = hitGrid.WorldToCell(rb.position + movementFactor);
        Vector3 cellCenter = hitGrid.GetCellCenterWorld(hitCell);
        if (movementVector == Vector2.right || movementVector == Vector2.left) cellCenter.y = transform.position.y;
        else if (movementVector == Vector2.up || movementVector == Vector2.down) cellCenter.x = transform.position.x;

        // If moving horizontally, only get x position of cell center and keep y
        // If moving vertically, only get y position of cell center and keep x

        //Collider2D collider = GetComponent<Collider2D>();

        while (Mathf.Abs((transform.position - cellCenter).magnitude) >= 0.01f)
        {
            Vector3 myPosition = transform.position;// - (Vector3)collider.offset;
            transform.position += (cellCenter - myPosition) * fallMoveRate;

            yield return null;
        }
    }

    public void LoadPlayerData(PlayerLoadData data)
    {
        currentHealth = data.currentHealth;
        UIHealthBar.instance.SetHealthValue(currentHealth / (float)maxHealth);

        playerWeapon.LoadPlayerData(data);

        loadedData = true;
    }

    public PlayerLoadData GetPlayerLoadData()
    {
        PlayerLoadData data = playerWeapon.GetPlayerLoadData();
        data.currentHealth = currentHealth;
        
        return data;
    }

    public void RestAtShrine(float disabledControlsTime)
    {
        GainHealth(maxHealth);

        playerWeapon.RestAtShrine();

        StartCoroutine(DisableControlsForTime(disabledControlsTime));
    }

    private IEnumerator DisableControlsForTime(float time)
    {
        playerInput.DeactivateInput();

        yield return new WaitForSeconds(time);

        playerInput.ActivateInput();
    }

    private void PlayFootstepSound()
    {
        AudioClip footstepSound = mapManager.GetFootstepSound(rb.position + Vector2.up * 0.1f);

        // This means not on the regular tilemap, but on something like a moving platform instead
        // The FootstepHelper should be on whatever object the player is on
        if (footstepSound == null)
        {
            RaycastHit2D hit = Physics2D.Raycast(collider.bounds.center, Vector2.down, heightTestPlayer, groundMask);
            
            if (hit.collider != null)
            {
                FootstepHelper hitGround = hit.collider.GetComponent<FootstepHelper>();

                footstepSound = hitGround.footstepSounds[Random.Range(0, hitGround.footstepSounds.Length)];
            }
        }

        if (footstepSound) audioSource.PlayOneShot(footstepSound, 2.0f);
    }

    protected virtual void GetMaxHPFromLevel()
    {
        maxHealth = baseMaxHealth + hpIncreasePerLevelUp * XPManager.GetHPLevelUps();

        currentHealth = maxHealth;
        UIHealthBar.instance.SetHealthValue(1);
    }
}
