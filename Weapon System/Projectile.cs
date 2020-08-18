// (c) Gijs Sickenga, 2018 //

using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Baseclass for all projectiles in the game.
/// </summary>
public class Projectile : MonoBehaviour
{
    private const string DEFAULT_HEADER = "Default Settings";
    private const string CLUSTER_HEADER = "Cluster Settings";

    private Rigidbody _rigidbody;
    public Rigidbody Rigidbody
    {
        get
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }
            return _rigidbody;
        }
    }

    private MeshRenderer _meshRenderer;
    public MeshRenderer MeshRenderer
    {
        get
        {
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
            }
            return _meshRenderer;
        }
    }

    public enum AnchorType
    {
        ProjectileTransform,
        CollisionPoint
    }

    public enum Direction
    {
        RandomDome,
        RandomSphere,
        ParentDirection
    }

    [System.Serializable]
    public struct AnchoredEffect
    {
        [Tooltip("A GameObject with a particle system on it.")]
        public GameObject effect;
        [Tooltip("Where to spawn the particle effect.")]
        public Projectile.AnchorType anchor;
        [Tooltip("Whether to parent the effect in the collided object or not.")]
        public bool parentInTarget;
        public bool spawnOnCollision;
        public bool spawnOnTimedExplode;
    }

    [System.Serializable]
    public struct UnparentOnDestroyEffect
    {
        [Tooltip("A GameObject with a particle system on it.")]
        public GameObject effect;
        [Tooltip("How long to wait before destroying the effect after it has been unparented.")]
        public float destroyDelay;
    }

    [System.Serializable]
    public struct OnDestroyProjectile
    {
        [Tooltip("The projectile to shoot.")]
        public ProjectileBlueprint projectile;
        [Tooltip("Where to spawn the projectile.")]
        public Projectile.AnchorType anchor;
        [Tooltip("What direction to launch the projectile in.")]
        public Projectile.Direction direction;
        [Tooltip("Whether the spawnVelocity determines velocity directly, or becomes a percentage scalar for the parent projectile's velocity.")]
        public bool inheritParentVelocity;
        [Tooltip("What velocity to launch the projectile at. If inheriting parent velocity, this becomes a scalar for that velocity.")]
        public float spawnVelocity;
        [Tooltip("The angle to deviate from the base direction.")]
        public Vector3 relativeAngle;
        public bool spawnOnCollision;
        public bool spawnOnTimedExplode;
    }

    /// <summary>
    /// The height below which to destroy projectiles.
    /// </summary>
    private static float _destroyHeight = -5;

    private Coroutine _explosionRoutine = null;
    private bool _explodedOverTime = false;
    private bool _collisionOccured = false;
    private bool _dealtDamage = false;
    private SphereCollider _explosionCollider = null;

    private ProjectileBlueprint _stats;
    /// <summary>
    /// The stats of the projectile, such as projectile type, damage type and damage amount.
    /// </summary>
    public ProjectileBlueprint Stats
    {
        get
        {
            return _stats;
        }

        set
        {
            _stats = value;
            gameObject.name = _stats.Name;
        }
    }

    private GameObject _shotBy;
    /// <summary>
    /// The ship or structure that shot the projectile.
    /// </summary>
    public GameObject ShotBy
    {
        get
        {
            return _shotBy;
        }

        set
        {
            _shotBy = value;
        }
    }

    [Header(DEFAULT_HEADER)]
    [SerializeField]
    [Tooltip("The particle effects that play when the projectile explodes by hitting a solid object or by exploding over time.")]
    private List<AnchoredEffect> _onDestroyEffects = new List<AnchoredEffect>();

    [SerializeField]
    [Tooltip("The particle effects that play when the projectile hits the water.")]
    private List<GameObject> _waterImpactEffects = new List<GameObject>();

    [SerializeField]
    [Tooltip("Particle effects that need to be unparented when the projectile is destroyed. Use for effects like trails that need to fade out smoothly.")]
    private List<UnparentOnDestroyEffect> _unparentOnDestroyEffects = new List<UnparentOnDestroyEffect>();

    [Header(CLUSTER_HEADER)]
    [SerializeField]
    [Tooltip("Projectiles that need to be spawned when the projectile is destroyed.")]
    private List<OnDestroyProjectile> _onDestroyProjectiles = new List<OnDestroyProjectile>();

    /// <summary>
    /// Spawns a given AnchoredParticleEffect at a position, based on its anchor setting.
    /// </summary>
    /// <param name="anchoredEffect">The effect to spawn, and info on where to spawn it at.</param>
    /// <param name="collisionPoint">The specific collision point position, used for CollisionPoint anchors.</param>
    private void SpawnAnchoredEffect(AnchoredEffect anchoredEffect, Vector3 collisionPoint, Transform effectParent = null)
    {
        switch (anchoredEffect.anchor)
        {
            case AnchorType.ProjectileTransform:
            {
                Instantiate(anchoredEffect.effect, transform.position, Quaternion.identity, effectParent);
                break;
            }
            case AnchorType.CollisionPoint:
            {
                Instantiate(anchoredEffect.effect, collisionPoint, Quaternion.identity, effectParent);
                break;
            }
        }
    }

    /// <summary>
    /// Spawns a given OnDestroyProjectile at a position, based on its spawn location setting.
    /// </summary>
    /// <param name="projectileSettings">The projectile to spawn, and info on how to spawn it.</param>
    /// <param name="collisionPoint">The specific collision point position, used for CollisionPoint spawn points.</param>
    private void SpawnOnDestroyProjectile(OnDestroyProjectile projectileSettings, Vector3 collisionPoint)
    {
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        // Determine spawn position.
        switch (projectileSettings.anchor)
        {
            case AnchorType.ProjectileTransform:
            {
                spawnPosition = transform.position;
                break;
            }
            case AnchorType.CollisionPoint:
            {
                spawnPosition = collisionPoint;
                break;
            }
        }

        // Determine spawn rotation.
        Vector3 spawnDirection = Vector3.zero;
        switch (projectileSettings.direction)
        {
            case Direction.RandomDome:
            {
                spawnDirection = Random.insideUnitSphere.normalized;
                // Force positive y so direction can only be in an upwards dome shape.
                spawnDirection.y = Mathf.Abs(spawnDirection.y);
                break;
            }
            case Direction.RandomSphere:
            {
                spawnDirection = Random.insideUnitSphere.normalized;
                break;
            }
            case Direction.ParentDirection:
            {
                spawnDirection = transform.forward;
                break;
            }
        }
        spawnRotation = Quaternion.LookRotation(spawnDirection) * Quaternion.Euler(projectileSettings.relativeAngle);

        GameObject spawnedProjectile = projectileSettings.projectile.Spawn(spawnPosition, spawnRotation, (Layers.Names)gameObject.layer, ShotBy);

        // Assign initial velocity.
        float spawnVelocityForce = projectileSettings.spawnVelocity * (projectileSettings.inheritParentVelocity ? Rigidbody.velocity.magnitude : 1);
        Vector3 spawnVelocity = spawnedProjectile.transform.forward * spawnVelocityForce;
        Rigidbody projectileBody = spawnedProjectile.GetComponent<Rigidbody>();
        projectileBody.AddForce(spawnVelocity, ForceMode.VelocityChange);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.WATER_SURFACE))
        {
            foreach (GameObject effect in _waterImpactEffects)
            {
                // Since we don't have a specific collision point, use position instead.
                Instantiate(effect, transform.position, Quaternion.identity);
            }
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Prevent multiple collision calls in a row.
        if ((_collisionOccured && !_explodedOverTime) || (_explodedOverTime && _dealtDamage))
            return;

        if (_explosionCollider != null)
        {
            _explosionCollider.enabled = false;
            Rigidbody.isKinematic = true;
        }

        _collisionOccured = true;
        ContactPoint contact = collision.contacts[0];

        // Try to grab health script so we can apply damage.
        Health targetHealth = contact.otherCollider.transform.GetComponentInParent<Health>();

        if (targetHealth != null)
        {
            // Hit an object with health, so damage it.
            targetHealth.Damage(Stats.Damage);
            _dealtDamage = true; // ugly fix so over time explosions deal damage guaranteed
        }

        UnitData unitData = contact.otherCollider.transform.GetComponentInParent<UnitData>();

        if (unitData != null)
        {
            unitData.ProcessShipHit(ShotBy);
            _dealtDamage = true; // ugly fix so over time explosions deal damage guaranteed
        }

        // Only spawn FX and perform cleanup if this impact was caused before the projectile exploded over time.
        if (!_explodedOverTime)
        {
            // Instantiate all impact effects.
            foreach (AnchoredEffect effect in _onDestroyEffects)
            {
                if (effect.spawnOnCollision)
                {
                    SpawnAnchoredEffect(effect, contact.point, effect.parentInTarget ? contact.otherCollider.transform : null);
                }
            }

            // Instantiate all OnDestroyProjectiles.
            foreach (OnDestroyProjectile projectileSettings in _onDestroyProjectiles)
            {
                if (projectileSettings.spawnOnCollision)
                {
                    SpawnOnDestroyProjectile(projectileSettings, contact.point);
                }
            }

            // Prevent projectile from exploding over time.
            if (_explosionRoutine != null)
            {
                StopCoroutine(_explosionRoutine);
            }

            CleanupAndDestroy();
        }
    }

    protected virtual void Start()
    {
        if (Stats.ExplodesAfterSeconds)
        {
            _explosionRoutine = StartCoroutine(ExplodeAfterSeconds());
        }
    }

    protected virtual void Update()
    {
        // Rotate so forward always faces moving direction.
        if (Rigidbody.velocity.normalized != Vector3.zero)
            transform.localRotation = Quaternion.LookRotation(Rigidbody.velocity.normalized);

        // Destroy projectile if it falls too low below the world.
        if (transform.position.y < _destroyHeight)
        {
            CleanupAndDestroy();
        }
    }

    protected virtual IEnumerator ExplodeAfterSeconds()
    {
        yield return new WaitForSeconds(Stats.ExplosionTimer);

        // Mark the projectile as exploded.
        _explodedOverTime = true;

        // Instantiate all explosion effects.
        foreach (AnchoredEffect effect in _onDestroyEffects)
        {
            if (effect.spawnOnTimedExplode)
            {
                SpawnAnchoredEffect(effect, transform.position);
            }
        }

        // Instantiate all OnDestroyProjectiles.
        foreach (OnDestroyProjectile projectileSettings in _onDestroyProjectiles)
        {
            if (projectileSettings.spawnOnTimedExplode)
            {
                SpawnOnDestroyProjectile(projectileSettings, transform.position);
            }
        }

        if (Stats.ExplosionDealsDamage)
        {
            // Create an explosion collider.
            _explosionCollider = gameObject.AddComponent<SphereCollider>();
            float relativeColliderScalar = 1 / ((transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3);
            _explosionCollider.radius = Stats.ExplosionRadius * relativeColliderScalar;

            // Freeze the projectile in mid-air.
            Rigidbody.useGravity = false;
            Rigidbody.velocity = Vector3.zero;

            // Hide the projectile's default model.
            if (MeshRenderer != null)
            {
                MeshRenderer.enabled = false;
            }

            // Wait for the explosion to fade out.
            yield return new WaitForSeconds(Stats.ExplosionDuration);
        }

        CleanupAndDestroy();
    }

    /// <summary>
    /// Performs any required cleanup, like unparenting particle effects, before destroying the projectile.
    /// Override in subclass if additional cleanup is required there, but make sure to call base at the end.
    /// </summary>
    protected virtual void CleanupAndDestroy()
    {
        foreach (UnparentOnDestroyEffect unparentEffect in _unparentOnDestroyEffects)
        {
            // Unparent effect and destroy after specified time.
            unparentEffect.effect.transform.parent = null;
            DestroyAfterSeconds effectDestroyScript = unparentEffect.effect.AddComponent<DestroyAfterSeconds>();
            effectDestroyScript.destroyDelay = unparentEffect.destroyDelay;
        }

        // All cleanup handled, so we can safely destroy.
        Destroy(gameObject);
    }
}
