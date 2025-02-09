#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    public class MatrixEditorWindow : EditorWindow
    {
        readonly Color _textColor = Color.black;
        readonly Color _defaultColor = Color.white * new Color(1, 1, 1, 0.5f);
        readonly Color _selectedColor = Color.yellow * new Color(1, 1, 1, 0.5f);
        readonly Color _disabledColor = Color.black * new Color(1, 1, 1, 0.25f);

        SerializedObject _serializedMatrixObject;
        Matrix _matrix;
        Matrix.Node _selectedNode;

        private int _selectedPartition = -1;
        private Vector3 _selectedPartitionCenter;
        private bool _isPartitionSelected => _selectedPartition != -1;

        private MatrixPathfinder _pathfinder;
        private bool _showPathfinderControls = false;
        private bool _findPathWithActiveNodes = false;
        private Matrix.Node _pathStartNode;
        private Matrix.Node _pathEndNode;
        private List<Matrix.Node> _currentPath;
        private Color _pathColor = Color.yellow;

        [MenuItem("Darklight/Window/Matrix Editor")]
        private static void ShowWindow()
        {
            EditorWindow window = GetWindow<MatrixEditorWindow>();
            window.titleContent = new GUIContent("Matrix Editor");
            window.minSize = new Vector2(300, 200);
            window.Show();
        }

        public static void ShowWindow(Matrix targetMatrix)
        {
            var window = GetWindow<MatrixEditorWindow>();
            window._matrix = targetMatrix;
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            Selection.selectionChanged += OnSelectionChanged;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            if (Selection.activeGameObject != null)
            {
                var matrix = Selection.activeGameObject.GetComponent<Matrix>();
                if (matrix != null)
                {
                    _matrix = matrix;
                    Repaint();
                }
            }
        }

        void OnGUI()
        {
            // Select target Matrix
            _matrix = (Matrix)EditorGUILayout.ObjectField("Matrix", _matrix, typeof(Matrix), true);
            if (_matrix == null)
            {
                EditorGUILayout.HelpBox(
                    "Please assign a target Matrix object.",
                    MessageType.Warning
                );
                return;
            }
            if (_matrix == null)
                return;

            // Update serialized object
            _serializedMatrixObject = new SerializedObject(_matrix);
            _serializedMatrixObject.Update();

            Matrix.GUI.DrawGUI(_matrix);

            /*
            // Draw Selected Node Info
            Matrix.GUI.DrawNodeInfoGUI(_selectedNode, ref _selectedNodeIsExpanded);

            // Draw Partition Toggle and List
            MatrixPartitionGUI.DrawPartitionToggle(ref _showPartitions);
            if (_showPartitions)
            {
                _partitionFoldoutToggle = EditorGUILayout.Foldout(
                    _partitionFoldoutToggle,
                    "Partitions",
                    true
                );
                if (_partitionFoldoutToggle)
                {
                    MatrixPartitionGUI.DrawPartitionList(
                        _matrix,
                        ref _partitionScrollPosition,
                        _partitionFoldouts,
                        SetSelectedNode,
                        (node) => SceneView.lastActiveSceneView?.LookAt(node.Position)
                    );
                }
            }
            */

            // Check for pathfinder and draw controls
            _pathfinder = _matrix.GetComponent<MatrixPathfinder>();
            if (_pathfinder != null)
            {
                MatrixPathfinderGUI.DrawPathfinderControls(
                    _matrix,
                    _pathfinder,
                    _selectedNode,
                    _pathStartNode,
                    _pathEndNode,
                    _currentPath,
                    _findPathWithActiveNodes,
                    _pathColor,
                    FindPath,
                    ClearPath,
                    ref _showPathfinderControls
                );
            }

            _serializedMatrixObject.ApplyModifiedProperties();

            // Add partition selection controls
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (_isPartitionSelected)
            {
                if (GUILayout.Button("Back to Partitions"))
                {
                    ClearPartitionSelection();
                }
                EditorGUILayout.LabelField($"Viewing Partition {_selectedPartition}");
            }

            EditorGUILayout.EndHorizontal();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (_matrix == null)
                return;
            if (_matrix.isActiveAndEnabled == false)
                return;

            Matrix.GUI.DrawSceneGUI(_matrix, SetSelectedNode);

            sceneView.Repaint();
        }

        void SetSelectedNode(Matrix.Node node)
        {
            _selectedNode = node;
            Repaint();
        }

        private void FindPath()
        {
            if (_pathfinder == null || !_pathStartNode.IsValid || !_pathEndNode.IsValid)
                return;

            List<Matrix.Node> nodes = _findPathWithActiveNodes
                ? _matrix.GetMap().GetEnabledNodes()
                : _matrix.GetMap().GetDisabledNodes();
            _currentPath = _pathfinder.FindPath(nodes, _pathStartNode, _pathEndNode);

            if (_currentPath == null || _currentPath.Count == 0)
            {
                Debug.LogWarning("No path found between selected nodes.");
            }

            SceneView.RepaintAll();
        }

        private void ClearPath()
        {
            _currentPath = null;
            _pathStartNode.Reset();
            _pathEndNode.Reset();
            SceneView.RepaintAll();
        }

        private void SelectPartition(int partitionKey, Vector3 center)
        {
            _selectedPartition = partitionKey;
            _selectedPartitionCenter = center;
            _selectedNode.Reset();

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAt(
                    _selectedPartitionCenter,
                    SceneView.lastActiveSceneView.rotation,
                    5f
                );
            }
        }

        private void ClearPartitionSelection()
        {
            _selectedPartition = -1;
            _selectedNode.Reset();
        }
    }
}

#endif
