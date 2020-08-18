// (c) Gijs Sickenga, 2018 //

using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif

/// <summary>
/// Class with helper functions that are safe to use in editor and build.
/// </summary>
public static class ExFuncs
{
    #region Math
    //________________________________________ Centroid ________________________________________//
    /// <summary>
    /// Returns the centroid of a list of Vector2 points.
    /// The centroid is the point precisely in the middle of all other points.
    /// </summary>
    public static Vector2 Centroid(IList<Vector2> points)
    {
        Vector2 summedPoints = Vector2.zero;
        if (points == null || points.Count == 0)
        {
            return summedPoints;
        }

        foreach (Vector2 point in points)
        {
            summedPoints += point;
        }

        // Centroid is sum of all points, divided by amount of points.
        return summedPoints / points.Count;
    }
    /// <summary>
    /// Returns the centroid of a list of Vector3 points.
    /// The centroid is the point precisely in the middle of all other points.
    /// </summary>
    public static Vector3 Centroid(IList<Vector3> points)
    {
        Vector3 summedPoints = Vector3.zero;
        if (points == null || points.Count == 0)
        {
            return summedPoints;
        }

        foreach (Vector3 point in points)
        {
            summedPoints += point;
        }

        // Centroid is sum of all points, divided by amount of points.
        return summedPoints / points.Count;
    }

    //________________________________________ ClampPositive ________________________________________//
    /// <summary>
    /// Clamps a value between 0 and infinity.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    public static int ClampPositive(int value)
    {
        return (int)Mathf.Clamp(value, 0, Mathf.Infinity);
    }
    /// <summary>
    /// Clamps a value between 0 and infinity.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    public static float ClampPositive(float value)
    {
        return Mathf.Clamp(value, 0, Mathf.Infinity);
    }

    //________________________________________ DistanceAcrossRange ________________________________________//
    /// <summary>
    /// Returns a value between 0 and 1, indicating how far the given value is along the given range.
    /// </summary>
    /// <param name="min">Lower bound of the range.</param>
    /// <param name="max">Higher bound of the range.</param>
    /// <param name="value">Value for which to check how far along the range it is.</param>
    public static float DistanceAcrossRange(float min, float max, float value)
    {
        // Account for range of 0 distance.
        if (max - min == 0)
        {
            return value >= max ? 1 : 0;
        }

        return Mathf.Clamp01((1 / (max - min)) * (value - min));
    }
    /// <summary>
    /// Returns a value between 0 and 1, indicating how far the given value is along the given range.
    /// </summary>
    /// <param name="range">Range to check value against. X should be lower bound, Y higher bound.</param>
    /// <param name="value">Value for which to check how far along the range it is.</param>
    public static float DistanceAcrossRange(Vector2 range, float value)
    {
        return DistanceAcrossRange(range.x, range.y, value);
    }

    //________________________________________ GreaterMagnitude ________________________________________//
    /// <summary>
    /// Returns whatever value is further from zero.
    /// </summary>
    public static int GreaterMagnitude(int a, int b)
    {
        bool firstIsGreater = Mathf.Abs(a) > Mathf.Abs(b);
        return firstIsGreater ? a : b;
    }
    /// <summary>
    /// Returns whatever value is further from zero.
    /// </summary>
    public static float GreaterMagnitude(float a, float b)
    {
        bool firstIsGreater = Mathf.Abs(a) > Mathf.Abs(b);
        return firstIsGreater ? a : b;
    }

