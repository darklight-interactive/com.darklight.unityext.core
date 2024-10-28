#if UNITY_EDITOR

using UnityEditor;

using UnityEngine;

namespace Darklight.UnityExt.Matrix.Editor
{
    [CustomEditor(typeof(Matrix), true)]
    public class MatrixCustomEditor : UnityEditor.Editor
    {
        SerializedObject _serializedObject;
        Matrix _script;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (Matrix)target;
        }

        protected virtual void DrawButtons()
        {
            // Add a button to open the Matrix Editor Window
            if (GUILayout.Button("Open Matrix Editor"))
            {
                // Open the MatrixEditorWindow and pass the current Matrix instance
                MatrixEditorWindow.ShowWindow(_script);
            }

            if (_script.CurrentState == State.INVALID && GUILayout.Button("Preload"))
            {
                _script.Preload();
            }
            else if (_script.CurrentState == State.PRELOADED && GUILayout.Button("Initialize"))
            {
                _script.Initialize();
            }
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();
            _script = (Matrix)target;

            DrawButtons();
            DrawPropertiesExcluding(serializedObject, "m_Script");

            _script = (Matrix)target;
            _script.Refresh();

            _serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
