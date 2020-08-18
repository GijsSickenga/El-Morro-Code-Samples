// (c) Gijs Sickenga, 2018 //

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

// TODO (Gijs):
// - Change CurrentWeapon prefab from GameObject to ScriptableObject (something like ProjectileWeaponStats).
// - Move firing sounds to weapon array so we don't play 27 individual sounds when firing broadside cannons once.

// - Nice-to-have: Make a tool to easily add new empty gameobjects, so the list of transforms can be easily filled when making a new ship layout.

/// <summary>
/// Represents a group of weapons that should be shot together.
/// </summary>
public class ProjectileWeaponArray : MonoBehaviour
{
    public enum ShootingOrder { InOrder, Random };

    [SerializeField]
    private UnityEvent _onReload;

    [SerializeField]
    private FloatReference _reloadTime;

    [SerializeField]
    [Tooltip("All weapon locations in the array.")]
    private List<Transform> _weaponSlots = new List<Transform>();

    [SerializeField]
    [Tooltip("All weapon categories the array supports.")]
    private List<WeaponCategory> _validCategories;
    public ReadOnlyCollection<WeaponCategory> ValidCategories
    {
        get
        {
            return _validCategories.AsReadOnly();
        }
    }

    /// <summary>
    /// Whether the specified weapon category is supported by the array.
    /// </summary>
    public bool CanEquipWeapon(WeaponCategory weaponCategory)
    {
        return ValidCategories.Contains(weaponCategory);
    }

    [SerializeField]
    [Tooltip("The weapon to place on all slots in the array.")]
    private GameObject _currentWeapon;
    /// <summary>
    /// The weapon to place on all slots in the array.
    /// </summary>
    public GameObject CurrentWeapon
    {
        get
        {
            return _currentWeapon;
        }

        set
        {
            _currentWeapon = ValidateWeapon(value);
        }
    }

    private List<ProjectileWeapon> _weapons = new List<ProjectileWeapon>();
    private List<ProjectileWeapon> _weaponsInShootingOrder = new List<ProjectileWeapon>();

    [SerializeField]
    [Tooltip("The layer the weapons' projectiles will be on.")]
    private Layers.Names _projectileLayer;

    [SerializeField]
    [Tooltip("What order the weapons in the array will be shot in.")]
    private ShootingOrder _shootingOrder = ShootingOrder.InOrder;
    public ShootingOrder WeaponShootingOrder
    {
        get
        {
            return _shootingOrder;
        }
    }

    [SerializeField]
    [Tooltip("Whether the array can be triggered again, before all weapons in it finish firing.")]
    private bool _canShootBeforeShootingCycleComplete = false;
    /// <summary>
    /// Whether the array can be triggered again, before all weapons in it finish firing.
    /// </summary>
    public bool CanShootBeforeShootingCycleComplete
    {
        get
        {
            return _canShootBeforeShootingCycleComplete;
        }
    }

    [SerializeField]
    [Range(0, 1000)]
    [Tooltip("The delay in seconds between each weapon shot.")]
    private float _baseDelayBetweenShots = 0f;
    /// <summary>
    /// The total delay in seconds between each weapon shot.
    /// Takes all delay between shot modifiers into account.
    /// </summary>
    public float DelayBetweenShots
    {
        get
        {
            return _baseDelayBetweenShots;
        }
    }

    [SerializeField]
    [Tooltip("The random delay range in seconds between each weapon shot. Added onto base delay.")]
    private Vector2 _randomDelayBetweenShotsMinMax = Vector2.zero;
    /// <summary>
    /// The random delay in seconds between each weapon shot.
    /// Added onto base delay.
    /// </summary>
    public Vector2 RandomDelayBetweenShotsMinMax
    {
        get
        {
            return _randomDelayBetweenShotsMinMax;
        }
    }

