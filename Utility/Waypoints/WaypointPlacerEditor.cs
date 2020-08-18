// (c) Gijs Sickenga, 2018 //

using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for WaypointPlacer.cs.
/// </summary>
[CustomEditor(typeof(WaypointPlacer))]
public class WaypointPlacerEditor : Editor
{
    private GameObject _selectedObject;

    private WaypointPlacer SelectedScript
    {
        get
        {
            return (WaypointPlacer)target;
        }
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Apply Changes To Waypoint"))
        {
            SelectedScript.waypoint.Initialize(_selectedObject.transform);
            Selection.activeObject = SelectedScript.waypoint;
            EditorUtility.SetDirty(SelectedScript.waypoint);

            DestroyImmediate(_selectedObject);
        }
    }
    
    private void OnSceneGUI()
    {
        // Orange.
        Handles.color = new Color(1, 0.3f, 0, 0.45f);
        Handles.DrawSolidDisc(_selectedObject.transform.position, Vector3.up, Waypoint.GIZMO_ARC_RADIUS);
        Handles.color = new Color(0.8f, 0.15f, 0);
        Handles.DrawWireDisc(_selectedObject.transform.position, Vector3.up, Waypoint.GIZMO_ARC_RADIUS);

        Handles.color = Color.blue;

        Vector3 arrowPoint = _selectedObject.transform.position + (_selectedObject.transform.forward * Waypoint.TRIPLE_GIZMO_ARC_RADIUS);
        Vector3 lineBackOffset = _selectedObject.transform.forward * Waypoint.GIZMO_ARC_RADIUS;
        Vector3 lineHorizontalOffset = _selectedObject.transform.right * Waypoint.GIZMO_ARC_RADIUS;
        Vector3 lineVerticalOffset = _selectedObject.transform.up * Waypoint.GIZMO_ARC_RADIUS;

        // Draw arrow to indicate waypoint forward direction.
        Handles.DrawLine(_selectedObject.transform.position, arrowPoint);
        Handles.DrawLine(arrowPoint, arrowPoint - lineBackOffset - lineHorizontalOffset);
        Handles.DrawLine(arrowPoint, arrowPoint - lineBackOffset + lineHorizontalOffset);
        Handles.DrawLine(arrowPoint, arrowPoint - lineBackOffset + lineVerticalOffset);
    }

    private void OnEnable()
    {
        _selectedObject = SelectedScript.gameObject;
    }

    private void OnDisable()
    {
        // Destroy the waypoint placer when it is deselected.
        if (_selectedObject != null)
        {
            DestroyImmediate(_selectedObject);
        }
    }
}
