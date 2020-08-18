// (c) Gijs Sickenga, 2018 //

using System.Collections;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Controls how the ship is affected by physics and physics events.
/// </summary>
public class ShipPhysics : MonoBehaviour
{
    private const string READ_ONLY_BOX_TITLE = "Read Only Values";
    private const string ANIMATIONS_BOX_TITLE = "Animations";
    private const string SAILING_BOX_TITLE = "Sailing";
    private const string STEERING_BOX_TITLE = "Steering";
    private const string RAMMING_BOX_TITLE = "Ramming";
    private const string SWAY_BOX_TITLE = "Sway";
    private const string ON_COLLISION_TITLE = "On Collision";

    private ShipStats _shipStats;
    public ShipStats ShipStats
    {
        get
        {
            if (_shipStats == null)
            {
                _shipStats = GetComponent<ShipStats>();
            }
            return _shipStats;
        }
    }

    #region Boolean Properties
    /// <summary>
    /// Whether the ship responds to steering input.
    /// </summary>
    public bool CanReceiveInput
    {
        get;
        set;
    }

    /// <summary>
    /// Whether the ship is currently turning clockwise or counter-clockwise.
    /// </summary>
    public bool IsTurning
    {
        get;
        private set;
    }

    private bool _isBeingSpedUp = false;
    private bool _isBeingSpedUpThisFrame = false;
    /// <summary>
    /// Whether the ship is currently being sped up from outside ShipPhysics.
    /// </summary>
    public bool IsBeingSpedUp
    {
        get { return _isBeingSpedUp; }
        private set
        {
            _isBeingSpedUp = value;
            if (IsBeingSpedUp)
            {
                _isBeingSpedUpThisFrame = true;
            }
        }
    }

    /// <summary>
    /// Whether the ship is currently being steered from outside ShipPhysics.
    /// </summary>
    public bool IsBeingSteered
    {
        get { return IsBeingSteeredClockwise || IsBeingSteeredCounterClockwise; }
    }

    private bool _isBeingSteeredClockwise = false;
    private bool _isBeingSteeredClockwiseThisFrame = false;
    /// <summary>
    /// Whether the ship is currently being steered clockwise from outside ShipPhysics.
    /// </summary>
    public bool IsBeingSteeredClockwise
    {
        get { return _isBeingSteeredClockwise; }
        private set
        {
            _isBeingSteeredClockwise = value;
            if (IsBeingSteeredClockwise)
            {
                _isBeingSteeredClockwiseThisFrame = true;
            }
        }
    }

    private bool _isBeingSteeredCounterClockwise = false;
    private bool _isBeingSteeredCounterClockwiseThisFrame = false;
    /// <summary>
    /// Whether the ship is currently being steered counter-clockwise from outside ShipPhysics.
    /// </summary>
    public bool IsBeingSteeredCounterClockwise
    {
        get { return _isBeingSteeredCounterClockwise; }
        private set
        {
            _isBeingSteeredCounterClockwise = value;
            if (IsBeingSteeredCounterClockwise)
            {
                _isBeingSteeredCounterClockwiseThisFrame = true;
            }
        }
    }
    #endregion

    #region Current & Read Only Values
    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    [Tooltip("The current sailing velocity of the ship.")]
    private float _sailVelocity = 0f;
    /// <summary>
    /// The current sailing velocity of the ship.
    /// </summary>
    public float SailVelocity
    {
        get { return _sailVelocity; }
        set { _sailVelocity = isDashing ? Mathf.Max(0, value) : Mathf.Clamp(value, 0, MaxSailVelocity); }
    }

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    [Tooltip("The current velocity as a Vector3")]
    private Vector3 _rigidBodyVelocity;
    public Vector3 RigidBodyVelocity
    {
        get { return _rigidBodyVelocity; }
        set 
        { 
            _rigidBodyVelocity = value; 
            ShipStats.ShipRigidbody.velocity = _rigidBodyVelocity;
        }
    }

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    private float _rammingVelocity;
    public float RammingVelocity
    {
        get { return _rammingVelocity; }
        set { _rammingVelocity = Mathf.Clamp(value, 0, MaxRammingVelocity); }
    }

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    private Vector3 _rammingDirection;
    public Vector3 RammingDirection
    {
        get { return _rammingDirection; }
    }

    private bool _shouldProcessRamming = true;
    public bool ShouldProcessRamming
    {
        get { return _shouldProcessRamming; }
        set { _shouldProcessRamming = value; }
    }

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    [Tooltip("The time in seconds it takes the ship to stop moving from max velocity through friction.")]
    private float _sailingSlowdownTime = 0f;
    /// <summary>
    /// The time in seconds it takes the ship to stop moving from max velocity through friction.
    /// </summary>
    public float SailingSlowdownTime
    {
        get { return _sailingSlowdownTime; }
    }