    /// <summary>
    /// Whether the weapon's array is currently shooting.
    /// </summary>
    private bool _shooting = false;
    /// <summary>
    /// Whether the weapon's array is currently shooting.
    /// </summary>
    public bool IsShooting
    {
        get
        {
            return _shooting;
        }
    }

    /// <summary>
    /// Whether the weapon's array is currently reloading.
    /// </summary>
    private bool _reloading = false;
    /// <summary>
    /// Whether the weapon's array is currently reloading.
    /// </summary>
    public bool IsReloading
    {
        get
        {
            return _reloading;
        }
    }

    /// <summary>
    /// Whether the weapon's array is currently able to shoot.
    /// </summary>
    public bool CanShoot
    {
        get
        {
            return !IsShooting && !IsReloading;
        }
    }

    /// <summary>
    /// Aims all weapons up.
    /// </summary>
    public void AimUp(float rotationVelocity)
    {
        foreach (ProjectileWeapon weapon in _weapons)
        {
            weapon.AimUp(rotationVelocity);
        }
    }

    /// <summary>
    /// Aims all weapons down.
    /// </summary>
    public void AimDown(float rotationVelocity)
    {
        foreach (ProjectileWeapon weapon in _weapons)
        {
            weapon.AimDown(rotationVelocity);
        }
    }

    /// <summary>
    /// Aims all weapons clockwise.
    /// </summary>
    public void AimClockwise(float rotationVelocity)
    {
        foreach (ProjectileWeapon weapon in _weapons)
        {
            weapon.AimClockwise(rotationVelocity);
        }
    }

    /// <summary>
    /// Aims all weapons counter-clockwise.
    /// </summary>
    public void AimCounterClockwise(float rotationVelocity)
    {
        foreach (ProjectileWeapon weapon in _weapons)
        {
            weapon.AimCounterClockwise(rotationVelocity);
        }
    }

    /// <summary>
    /// Shoots all weapons in the array.
    /// </summary>
    /// <param name="shotBy">The ship or structure that shot the projectile.</param>
    /// <param name="baseBody">The optional rigidbody of the base the weapon is attached to.</param>
    public void Shoot(GameObject shotBy, Rigidbody baseBody = null)
    {
        if (!CanShootBeforeShootingCycleComplete)
        {
            if (!CanShoot)
            {
                return;
            }
        }

        // All weapons ready: execute shooting cycle.
        StartCoroutine(ShootingCycle(shotBy, baseBody));
    }

    /// <summary>
    /// Executes the full shooting cycle of the array, firing all weapons in it.
    /// </summary>
    /// <param name="shotBy">The ship or structure that shot the projectile.</param>
    /// <param name="baseBody">The optional rigidbody of the base the weapon is attached to.</param>
    private IEnumerator ShootingCycle(GameObject shotBy, Rigidbody baseBody = null)
    {
        _shooting = true;
        _weaponsInShootingOrder = null;

        // Determine weapon order from shooting order.
        switch (WeaponShootingOrder)
        {
            case ShootingOrder.InOrder:
                _weaponsInShootingOrder = _weapons;
                break;

            case ShootingOrder.Random:
                _weaponsInShootingOrder = _weapons.OrderBy(x => Random.Range(0, _weapons.Count)).ToList();
                break;
        }

        // Loop over all weapons in the array.
        foreach (ProjectileWeapon weapon in _weaponsInShootingOrder)
        {
            // Make sure the weapon can shoot.
            if (weapon.CanShoot)
            {
                // Shoot the current weapon in the array.
                weapon.Shoot(_projectileLayer, shotBy, baseBody);
            }

            // Generate random extra delay.
            float randomDelay = Random.Range(RandomDelayBetweenShotsMinMax.x, RandomDelayBetweenShotsMinMax.y);
            float nextShotDelay = DelayBetweenShots + randomDelay;

            // Only wait between shots if the delay is > 0.
            if (nextShotDelay > 0)
            {
                // Wait the designated time before shooting the next weapon in the array.
                yield return new WaitForSeconds(nextShotDelay);
            }
        }

        // All weapons have been shot.
        _shooting = false;

        // Initiate reloading cycle.
        Reload();

        yield break;
    }

