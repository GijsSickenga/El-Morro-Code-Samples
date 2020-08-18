// (c) Gijs Sickenga, 2018 //

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Baseclass for all projectile weapons.
/// </summary>
public class ProjectileWeapon : Weapon
{
    [SerializeField]
    protected WeaponCategory _category;
    public WeaponCategory Category
    {
        get
        {
            return _category;
        }
    }

    [SerializeField]
    [Tooltip("All projectile types shot by the weapon in a single salvo, combined with their respective spawn points.")]
    protected List<ProjectileSlot> _projectileSlots = new List<ProjectileSlot>();

    [SerializeField]
    [Tooltip("The AudioSource to play weapon sound fx with.")]
    protected AudioSource _audioSource;

    [SerializeField]
    [Tooltip("A list of sounds the weapon will randomly pick from when it shoots a projectile.")]
    protected List<AudioClip> _shootingSounds = new List<AudioClip>();

    [SerializeField]
    [Tooltip("The minimum and maximum pitch for the shooting sounds to play at.")]
    protected Vector2 _randomShootPitchMinMax;

    [SerializeField]
    [Tooltip("The sound the weapon plays when it reloads.")]
    protected AudioClip _reloadingSound;

    [SerializeField]
    [Tooltip("The minimum and maximum pitch for the reloading sound to play at.")]
    protected Vector2 _randomReloadPitchMinMax;

    /// <summary>
    /// The total number of shots per salvo.
    /// </summary>
    public int ShotsPerSalvo
    {
        get
        {
            return _projectileSlots.Count;
        }
    }

    [SerializeField]
    [Range(0, 1000)]
    [Tooltip("The delay in seconds between sequential shots in a salvo.")]
    protected float _baseDelayBetweenShots = 0f;
    /// <summary>
    /// The total delay in seconds between sequential shots in a salvo.
    /// Takes all delay between shots modifiers into account.
    /// </summary>
    public float DelayBetweenShots
    {
        get
        {
            return _baseDelayBetweenShots;
        }
    }

    [SerializeField]
    [Tooltip("The base velocity of the projectile being fired from the weapon.")]
    protected float _baseProjectileVelocity;
    /// <summary>
    /// The total velocity projectiles will be fired at.
    /// Takes all projectile velocity modifiers into account.
    /// </summary>
    public float ProjectileVelocity
    {
        get
        {
            return _baseProjectileVelocity;
        }
    }

    [SerializeField]
    [Range(1, 1000)]
    [Tooltip("The base number of salvos to shoot per shooting cycle.")]
    protected int _baseSalvosPerShootingCycle = 1;
    /// <summary>
    /// The total number of salvos to shoot per shooting cycle.
    /// Takes all salvos per shooting cycle modifiers into account.
    /// </summary>
    public int SalvosPerShootingCycle
    {
        get
        {
            return _baseSalvosPerShootingCycle;
        }
    }

    [SerializeField]
    [Range(0, 1000)]
    [Tooltip("The delay in seconds between sequential salvos in a shooting cycle.")]
    protected float _baseDelayBetweenSalvos = 0f;
    /// <summary>
    /// The total delay in seconds between sequential salvos in a shooting cycle.
    /// Takes all delay between salvos modifiers into account.
    /// </summary>
    public float DelayBetweenSalvos
    {
        get
        {
            return _baseDelayBetweenSalvos;
        }
    }

    /// <summary>
    /// Whether the weapon is currently shooting.
    /// </summary>
    protected bool _shooting = false;
    /// <summary>
    /// Whether the weapon is currently shooting.
    /// </summary>
    public bool IsShooting
    {
        get
        {
            return _shooting;
        }
    }

    /// <summary>
    /// Whether the weapon is currently able to shoot.
    /// </summary>
    public bool CanShoot
    {
        get
        {
            return !IsShooting;
        }
    }

