
namespace Darklight.UnityExt.Behaviour.Editor
{

#if UNITY_EDITOR

    using System;

    using Darklight.UnityExt.Editor;

    using UnityEditor;

    using UnityEngine;

    [CustomEditor(typeof(ScriptableDataBase), true)]
    public class ScriptableDataCustomEditor : UnityEditor.Editor
    {
        Type _dataType;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty dataProperty = serializedObject.FindProperty("_data");
            _dataType = target.GetType().BaseType.GetGenericArguments()[0];

            if (dataProperty == null)
            {
                EditorGUILayout.HelpBox("Could not find _data property.", MessageType.Error);
                return;
            }

            // << CREATE DATA >> -------------------------- //
            if (dataProperty.managedReferenceValue == null)
            {
                var newData = Activator.CreateInstance(_dataType);
                dataProperty.managedReferenceValue = newData;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
            else
            {
                // Draw the data's fields directly
                CustomInspectorGUI.DrawAllSerializedFields(dataProperty);
            }

            Debug.Log("ScriptableDataCustomEditor OnInspectorGUI");

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}