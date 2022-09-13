using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellHUD : MonoBehaviour
{
    public static SpellHUD instance { get; private set; }

    public GameObject spellFramePrefab;

    public List<MagicSpellScriptableObject> allSpells;
    private List<MagicSpellScriptableObject> availableSpells;
    
    public Transform spellFrameParent;

    private bool canUseMagic;

    private List<UISpellFrame> spellFrames;

    private MagicSpellScriptableObject currentSelectedSpell;
    private int currentSpellIndex;

    private static string[] startingSpells;

    private void Awake()
    {
        instance = this;
    }

    private void OnDisable()
    {
        Keyboard.current.onTextInput -= TryQuickSwapSpell;
    }

    private void Start()
    {
        // Initialize the available spells
       availableSpells ??= new List<MagicSpellScriptableObject>();
       if (startingSpells != null) SetAvailableSpells(startingSpells);
       
        spellFrames ??= new List<UISpellFrame>();

        for (int i = 0; i < availableSpells.Count; i++)
        {
            if (spellFrames.Find((item) =>
                    item.GetCorrespondingSpell().spellName.Equals(availableSpells[i].spellName))) continue;
            
            UISpellFrame spellFrame = Instantiate(spellFramePrefab, spellFrameParent).GetComponent<UISpellFrame>();

            spellFrame.Initialize();
            // 41 = 1, 42 = 2, ..., 50 = 0
            spellFrame.SetHotkey(i + 1);
            
            spellFrame.SetCorrespondingSpell(availableSpells[i]);

            spellFrames.Add(spellFrame);
        }

        if (spellFrames.Count > 0)
        {
            spellFrames[0].MarkAsSelected();
            currentSelectedSpell = spellFrames[0].GetCorrespondingSpell();
            currentSpellIndex = 0;
        }
        
        Keyboard.current.onTextInput += TryQuickSwapSpell;
    }

    private void TryQuickSwapSpell(char pressedCharacter)
    {
        // Return if you cant use magic
        if (!canUseMagic) return;

        // Return if a digit was not pressed
          if (!Char.IsDigit(pressedCharacter)) return;

        int pressedDigit = pressedCharacter - '0';
        
        if (spellFrames.Count >= pressedDigit) SelectSpecificSpell(pressedDigit);
    }
    
    public static string[] GetAvailableSpells()
    {
        string[] spells = new string[instance.availableSpells.Count];

        for (int i = 0; i < spells.Length; i++) spells[i] = instance.availableSpells[i].spellName;

        return spells;
    }

    private void SetAvailableSpells(string[] spells)
    {
        availableSpells ??= new List<MagicSpellScriptableObject>();
        
        foreach (var spell in allSpells)
            if (spells.Contains(spell.spellName)) availableSpells.Add(spell);
    }

    public static void GainSpell(string spell)
    {
        MagicSpellScriptableObject spellToAdd =
            instance.allSpells.Find((spellItem) => spellItem.spellName.Equals(spell));


        if (!instance.availableSpells.Contains(spellToAdd))
        {
            instance.availableSpells.Add(spellToAdd);
            UISpellFrame spellFrame = Instantiate(instance.spellFramePrefab, instance.spellFrameParent).GetComponent<UISpellFrame>();

            spellFrame.Initialize();
            // This has not been added to the spells yet, so it is Count instead of count - 1
            spellFrame.SetHotkey(instance.availableSpells.Count);

            spellFrame.SetCorrespondingSpell(spellToAdd);

            instance.spellFrames.Add(spellFrame);
        }
    }

    public static void LoadSpells(params string[] spells)
    {
        if (instance)
        {
            instance.availableSpells ??= new List<MagicSpellScriptableObject>();
            instance.spellFrames ??= new List<UISpellFrame>();

            foreach (var spell in spells) GainSpell(spell);
        }
        else
        {
            startingSpells = spells;
        }
    }

    public static void GainOnlyDefaultSpells()
    {
        instance.availableSpells = new List<MagicSpellScriptableObject>();
        
        instance.availableSpells.Add(instance.allSpells.Find((spell) => spell.spellName.Equals("Fireball")));
        instance.availableSpells.Add(instance.allSpells.Find((spell) => spell.spellName.Equals("Heal")));
    }
    
    public void MarkSpellAffordability(float currentMagic)
    {
        spellFrames.ForEach((frame) => frame.MarkSpellAffordability(currentMagic));
    }

    public MagicSpellScriptableObject GetSelectedSpell()
    {
        return currentSelectedSpell;
    }

    public void CycleSelectedSpell()
    {
        if (!canUseMagic) return;

        spellFrames[currentSpellIndex].Deselect();

        currentSpellIndex = (currentSpellIndex + 1) % spellFrames.Count;

        spellFrames[currentSpellIndex].MarkAsSelected();
        currentSelectedSpell = spellFrames[currentSpellIndex].GetCorrespondingSpell();

    }

    private void SelectSpecificSpell(int spellNumber)
    {
        if (!canUseMagic || spellNumber > spellFrames.Count) return;

        spellFrames[currentSpellIndex].Deselect();

        spellFrames[spellNumber - 1].MarkAsSelected();

        currentSpellIndex = spellNumber - 1;

        currentSelectedSpell = spellFrames[spellNumber - 1].GetCorrespondingSpell();
    }

    public void PlayerCanUseMagic()
    {
        canUseMagic = true;
        spellFrameParent.gameObject.SetActive(true);
    }

    public void PlayerCantUseMagic()
    {
        canUseMagic = false;
        spellFrameParent.gameObject.SetActive(false);
    }

    public bool GetCanUseMagic()
    {
        return canUseMagic;
    }
}
