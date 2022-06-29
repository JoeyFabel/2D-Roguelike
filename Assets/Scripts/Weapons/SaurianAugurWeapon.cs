using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SaurianAugurWeapon : WeaponController
{
    // Melee Offsets:
    // North X:
    // North Y:
    // South X:
    // South Y:

    //public MagicSpellScriptableObject currentSpell;
    public float magicIncreasePerLevelUp = 1f;
    
    public float magicRegenRate = 0.1f;

    public float northSpellXOffset;
    public float northSpellYOffset;
    public float southSpellXOffset;
    public float southSpellYOffset;
    public float pureSouthFlippedXOffset = -0.276f;
    public float pureNorthFlippedXOffset = 0.194f;

    public float baseMaxMagic = 10f;

    [SerializeField]
    private float maxMagic;
    private float currentMagic;

    private PlayerController player;

    public AudioSource spellSfxSource;

    //private float originalSize;

    protected override void Start()
    {
        base.Start();

        player = GetComponent<PlayerController>();

        UIHealthBar.instance.EnableMagicBar();

        //currentSpellIcon.sprite = currentSpell.uiImage;
        //originalSize = magicQuantityMask.rectTransform.rect.width;        

        CalculateMaxMPFromLevel();
        XPManager.OnLevelUp += CalculateMaxMPFromLevel;
        if (!loadedData) currentMagic = maxMagic;

        SpellHUD.instance.PlayerCanUseMagic();

        UpdateMagicQuantity();
    }

    private void OnDestroy()
    {
        XPManager.OnLevelUp -= CalculateMaxMPFromLevel;
    }

    private void CalculateMaxMPFromLevel()
    {
        maxMagic = baseMaxMagic + magicIncreasePerLevelUp * XPManager.GetMPLevelUps();

        currentMagic = maxMagic;
        UIHealthBar.instance.SetMagicValue(1);
    }

    public override void Attack(Vector2 lookDirection)
    {
        this.lookDirection = lookDirection;

        if (CanAttack()) MeleeAttack();
    }

    private void MeleeAttack()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        animator.SetTrigger("Melee Attack");

        StartCoroutine(DisableMovementDuringAttack());
    }

    public override void SpecialAttack(Vector2 lookDirection)
    {
        this.lookDirection = lookDirection;

        if (CanAttack() && currentMagic >= SpellHUD.instance.GetSelectedSpell().magicCost) StartCastingSpell(SpellHUD.instance.GetSelectedSpell());
    }

    private void StartCastingSpell(MagicSpellScriptableObject currentSpell)
    {
        animator.SetTrigger("Ranged Attack");

        StartCoroutine(WaitForSpellToCast(currentSpell));
    }

    private IEnumerator WaitForSpellToCast(MagicSpellScriptableObject currentSpell)
    {
        float enterTime = Time.time;

        float enterHealth = player.health;

        Vector2 spellCastPosition = transform.position;

        if (lookDirection == Vector2.up && sprite.flipX)
        {
            spellCastPosition.x += pureNorthFlippedXOffset;
            spellCastPosition.y += northSpellYOffset;
        }
        else if (lookDirection == Vector2.down && sprite.flipX)
        {
            spellCastPosition.x += pureSouthFlippedXOffset;
            spellCastPosition.y += southSpellYOffset;
        }
        else if (lookDirection.y > 0)
        {
            if (lookDirection.x >= 0)
            {
                // NE
                spellCastPosition.x += northSpellXOffset;
                spellCastPosition.y += northSpellYOffset;
            }
            else
            {
                // NW

                spellCastPosition.x -= northSpellXOffset;
                spellCastPosition.y += northSpellYOffset;
            }
        }
        else
        {
            if (lookDirection.x >= 0)
            {
                // SE

                spellCastPosition.x += southSpellXOffset;
                spellCastPosition.y += southSpellYOffset;
            }
            else
            {
                // SW

                spellCastPosition.x -= southSpellXOffset;
                spellCastPosition.y += southSpellYOffset;
            }
        }

        GameObject spellPrepEffect = null;

        if (currentSpell.preparationPrefab) spellPrepEffect = Instantiate(currentSpell.preparationPrefab, spellCastPosition, Quaternion.identity);
        if (currentSpell.prepSound) spellSfxSource.PlayOneShot(currentSpell.prepSound);        

        while (Time.time <= enterTime + currentSpell.castTime)
        {
            if (player.health != enterHealth)
            {
                animator.SetBool("Attacking", false);
                
                if (spellPrepEffect) Destroy(spellPrepEffect);

                yield break;
            }

            yield return null;
        }

        animator.SetBool("Attacking", false);

        if (spellPrepEffect) Destroy(spellPrepEffect);

        if (currentSpell.castSound) spellSfxSource.PlayOneShot(currentSpell.castSound);

        currentMagic -= currentSpell.magicCost;
        UpdateMagicQuantity();

        Spell castSpell = Instantiate(currentSpell.spellPrefab, spellCastPosition, Quaternion.identity).GetComponent<Spell>();
        castSpell.OnCast(lookDirection, player.gameObject);
    }

    private void UpdateMagicQuantity()
    {
        //magicQuantityMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize * (currentMagic / maxMagic));
        UIHealthBar.instance.SetMagicValue(currentMagic / maxMagic);

        //if (currentMagic < currentSpell.magicCost) currentSpellIcon.color = new Color(currentSpellIcon.color.r, currentSpellIcon.color.g, currentSpellIcon.color.b, 0.75f);
        //else currentSpellIcon.color = new Color(currentSpellIcon.color.r, currentSpellIcon.color.g, currentSpellIcon.color.b, 1f);
        SpellHUD.instance.MarkSpellAffordability(currentMagic);
    }

    private void Update()
    {
        if (player.health > 0)
        {
            currentMagic = Mathf.Clamp(currentMagic + magicRegenRate * Time.deltaTime, 0f, maxMagic);
            UpdateMagicQuantity();        
        }
    }

    public override void LoadPlayerData(PlayerLoadData data)
    {
        currentMagic = data.currentMagic;
        if (player) UpdateMagicQuantity(); // dont do this when loading after swapping characters unless Start has already run

        loadedData = true;
    }

    public override PlayerLoadData GetPlayerLoadData()
    {
        PlayerLoadData data = new PlayerLoadData();
        
        data.numProjectiles = 0;
        data.currentMagic = (int)currentMagic;

        return data;
    }

    public override void RestAtShrine()
    {
        currentMagic = maxMagic;
        UpdateMagicQuantity();
    }
}
