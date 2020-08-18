// (c) Gijs Sickenga, 2018 //

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an amount of damage of a certain DamageType.
/// </summary>
[System.Serializable]
public struct Damage
{
    public Damage(int amount, DamageType type)
    {
        this.amount = amount;
        this.type = type;
    }

    /// <summary>
    /// The amount of damage inflicted.
    /// </summary>
    public int amount;

    /// <summary>
    /// The type of the damage inflicted.
    /// </summary>
    public DamageType type;
}
