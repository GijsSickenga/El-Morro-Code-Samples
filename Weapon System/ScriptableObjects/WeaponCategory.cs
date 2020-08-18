// (c) Gijs Sickenga, 2018 //

using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// Represents a named weapon category.
/// Every weapon is a member of one such category.
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "New Weapon Category", menuName = "Weapons/Labels/Weapon Category", order = 3)]
public class WeaponCategory : ScriptableObject
{
    [SerializeField]
    [Tooltip("All projectile categories the weapon can shoot.")]
    private List<ProjectileCategory> _validProjectileCategories;
    /// <summary>
    /// All projectile categories the weapon can shoot.
    /// </summary>
    public ReadOnlyCollection<ProjectileCategory> ValidProjectileCategories
    {
        get
        {
            return _validProjectileCategories.AsReadOnly();
        }
    }

    /// <summary>
    /// Returns whether the specified projectile category can be shot by the weapon.
    /// </summary>
    public bool CanShootProjectile(ProjectileCategory category)
    {
        return _validProjectileCategories.Contains(category);
    }
}
