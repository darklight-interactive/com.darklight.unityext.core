#if UNITY_EDITOR

using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Matrix;

using UnityEditor;

using UnityEngine;

namespace Darklight.UnityExt.Matrix.Editor
{
    public class MatrixEditorWindow : EditorWindow
    {
        const string WINDOW_NAME = "Matrix Editor";

        readonly Color _textColor = Color.black;
        readonly Color _defaultColor = Color.white * new Color(1, 1, 1, 0.5f);
        readonly Color _selectedColor = Color.yellow * new Color(1, 1, 1, 0.5f);
        readonly Color _disabledColor = Color.black * new Color(1, 1, 1, 0.25f);

        SerializedObject _serializedMatrixObject;
        Matrix _matrix;
        Matrix.Node _selectedNode;
        bool _selectedNodeIsExpanded = false;
        Vector2 _scrollPosition;
        bool _showMatrixInfo = false;
        bool _showNodeKeys = false;
        bool _showNodeCoordinates = false;
        bool _showNodePositions = false;
        bool _drawNodeButtons = true;


        Vector2 _partitionScrollPosition;
        bool _showPartitions = false;
        bool _partitionFoldoutToggle = false;
        Dictionary<int, bool> _partitionFoldouts = new Dictionary<int, bool>();

        private MatrixPathfinder _pathfinder;
        private bool _findPathWithActiveNodes = false;
        private Matrix.Node _pathStartNode;
        private Matrix.Node _pathEndNode;
        private List<Matrix.Node> _currentPath;
        private Color _pathColor = Color.yellow;

        Matrix.Node.Visitor SceneGUINodeVisitor => new Matrix.Node.Visitor(node =>
        {
            Vector3 position = node.Position;
            Vector2 dimensions = node.Dimensions;
            Quaternion rotation = node.Rotation;
            Vector3 normal = node.Normal;

            // << GET COLOR >>
            Color color = _defaultColor;
            if (_showPartitions)
            {
                color = _matrix.Info.GenerateColorFromPartitionKey(node.Partition);
            }
            else if (node.Enabled == false)
                color = _disabledColor;

            if (node == _selectedNode)
                color = _selectedColor;



            // << DRAW BACKGROUND >>
            //CustomGizmos.DrawSolidRect(position, dimensions, normal, color);

            // << DRAW BUTTON >>
            float size = node.AverageSpan;
            CustomGizmos.DrawButtonHandle(position, size * 0.9f, normal, color, () =>
                {
                    SetSelectedNode(node);
                }, Handles.DotHandleCap);

            // << DRAW LABEL >>
            string label = "";
            if (_showNodeKeys)
                label = node.Key.ToString();
            else if (_showNodeCoordinates)
                label = node.Coordinate.ToString();
            else if (_showNodePositions)
                label = node.Position.ToString();

            CustomGizmos.DrawLabel(label, position, new GUIStyle()
            {
                fontSize = 8,
                normal = new GUIStyleState()
                {
                    textColor = _textColor
                },
                alignment = TextAnchor.MiddleCenter
            });

            return true;
        });

        [MenuItem("Window/Darklight/MatrixEditorWindow")]
        public static void ShowWindow()
        {
            GetWindow<MatrixEditorWindow>(WINDOW_NAME);
        }

        // Overloaded ShowWindow method to accept a Matrix instance
        public static void ShowWindow(Matrix targetMatrix)
        {
            var window = GetWindow<MatrixEditorWindow>(WINDOW_NAME);
            window._matrix = targetMatrix;
        }


        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Matrix Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Select target Matrix
            _matrix = (Matrix)EditorGUILayout.ObjectField("Matrix", _matrix, typeof(Matrix), true);

            if (_matrix == null)
            {
                EditorGUILayout.HelpBox("Please assign a target Matrix object.", MessageType.Warning);
                return;
            }

            _serializedMatrixObject = new SerializedObject(_matrix);


            DrawNodeVisualizationToggles();

