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

        // Fields to store the last-known transform state
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (Matrix)target;

            // Initialize transform state and set up change listeners
            if (_script != null)
            {
                _lastPosition = _script.transform.position;
                _lastRotation = _script.transform.rotation;
            }

            EditorApplication.update += CheckTransformChanges;
            Undo.undoRedoPerformed += OnUndoRedo;

            _script.Preload();
        }

        private void OnDisable()
        {
            EditorApplication.update -= CheckTransformChanges;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void CheckTransformChanges()
        {
            if (_script == null) return;

            // Check for changes in the position, rotation, or scale
            if (_script.transform.position != _lastPosition ||
                _script.transform.rotation != _lastRotation)
            {
                // Update the last-known state
                _lastPosition = _script.transform.position;
                _lastRotation = _script.transform.rotation;

                // Respond to the change
                //Debug.Log("Transform has changed!");
                _script.Refresh();

                // Refresh the editor if needed
                Repaint();
            }
        }

        private void OnUndoRedo()
        {
            if (_script != null)
            {
                // Handle undo/redo for the transform changes
                //Debug.Log("Transform changed due to undo/redo!");
                _script.Refresh();

                // Update last-known transform state in case it has changed
                _lastPosition = _script.transform.position;
                _lastRotation = _script.transform.rotation;

                // Refresh the editor if needed
                Repaint();
            }
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
        }
        public override void OnInspectorGUI()
        {
            _serializedObject.Update();
            _script = (Matrix)target;

            EditorGUI.BeginChangeCheck();

            DrawButtons();
            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                _script.Refresh();
            }
        }
    }
}
#endif
