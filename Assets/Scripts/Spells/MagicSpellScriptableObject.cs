using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spell")]
public class MagicSpellScriptableObject : ScriptableObject
{
    public Sprite uiImage;

    public string spellName;

    public float damage;
    public float castTime;
    public float magicCost;

    public AudioClip castSound;
    public AudioClip prepSound;

    public GameObject spellPrefab;
    public GameObject preparationPrefab;

    [Tooltip("If true, the spell will constantly cast until the action is cancelled. Most useful for things like healing spells or spells that constantly deal small amounts of damage.")]
    public bool continuousUseSpell = false;
}