            if (_matrix != null && _serializedMatrixObject != null)
            {
                _serializedMatrixObject.Update();

                // Draw Matrix Info
                    MatrixCustomEditor.DrawMatrixInfo(_matrix.Info, ref _showMatrixInfo);



                // Check for pathfinder
                _pathfinder = _matrix.GetComponent<MatrixPathfinder>();

                if (_pathfinder != null)
                {
                    DrawPathfindingControls();
                }

                // Draw Partition List
                if (_showPartitions)
                {
                    _partitionFoldoutToggle = EditorGUILayout.Foldout(_partitionFoldoutToggle, "Partitions", true);
                    if (_partitionFoldoutToggle)
                    {
                        DrawPartitionList();
                    }
                }


                EditorGUILayout.Space();
                
                // Selected Node Info
                if (_selectedNode == null)
                {
                    EditorGUILayout.LabelField("Select a node to inspect.");
                }
                else
                {
                    EditorGUILayout.LabelField($"Selected Node: {_selectedNode.Key}");
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Node Data");

                    CustomInspectorGUI.DrawClassAsShowOnly(_selectedNode, ref _selectedNodeIsExpanded);
                }

                _serializedMatrixObject.ApplyModifiedProperties();
            }
            else
            {
                EditorGUILayout.HelpBox("Please assign a target Matrix object.", MessageType.Warning);
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (_matrix == null) return;
            if (_matrix.isActiveAndEnabled == false) return;

            if (_drawNodeButtons)
            {
                _matrix.SendVisitorToAllNodes(SceneGUINodeVisitor);
            }


            // Draw path if exists
            if (_currentPath != null && _currentPath.Count > 1)
            {
                Handles.color = _pathColor;
                for (int i = 0; i < _currentPath.Count - 1; i++)
                {
                    var startPos = _currentPath[i].Position;
                    var endPos = _currentPath[i + 1].Position;
                    
                    // Draw line
                    Handles.DrawLine(startPos, endPos, 2f);
                    
                    // Draw connection points
                    float sphereSize = 0.2f;
                    Handles.SphereHandleCap(
                        0,
                        startPos,
                        Quaternion.identity,
                        sphereSize,
                        EventType.Repaint
                    );
                }
                
                // Draw last node
                Handles.SphereHandleCap(
                    0,
                    _currentPath[_currentPath.Count - 1].Position,
                    Quaternion.identity,
                    0.2f,
                    EventType.Repaint
                );
            }

            // Highlight start and end nodes
            if (_pathStartNode != null)
            {
                Handles.color = Color.green;
                Handles.DrawWireCube(_pathStartNode.Position, Vector3.one * 0.5f);
            }
            if (_pathEndNode != null)
            {
                Handles.color = Color.red;
                Handles.DrawWireCube(_pathEndNode.Position, Vector3.one * 0.5f);
            }

            sceneView.Repaint();
        }

        void SetSelectedNode(Matrix.Node node)
        {
            _selectedNode = node;

            Repaint();

            Debug.Log($"{WINDOW_NAME}: SetSelectedNode: " + node.Key);
        }

        private void DrawNodeVisualizationToggles()
        {
            bool anyOptionSelected = _showNodeKeys || _showNodeCoordinates || _showNodePositions;

            if (!anyOptionSelected)
            {
                // Show all options when none are selected
                _showNodeKeys = EditorGUILayout.Toggle("Show Node Keys", _showNodeKeys);
                _showNodeCoordinates = EditorGUILayout.Toggle("Show Node Coordinates", _showNodeCoordinates);
                _showNodePositions = EditorGUILayout.Toggle("Show Node Positions", _showNodePositions);
            }
            else
            {
                // Show only the selected option and set others to false
                if (_showNodeKeys)
                {
                    _showNodeKeys = EditorGUILayout.Toggle("Show Node Keys", _showNodeKeys);
                    _showNodeCoordinates = false;
                    _showNodePositions = false;
                }
                else if (_showNodeCoordinates)
                {
                    _showNodeCoordinates = EditorGUILayout.Toggle("Show Node Coordinates", _showNodeCoordinates);
                    _showNodeKeys = false;
                    _showNodePositions = false;
                }
                else if (_showNodePositions)
                {
                    _showNodePositions = EditorGUILayout.Toggle("Show Node Positions", _showNodePositions);
                    _showNodeKeys = false;
                    _showNodeCoordinates = false;
                }
            }

            // Draw Node Buttons
            _drawNodeButtons = EditorGUILayout.Toggle("Draw Node Buttons", _drawNodeButtons);

            // Show Partitions
            EditorGUILayout.Space();
            _showPartitions = EditorGUILayout.Toggle("Show Partitions", _showPartitions);



        }


