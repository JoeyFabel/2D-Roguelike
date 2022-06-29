using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Key = UnityEngine.InputSystem.Key;

public class UISpellFrame : MonoBehaviour
{
    public static float selectedSizeMultiplier = 1.2f;

    public Sprite defaultFrame;
    public Sprite selectedFrame;

    public Image spellIcon;

    public Text hotkeyText;
    private Key hotkey;

    public bool IsSelected { get; private set; }

    private Image spellFrame;

    private MagicSpellScriptableObject correspondingSpell;

    public void Initialize()
    {
        spellFrame = GetComponent<Image>();

        Deselect();
    }

    public void SetHotkey(int hotkeyNumber)
    {
        hotkey = (Key)(40 + hotkeyNumber);
        hotkeyText.text = hotkeyNumber.ToString();
    }

    public Key GetHotkey()
    {
        return hotkey;
    }

    public void SetCorrespondingSpell(MagicSpellScriptableObject spell)
    {
        correspondingSpell = spell;

        spellIcon.sprite = spell.uiImage;
    }

    public MagicSpellScriptableObject GetCorrespondingSpell()
    {
        return correspondingSpell;
    }

    public void MarkAsSelected()
    {
        IsSelected = true;

        spellFrame.sprite = selectedFrame;

        spellFrame.rectTransform.localScale = new Vector3(selectedSizeMultiplier, selectedSizeMultiplier, 1);
    }

    public void Deselect()
    {
        IsSelected = false;

        spellFrame.sprite = defaultFrame;

        spellFrame.rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// Partially fades out a spell if it is unaffordable.
    /// </summary>
    /// <param name="currentMagic">The amount of magic that the player currently has</param>
    public void MarkSpellAffordability(float currentMagic)
    {
        if (currentMagic < correspondingSpell.magicCost) spellIcon.color = new Color(spellIcon.color.r, spellIcon.color.g, spellIcon.color.b, 0.6f);
        else spellIcon.color = new Color(spellIcon.color.r, spellIcon.color.g, spellIcon.color.b, 1f);
    }

    public bool Equals(UISpellFrame other)
    {
        return other.correspondingSpell.spellName.Equals(correspondingSpell.spellName);
    }
}
