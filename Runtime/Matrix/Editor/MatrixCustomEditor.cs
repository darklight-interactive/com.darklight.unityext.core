#if UNITY_EDITOR

using UnityEditor;

using UnityEngine;

namespace Darklight.UnityExt.Matrix.Editor
{
    [CustomEditor(typeof(Matrix))]
    public class MatrixCustomEditor : UnityEditor.Editor
    {
        SerializedObject _serializedObject;
        Matrix _script;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (Matrix)target;
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            // Add a button to open the Matrix Editor Window
            if (GUILayout.Button("Open Matrix Editor"))
            {
                // Open the MatrixEditorWindow and pass the current Matrix instance
                MatrixEditorWindow.ShowWindow(_script);
            }

            // Display default inspector properties
            base.OnInspectorGUI();

            _serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
