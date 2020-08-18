// (c) Gijs Sickenga, 2018 //

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Delegates function execution while in editor.
/// </summary>
public static class FunctionDelegator
{
    public delegate void DelayedFunction();

    /// <summary>
    /// Waits for inspectors to update before executing the given callback method.
    /// Executes immediately if not in edit mode.
    /// </summary>
    public static void ExecuteWhenSafe(DelayedFunction function)
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.delayCall += new UnityEditor.EditorApplication.CallbackFunction(function);
        }
        else
        {
            function();
        }
#else
        function();
#endif
    }
}
