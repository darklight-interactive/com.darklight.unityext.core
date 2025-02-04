using System;
using System.IO;
using Darklight.UnityExt.Editor;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Behaviour.Editor
{
#if UNITY_EDITOR

    [CustomEditor(typeof(ScriptableDataBase), true)]
    public class ScriptableDataCustomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Determine if we're dealing with a class or a struct in T
            Type dataType = target.GetType().BaseType?.GetGenericArguments()[0];
            bool isClassType = dataType?.IsClass ?? false;

            SerializedProperty dataProperty = serializedObject.FindProperty("_data");
            if (dataProperty == null)
            {
                EditorGUILayout.HelpBox(
                    "Could not find the correct data property.",
                    MessageType.Error
                );
                return;
            }

            string dataTypeDisplayName = isClassType ? "Class" : "Struct";
            EditorGUILayout.LabelField(
                $"{dataType.Name} : {dataTypeDisplayName}",
                EditorStyles.boldLabel
            );
            CustomInspectorGUI.DrawAllFieldsInProperty(dataProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(ScriptableDataBase), true)]
    public class ScriptableDataPropertyDrawer : PropertyDrawer
    {
        private const float ButtonHeight = 25f;
        private const float Padding = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = EditorGUI.GetPropertyHeight(property, label, true);
            return baseHeight + ButtonHeight + Padding * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the type of data this ScriptableData holds
            Type scriptableDataType = fieldInfo.FieldType;
            Type dataType = null;

            // Safely get the generic argument type
            if (scriptableDataType != null)
            {
                Type baseType = scriptableDataType;
                while (baseType != null && !baseType.IsGenericType)
                {
                    baseType = baseType.BaseType;
                }

                if (baseType != null && baseType.GetGenericArguments().Length > 0)
                {
                    dataType = baseType.GetGenericArguments()[0];
                }
            }

            if (dataType == null)
            {
                EditorGUI.HelpBox(position, "Invalid ScriptableData type", MessageType.Error);
                EditorGUI.EndProperty();
                return;
            }

            // Draw the main property field
            Rect propertyRect = position;
            propertyRect.height = EditorGUI.GetPropertyHeight(property, label, true);
            EditorGUI.PropertyField(propertyRect, property, label, true);

            // Draw the "Create Asset" button
            Rect buttonRect = position;
            buttonRect.y = position.y + propertyRect.height + Padding;
            buttonRect.height = ButtonHeight;

            if (GUI.Button(buttonRect, $"Create New {dataType.Name} ScriptableData Asset"))
            {
                CreateNewScriptableDataAsset(scriptableDataType, dataType);
            }

            EditorGUI.EndProperty();
        }

        private void CreateNewScriptableDataAsset(Type scriptableDataType, Type dataType)
        {
            if (scriptableDataType == null || dataType == null)
            {
                Debug.LogError("Cannot create ScriptableData asset: Invalid type information");
                return;
            }

            // Create the scriptable object instance
            var asset = ScriptableObject.CreateInstance(scriptableDataType);
            if (asset == null)
            {
                Debug.LogError($"Failed to create instance of {scriptableDataType.Name}");
                return;
            }

            // Ensure the Assets/ScriptableData directory exists
            string directoryPath = "Assets/ScriptableData";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Generate a unique file name
            string path = $"{directoryPath}/{dataType.Name}Data.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            try
            {
                // Create and save the asset
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Select the newly created asset
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;

                // Initialize the data if needed
                if (asset is ScriptableDataBase scriptableData)
                {
                    var method = scriptableDataType.GetMethod("Refresh");
                    method?.Invoke(asset, null);
                    EditorUtility.SetDirty(asset);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create ScriptableData asset: {e.Message}");
            }
        }
    }
#endif
}
