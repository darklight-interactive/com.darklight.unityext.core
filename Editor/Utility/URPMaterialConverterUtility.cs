using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility that adds a context menu option to convert selected materials to URP.
/// </summary>
public static class MaterialConverterUtility
{
    const string AssetMenuPath = "Assets/DarklightTools/Convert Selected Built-in Materials to URP";
    const string ConvertToURPMenuItem = "Edit/Rendering/Materials/Convert Selected Built-in Materials to URP";

    /// <summary>
    /// Validates if the menu item should be enabled based on material selection.
    /// </summary>
    /// <returns>True if there are materials selected, false otherwise.</returns>
    [MenuItem(AssetMenuPath, validate = true)]
    private static bool ValidateConvertToURP()
    {
        return Selection.GetFiltered<Material>(SelectionMode.Assets).Length > 0;
    }

    /// <summary>
    /// Converts the selected materials to URP using Unity's built-in conversion utility.
    /// </summary>
    [MenuItem(AssetMenuPath, priority = 25)]
    private static void ConvertToURP()
    {
        Material[] selectedMaterials = Selection.GetFiltered<Material>(SelectionMode.Assets);
        
        if (selectedMaterials.Length == 0)
        {
            Debug.LogWarning("No materials selected for conversion.");
            return;
        }

        // Open the Render Pipeline Converter window
        EditorApplication.ExecuteMenuItem(ConvertToURPMenuItem);
        
        Debug.Log($"Please use the Render Pipeline Converter window to convert {selectedMaterials.Length} selected materials to URP.");
    }
}