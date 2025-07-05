using NaughtyAttributes;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor.Example
{
    /// <summary>
    /// Example class demonstrating various NaughtyAttributes implementations
    /// </summary>
    public class EditorExample : MonoBehaviour
    {
        [Header("Basic Examples")]
        public int exampleInt = 42;

        [ResizableTextArea]
        public string resizableTextArea =
            "This is a resizable text area where you can see the whole text. Unlike Unity's Multiline or TextArea attributes where you can see only 3 rows of a given text.";

        [ProgressBar("Progress", 1, EColor.Green)]
        public float progressBar = 0.5f;

        [ShowNativeProperty]
        public int nativeProperty => 100;
    }

    [CustomEditor(typeof(EditorExample))]
    public class EditorExampleEditor : NaughtyInspector
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Custom Button"))
            {
                Debug.Log("Custom Editor Button clicked!");
            }

            // Draw NaughtyAttributes properties
            base.OnInspectorGUI();

            // Draw your custom properties after
            EditorExample myTarget = (EditorExample)target;

            EditorGUILayout.Space();

            // If you modify values, mark dirty
            if (GUI.changed)
            {
                EditorUtility.SetDirty(myTarget);
            }
        }
    }
}
