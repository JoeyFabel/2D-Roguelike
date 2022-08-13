using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Consumable (Item)")]
public class Consumable : Item
{
    public GameObject objectToSpawn;
    public bool throwObject;
    
    public int healthToGain;
    public float manaToGain;
}