    [SerializeField]
    [Range(0, 1000)]
    [Tooltip("The base reload time for the weapon in seconds.")]
    protected float _baseReloadTime;
    /// <summary>
    /// The total time in seconds it takes for the weapon to reload.
    /// Takes all reload time modifiers into account.
    /// </summary>
    public float ReloadTime
    {
        get
        {
            return _baseReloadTime;
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Starts a coroutine that executes the full shooting cycle of the weapon.
    /// </summary>
    /// <param name="projectileLayer">The collision layer to set the weapon's projectiles to.</param>
    /// <param name="shotBy">The ship or structure that shot the projectile.</param>
    /// <param name="baseBody">The optional rigidbody of the base the weapon is attached to.</param>
    public virtual void Shoot(Layers.Names projectileLayer, GameObject shotBy, Rigidbody baseBody = null)
    {
        StartCoroutine(ShootingCycle(projectileLayer, shotBy, baseBody));
    }

    /// <summary>
    /// Executes the full shooting cycle of the weapon.
    /// Use "yield return StartCoroutine(base.ShootingCycle());" when calling base from subclass.
    /// </summary>
    /// <param name="projectileLayer">The collision layer to set the weapon's projectiles to.</param>
    /// <param name="shotBy">The ship or structure that shot the projectile.</param>
    /// <param name="baseBody">The optional rigidbody of the base the weapon is attached to.</param>
    protected virtual IEnumerator ShootingCycle(Layers.Names projectileLayer, GameObject shotBy, Rigidbody baseBody = null)
    {
        // Flag _shooting true until final salvo has been shot.
        _shooting = true;

        // Loop over all salvos in the shooting cycle.
        for (int i = 1; i <= SalvosPerShootingCycle; i++)
        {
            // Loop over all shots in the salvo.
            for (int j = 1; j <= ShotsPerSalvo; j++)
            {
                // Spawn the current projectile in the salvo and launch it.
                LaunchProjectile(_projectileSlots[j - 1], projectileLayer, shotBy, baseBody);

                if (DelayBetweenShots > 0)
                {
                    yield return new WaitForSeconds(DelayBetweenShots);
                }
            }

            // Skip the delay after the final salvo.
            if (i != SalvosPerShootingCycle)
            {
                if (DelayBetweenSalvos > 0)
                {
                    yield return new WaitForSeconds(DelayBetweenSalvos);
                }
            }
        }

        // All salvos have been shot.
        _shooting = false;

        yield break;
    }

    /// <summary>
    /// Spawns a certain projectile based on the ProjectileSlot passed in and launches it.
    /// Override in subclass to add weapon specific FX, like barrel smoke.
    /// Don't forget to spawn the projectile or call this base method when overriding in subclass.
    /// </summary>
    /// <param name="projectileLayer">The collision layer to set the projectile to.</param>
    /// <param name="shotBy">The ship or structure that shot the projectile.</param>
    /// <param name="baseBody">The optional rigidbody of the base the weapon is attached to.</param>
    protected virtual void LaunchProjectile(ProjectileSlot projectileSlot, Layers.Names projectileLayer, GameObject shotBy, Rigidbody baseBody = null)
    {
        // Spawn an instance of the projectile.
        GameObject newProjectile = projectileSlot.Projectile.Spawn(projectileSlot.SpawnPoint, projectileLayer, shotBy);

        // Spawn velocity is projectile velocity in forward direction.
        Vector3 spawnVelocity = projectileSlot.SpawnPoint.transform.forward * ProjectileVelocity;
        // If a moving base rigidbody was passed in, take a part of its velocity and add it to the spawnVelocity.
        if (baseBody != null)
        {
            spawnVelocity += baseBody.velocity * projectileSlot.Projectile.BaseVelocityMultiplier;
        }
        
        // Add the projectile's starting velocity and its base object's velocity together to launch it.
        Rigidbody projectileBody = newProjectile.GetComponent<Rigidbody>();
        projectileBody.AddForce(spawnVelocity, ForceMode.VelocityChange);

        PlayRandomShootingSound();
    }

    /// <summary>
    /// Selects a random shooting sound and plays it at a random pitch.
    /// </summary>
    /// <returns>Whether a sound was played.</returns>
    protected virtual bool PlayRandomShootingSound()
    {
        if (_shootingSounds.Count > 0)
        {
            int soundToPlay = 0;

            if (_shootingSounds.Count > 1)
            {
                soundToPlay = Random.Range(0, _shootingSounds.Count);
            }

            float randomPitch = Random.Range(_randomShootPitchMinMax.x, _randomShootPitchMinMax.y);
            _audioSource.pitch = randomPitch + 1;

            _audioSource.PlayOneShot(_shootingSounds[soundToPlay]);

            // Reset the pitch to default value.
            _audioSource.pitch = 1;

            // Played a sound, so return true.
            return true;
        }

        // Played no sound, so return false;
        return false;
    }

    /// <summary>
    /// Plays the reloading sound at a random pitch.
    /// </summary>
    /// <returns>Whether a sound was played.</returns>
    protected virtual bool PlayReloadingSound()
    {
        // Make sure a reloading sound is specified for this weapon.
        if (_reloadingSound != null)
        {
            float randomPitch = Random.Range(_randomReloadPitchMinMax.x, _randomReloadPitchMinMax.y);
            _audioSource.pitch = randomPitch + 1;

            _audioSource.PlayOneShot(_reloadingSound);

            // Reset the pitch to default value.
            _audioSource.pitch = 1;

            // Played a sound, so return true.
            return true;
        }

        // Played no sound, so return false;
        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Check if a weapon category was specified.
        if (Category == null)
        {
            for (int i = 0; i < _projectileSlots.Count; i++)
            {
                _projectileSlots[i] = new ProjectileSlot(null, _projectileSlots[i].SpawnPoint);
            }
            return;
        }
        else
        {
            // Category specified, now make sure all projectiles are supported by the weapon.
            for (int i = 0; i < _projectileSlots.Count; i++)
            {
                if (_projectileSlots[i].Projectile != null)
                {
                    if (!Category.CanShootProjectile(_projectileSlots[i].Projectile.Category))
                    {
                        Debug.LogWarning(_projectileSlots[i].Projectile.Category.name + " projectiles are not supported by this weapon's category.", this);

                        // Set projectile to null after logging warning message.
                        _projectileSlots[i] = new ProjectileSlot(null, _projectileSlots[i].SpawnPoint);
                    }
                }
            }
        }
    }
#endif
}
