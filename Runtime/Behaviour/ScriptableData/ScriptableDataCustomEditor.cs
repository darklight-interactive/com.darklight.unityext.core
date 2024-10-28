using System;

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
                EditorGUILayout.HelpBox("Could not find the correct data property.", MessageType.Error);
                return;
            }

            string dataTypeDisplayName = isClassType ? "Class" : "Struct";
            EditorGUILayout.LabelField($"{dataType.Name} : {dataTypeDisplayName}", EditorStyles.boldLabel);
            CustomInspectorGUI.DrawAllFieldsInProperty(dataProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