    /// <summary>
    /// Starts a coroutine that executes the full reloading cycle of the weapon's array.
    /// </summary>
    protected virtual void Reload()
    {
        StartCoroutine(ReloadingCycle());
    }

    /// <summary>
    /// Executes the full reloading cycle of the weapon's array.
    /// </summary>
    protected virtual IEnumerator ReloadingCycle()
    {
        _reloading = true;

        // TODO (Gijs): Throw event on weapon when done, and keep a list of all weapons that have reloaded in array. Then start reloading at the end of that cycle.
        
        // Tell reloadbar to animate by throwing reload event.
        _onReload.Invoke();

        // TODO (Gijs): Change this so it takes the reloadtime off of a ScriptableObject that represents the weapon.
        // Wait the designated time for the reload to complete.
        //yield return new WaitForSeconds(CurrentWeapon.GetComponent<ProjectileWeapon>().ReloadTime);
        yield return new WaitForSeconds(_reloadTime.Value);
        _reloading = false;

        yield break;
    }

    /// <summary>
    /// Verifies whether the given GameObject is a weapon, and applies it to all slots in the array if it is.
    /// Returns the weapon if it was valid, and returns null otherwise.
    /// </summary>
    private GameObject ValidateWeapon(GameObject weapon)
    {
        // Remove pre-existing weapons so we can spawn new ones.
        FunctionDelegator.ExecuteWhenSafe(RemoveAllWeapons);

        // Check if a weapon prefab was specified.
        if (weapon == null)
        {
            // No prefab specified.
            return null;
        }
        else
        {
            // Prefab specified, make sure it is actually a weapon, and if it is, whether the array supports it.
            ProjectileWeapon weaponScript = weapon.GetComponent<ProjectileWeapon>();
            if (weaponScript == null)
            {
                Debug.LogWarning("Speficied prefab does not contain a ProjectileWeapon script.", this);
                return null;
            }
            else if (!CanEquipWeapon(weaponScript.Category))
            {
                Debug.LogWarning("Speficied weapon is not supported by this array.", this);
                return null;
            }

            // Weapon specified, so fill the array with instances of that weapon.
            if (_weaponSlots.Count > 0)
            {
                foreach (Transform weaponSlot in _weaponSlots)
                {
                    if (weaponSlot != null)
                    {
                        // Make sure we only spawn weapon instances if this is a ship in the scene during play mode.
                        if (Application.isPlaying && !ExFuncs.IsPrefab(this))
                        {
                            FunctionDelegator.ExecuteWhenSafe(()=>
                            {
                                GameObject weaponInstance = Instantiate(weapon, weaponSlot.position, weaponSlot.rotation, weaponSlot);
                                _weapons.Add(weaponInstance.GetComponent<ProjectileWeapon>());
                            });
                        }
                    }
                    else
                    {
                        Debug.LogError("One of the weapon slot transforms in " + gameObject.name + " is null.", this);
                    }
                }
            }
        }

        // If we reached this point, the given prefab was a valid weapon, so we can return it.
        return weapon;
    }

    private void RemoveAllWeapons()
    {
        foreach (Transform weaponSlot in _weaponSlots)
        {
            if (Application.isPlaying)
            {
                for (int i = weaponSlot.childCount - 1; i >= 0; i--)
                {
                    Destroy(weaponSlot.GetChild(i).gameObject);
                }
            }
            else
            {
                for (int i = weaponSlot.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(weaponSlot.GetChild(i).gameObject, true);
                }
            }
        }
        _weapons.Clear();
    }

    private void Awake()
    {
        // Verify weapons when ship spawns in the scene.
        _currentWeapon = ValidateWeapon(_currentWeapon);
    }

    private void OnValidate()
    {
        // Verify weapons set through editor UI.
        _currentWeapon = ValidateWeapon(_currentWeapon);
    }
}