    //________________________________________ LargestDistance ________________________________________//
    /// <summary>
    /// Returns the largest distance between any 2 points in the provided list.
    /// </summary>
    public static float LargestDistance(IList<Vector2> points)
    {
        // Only query unique points.
        Vector2[] uniquePoints = points.Distinct().ToArray();

        float largestDistance = 0;
        float distance = 0;
        // Never query i == 0, because every combination will have already been done.
        for (int i = uniquePoints.Length - 1; i > 0; i--)
        {
            // Only query j < i to prevent double checks and same point checks.
            for (int j = 0; j < i; j++)
            {
                distance = Vector2.Distance(uniquePoints[i], uniquePoints[j]);
                largestDistance = Mathf.Max(distance, largestDistance);
            }
        }
        return largestDistance;
    }
    /// <summary>
    /// Returns the largest distance between any 2 points in the provided list.
    /// </summary>
    public static float LargestDistance(IList<Vector3> points)
    {
        // Only query unique points.
        Vector3[] uniquePoints = points.Distinct().ToArray();

        float largestDistance = 0;
        float distance = 0;
        // Never query i == 0, because every combination will have already been done.
        for (int i = uniquePoints.Length - 1; i > 0; i--)
        {
            // Only query j < i to prevent double checks and same point checks.
            for (int j = 0; j < i; j++)
            {
                distance = Vector3.Distance(uniquePoints[i], uniquePoints[j]);
                largestDistance = Mathf.Max(distance, largestDistance);
            }
        }
        return largestDistance;
    }

    //________________________________________ RoundToDecimal ________________________________________//
    /// <summary>
    /// Rounds the given value to the specified number of decimals.
    /// </summary>
    /// <param name="value">Value to round.</param>
    /// <param name="decimals">Amount of decimals to round to.</param>
    public static float RoundToDecimal(float value, int decimals)
    {
        return (float)System.Math.Round(value, decimals);
    }
    #endregion

    #region Prefabs
    //________________________________________ IsPrefab ________________________________________//
    /// <summary>
    /// Returns whether the given Object is a prefab (or part of one).
    /// Always returns false in build.
    /// </summary>
    /// <param name="objectToCheck">The Object for which to check whether it is a prefab.</param>
    /// <returns>True if the Object is a prefab and false if it isn't.</returns>
    public static bool IsPrefab(Object objectToCheck)
    {
#if UNITY_EDITOR
        return PrefabUtility.GetPrefabType(objectToCheck) == PrefabType.Prefab;
#else
        return false;
#endif
    }
    #endregion

    #region UnityEvents
    //________________________________________ AddPersistentListener ________________________________________//
    /// <summary>
    /// Adds the specified listener to the specified event as a persistent listener.
    /// When in build the listener is added as a non-persistent listener instead.
    /// </summary>
    /// <param name="unityEvent">The event to add a listener to.</param>
    /// <param name="listener">The listener to add to the event.</param>
    /// <param name="addOnlyOnce">Whether the given listener should only be added to the event if it wasn't already.</param>
    public static void AddPersistentListener(UnityEvent unityEvent, UnityAction listener, bool addOnlyOnce = false)
    {
#if UNITY_EDITOR
        if (addOnlyOnce)
        {
            if (EventContainsPersistentListener(unityEvent, listener))
            {
                return;
            }
        }
        UnityEventTools.AddPersistentListener(unityEvent, listener);
#else
        if (addOnlyOnce)
        {
            if (EventContainsPersistentListener(unityEvent, listener))
            {
                return;
            }
            unityEvent.RemoveListener(listener);
        }
        unityEvent.AddListener(listener);
#endif
    }
    public static void AddPersistentListener<T0>(UnityEvent<T0> unityEvent, UnityAction<T0> listener, bool addOnlyOnce = false)
    {
#if UNITY_EDITOR
        if (addOnlyOnce)
        {
            if (EventContainsPersistentListener(unityEvent, listener))
            {
                return;
            }
        }
        UnityEventTools.AddPersistentListener(unityEvent, listener);
#else
        if (addOnlyOnce)
        {
            if (EventContainsPersistentListener(unityEvent, listener))
            {
                return;
            }
            unityEvent.RemoveListener(listener);
        }
        unityEvent.AddListener(listener);
#endif
    }
    public static void AddPersistentListener<T0, T1>(UnityEvent<T0, T1> unityEvent, UnityAction<T0, T1> listener, bool addOnlyOnce = false)
    {
#if UNITY_EDITOR
        if (addOnlyOnce)
        {
            if (EventContainsPersistentListener(unityEvent, listener))
            {
                if (EventContainsPersistentListener(unityEvent, listener))
                {
                    return;
                }
                return;
            }
        }
        UnityEventTools.AddPersistentListener(unityEvent, listener);
#else
        if (addOnlyOnce)
        {
            unityEvent.RemoveListener(listener);
        }
        unityEvent.AddListener(listener);
#endif
    }
    public static void AddPersistentListener<T0, T1, T2>(UnityEvent<T0, T1, T2> unityEvent, UnityAction<T0, T1, T2> listener, bool addOnlyOnce = false)
    {
#if UNITY_EDITOR
        if (addOnlyOnce)
        {
            if (EventContainsPersistentListener(unityEvent, listener))
            {
                if (EventContainsPersistentListener(unityEvent, listener))
                {
                    return;
                }
                return;
            }
        }
        UnityEventTools.AddPersistentListener(unityEvent, listener);
#else
        if (addOnlyOnce)
        {
            unityEvent.RemoveListener(listener);
        }
        unityEvent.AddListener(listener);
#endif
    }
    public static void AddPersistentListener<T0, T1, T2, T3>(UnityEvent<T0, T1, T2, T3> unityEvent, UnityAction<T0, T1, T2, T3> listener, bool addOnlyOnce = false)
    {
#if UNITY_EDITOR
        if (addOnlyOnce)
        {
            if (EventContainsPersistentListener(unityEvent, listener))
            {
                if (EventContainsPersistentListener(unityEvent, listener))
                {
                    return;
                }
                return;
            }
        }
        UnityEventTools.AddPersistentListener(unityEvent, listener);
#else
        if (addOnlyOnce)
        {
            unityEvent.RemoveListener(listener);
        }
        unityEvent.AddListener(listener);
#endif
    }

