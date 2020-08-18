// (c) Gijs Sickenga, 2018 //

using UnityEngine;

/// <summary>
/// Represents a location in the world that can be warped to.
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "New Waypoint", menuName = "Waypoint")]
public class Waypoint : ScriptableObject
{
    [System.Serializable]
    public struct VectorXZ
    {
        public VectorXZ(float x, float z)
        {
            this.x = x;
            this.z = z;
        }

        public float x;
        public float z;
    }

    [SerializeField]
    [Tooltip("The x and z values of the waypoint in world space.")]
    private VectorXZ _position;
    public Vector2 Position
    {
        get
        {
            return new Vector2(_position.x, _position.z);
        }
    }

    public Vector3 WorldPosition
    {
        get
        {
            return new Vector3(_position.x, 0, _position.z);
        }
    }

    [SerializeField]
    private Vector3 _rotation;
    public Quaternion Rotation
    {
        get
        {
            return Quaternion.Euler(_rotation);
        }
    }

    public const float GIZMO_ARC_RADIUS = 5;
    public const float TRIPLE_GIZMO_ARC_RADIUS = GIZMO_ARC_RADIUS * 3;

    /// <summary>
    /// Sets the waypoint's position and rotation to that of the given transform.
    /// </summary>
    public void Initialize(Transform newWaypointTransform)
    {
        _position = new VectorXZ(newWaypointTransform.position.x, newWaypointTransform.position.z);
        _rotation = newWaypointTransform.rotation.eulerAngles;
    }

    /// <summary>
    /// Warps the given object to the waypoint.
    /// </summary>
    /// <param name="objectToWarp">The transform of the object to warp to the waypoint.</param>
    public void WarpTo(Transform objectToWarp)
    {
        objectToWarp.position = new Vector3(_position.x, objectToWarp.position.y, _position.z);
        objectToWarp.rotation = Quaternion.Euler(_rotation);
    }

    /// <summary>
    /// Warps the given object to the waypoint.
    /// </summary>
    /// <param name="objectToWarp">The object to warp to the waypoint.</param>
    public void WarpTo(GameObject objectToWarp)
    {
        objectToWarp.transform.position = new Vector3(_position.x, objectToWarp.transform.position.y, _position.z);
        objectToWarp.transform.rotation = Quaternion.Euler(_rotation);
    }
}
