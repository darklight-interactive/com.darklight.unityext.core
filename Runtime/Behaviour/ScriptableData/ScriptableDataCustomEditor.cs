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

            // Choose the appropriate SerializedProperty based on type
            SerializedProperty dataProperty = serializedObject.FindProperty(isClassType ? "_dataClass" : "_dataStruct");
            if (dataProperty == null)
            {
                EditorGUILayout.HelpBox("Could not find the correct data property.", MessageType.Error);
                return;
            }

            // << Create or Draw Data >>
            // Check if _data is null and if it's a struct, assign a new instance by default.
            if (isClassType && dataProperty.managedReferenceValue == null)
            {
                if (dataType.IsValueType || dataType != typeof(UnityEngine.Object))
                {
                    dataProperty.managedReferenceValue = Activator.CreateInstance(dataType);
                }
                else
                {
                    EditorGUILayout.HelpBox("_data is null and can't be initialized.", MessageType.Warning);
                }
            }

            CustomInspectorGUI.DrawAllFieldsInProperty(dataProperty);
            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}