    //________________________________________ EventContainsPersistentListener ________________________________________//
    /// <summary>
    /// Returns whether the given event contains the given function as a persistent listener.
    /// </summary>
    /// <param name="unityEvent">Event for which to check if it has the specified persistent listener.</param>
    /// <param name="listener">Listener to search for in the event.</param>
    public static bool EventContainsPersistentListener(UnityEvent unityEvent, UnityAction listener)
    {
        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            if (unityEvent.GetPersistentMethodName(i).Equals(listener.Method.Name) && unityEvent.GetPersistentTarget(i).Equals(listener.Target))
            {
                return true;
            }
        }
        return false;
    }
    public static bool EventContainsPersistentListener<T0>(UnityEvent<T0> unityEvent, UnityAction<T0> listener)
    {
        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            if (unityEvent.GetPersistentMethodName(i).Equals(listener.Method.Name) && unityEvent.GetPersistentTarget(i).Equals(listener.Target))
            {
                return true;
            }
        }
        return false;
    }
    public static bool EventContainsPersistentListener<T0, T1>(UnityEvent<T0, T1> unityEvent, UnityAction<T0, T1> listener)
    {
        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            if (unityEvent.GetPersistentMethodName(i).Equals(listener.Method.Name) && unityEvent.GetPersistentTarget(i).Equals(listener.Target))
            {
                return true;
            }
        }
        return false;
    }
    public static bool EventContainsPersistentListener<T0, T1, T2>(UnityEvent<T0, T1, T2> unityEvent, UnityAction<T0, T1, T2> listener)
    {
        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            if (unityEvent.GetPersistentMethodName(i).Equals(listener.Method.Name) && unityEvent.GetPersistentTarget(i).Equals(listener.Target))
            {
                return true;
            }
        }
        return false;
    }
    public static bool EventContainsPersistentListener<T0, T1, T2, T3>(UnityEvent<T0, T1, T2, T3> unityEvent, UnityAction<T0, T1, T2, T3> listener)
    {
        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            if (unityEvent.GetPersistentMethodName(i).Equals(listener.Method.Name) && unityEvent.GetPersistentTarget(i).Equals(listener.Target))
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}