    // Place some space between different variable categories in the inspector.
    [Space(5)]

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    [Tooltip("The current turning velocity of the ship.")]
    private float _rotationalVelocity = 0f;
    /// <summary>
    /// The current turning velocity of the ship.
    /// Negative when turning counter-clockwise.
    /// </summary>
    public float RotationalVelocity
    {
        get { return _rotationalVelocity; }
        private set
        {
            _rotationalVelocity = Mathf.Clamp(value, -MaxRotationalVelocity, MaxRotationalVelocity);
            IsTurning = _rotationalVelocity != 0 ? true : false;
        }
    }

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    [Tooltip("The time in seconds it takes the ship to stop turning from max turning velocity through friction.")]
    private float _turningSlowdownTime = 0f;
    /// <summary>
    /// The time in seconds it takes the ship to stop turning from max turning velocity through friction.
    /// </summary>
    public float TurningSlowdownTime
    {
        get { return _turningSlowdownTime; }
    }

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    private bool isDashing = false;

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    private float _dashCooldownTimer = 0;
    #endregion

    #region Animations Settings
    [BoxGroup(ANIMATIONS_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The maximum clockwise & counterclockwise rotation for the steering wheel at max rotational velocity.")]
    private float _maxSteeringWheelRotation = 180;
    /// <summary>
    /// The maximum sailing velocity of the ship.
    /// </summary>
    public float MaxSteeringWheelRotation
    {
        get { return _maxSteeringWheelRotation; }
        private set { _maxSteeringWheelRotation = ExFuncs.ClampPositive(value); }
    }

    [BoxGroup(ANIMATIONS_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The maximum clockwise & counterclockwise rotation for the rudder at max rotational velocity.")]
    private float _maxRudderRotation = 45;
    /// <summary>
    /// The maximum sailing velocity of the ship.
    /// </summary>
    public float MaxRudderRotation
    {
        get { return _maxRudderRotation; }
        private set { _maxRudderRotation = ExFuncs.ClampPositive(value); }
    }
    #endregion

    #region Sailing Settings
    [BoxGroup(SAILING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The maximum sailing velocity of the ship in unity units per second.")]
    private float _maxSailVelocity = 40f;
    /// <summary>
    /// The maximum sailing velocity of the ship.
    /// </summary>
    public float MaxSailVelocity
    {
        get { return _maxSailVelocity; }
        private set
        {
            _maxSailVelocity = ExFuncs.ClampPositive(value);
            UpdateSailingSlowdownTime();
        }
    }

    /// <summary>
    /// Returns current sailing velocity portion of max sailing velocity.
    /// 1 = maxed out velocity.
    /// </summary>
    public float SailVelocityPortionOfMax
    {
        get { return SailVelocity / MaxSailVelocity; }
    }

    [BoxGroup(SAILING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The sailing acceleration of the ship in unity units per second.")]
    private float _sailAcceleration = 15f;
    /// <summary>
    /// The sailing acceleration of the ship.
    /// </summary>
    public float SailAcceleration
    {
        get { return _sailAcceleration; }
        private set { _sailAcceleration = ExFuncs.ClampPositive(value); }
    }

    [BoxGroup(SAILING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("How much friction is applied to the sailing velocity per second.")]
    private float _sailingFriction = 1.3333333f;
    /// <summary>
    /// How much friction is applied to the sailing velocity per second.
    /// </summary>
    public float SailingFriction
    {
        get { return _sailingFriction; }
        private set
        {
            _sailingFriction = ExFuncs.ClampPositive(value);
            UpdateSailingSlowdownTime();
        }
    }

    /// <summary>
    /// Updates the sailing slowdown time based on max velocity and friction.
    /// </summary>
    private void UpdateSailingSlowdownTime()
    {
        _sailingSlowdownTime = ExFuncs.RoundToDecimal(MaxSailVelocity / SailingFriction, 2);
    }

    [BoxGroup(SAILING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The time in seconds it takes the ship to slow down from max velocity when braking.")]
    private float _brakeSlowdownTime = 3f;
    /// <summary>
    /// The time in seconds it takes the ship to slow down from max velocity when braking.
    /// </summary>
    public float BrakeSlowdownTime
    {
        get { return _brakeSlowdownTime; }
        private set { _brakeSlowdownTime = ExFuncs.ClampPositive(value); }
    }

    [Header("Dashing")]
    [BoxGroup(SAILING_BOX_TITLE)]
    [SerializeField]
    private bool enableDash = false;

    [BoxGroup(SAILING_BOX_TITLE)]
    [SerializeField] [MinValue(0)] [ShowIf("enableDash")]
    [Tooltip("The amount of acceleration when performing a dash")]
    private float _dashAcceleration = 60f;
    public float DashAcceleration
    {
        get { return _dashAcceleration; }
    }

    [BoxGroup(SAILING_BOX_TITLE)]
    [SerializeField] [MinValue(0)] [ShowIf("enableDash")]
    [Tooltip("Time in seconds before velocity gets restricted again")]
    private float _maxDashDuration = 1f;
    public float MaxDashDuration
    {
        get { return _maxDashDuration; }
    }

    [BoxGroup(SAILING_BOX_TITLE)]
    [SerializeField] [MinValue(0)] [ShowIf("enableDash")]
    [Tooltip("Time in seconds for easing back to max velocity")]
    private float _dashSlowDownDuration = 0.1f;
    public float DashSlowDownDuration
    {
        get { return _dashSlowDownDuration; }
    }

    [BoxGroup(SAILING_BOX_TITLE)]
    [SerializeField] [ShowIf("enableDash")]
    [Tooltip("Time before dash is available again")]
    private FloatReference _dashCooldownDuration;
    public float DashCooldownDuration
    {
        get { return _dashCooldownDuration.Value; }
    }

    [BoxGroup(SAILING_BOX_TITLE)]
    [ShowIf("enableDash")]
    [Tooltip("Raised when the dash cool down timer starts running")]
    public GameEvent onShipDash;

    [BoxGroup(SAILING_BOX_TITLE)]
    [ShowIf("enableDash")]
    [Tooltip("The effect that plays when a dash is activated")]
    public GameObject dashEffect;
    #endregion

    #region Steering Settings
    [BoxGroup(STEERING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The maximum rotational velocity of the ship in degrees per second.")]
    private float _maxRotationalVelocity = 50f;
    /// <summary>
    /// The maximum rotational velocity of the ship.
    /// </summary>
    public float MaxRotationalVelocity
    {
        get { return _maxRotationalVelocity; }
        private set
        {
            _maxRotationalVelocity = ExFuncs.ClampPositive(value);
            UpdateTurningSlowdownTime();
        }
    }

    /// <summary>
    /// Returns current rotational velocity portion of max rotational velocity.
    /// 1 = maxed out velocity, clockwise.
    /// -1 = maxed out velocity, counter-clockwise.
    /// </summary>
    public float RotationalVelocityPortionOfMax
    {
        get { return RotationalVelocity / MaxRotationalVelocity; }
    }

    [BoxGroup(STEERING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The rotational acceleration of the ship in degrees per second.")]
    private float _rotationalAcceleration = 140f;
    /// <summary>
    /// The rotational acceleration of the ship in degrees per second.
    /// </summary>
    public float RotationalAcceleration
    {
        get { return _rotationalAcceleration; }
        private set { _rotationalAcceleration = ExFuncs.ClampPositive(value); }
    }

    [BoxGroup(STEERING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("How much friction is applied to the rotational velocity per second.")]
    private float _turningFriction = 25f;
    /// <summary>
    /// How much friction is applied to the rotational velocity per second.
    /// </summary>
    public float TurningFriction
    {
        get { return _turningFriction; }
        private set
        {
            _turningFriction = ExFuncs.ClampPositive(value);
            UpdateTurningSlowdownTime();
        }
    }

    /// <summary>
    /// Updates the turning slowdown time based on max velocity and friction.
    /// </summary>
    private void UpdateTurningSlowdownTime()
    {
        _turningSlowdownTime = ExFuncs.RoundToDecimal(MaxRotationalVelocity / TurningFriction, 2);
    }
    #endregion

    #region Ramming Settings
    [BoxGroup(RAMMING_BOX_TITLE)]
    [SerializeField] [MinValue(0), MaxValue(180)]
    [Tooltip("The minimum angle the collision has to be to be considered a big impact.")]
    private float _minRamImpactAngle = 60f;
    public float MinRamImpactAngle
    {
        get { return _minRamImpactAngle; }
    }

    [BoxGroup(RAMMING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The minimum speed difference to be considered a big impact.")]
    private float _minRamSpeedDifference = 20f;
    public float MinRamSpeedDifference
    {
        get { return _minRamSpeedDifference; }
    }

    [BoxGroup(RAMMING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    private float _maxRammingVelocity = 100;
    public float MaxRammingVelocity
    {
        get { return _maxRammingVelocity; }
    }

    [BoxGroup(RAMMING_BOX_TITLE)]
    [Header("Big Impact Victim")]
    [SerializeField] [MinValue(0)]
    [Tooltip("The magnitude of how much the affected ship gets pushed back, where 1 equals the speed difference.")]
    private float _pushMagnitude = 50f;
    public float PushingMagnitude
    {
        get { return _pushMagnitude; }
    }

    [BoxGroup(RAMMING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("How much friction is applied to the ramming velocity per second.")]
    private float _rammingFriction = 3f;
    public float RammingFriction
    {
        get { return _rammingFriction; }
    }

    [BoxGroup(RAMMING_BOX_TITLE)]
    [Header("Rammer")]
    [SerializeField] [MinValue(0)]
    [Tooltip("The magnitude of how much the hitter gets slowed down after a big impact, where 1 equals the speed difference.")]
    private float _slowDownOnImpactMagnitude = 0.5f;
    public float SlowDownOnImpactMagnitude
    {
        get { return _slowDownOnImpactMagnitude; }
    }

    [BoxGroup(RAMMING_BOX_TITLE)]
    [Header("Big v Big")]
    [SerializeField] [MinValue(0)]
    [Tooltip("The magnitude of how much both ships gets pushed back, where 1 equals the speed difference.")]
    private float _bigPushMagnitude = 0.6f;
    public float BigPushMagnitude
    {
        get { return _bigPushMagnitude; }
    }

    [BoxGroup(RAMMING_BOX_TITLE)]
    [Header("Damage")]
    [SerializeField] [MinValue(0)]
    [Tooltip("The damage the rammed ship will get on a full hit")]
    private float _damageFullHit = 10f;
    public float DamageFullHit
    {
        get { return _damageFullHit; }
    }

    [BoxGroup(RAMMING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The velocity a ship needs to have for a full hit ram")]
    private float _velocityDifferenceForFullHit = 60f;
    public float VelocityDifferenceForFullHit
    {
        get { return _velocityDifferenceForFullHit; }
    }

    [BoxGroup(RAMMING_BOX_TITLE)]
    [SerializeField]
    [Tooltip("Damage type for the ram")]
    private DamageType _ramDamageType;
    public DamageType RamDamageType
    {
        get { return _ramDamageType; }
    }

    [BoxGroup(RAMMING_BOX_TITLE)]
    [SerializeField]
    [Tooltip("The effect that plays when a ram takes place")]
    private GameObject _ramEffect;
    public GameObject RamEffect
    {
        get { return _ramEffect; }
    }
    #endregion

    #region Steering Sway Settings
    [BoxGroup(SWAY_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The maximum rotation the ship can keel over on its roll (Z) axis by turning at max sail & rotational velocities.")]
    private float _maxRoll = 10f;
    /// <summary>
    /// The maximum rotation the ship can keel over on its roll (Z) axis by turning at max sail & rotational velocities.
    /// </summary>
    public float MaxRoll
    {
        get { return _maxRoll; }
        private set { _maxRoll = ExFuncs.ClampPositive(value); }
    }

    [BoxGroup(SWAY_BOX_TITLE)]
    [SerializeField] [MinMaxSlider(0, 1)]
    [Tooltip("Determines the range over which the sailing velocity affects the sway angle.")]
    private Vector2 _sailSwayRange = new Vector2(0, 1);
    /// <summary>
    /// Determines the range over which the sailing velocity affects the sway angle.
    /// </summary>
    public Vector2 SailSwayRange
    {
        get { return _sailSwayRange; }
        private set
        {
            float lowerValue = Mathf.Min(value.x, value.y);
            float higherValue = Mathf.Max(value.x, value.y);
            _sailSwayRange = new Vector2(Mathf.Clamp01(lowerValue), Mathf.Clamp01(higherValue));
        }
    }

    [BoxGroup(SWAY_BOX_TITLE)]
    [SerializeField] [MinMaxSlider(0, 1)]
    [Tooltip("Determines the range over which the rotational velocity affects the sway angle.")]
    private Vector2 _turnSwayRange = new Vector2(0, 1);
    /// <summary>
    /// Determines the range over which the rotational velocity affects the sway angle.
    /// </summary>
    public Vector2 TurnSwayRange
    {
        get { return _turnSwayRange; }
        private set
        {
            float lowerValue = Mathf.Min(value.x, value.y);
            float higherValue = Mathf.Max(value.x, value.y);
            _turnSwayRange = new Vector2(Mathf.Clamp01(lowerValue), Mathf.Clamp01(higherValue));
        }
    }

    /// <summary>
    /// The roll (Z) rotation for the ship model when it is in neutral position.
    /// Steering sway uses this value as its centerpoint.
    /// </summary>
    public float NeutralRoll
    {
        get;
        private set;
    }

    /// <summary>
    /// Maps sail velocity portion of max to user set range.
    /// </summary>
    private float SailSwayScalar
    {
        get { return ExFuncs.DistanceAcrossRange(SailSwayRange, SailVelocityPortionOfMax); }
    }

    /// <summary>
    /// Maps rotational velocity portion of max to user set range.
    /// </summary>
    private float TurnSwayScalar
    {
        get { return Mathf.Sign(-RotationalVelocity) * ExFuncs.DistanceAcrossRange(TurnSwayRange, Mathf.Abs(RotationalVelocityPortionOfMax)); }
    }

    /// <summary>
    /// The sway cap in degrees for the ship while turning.
    /// The faster the ship goes, the more it can sway, up to MaxRoll degrees, in both directions.
    /// </summary>
    public float SwayCap
    {
        get { return MaxRoll * SailSwayScalar; }
    }

    /// <summary>
    /// Returns the sway angle as derived from max angle, current sailing velocity and current rotational velocity.
    /// Takes neutral angle into account.
    /// </summary>
    private float DesiredSwayAngle
    {
        get { return NeutralRoll + SwayCap * TurnSwayScalar; }
    }
    #endregion

    #region On Collision Settings

    [BoxGroup(ON_COLLISION_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The amount of deceleration applied on the ship on collision")]
    private float _suddenStopDeceleration = 30f;
    /// <summary>
    /// The maximum sailing velocity of the ship.
    /// </summary>
    public float SuddenStopDeceleration
    {
        get { return _suddenStopDeceleration; }
    }
    #endregion

    #region AI Helper Functions
    /// <summary>
    /// Returns the rotational velocity applied if no acceleration is added to the current rotational velocity.
    /// Takes into account the current velocity and the rotational friction slowdown time.
    /// </summary>
    public float RemainingRotationalVelocityApplied
    {
        get
        {
            int rotationSlowDownTimeRoundedUp = (int)Mathf.Ceil(TurningSlowdownTime);

            float totalRemaingRotationalVelocity = 0;

            for (int i = 0; i < rotationSlowDownTimeRoundedUp; i++)
            {
                if (TurningSlowdownTime - i < 1) // Less then one full second remaining
                {
                    float relativeTurningFriction = 1 / TurningSlowdownTime * (TurningSlowdownTime - i % 1);
                    float relativeDecrease = relativeTurningFriction * RotationalVelocity;
                    totalRemaingRotationalVelocity += RotationalVelocity - relativeDecrease;

                    break;
                }

                float frictionPercentage = 1 / TurningSlowdownTime;
                float decrease = (frictionPercentage * i) * RotationalVelocity;
                totalRemaingRotationalVelocity += RotationalVelocity - decrease;
            }

            return totalRemaingRotationalVelocity;
        }
    }
    #endregion

    #region Steering & Sailing Functions
    /// <summary>
    /// Sets the rotational velocity of the ship to turn clockwise.
    /// </summary>
    /// <param name="portionOfMax">Value between 0 and 1, determining how much of max rotational acceleration should be used to steer.</param>   
    public void SteerClockwise(float portionOfMax = 1)
    {
        if (!CanReceiveInput)
            return;

        // Adjust rotational velocity.
        portionOfMax = Mathf.Clamp01(portionOfMax);
        RotationalVelocity += RotationalAcceleration * Time.deltaTime * portionOfMax;
        IsBeingSteeredClockwise = true;
    }

    /// <summary>
    /// Sets the rotational velocity of the ship to turn counter-clockwise.
    /// </summary>
    /// <param name="portionOfMax">Value between 0 and 1, determining how much of max rotational acceleration should be used to steer.</param>   
    public void SteerCounterClockwise(float portionOfMax = 1)
    {
        if (!CanReceiveInput)
            return;

        // Adjust rotational velocity.
        portionOfMax = Mathf.Clamp01(portionOfMax);
        RotationalVelocity -= RotationalAcceleration * Time.deltaTime * portionOfMax;
        IsBeingSteeredCounterClockwise = true;
    }

    /// <summary>
    /// Applies the sailing acceleration to the sailing velocity.
    /// </summary>
    public void SpeedUp()
    {
        if (!CanReceiveInput)
            return;

        SailVelocity += SailAcceleration * Time.deltaTime;
        IsBeingSpedUp = true;
    }

    /// <summary>
    /// Applies friction to the sailing velocity to stop the ship.
    /// </summary>
    public void SlowDown()
    {
        if (!CanReceiveInput)
            return;

        SailVelocity = ApplyFriction(SailVelocity, MaxSailVelocity, BrakeSlowdownTime, Time.deltaTime);
    }

    private void AddRammingForce(Vector3 force)
    {
        Vector3 current = _rammingDirection * RammingVelocity;
        current += force;

        _rammingDirection = current.normalized;
        RammingVelocity = current.magnitude;
    }

    private void UpdateRudderRotation()
    {
        if (ShipStats.Rudder != null)
        {
            Vector3 rudderRotation = ShipStats.Rudder.transform.localEulerAngles;
            // Rudder turns in opposite direction of steering rotation.
            rudderRotation.y = -RotationalVelocityPortionOfMax * MaxRudderRotation;
            ShipStats.Rudder.transform.localEulerAngles = rudderRotation;
        }
    }

    private void UpdateSteeringWheelRotation()
    {
        if (ShipStats.SteeringWheel != null)
        {
            Vector3 wheelRotation = ShipStats.SteeringWheel.transform.localEulerAngles;
            // Steering wheel turns in opposite direction of steering rotation.
            wheelRotation.z = -RotationalVelocityPortionOfMax * MaxSteeringWheelRotation;
            ShipStats.SteeringWheel.transform.localEulerAngles = wheelRotation;
        }
    }
    #endregion

    #region Specific Physics Events
    //________________________________________ QuickStop ________________________________________//
    private Coroutine _quickStopRoutine = null;
    /// <summary>
    /// Makes the ship come to a stop over time.
    /// </summary>
    /// <param name="stopTime">Time in seconds it should take to slow down from max speed.</param>
    public void QuickStop(float stopTime)
    {
        if (_quickStopRoutine == null)
        {
            _quickStopRoutine = StartCoroutine(QuickStopRoutine(stopTime));
        }
    }

    /// <summary>
    /// Adjusts the ship's friction so it gradually comes to a halt.
    /// </summary>
    /// <param name="maxStopTime">The time it takes the ship to come to a halt from max velocity.</param>
    private IEnumerator QuickStopRoutine(float maxStopTime)
    {
        // Cache default friction to reset to at the end.
        float previousFriction = SailingFriction;
        float sailVelocityPortion = SailVelocity / MaxSailVelocity;

        // Set friction and wait until fully stopped.
        SailingFriction = MaxSailVelocity / maxStopTime;
        yield return new WaitForSeconds(maxStopTime * sailVelocityPortion);

        // Nullify remaining velocity to make sure we've fully stopped.
        RigidBodyVelocity = Vector3.zero;

        // Reset to default friction.
        SailingFriction = previousFriction;
        _quickStopRoutine = null;
        yield break;
    }

    //________________________________________ SetSailVelocityPortionOfMax ________________________________________//
    /// <summary>
    /// Sets the sail velocity as a given portion of max sail velocity.
    /// </summary>
    /// <param name="portionOfMax">Value between 0 and 1.</param>
    public void SetSailVelocityPortionOfMax(float portionOfMax)
    {
        // Sail velocity is automatically clamped between 0 and max.
        SailVelocity = MaxSailVelocity * portionOfMax;
    }

    //________________________________________ StopMoving ________________________________________//
    /// <summary>
    /// Stops the ship from moving instantly.
    /// </summary>
    public void StopMoving()
    {
        // Set velocities to zero.
        SailVelocity = 0;
        RotationalVelocity = 0;
        RammingVelocity = 0;
        ApplySway();

        // Nullify remaining velocity.
        RigidBodyVelocity = Vector3.zero;
    }

    //________________________________________ SuddenSlowDown ________________________________________//
    /// <summary>
    /// Slow down the ship with a certain force
    /// </summary>
    public void SuddenSlowDown()
    {
        SailVelocity -= _suddenStopDeceleration;
        RammingVelocity -= SuddenStopDeceleration;
    }

    //________________________________________ Dash ________________________________________//
    /// <summary>
    /// Rapidly move forward with a certain force
    /// </summary>
    public void Dash()
    {
        if (!enableDash || isDashing || _dashCooldownTimer < DashCooldownDuration)
        {
            return;
        }

        if (onShipDash != null)
            onShipDash.Raise();

        GameObject effect = Instantiate(dashEffect, Vector3.zero, Quaternion.identity, transform);
        effect.transform.localRotation = Quaternion.identity;
        effect.transform.localPosition = new Vector3(0, 3, 38); // transform.up * 3 + transform.forward * 38; // In front of the ship

        _dashCooldownTimer = 0;
        StartCoroutine(StartDashCoolDown());
        isDashing = true;
        SailVelocity += _dashAcceleration;
        StartCoroutine(DisableDashingAfterSeconds());
    }

    private IEnumerator StartDashCoolDown()
    {
        while (_dashCooldownTimer < DashCooldownDuration)
        {
            _dashCooldownTimer += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Handles the quick slow down after a dash
    /// </summary>
    /// <returns></returns>
    private IEnumerator DisableDashingAfterSeconds()
    {
        yield return new WaitForSeconds(_maxDashDuration);

        float timer = 0;
        float progress = 0;

        float topVelocity = SailVelocity;

        if (topVelocity > MaxSailVelocity)
        {
            while (timer < DashSlowDownDuration)
            {
                float externalProgressOnVelocity = Mathf.InverseLerp(topVelocity, MaxSailVelocity, SailVelocity);
                if (externalProgressOnVelocity - progress > 0.01f)
                {
                    timer = externalProgressOnVelocity * DashSlowDownDuration;
                }

                SailVelocity = Mathf.Lerp(topVelocity, MaxSailVelocity, timer / DashSlowDownDuration);
                progress = timer / DashSlowDownDuration;
                timer += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            isDashing = false;
        }

        isDashing = false;
        SailVelocity = _sailVelocity;
    }

    //________________________________________ Ramming ________________________________________//
    public void DetermineRammingSeverity(Collision collision)
    {
        if (!ShouldProcessRamming)
        {
            ShouldProcessRamming = true;
            return;
        }
        ShipPhysics otherShipPhysics = collision.gameObject.GetComponent<ShipPhysics>();
        otherShipPhysics.ShouldProcessRamming = false;
        Vector3 rammedVelocity = otherShipPhysics.RigidBodyVelocity;

        Vector3 velocity = RigidBodyVelocity;

        float dot = Vector3.Dot(transform.forward, (collision.contacts[0].point - transform.position).normalized);

        float collisionAngle = Mathf.Abs(Vector3.SignedAngle(transform.forward, collision.gameObject.transform.forward, Vector3.up));

        ShipPhysics hitter = dot < 0.7f ? otherShipPhysics : this;
        ShipPhysics target = dot < 0.7f ? this : otherShipPhysics;

        float speedDifferenceSqr = (velocity - rammedVelocity).sqrMagnitude;

        // Check wether the angle difference is big enough OR if the speed difference is big enough
        if (collisionAngle >= MinRamImpactAngle && collisionAngle <= 180 - MinRamImpactAngle)
        {
            //PrintRam(gameObject.name, "Big V Target", hitter.name, target.name, collisionAngle, dot, speedDifferenceSqr);
            DoCameraShakeIfPlayer(collision.gameObject);

            target.PushShipInDirection(hitter.RigidBodyVelocity, hitter.RigidBodyVelocity.sqrMagnitude, _pushMagnitude);
            hitter.SlowDownAfterImpact(speedDifferenceSqr);

            SpawnEffect(RamEffect, collision.contacts[0].point,  Quaternion.Inverse(hitter.transform.rotation), target.RammingVelocity * target.RammingDirection);
        }
        else if (speedDifferenceSqr >= _minRamSpeedDifference * _minRamSpeedDifference)
        {
            //PrintRam(gameObject.name, "Big V Big", hitter.name, target.name, collisionAngle, dot, speedDifferenceSqr);
            DoCameraShakeIfPlayer(collision.gameObject);
            StopMoving();
            PushShipInDirection(rammedVelocity, rammedVelocity.sqrMagnitude, _bigPushMagnitude);
            otherShipPhysics.StopMoving();
            otherShipPhysics.PushShipInDirection(velocity, velocity.sqrMagnitude, _bigPushMagnitude);

            SpawnEffect(RamEffect, collision.contacts[0].point,  Quaternion.identity, Vector3.zero);
        }
        else
        {
            //PrintRam(gameObject.name, "Small", hitter.name, target.name, collisionAngle, dot, speedDifferenceSqr);
            hitter.SlowDownAfterImpact(speedDifferenceSqr);
        }
    }

    public void HandleRamWithSolidObject()
    {
        DoCameraShakeIfPlayer(gameObject);
        Vector3 velocity = RigidBodyVelocity;
        StopMoving();
        PushShipInDirection(-velocity, velocity.sqrMagnitude, _bigPushMagnitude);
    }

    private void PrintRam(string handler, string ram, string hitter, string target, float angle, float dot, float speedDifferenceSqr)
    {
        Debug.Log(handler + "// " + ram + " where hitter = " + hitter + ", and target is: " + target + ". Angle: " + angle + ", speedDiff: " + Mathf.Sqrt(speedDifferenceSqr));
    }

    /// <summary>
    /// Pushes the ship in a certain direction with a relevant force to the collision
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speedDifferenceSqr"></param>
    public void PushShipInDirection(Vector3 direction, float speedDifferenceSqr, float magnitude)
    {
        if (speedDifferenceSqr <= 0)
        {
            return;
        }

        float speedDiff = Mathf.Sqrt(speedDifferenceSqr);

        AddRammingForce(direction.normalized * speedDiff * magnitude);

        // Do Damage
        Damage damage = new Damage((int)(Mathf.Clamp01(speedDiff / VelocityDifferenceForFullHit) * DamageFullHit), RamDamageType);
        GetComponent<Health>().Damage(damage);
    }
    
    /// <summary>
    /// Decelerate the ship with a certain force
    /// </summary>
    /// <param name="speedDifferenceSqr"></param>
    public void SlowDownAfterImpact(float speedDifferenceSqr)
    {
        if (speedDifferenceSqr <= 0)
        {
            return;
        }

        SailVelocity -= Mathf.Sqrt(speedDifferenceSqr) * _slowDownOnImpactMagnitude;
    }

    /// <summary>
    /// Helper function to shake the camera if the affected ship is the player
    /// </summary>
    /// <param name="go"></param>
    private void DoCameraShakeIfPlayer(GameObject go)
    {
        if (gameObject.CompareTag(Tags.PLAYER) || go.CompareTag(Tags.PLAYER))
        {
            SimpleCameraShakeInCinemachine.instance.DoLightCameraShake();
        }
    }

    private void SpawnEffect(GameObject effect, Vector3 position, Quaternion rotation, Vector3 force)
    {
        GameObject particle = Instantiate(effect, position, rotation);
        Rigidbody rb = particle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity += force;
        }
    }
    #endregion

    #region Physics Calculations
    /// <summary>
    /// Applies friction to the given variable.
    /// Friction is value per second.
    /// </summary>
    /// <param name="value">The value  to apply friction to.</param>
    /// <param name="frictionAmountPerSecond">The amount of friction per second.</param>
    /// <param name="deltaTime">The timestep to use in calculating the magnitude of the friction.</param>
    private float ApplyFriction(float value, float frictionAmountPerSecond, float deltaTime)
    {
        // Calculate friction amount.
        float frictionAmount = frictionAmountPerSecond * deltaTime;
        // Take the smallest of calculated friction amount and remainder of value.
        frictionAmount = Mathf.Min(frictionAmount, Mathf.Abs(value));
        // Take sign of value into account so friction is applied towards 0.
        float targetValue = value - frictionAmount * Mathf.Sign(value);

        return targetValue;
    }

    /// <summary>
    /// Applies friction to the given variable.
    /// Friction is total time to slow down from max.
    /// </summary>
    /// <param name="value">The value to apply friction to.</param>
    /// <param name="maxValue">The maximum value of the variable that friction is being applied to.</param>
    /// <param name="degradationTimeFromMax">The time it takes for the value to reach 0 from max in seconds through friction.</param>
    /// <param name="deltaTime">The timestep to use in calculating the magnitude of the friction.</param>
    private float ApplyFriction(float value, float maxValue, float degradationTimeFromMax, float deltaTime)
    {
        // Calculate current and degradation values as portion of max value.
        float currentPortionOfMax = Mathf.Abs(value) / maxValue;
        float portionOfMaxToDegrade = 1 / degradationTimeFromMax * deltaTime;

        // Subtract degradation amount from current amount and clamp.
        float targetPortionOfMax = currentPortionOfMax - portionOfMaxToDegrade;
        targetPortionOfMax = Mathf.Clamp01(targetPortionOfMax);

        // Set target value as portion of max value.
        float targetValue = maxValue * targetPortionOfMax * Mathf.Sign(value);

        return targetValue;
    }

    /// <summary>
    /// Applies the current sailing velocity to the rigidbody.
    /// </summary>
    private void ApplySailVelocity()
    {
        RigidBodyVelocity = transform.forward * SailVelocity + _rammingDirection * RammingVelocity;
    }

    /// <summary>
    /// Applies the current rotational velocity to the transform.
    /// Only call from FixedUpdate().
    /// </summary>
    private void ApplyRotationalVelocity()
    {
        float deltaAngle = RotationalVelocity * Time.fixedDeltaTime;
        // Yaw axis (Y) determines steering direction.
        transform.Rotate(0, deltaAngle, 0, Space.World);

        UpdateRudderRotation();
        UpdateSteeringWheelRotation();
    }

    /// <summary>
    /// Applies sway to the ship depending on how fast it is moving and turning.
    /// Only call from FixedUpdate().
    /// </summary>
    private void ApplySway()
    {
        float deltaAngle = Mathf.DeltaAngle(transform.eulerAngles.z, DesiredSwayAngle);
        // Roll axis (Z) determines sway.
        transform.Rotate(0, 0, deltaAngle, Space.Self);
    }
    #endregion

    /// <summary>
    /// Initialize variables.
    /// </summary>
    private void Start()
    {
        _dashCooldownTimer = DashCooldownDuration;
        isDashing = false;
        _shouldProcessRamming = true;
        CanReceiveInput = true;
        NeutralRoll = transform.eulerAngles.z;
    }

    /// <summary>
    /// Reset some variables.
    /// </summary>
    private void LateUpdate()
    {
        if (!_isBeingSpedUpThisFrame)
        {
            IsBeingSpedUp = false;
        }
        if (!_isBeingSteeredClockwiseThisFrame)
        {
            IsBeingSteeredClockwise = false;
        }
        if (!_isBeingSteeredCounterClockwiseThisFrame)
        {
            IsBeingSteeredCounterClockwise = false;
        }

        _isBeingSpedUpThisFrame = false;
        _isBeingSteeredClockwiseThisFrame = false;
        _isBeingSteeredCounterClockwiseThisFrame = false;
    }

    /// <summary>
    /// Handle physics calculations.
    /// </summary>
    private void FixedUpdate()
    {
        // Apply friction.
        if (!IsBeingSpedUp)
        {
            SailVelocity = ApplyFriction(SailVelocity, SailingFriction, Time.fixedDeltaTime);
        }
        if (!IsBeingSteered)
        {
            RotationalVelocity = ApplyFriction(RotationalVelocity, TurningFriction, Time.fixedDeltaTime);
        }
        if (RammingVelocity > 0)
        {
            RammingVelocity = ApplyFriction(RammingVelocity, RammingFriction, Time.fixedDeltaTime);
        }

        // Apply calculated velocities to ship rigidbody.
        ApplySailVelocity();
        ApplyRotationalVelocity();
        ApplySway();
    }

    /// <summary>
    /// Perform logic after inspector update.
    /// </summary>
    private void OnValidate()
    {
        // Update values.
        UpdateSailingSlowdownTime();
        UpdateTurningSlowdownTime();
    }
}
