#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Darklight.UnityExt.Matrix;
using static Darklight.UnityExt.Matrix.Matrix;

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

        private bool _showMatrixInfo = false;

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

        #region < PRIVATE_METHODS > [[ Internal Handlers ]] ================================================================
        private void CheckTransformChanges()
        {
            if (_script == null)
                return;

            // Check for changes in the position, rotation, or scale
            if (
                _script.transform.position != _lastPosition
                || _script.transform.rotation != _lastRotation
            )
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
        #endregion

        protected virtual void DrawButtons()
        {
            // Add a button to open the Matrix Editor Window
            if (GUILayout.Button("Open Matrix Editor"))
            {
                // Open the MatrixEditorWindow and pass the current Matrix instance
                MatrixEditorWindow.ShowWindow(_script);
            }

            if (_script.CurrentState == Matrix.State.INVALID && GUILayout.Button("Preload"))
            {
                _script.Preload();
            }

            if (_script.CurrentState == Matrix.State.PRELOADED && GUILayout.Button("Refresh"))
            {
                _script.Refresh();
            }
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();
            _script = (Matrix)target;

            EditorGUI.BeginChangeCheck();

            DrawButtons();
            DrawMatrixInfo(_script.Info, ref _showMatrixInfo);
            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                _script.Refresh();
            }
        }

        public static void DrawMatrixInfo(MatrixInfo info, ref bool showMatrixInfo)
        {
            showMatrixInfo = EditorGUILayout.Foldout(showMatrixInfo, "Matrix Info");

            if (!showMatrixInfo)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Center", info.Center.ToString());
            EditorGUILayout.LabelField("Rotation", info.Rotation.ToString());

            EditorGUILayout.LabelField("Bounds", info.Bounds.ToString());
            EditorGUILayout.LabelField("Dimensions", info.Dimensions.ToString());
            EditorGUILayout.LabelField("Origin Key", info.OriginKey.ToString());
            EditorGUILayout.LabelField("Terminal Key", info.TerminalKey.ToString());
            EditorGUILayout.LabelField("Alignment", info.OriginAlignment.ToString());

            EditorGUILayout.LabelField("Partition Size", info.PartitionSize.ToString());
            EditorGUILayout.LabelField("Node Size", info.NodeSize.ToString());
            EditorGUILayout.LabelField("Node Spacing", info.NodeSpacing.ToString());
            EditorGUILayout.LabelField("Node Bonding", info.NodeBonding.ToString());
            EditorGUILayout.LabelField("Center Nodes", info.CenterNodes.ToString());

            EditorGUILayout.LabelField("Swizzle", info.Swizzle.ToString());

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
    }
}
#endif