        private void DrawPartitionList()
        {
            if (_matrix == null || _matrix.Map == null) return;

            using (var scrollView = new EditorGUILayout.ScrollViewScope(_partitionScrollPosition))
            {
                _partitionScrollPosition = scrollView.scrollPosition;

                Dictionary<int, HashSet<Vector2Int>> partitions = _matrix.Map.GetPartitions();
                if (partitions.Count == 0)
                {
                    EditorGUILayout.HelpBox("No partitions found.", MessageType.Info);
                    return;
                }

                foreach (var partition in partitions)
                {
                    // Ensure the partition has a foldout state
                    if (!_partitionFoldouts.ContainsKey(partition.Key))
                    {
                        _partitionFoldouts[partition.Key] = false;
                    }

                    // Draw partition header with node count
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        Color partitionColor = _matrix.Info.GenerateColorFromPartitionKey(partition.Key);
                        EditorGUI.DrawRect(GUILayoutUtility.GetRect(16, 16), partitionColor);
                        
                        _partitionFoldouts[partition.Key] = EditorGUILayout.Foldout(
                            _partitionFoldouts[partition.Key],
                            $"Partition {partition.Key} ({partition.Value.Count} nodes)",
                            true
                        );
                    }
                    EditorGUILayout.EndHorizontal();

                    // Draw partition contents if expanded
                    if (_partitionFoldouts[partition.Key])
                    {
                        EditorGUI.indentLevel++;
                        foreach (var nodeKey in partition.Value)
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                // Indent the content
                                GUILayout.Space(20);

                                var node = _matrix.Map.GetNodeByKey(nodeKey);
                                if (node != null)

                                {
                                    // Draw node info
                                    EditorGUILayout.LabelField($"Key: {nodeKey}");
                                    
                                    // Add a select button
                                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                                    {
                                        SetSelectedNode(node);
                                        SceneView.lastActiveSceneView?.LookAt(node.Position);
                                    }
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }

        private void DrawPathfindingControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pathfinding", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                _findPathWithActiveNodes = EditorGUILayout.Toggle("Find Path With Active Nodes", _findPathWithActiveNodes);

                // Start Node
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Start Node:", _pathStartNode != null ? _pathStartNode.Coordinate.ToString() : "Not Set");
                GUI.enabled = _selectedNode != null;
                if (GUILayout.Button("Set Selected As Start", GUILayout.Width(150)))
                {
                    _pathStartNode = _selectedNode;
                    _currentPath = null; // Clear current path
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                // End Node
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("End Node:", _pathEndNode != null ? _pathEndNode.Coordinate.ToString() : "Not Set");
                GUI.enabled = _selectedNode != null;
                if (GUILayout.Button("Set Selected As End", GUILayout.Width(150)))
                {
                    _pathEndNode = _selectedNode;
                    _currentPath = null; // Clear current path
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                // Find Path Button
                EditorGUILayout.Space();
                GUI.enabled = _pathStartNode != null && _pathEndNode != null;
                if (GUILayout.Button("Find Path"))
                {
                    FindPath();
                }
                GUI.enabled = true;

                // Path Color
                _pathColor = EditorGUILayout.ColorField("Path Color", _pathColor);

                // Clear Path Button
                if (_currentPath != null && _currentPath.Count > 0)
                {
                    if (GUILayout.Button("Clear Path"))
                    {
                        ClearPath();
                    }
                }
            }
        }

        private void FindPath()
        {
            if (_pathfinder == null || _pathStartNode == null || _pathEndNode == null)
                return;

            List<Matrix.Node> nodes = _findPathWithActiveNodes ? _matrix.Map.ActiveNodes : _matrix.Map.InactiveNodes;
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
            _pathStartNode = null;
            _pathEndNode = null;
            SceneView.RepaintAll();
        }
    }
#endif
}