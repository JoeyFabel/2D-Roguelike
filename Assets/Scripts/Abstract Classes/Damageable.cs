using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public abstract class Damageable : MonoBehaviour
{
    public enum DamageTypes
    {
        Fire,
        Slash,
        Thrust,
        Blunt,
        Magic
    }
    #region Resistance Structures and classes

    [Serializable]
public struct Resistance
{
    // The percentage of resistance (ie 0 = 0% resistance, 1 = 100% resistance, 0.5 = 50% resistance)
    // Negative numbers are weaknesses and positive numbers are resistances

    public DamageTypes attackType;
    public float resistance;

        public Resistance(DamageTypes damageType, float resistanceValue)
        {
            attackType = damageType;
            resistance = resistanceValue;
        }
}

    [Serializable]
    public struct Resistances
    {
        public Resistance[] resistances;

        public Resistances(Resistance fireResistance, Resistance slashResistance, Resistance thrustResistance, Resistance bluntResistance, Resistance magicResistance)
        {
            fireResistance.attackType = DamageTypes.Fire;
            slashResistance.attackType = DamageTypes.Slash;
            thrustResistance.attackType = DamageTypes.Thrust;
            bluntResistance.attackType = DamageTypes.Blunt;
            magicResistance.attackType = DamageTypes.Magic;

            resistances = new Resistance[5] { fireResistance, slashResistance, thrustResistance, bluntResistance, magicResistance };

            /*
            int numDamageTypes = Enum.GetNames(typeof(DamageTypes)).Length;

            List<Resistance> resistanceList = new List<Resistance>();
            resistanceList.AddRange(startingResistances);

            if (resistanceList.Count < numDamageTypes)
            {
                for (int i = 0; i < numDamageTypes; i++)
                {
                    var res = resistanceList.FindIndex(item => (int)item.attackType == i);

                    // res == -1 if it does not exist, else it is the index
                    if (res >= 0) continue;
                    else
                    {
                        Resistance newResistance
                    }
                }
            } */
        }

        public Resistances(float fireResistance, float slashResistance, float thrustResistance, float bluntResistance, float magicResistance)
        {
            resistances = new Resistance[5];

            resistances[0] = new Resistance(DamageTypes.Fire, fireResistance);
            resistances[1] = new Resistance(DamageTypes.Slash, slashResistance);
            resistances[2] = new Resistance(DamageTypes.Thrust, thrustResistance);
            resistances[3] = new Resistance(DamageTypes.Blunt, bluntResistance);
            resistances[4] = new Resistance(DamageTypes.Magic, magicResistance);
        }

        public float GetResistanceValue(DamageTypes attackType)
        {
            foreach (var resistance in resistances)
            {
                if (resistance.attackType == attackType) return resistance.resistance;
            }

            return 0f;
        }
    }
    #endregion

    public int maxHealth;
    protected float currentHealth;

    public Resistances resistances = new Resistances(0f, 0f, 0f, 0f, 0f);

    [Header("Audio")]
    public AudioClip[] damageSounds;
    public AudioClip[] deathSounds;
    public AudioClip fireballImpactSound;
    public AudioClip slashImpactSound;
    public AudioClip thrustImpactSound;
    public AudioClip bluntImpactSound;
    public AudioClip magicImpactSound;

    protected AudioSource audioSource;

    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();

        currentHealth = maxHealth;
    }

    public AudioClip GetWeaponHitSound(DamageTypes weaponType)
    {
        switch (weaponType)
        {
            case DamageTypes.Fire:
                return fireballImpactSound;
            case DamageTypes.Slash:
                return slashImpactSound;
            case DamageTypes.Thrust:
                return thrustImpactSound;
            case DamageTypes.Blunt:
                return bluntImpactSound;
            case DamageTypes.Magic:
                return magicImpactSound;
        }

        return null;
    }

    /// <summary>
    /// Subtracts the specified amount from the damageables health, checks for death, and plays any audio sfx.
    /// This is the ApplyDamage that always actually applies the damage.
    /// </summary>
    /// <param name="amount">The amount of damage to take</param>
    public virtual void ApplyDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f) Death();
        else PlayDamagedSound();
    }

    /// <summary>
    /// Applies damage to the damageable, after factoring in resistances.
    /// This just calls the other <see cref="ApplyDamage(float)"/> method.
    /// </summary>
    /// <param name="amount">The amount of damage to take</param>
    /// <param name="damageType">The damage source's damage type</param>
    public virtual void ApplyDamage(float amount, DamageTypes damageType)
    {
        float damage = amount - (amount * resistances.GetResistanceValue(damageType));

        // Right now, technically could heal if resistances > 100%, but not implemented, maybe never will be
        if (damage >= 0) ApplyDamage(damage);
    }

    protected abstract void Death();    

    protected void PlayDamagedSound()
    {
        AudioClip clipToPlay = damageSounds[Random.Range(0, damageSounds.Length)];

        audioSource.PlayOneShot(clipToPlay);
    }
}


