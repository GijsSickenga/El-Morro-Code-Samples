// (c) Gijs Sickenga, 2018 //

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains projectile type and spawn point info for a projectile being fired
/// from a weapon.
/// </summary>
[System.Serializable]
public struct ProjectileSlot
{
    public ProjectileSlot(ProjectileBlueprint projectile, Transform spawnPoint)
    {
        _projectile = projectile;
        _spawnPoint = spawnPoint;
    }

    [SerializeField]
    [Tooltip("The projectile that will be spawned when the weapon is shot.")]
    private ProjectileBlueprint _projectile;
    /// <summary>
    /// The projectile that will be spawned when the weapon is shot.
    /// </summary>
    public ProjectileBlueprint Projectile
    {
        get
        {
            return _projectile;
        }
    }

    [SerializeField]
    [Tooltip("The transform on the weapon where the projectile will spawn.")]
    private Transform _spawnPoint;
    /// <summary>
    /// The transform on the weapon where the projectile will spawn.
    /// </summary>
    public Transform SpawnPoint
    {
        get
        {
            return _spawnPoint;
        }
    }
}
