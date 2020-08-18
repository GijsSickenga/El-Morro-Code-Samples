// (c) Gijs Sickenga, 2018 //

using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using UnityEditor.Callbacks;

/// <summary>
/// Checks if all the project's tags and layers are accounted for in the Tags and Layers classes.
/// </summary>
public static class TagsAndLayersChecker
{
    private const int DEFAULT_UNITY_TAGS_AMOUNT = 7;

    // Called whenever Unity recompiles scripts.
    [DidReloadScripts]
    private static void VerifyTagsAndLayers()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        // Get number of const fields in Tags class.
        FieldInfo[] constTags = typeof(Tags).GetFields().Where(field => (field.IsLiteral && !field.IsInitOnly)).ToArray();
        int numberOfCustomTags = constTags.Count() - DEFAULT_UNITY_TAGS_AMOUNT;

        if (numberOfCustomTags < tagsProp.arraySize)
        {
            Debug.LogWarning("Tags.cs: " + numberOfCustomTags + ", Tag count: " + tagsProp.arraySize);
            Debug.LogWarning("Tags missing in Tags.cs. Every tag in the project should have a corresponding const variable in Tags.cs.");
        }

        int numberOfCustomLayers = Enum.GetNames(typeof(Layers.Names)).Count();
        int numberOfLayersInTagManager = 0;
        
        for (int i = layersProp.arraySize - 1; i > -1; i--)
        {
            if (!string.IsNullOrEmpty(layersProp.GetArrayElementAtIndex(i).stringValue))
            {
                numberOfLayersInTagManager++;
            }
        }

        if (numberOfCustomLayers < numberOfLayersInTagManager)
        {
            Debug.LogWarning("Layers.Names: " + numberOfCustomLayers + ", Layer count: " + numberOfLayersInTagManager);
            Debug.LogWarning("Layers missing in Layers.cs. Every layer in the project should have a corresponding enum value in Layers.Names.");
        }
    }
}
