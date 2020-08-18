// (c) Gijs Sickenga, 2018 //

using UnityEngine;

/// <summary>
/// Represents a named projectile category.
/// Every projectile is a member of one such category.
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "New Projectile Category", menuName = "Weapons/Labels/Projectile Category", order = 2)]
public class ProjectileCategory : ScriptableObject { }
