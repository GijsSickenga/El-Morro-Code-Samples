using System;
using System.Collections.Generic;
using System.Reflection;
using ElMorro.DialogueSystem;
using UnityEditor;
using UnityEngine;

/// <summary>
/// PropertyDrawer for DialogueParagraph.
/// </summary>
[CustomPropertyDrawer(typeof(DialogueParagraph))]
public class DialogueParagraphDrawer : PropertyDrawer
{
    // Flags for getting all fields in a class.
    private const BindingFlags FLAGS = BindingFlags.Public |
                                       BindingFlags.NonPublic |
                                       BindingFlags.Static |
                                       BindingFlags.Instance |
                                       BindingFlags.DeclaredOnly;

    private const string SKIP_METHOD_FIELD = "_skipMethod";
    private const string GAME_EVENT_HANDLER_FIELD = "_gameEventHandler";
    private const string PLAYER_INPUT_HANDLER_FIELD = "_playerInputHandler";
    private const string TIMER_HANDLER_FIELD = "_timerHandler";

    public override void OnGUI(Rect baseRect, SerializedProperty baseProperty, GUIContent label)
    {
		// Cache copy of default label to prevent the label name from being altered by Unity.
        GUIContent fieldLabel = new GUIContent(label);

        // UI space for the base foldout.
        Rect foldoutRect = new Rect(baseRect.x, baseRect.y, baseRect.width, EditorGUIUtility.singleLineHeight);

        if (baseProperty.isExpanded = EditorGUI.Foldout(foldoutRect, baseProperty.isExpanded, fieldLabel))
        {
            // Indent all child variables.
            EditorGUI.indentLevel++;

            // ** SKIP METHOD ENUM ** //
            Rect skipMethodRect;
            SerializedProperty skipMethodProperty;
            DrawSkipMethod(baseProperty, baseRect, foldoutRect, out skipMethodRect, out skipMethodProperty);

            // ** EVENT HANDLER FOLDOUT ** //
            Rect eventHandlerRect;
            DrawEventHandler(baseProperty, skipMethodProperty, baseRect, skipMethodRect, out eventHandlerRect);

            // ** REMAINING FIELDS ** //
            var childProperties = GetNestedPropertiesExcluding(typeof(DialogueParagraph), baseProperty, typeof(ParagraphEventHandler), typeof(ParagraphEventHandler.Types));
            var childRects = GetRects(childProperties, baseRect, eventHandlerRect.y + eventHandlerRect.height);
            for (int i = 0; i < childProperties.Count; i++)
            {
                EditorGUI.PropertyField(childRects[i], childProperties[i], true);
            }

            // Revert to previous indent level.
            EditorGUI.indentLevel--;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = 0;
        if (property.isExpanded)
        {
            float vSpacing = EditorGUIUtility.standardVerticalSpacing;

            // Foldout height.
            totalHeight += EditorGUIUtility.singleLineHeight;

            // Skip method height.
            var skipMethodProperty = property.FindPropertyRelative(SKIP_METHOD_FIELD);
            totalHeight += vSpacing;
            totalHeight += EditorGUI.GetPropertyHeight(skipMethodProperty);

            // Event handler height.
            var eventHandlerType = (ParagraphEventHandler.Types)property.FindPropertyRelative(SKIP_METHOD_FIELD).enumValueIndex;
            var eventHandlerProperty = GetEventHandlerProperty(eventHandlerType, property);
            totalHeight += vSpacing;
            totalHeight += EditorGUI.GetPropertyHeight(eventHandlerProperty);

            // Remaining fields height.
            var childProperties = GetNestedPropertiesExcluding(typeof(DialogueParagraph), property, typeof(ParagraphEventHandler), typeof(ParagraphEventHandler.Types));
            foreach (var childProperty in childProperties)
            {
                totalHeight += vSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(childProperty);
            }
        }
        else
        {
            // Closed foldout height.
            totalHeight = EditorGUI.GetPropertyHeight(property);
        }

        return totalHeight;
    }

    /// <summary>
    /// Draws the enum dropdown for the dialogue skip method.
    /// </summary>
    private static void DrawSkipMethod(SerializedProperty baseProperty, Rect baseRect, Rect previousRect, out Rect skipMethodRect, out SerializedProperty skipMethodProperty)
    {
        // Grab skip method enum as a SerializedProperty.
        skipMethodProperty = baseProperty.FindPropertyRelative(SKIP_METHOD_FIELD);
        // UI space for the skip method enum.
        skipMethodRect = GetScaledPropertyRect(baseRect.x, previousRect.y + previousRect.height, baseRect.width, skipMethodProperty, true);
        // Draw skip method enum.
        EditorGUI.PropertyField(skipMethodRect, skipMethodProperty);
    }

    /// <summary>
    /// Draws the correct event handler based on the selected skip method.
    /// </summary>
    private static void DrawEventHandler(SerializedProperty baseProperty, SerializedProperty skipMethodProperty, Rect baseRect, Rect previousRect, out Rect eventHandlerRect)
    {
        // Grab the selected ParagraphEventHandler as a SerializedProperty.
        var eventHandlerType = (ParagraphEventHandler.Types)skipMethodProperty.enumValueIndex;
        SerializedProperty eventHandlerProperty = GetEventHandlerProperty(eventHandlerType, baseProperty);
        // UI space for the ParagraphEventHandler.
        eventHandlerRect = GetScaledPropertyRect(baseRect.x, previousRect.y + previousRect.height, baseRect.width, eventHandlerProperty, true);
        // Draw the selected ParagraphEventHandler.
        EditorGUI.PropertyField(eventHandlerRect, eventHandlerProperty, true);
    }

    /// <summary>
    /// Returns the event handler property that corresponds to the given skip method.
    /// </summary>
    private static SerializedProperty GetEventHandlerProperty(ParagraphEventHandler.Types eventHandlerType, SerializedProperty property)
    {
        SerializedProperty eventHandlerProperty = null;

        switch (eventHandlerType)
        {
            case ParagraphEventHandler.Types.GameEvent:
                eventHandlerProperty = property.FindPropertyRelative(GAME_EVENT_HANDLER_FIELD);
                break;

            case ParagraphEventHandler.Types.PlayerInput:
                eventHandlerProperty = property.FindPropertyRelative(PLAYER_INPUT_HANDLER_FIELD);
                break;

            case ParagraphEventHandler.Types.Timer:
                eventHandlerProperty = property.FindPropertyRelative(TIMER_HANDLER_FIELD);
                break;
        }

        return eventHandlerProperty;
    }

    /// <summary>
    /// Returns a rect that is scaled such that the given property fits in it.
    /// </summary>
    private static Rect GetScaledPropertyRect(float x, float y, float width, SerializedProperty property, bool createVerticalSpacing)
    {
        float verticalSpacing = (createVerticalSpacing ? EditorGUIUtility.standardVerticalSpacing : 0);
        return new Rect(x, y + verticalSpacing, width, EditorGUI.GetPropertyHeight(property));
    }

    /// <summary>
    /// Returns an array of rects that are scaled to fit the elements of the passed in properties.
    /// Each array element corresponds to its respective element in the passed in properties list.
    /// </summary>
    /// <param name="properties">The properties to display.</param>
    /// <param name="outerRect">The outer Rect in which the properties will be displayed.</param>
    /// <param name="startingY">The Y position to start the first Rect at.</param>
    private static Rect[] GetRects(IList<SerializedProperty> properties, Rect outerRect, float startingY)
    {
        Rect[] rects = new Rect[properties.Count];

        for (int i = 0; i < properties.Count; i++)
        {
            float y = (i == 0 ? startingY : rects[i - 1].y + rects[i - 1].height);
            rects[i] = GetScaledPropertyRect(outerRect.x, y, outerRect.width, properties[i], true);
        }

        return rects;
    }

    /// <summary>
    /// Returns a list of all nested properties, excluding any properties that are of one of the given types.
    /// </summary>
    /// <param name="parentType">Type of the parent property.</param>
    /// <param name="parentProperty">The property from which to extract nested properties.</param>
    /// <param name="typesToExclude">The types for which to exclude properties from the returned list.</param>
    private static List<SerializedProperty> GetNestedPropertiesExcluding(Type parentType, SerializedProperty parentProperty, params Type[] typesToExclude)
    {
        List<SerializedProperty> childProperties = new List<SerializedProperty>();

        int childDepth = parentProperty.depth + 1;
        // Make a copy of parentProperty for traversal.
        var parentPropertyCopy = parentProperty.Copy();
        foreach (SerializedProperty childProperty in parentPropertyCopy)
        {
            // Only parse direct children.
            if (childProperty.depth == childDepth)
            {
                // Get field info for the current nested property.
                FieldInfo childField = parentType.GetField(childProperty.name, FLAGS);
                if (!IsTypeOrSubclass(childField.FieldType, typesToExclude))
                {
                    // Grab the actual nested property from the untraversed parent property.
                    var relativeChildProperty = parentProperty.FindPropertyRelative(childField.Name);
                    childProperties.Add(relativeChildProperty);
                }
            }
        }

        return childProperties;
    }

    /// <summary>
    /// Checks if targetType is of a type - or subclass of a type - in the typesToCheck.
    /// </summary>
    private static bool IsTypeOrSubclass(Type targetType, params Type[] typesToCheck)
    {
        foreach (Type sourceType in typesToCheck)
        {
            if (targetType.Equals(sourceType) || targetType.BaseType.Equals(sourceType))
            {
                return true;
            }
        }
        return false;
    }
}
