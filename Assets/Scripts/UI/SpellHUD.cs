using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellHUD : MonoBehaviour
{
    public static SpellHUD instance { get; private set; }

    public GameObject spellFramePrefab;

    public List<MagicSpellScriptableObject> availableSpells;

    public Transform spellFrameParent;

    private bool canUseMagic;

    private List<UISpellFrame> spellFrames;

    private MagicSpellScriptableObject currentSelectedSpell;
    private int currentSpellIndex;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Initialize the spells!
        //if (availableSpells.Count == 0) return;

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
    }

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
