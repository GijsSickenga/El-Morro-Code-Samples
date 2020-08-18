// (c) Gijs Sickenga, 2018 //

using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Represents a specific kind of projectile and its stats, and contains a Spawn method to spawn the projectile.
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "New Projectile Blueprint", menuName = "Weapons/Projectile Blueprint", order = 1)]
public class ProjectileBlueprint : ScriptableObject
{
    [SerializeField]
    [Tooltip("The name of the projectile.")]
    protected string _name;
    /// <summary>
    /// The name of the projectile.
    /// </summary>
    public string Name
    {
        get
        {
            return _name;
        }
    }

    [SerializeField]
    [Tooltip("The category of the projectile.")]
    protected ProjectileCategory _category;
    /// <summary>
    /// The category of the projectile.
    /// </summary>
    public ProjectileCategory Category
    {
        get
        {
            return _category;
        }
    }

    [SerializeField]
    [Tooltip("The prefab to spawn when the projectile is shot.")]
    protected GameObject _prefab;
    /// <summary>
    /// The prefab to spawn when the projectile is shot.
    /// </summary>
    public GameObject Prefab
    {
        get
        {
            return _prefab;
        }
    }

    [SerializeField]
    [Tooltip("The amount of damage the projectile causes on impact. When the projectile is explosive, the explosion does the same damage an impact would.")]
    protected Damage _damage;
    /// <summary>
    /// The amount of damage the projectile causes on impact.
    /// When the projectile is explosive, the explosion does the same damage an impact would.
    /// </summary>
    public Damage Damage
    {
        get
        {
            return _damage;
        }
    }

    [SerializeField]
    [Tooltip("What percentage of base velocity the projectile inherits if it is shot from a moving base.")]
    protected float _baseVelocityPercentage = 80f;
    /// <summary>
    /// A multiplier indicating the amount of velocity the projectile inherits if it is shot from a moving base.
    /// </summary>
    public float BaseVelocityMultiplier
    {
        get
        {
            return 0.01f * _baseVelocityPercentage;
        }
    }

    [SerializeField]
    [Tooltip("Whether the projectile explodes after a set amount of time.")]
    protected bool _explodesAfterSeconds = false;
    /// <summary>
    /// Whether the projectile explodes after a set amount of time.
    /// </summary>
    public bool ExplodesAfterSeconds
    {
        get
        {
            return _explodesAfterSeconds;
        }
    }

    [SerializeField]
    [Tooltip("The lifetime of the projectile before it explodes.")]
    [ShowIf("_explodesAfterSeconds")]
    protected float _explosionTimer = 0.75f;
    /// <summary>
    /// The lifetime of the projectile before it explodes.
    /// </summary>
    public float ExplosionTimer
    {
        get
        {
            return _explosionTimer;
        }
    }

    [SerializeField]
    [Tooltip("Whether the explosion should deal damage. The damage of the explosion is equal to the projectile's normal damage.")]
    [ShowIf("_explodesAfterSeconds")]
    protected bool _explosionDealsDamage = false;
    /// <summary>
    /// Whether the explosion should deal damage.
    /// </summary>
    public bool ExplosionDealsDamage
    {
        get
        {
            return _explosionDealsDamage;
        }
    }

    [SerializeField]
    [Tooltip("The radius of the explosion when the projectile explodes after the timer has run out.")]
    [ShowIf("_explosionDealsDamage")]
    protected float _explosionRadius = 5f;
    /// <summary>
    /// The radius of the explosion when the projectile explodes after the timer has run out.
    /// </summary>
    public float ExplosionRadius
    {
        get
        {
            return _explosionRadius;
        }
    }

    [SerializeField]
    [Tooltip("How long the explosion lingers.")]
    [ShowIf("_explosionDealsDamage")]
    protected float _explosionDuration = 0.25f;
    /// <summary>
    /// How long the explosion lingers.
    /// </summary>
    public float ExplosionDuration
    {
        get
        {
            return _explosionDuration;
        }
    }

    /// <summary>
    /// Spawns an instance of this projectile in the scene at the specified spawn point.
    /// </summary>
    /// <param name="spawnPoint">The spawn point to spawn the projectile at.</param>
    /// <param name="projectileLayer">The collision layer to set the projectile to.</param>
    /// <param name="shotBy">The ship or structure that shot the projectile.</param>
    /// <param name="parent">Optional parent transform for the projectile.</param>
    /// <returns>The spawned projectile.</returns>
    public GameObject Spawn(Transform spawnPoint, Layers.Names projectileLayer, GameObject shotBy, Transform parent = null)
    {
        GameObject newProjectile = GameObject.Instantiate(_prefab, spawnPoint.position, spawnPoint.rotation, parent);
        Projectile projectileScript = newProjectile.GetComponent<Projectile>();
        projectileScript.Stats = this;
        projectileScript.ShotBy = shotBy;
        newProjectile.layer = (int)projectileLayer;
        return newProjectile;
    }
    public GameObject Spawn(Vector3 position, Quaternion rotation, Layers.Names projectileLayer, GameObject shotBy, Transform parent = null)
    {
        GameObject newProjectile = GameObject.Instantiate(_prefab, position, rotation, parent);
        Projectile projectileScript = newProjectile.GetComponent<Projectile>();
        projectileScript.Stats = this;
        projectileScript.ShotBy = shotBy;
        newProjectile.layer = (int)projectileLayer;
        return newProjectile;
    }
}
