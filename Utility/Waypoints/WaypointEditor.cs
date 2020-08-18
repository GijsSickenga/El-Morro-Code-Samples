// (c) Gijs Sickenga, 2018 //

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for Waypoint.cs.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(Waypoint))]
public class WaypointEditor : Editor
{
    /// <summary>
    /// Returns the selected instance of the script or ScriptableObject.
    /// Only reliable in single selection.
    /// </summary>
    private Waypoint SelectedScript
    {
        get
        {
            return (Waypoint)target;
        }
    }

    /// <summary>
    /// Returns the name of the selected script or ScriptableObject as it is shown in the editor.
    /// Only reliable in single selection.
    /// </summary>
    private string InstanceName
    {
        get
        {
            return serializedObject.targetObject.name;
        }
    }

    // This is static so that there can only ever be one callback to OnSceneGUI at any given time.
    private static SceneView.OnSceneFunc _onSceneGUIListener = null;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Only show button when a single waypoint is selected.
        if (Selection.objects.Length == 1)
        {
            if (GUILayout.Button("Customize With Waypoint Placer"))
            {
                GameObject spawnPointPlacer = new GameObject(SelectedScript.name + " Placer");
                WaypointPlacer placerScript = spawnPointPlacer.AddComponent<WaypointPlacer>();
                placerScript.waypoint = SelectedScript;
                SelectedScript.WarpTo(placerScript.transform);
                Selection.activeGameObject = spawnPointPlacer;
            }
        }
    }

    protected virtual void OnEnable()
    {
        // Make sure this is a named instance.
        if (string.IsNullOrEmpty(target.name))
            return;

        AddOnSceneGUIListener();
    }

    protected virtual void OnDisable()
    {
        RemoveOnSceneGUIListener();
    }

    protected virtual void OnDestroy()
    {
        RemoveOnSceneGUIListener();
    }

    private void AddOnSceneGUIListener()
    {
        if (_onSceneGUIListener == null)
        {
            _onSceneGUIListener = OnSceneGUI;
            SceneView.onSceneGUIDelegate += _onSceneGUIListener;
        }
    }

    private void RemoveOnSceneGUIListener()
    {
        if (_onSceneGUIListener != null)
        {
            SceneView.onSceneGUIDelegate -= _onSceneGUIListener;
            _onSceneGUIListener = null;
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        ProcessInput();
        DrawHandlesForAllSelectedWaypoints();
    }

    /// <summary>
    /// Returns whether the given key was pressed just now.
    /// Always call from within OnGUI type functions, otherwise Event.current will be null.
    /// </summary>
    private static bool GetKeyDown(KeyCode key)
    {
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                {
                    if (Event.current.keyCode == (key))
                    {
                        return true;
                    }
                    break;
                }
        }

        return false;
    }

    /// <summary>
    /// Processes input for the editor script.
    /// Only call from OnSceneGUI.
    /// </summary>
    private void ProcessInput()
    {
        if (GetKeyDown(KeyCode.F))
        {
            // Make sure that the scene window is in focus.
            if (SceneView.focusedWindow == SceneView.lastActiveSceneView)
            {
                // Cache existing selection.
                var previousSelection = Selection.objects;

                // Create temporary object at center of all selected waypoints.
                GameObject tempSelectionObject = new GameObject();
                List<Vector3> allWaypointPositions = new List<Vector3>();
                foreach (Waypoint w in targets)
                {
                    allWaypointPositions.Add(w.WorldPosition);
                }
                tempSelectionObject.transform.position = ExFuncs.Centroid(allWaypointPositions);

                // Set selection to temporary object and adjust camera to view it.
                Selection.activeGameObject = tempSelectionObject;
                if (targets.Length == 1)
                {
                    // Focus on single selected waypoint.
                    SceneView.lastActiveSceneView.FrameSelected();
                }
                else
                {
                    Vector3 centroidPosition = tempSelectionObject.transform.position;
                    Quaternion sceneViewRotation = SceneView.lastActiveSceneView.rotation;
                    float largestDistanceBetweenWaypoints = ExFuncs.LargestDistance(allWaypointPositions);

                    // Turn scene camera towards centroid of all selected waypoints, and zoom out to fit all waypoints in the view.
                    SceneView.lastActiveSceneView.LookAt(centroidPosition, sceneViewRotation, largestDistanceBetweenWaypoints);
                }

                // Destroy temporary selection object and reset previous selection.
                DestroyImmediate(tempSelectionObject);
                Selection.objects = previousSelection;
            }
        }
    }

    /// <summary>
    /// Draws handles for all currently selected waypoints.
    /// </summary>
    private void DrawHandlesForAllSelectedWaypoints()
    {
        foreach (Waypoint w in targets)
        {
            DrawHandles(w);
        }
    }

    /// <summary>
    /// Draws handles for a single waypoint.
    /// Only call from OnSceneGUI.
    /// </summary>
    private static void DrawHandles(Waypoint waypoint)
    {
        Vector3 waypointPosition = new Vector3(waypoint.Position.x, 0, waypoint.Position.y);
        Vector3 waypointForward = waypoint.Rotation * Vector3.forward;
        Vector3 waypointRight = waypoint.Rotation * Vector3.right;
        Vector3 waypointUp = waypoint.Rotation * Vector3.up;

        // Orange.
        Handles.color = new Color(1, 0.3f, 0, 0.45f);
        Handles.DrawSolidDisc(waypointPosition, Vector3.up, Waypoint.GIZMO_ARC_RADIUS);
        Handles.color = new Color(0.8f, 0.15f, 0);
        Handles.DrawWireDisc(waypointPosition, Vector3.up, Waypoint.GIZMO_ARC_RADIUS);

        Handles.color = Color.blue;

        Vector3 arrowPoint = waypointPosition + (waypointForward * Waypoint.TRIPLE_GIZMO_ARC_RADIUS);
        Vector3 lineBackOffset = waypointForward * Waypoint.GIZMO_ARC_RADIUS;
        Vector3 lineHorizontalOffset = waypointRight * Waypoint.GIZMO_ARC_RADIUS;
        Vector3 lineVerticalOffset = waypointUp * Waypoint.GIZMO_ARC_RADIUS;

        // Draw arrow to indicate waypoint forward direction.
        Handles.DrawLine(waypointPosition, arrowPoint);
        Handles.DrawLine(arrowPoint, arrowPoint - lineBackOffset - lineHorizontalOffset);
        Handles.DrawLine(arrowPoint, arrowPoint - lineBackOffset + lineHorizontalOffset);
        Handles.DrawLine(arrowPoint, arrowPoint - lineBackOffset + lineVerticalOffset);
    }
}
