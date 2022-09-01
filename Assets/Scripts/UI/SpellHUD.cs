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
        // Initialize the spells!
        //if (availableSpells.Count == 0) return;

        print("creating spells");
        
        availableSpells = new List<MagicSpellScriptableObject>(allSpells);
        
        spellFrames = new List<UISpellFrame>();

        for (int i = 0; i < availableSpells.Count; i++)
        {
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
        if (canUseMagic) return;

        // Return if a digit was not pressed
        if (!Char.IsDigit(pressedCharacter)) return;

        int pressedDigit = (int)pressedCharacter - (int)'0';
        
        if (spellFrames.Count >= pressedDigit) SelectSpecificSpell(pressedDigit);
    }
    
    /*
    private void Update()
    {
        // Tries to quick swap spells, cant find a better way to do this
        if (!canUseMagic) return;

        if (spellFrames.Count >= 1 && Keyboard.current.digit1Key.wasPressedThisFrame) SelectSpecificSpell(1);        
        else if (spellFrames.Count >= 2 && Keyboard.current.digit2Key.wasPressedThisFrame) SelectSpecificSpell(2);
        else if (spellFrames.Count >= 3 && Keyboard.current.digit3Key.wasPressedThisFrame) SelectSpecificSpell(3);
        else if (spellFrames.Count >= 4 && Keyboard.current.digit4Key.wasPressedThisFrame) SelectSpecificSpell(4);
        else if (spellFrames.Count >= 5 && Keyboard.current.digit5Key.wasPressedThisFrame) SelectSpecificSpell(5);
        else if (spellFrames.Count >= 6 && Keyboard.current.digit6Key.wasPressedThisFrame) SelectSpecificSpell(6);
        else if (spellFrames.Count >= 7 && Keyboard.current.digit7Key.wasPressedThisFrame) SelectSpecificSpell(7);
        else if (spellFrames.Count >= 8 && Keyboard.current.digit8Key.wasPressedThisFrame) SelectSpecificSpell(8);
        else if (spellFrames.Count >= 9 && Keyboard.current.digit9Key.wasPressedThisFrame) SelectSpecificSpell(9);
        else if (spellFrames.Count >= 10 && Keyboard.current.digit0Key.wasPressedThisFrame) SelectSpecificSpell(10);
    }
    */

    public string[] GetAvailableSpells()
    {
        string[] spells = new string[availableSpells.Count];

        for (int i = 0; i < spells.Length; i++) spells[i] = availableSpells[i].spellName;

        return spells;
    }

    public void SetAvailableSpells(string[] spells)
    {
        availableSpells = new List<MagicSpellScriptableObject>();
        
        foreach (var spell in allSpells)
            if (spells.Contains(spell.spellName)) availableSpells.Add(spell);
    }

    public static void GainSpell(string spell)
    {
        MagicSpellScriptableObject spellToAdd =
            instance.allSpells.Find((spellItem) => spellItem.spellName.Equals(spell));
        
        instance.availableSpells.Add(spellToAdd);
        
        
        UISpellFrame spellFrame = Instantiate(instance.spellFramePrefab, instance.spellFrameParent).GetComponent<UISpellFrame>();

        spellFrame.Initialize();
        // 41 = 1, 42 = 2, ..., 50 = 0
        spellFrame.SetHotkey(instance.availableSpells.Count - 1);
            
        spellFrame.SetCorrespondingSpell(spellToAdd);

        instance.spellFrames.Add(spellFrame);
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

    public void SelectSpecificSpell(int spellNumber)
    {
        print("selecting spell " + spellNumber);

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
}
