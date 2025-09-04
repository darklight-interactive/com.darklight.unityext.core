using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    [CustomPropertyDrawer(typeof(CreateAssetAttribute))]
    public class CreateAssetDrawer : PropertyDrawer
    {
        private const float ButtonHeight = 20f;
        private const float Padding = 2f;
        private const float MinButtonWidth = 60f; // Minimum width for the button to be readable

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = EditorGUI.GetPropertyHeight(property, label, true);

            // Only show button if property is null
            if (property.objectReferenceValue == null)
            {
                // Check if we need to place button on next line
                float availableWidth =
                    EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 20f; // Account for margins
                bool buttonOnNextLine = availableWidth < MinButtonWidth;

                if (buttonOnNextLine)
                    return baseHeight + ButtonHeight + Padding;
            }

            return baseHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Only show button if property is null
            if (property.objectReferenceValue == null)
            {
                // Calculate available width for button placement decision
                float availableWidth = position.width - EditorGUIUtility.labelWidth - 20f; // Account for margins
                bool buttonOnNextLine = availableWidth < MinButtonWidth;

                if (buttonOnNextLine)
                {
                    // Draw property and button on separate lines
                    Rect propertyRect = position;
                    propertyRect.height = EditorGUI.GetPropertyHeight(property, label, true);
                    EditorGUI.PropertyField(propertyRect, property, label, true);

                    // Draw the "Create" button on next line
                    Rect buttonRect = position;
                    buttonRect.y = position.y + propertyRect.height + Padding;
                    buttonRect.height = ButtonHeight;

                    if (GUI.Button(buttonRect, $"New {GetTypeName(property)}"))
                    {
                        CreateAndAssignAsset(property);
                    }
                }
                else
                {
                    // Draw property and button on same line
                    Rect propertyRect = position;
                    propertyRect.height = EditorGUI.GetPropertyHeight(property, label, true);
                    propertyRect.width = position.width - MinButtonWidth - Padding;
                    EditorGUI.PropertyField(propertyRect, property, label, true);

                    // Draw the "Create" button on same line
                    Rect buttonRect = position;
                    buttonRect.x = propertyRect.xMax + Padding;
                    buttonRect.y = position.y;
                    buttonRect.width = MinButtonWidth;
                    buttonRect.height = ButtonHeight;

                    if (GUI.Button(buttonRect, "New"))
                    {
                        CreateAndAssignAsset(property);
                    }
                }
            }
            else
            {
                // Property is not null, just draw the property field normally
                EditorGUI.PropertyField(position, property, label, true);
            }

            EditorGUI.EndProperty();
        }

        void CreateAndAssignAsset(SerializedProperty property)
        {
            Type type = fieldInfo.FieldType;

            if (type.IsArray)
                type = type.GetElementType();

            // << SCRIPTABLE OBJECT >>
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                ScriptableObject asset = ScriptableObject.CreateInstance(type);
                SaveAsset(property, asset, type.Name + ".asset");
            }
            // << MATERIAL >>
            else if (type == typeof(Material))
            {
                var attr = attribute as CreateAssetAttribute;
                string shaderName = attr?.defaultShader ?? "Standard";
                Shader shader = Shader.Find(shaderName);
                if (shader == null)
                    shader = Shader.Find("Standard");
                Material mat = new Material(shader);
                SaveAsset(property, mat, "NewMaterial.mat");
            }
            else
            {
                Debug.LogError(
                    $"CreateAsset does not support creating assets of type {type.Name} automatically."
                );
            }
        }

        void SaveAsset(SerializedProperty property, UnityEngine.Object asset, string defaultName)
        {
            var attr = attribute as CreateAssetAttribute;
            string defaultPath = attr?.defaultPath ?? "Assets";

            if (!defaultPath.StartsWith("Assets/"))
            {
                if (defaultPath.StartsWith("Resources/"))
                    defaultPath = defaultPath.Replace("Resources/", "Assets/Resources/");
                else
                    defaultPath = "Assets/Resources/" + defaultPath.TrimStart('/');
            }

            List<string> createdFolders = EnsureFolderExists(defaultPath);

            string path = EditorUtility.SaveFilePanelInProject(
                "Save New Asset",
                defaultName,
                "asset",
                "Specify where to save the new asset.",
                defaultPath
            );

            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    property.objectReferenceValue = asset;
                    property.serializedObject.ApplyModifiedProperties();

                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = asset;

                    EditorUtility.SetDirty(asset);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create asset: {e.Message}");
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }
            else
            {
                // User canceled — clean up empty folders we just created
                foreach (var folder in createdFolders)
                {
                    if (IsFolderEmpty(folder))
                    {
                        AssetDatabase.DeleteAsset(folder);
                    }
                }
            }

            GUIUtility.ExitGUI();
        }

        /// <summary>
        /// Gets the display name for the type of the property field
        /// </summary>
        /// <param name="property">The serialized property to get the type name for</param>
        /// <returns>The display name of the type</returns>
        string GetTypeName(SerializedProperty property)
        {
            Type type = fieldInfo.FieldType;

            if (type.IsArray)
                type = type.GetElementType();

            // Remove "ScriptableObject" suffix if present for cleaner display
            string typeName = type.Name;
            if (typeName.EndsWith("ScriptableObject"))
            {
                typeName = typeName.Replace("ScriptableObject", "");
            }

            return typeName;
        }

        List<string> EnsureFolderExists(string path)
        {
            List<string> createdFolders = new List<string>();

            string[] parts = path.Split('/');
            string current = "Assets";

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                    createdFolders.Add(next);
                }
                current = next;
            }

            return createdFolders;
        }

        bool IsFolderEmpty(string folderPath)
        {
            string[] assets = AssetDatabase.FindAssets("", new[] { folderPath });
            return assets.Length == 0;
        }
    }
}
