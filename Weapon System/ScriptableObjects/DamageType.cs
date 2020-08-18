// (c) Gijs Sickenga, 2018 //

using UnityEngine;

/// <summary>
/// Represents a form of damage.
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "New Damage Type", menuName = "Weapons/Labels/Damage Type", order = 1)]
public class DamageType : ScriptableObject
{
    [SerializeField]
    [Tooltip("The name of the damage type.")]
    private string _name;
    /// <summary>
    /// The name of the damage type.
    /// </summary>
    public string Name
    {
        get
        {
            return _name;
        }
    }
}
