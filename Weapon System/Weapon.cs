// (c) Gijs Sickenga, 2018 //

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Baseclass for all weapons.
/// Contains aiming behaviour.
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The transform used to rotate the weapon clockwise or counter-clockwise.")]
    protected Transform _yawRotationTransform;
    /// <summary>
    /// The transform used to rotate the weapon clockwise or counter-clockwise.
    /// </summary>
    public Transform YawRotationTransform
    {
        get
        {
            return _yawRotationTransform;
        }
    }

    [SerializeField]
    [Tooltip("The minimum and maximum \"left & right\" rotation.")]
    protected Vector2 _yawRotationMinMax;
    /// <summary>
    /// The minimum and maximum "left & right" rotation.
    /// X = left. Y = right.
    /// </summary>
    private Vector2 YawRotationMinMax
    {
        get
        {
            Vector2 minMax = Vector2.zero;
            minMax.x = _yawRotationMinMax.x + _yawTransformRotationDefault.y;
            minMax.y = _yawRotationMinMax.y + _yawTransformRotationDefault.y;
            return minMax;
        }
    }

    [SerializeField]
    [Tooltip("How much the weapon is rotated on its yaw axis in its starting position, relative to neutral aim.")]
    protected float _yawRotationOffset;
    private Vector3 _yawTransformRotationDefault;

    [SerializeField]
    [Tooltip("The transform used to aim the weapon up or down.")]
    protected Transform _pitchRotationTransform;
    /// <summary>
    /// The transform used to aim the weapon up or down.
    /// </summary>
    public Transform PitchRotationTransform
    {
        get
        {
            return _pitchRotationTransform;
        }
    }

    [SerializeField]
    [Tooltip("The minimum and maximum \"up & down\" rotation.")]
    protected Vector2 _pitchRotationMinMax;
    /// <summary>
    /// The minimum and maximum "up & down" rotation.
    /// X = down. Y = up.
    /// </summary>
    private Vector2 PitchRotationMinMax
    {
        get
        {
            Vector2 minMax = Vector2.zero;
            minMax.x = -_pitchRotationMinMax.y + _pitchTransformRotationDefault.x;
            minMax.y = -_pitchRotationMinMax.x + _pitchTransformRotationDefault.x;
            return minMax;
        }
    }

    [SerializeField]
    [Tooltip("How much the weapon is rotated on its pitch axis in its starting position, relative to neutral aim.")]
    protected float _pitchRotationOffset;
    private Vector3 _pitchTransformRotationDefault;

    protected virtual void Start()
    {
        // Cache default yaw & pitch (starting values without starting offset added).
        _yawTransformRotationDefault = YawRotationTransform.localEulerAngles;
        _pitchTransformRotationDefault = PitchRotationTransform.localEulerAngles;

        // Rotate around weapon's relative transform.up.
        YawRotationTransform.localRotation = Quaternion.Euler(YawRotationTransform.localEulerAngles.x,
                                                              _yawTransformRotationDefault.y + _yawRotationOffset,
                                                              YawRotationTransform.localEulerAngles.z);

        // Rotate around weapon's relative transform.right.
        PitchRotationTransform.localRotation = Quaternion.Euler(_pitchTransformRotationDefault.x + _pitchRotationOffset,
                                                                PitchRotationTransform.localEulerAngles.y,
                                                                PitchRotationTransform.localEulerAngles.z);
    }

    /// <summary>
    /// Rotates the weapon clockwise.
    /// </summary>
    /// <param name="rotationalVelocity">How fast the rotation should be.</param>
    public void AimClockwise(float rotationalVelocity, bool overTime = true)
    {
        float degrees = rotationalVelocity * (overTime ? Time.deltaTime : 1);

        // Rotate around weapon's relative transform.up.
        YawRotationTransform.Rotate(Vector3.up, degrees);

        // Clamp rotation in case we overshot.
        ClampYawRotation();
    }

    /// <summary>
    /// Rotates the weapon counter-clockwise.
    /// </summary>
    /// <param name="rotationalVelocity">How fast the rotation should be.</param>
    public void AimCounterClockwise(float rotationalVelocity, bool overTime = true)
    {
        float degrees = rotationalVelocity * (overTime ? Time.deltaTime : 1);

        // Rotate around weapon's relative transform.up.
        YawRotationTransform.Rotate(Vector3.up, -degrees);

        // Clamp rotation in case we overshot.
        ClampYawRotation();
    }

    /// <summary>
    /// Aims the weapon up.
    /// </summary>
    /// /// <param name="rotationalVelocity">How fast the rotation should be.</param>
    public void AimUp(float rotationalVelocity, bool overTime = true)
    {
        float degrees = rotationalVelocity * (overTime ? Time.deltaTime : 1);

        // Rotate around weapon's relative transform.right.
        PitchRotationTransform.Rotate(Vector3.right, -degrees);

        // Clamp rotation in case we overshot.
        ClampPitchRotation();
    }

    /// <summary>
    /// Aims the weapon down.
    /// </summary>
    /// /// <param name="rotationalVelocity">How fast the rotation should be.</param>
    public void AimDown(float rotationalVelocity, bool overTime = true)
    {
        float degrees = rotationalVelocity * (overTime ? Time.deltaTime : 1);

        // Rotate around weapon's relative transform.right.
        PitchRotationTransform.Rotate(Vector3.right, degrees);

        // Clamp rotation in case we overshot.
        ClampPitchRotation();
    }

    protected void ClampYawRotation()
    {
        float newYawRotation = Mathf.DeltaAngle(0, YawRotationTransform.localEulerAngles.y);
        newYawRotation = Mathf.Clamp(newYawRotation, YawRotationMinMax.x, YawRotationMinMax.y);

        YawRotationTransform.localRotation = Quaternion.Euler(_yawTransformRotationDefault.x, newYawRotation, _yawTransformRotationDefault.z);
    }

    protected void ClampPitchRotation()
    {
        float newPitchRotation = Mathf.DeltaAngle(0, PitchRotationTransform.localEulerAngles.x);
        newPitchRotation = Mathf.Clamp(newPitchRotation, PitchRotationMinMax.x, PitchRotationMinMax.y);

        PitchRotationTransform.localRotation = Quaternion.Euler(newPitchRotation, _pitchTransformRotationDefault.y, _pitchTransformRotationDefault.z);
    }
}
