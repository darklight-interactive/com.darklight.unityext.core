#if UNITY_EDITOR

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

        SerializedObject _serializedMatrixObject;
        Matrix _matrix;
        Node _selectedNode;
        bool _selectedNodeIsExpanded = false;
        Vector2 _scrollPosition;

        Node.Visitor SceneGUINodeVisitor => new Node.Visitor(node =>
        {
            Vector3 position = node.Position;
            Vector2 dimensions = node.Dimensions;
            Quaternion rotation = node.Rotation;
            Vector3 normal = node.Normal;

            // << GET COLOR >>
            Color color = _defaultColor;
            if (node == _selectedNode)
                color = _selectedColor;

            // << DRAW BACKGROUND >>
            CustomGizmos.DrawSolidRect(position, dimensions, normal, color);

            // << DRAW BUTTON >>
            float size = node.AverageSpan;
            CustomGizmos.DrawButtonHandle(position, size, normal, color, () =>
                {
                    SetSelectedNode(node);
                }, Handles.DotHandleCap);

            // << DRAW LABEL >>
            CustomGizmos.DrawLabel($"{node.Key}", position, new GUIStyle()
            {
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
            _serializedMatrixObject = new SerializedObject(_matrix);


            if (_matrix != null && _serializedMatrixObject != null)
            {
                _serializedMatrixObject.Update(); // Ensure SerializedObject is up-to-date

                EditorGUILayout.Space();
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

                _serializedMatrixObject.ApplyModifiedProperties(); // Apply any changes made in the GUI
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

            _matrix.SendVisitorToAllNodes(SceneGUINodeVisitor);

            // Repaint the scene view to update the handles in real-time
            sceneView.Repaint();
        }

        void SetSelectedNode(Node node)
        {
            _selectedNode = node;
            Repaint();

            Debug.Log($"{WINDOW_NAME}: SetSelectedNode: " + node.Key);
        }
    }
#endif
}