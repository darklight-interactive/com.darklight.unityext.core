#if UNITY_EDITOR

using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Matrix;

using UnityEditor;

using UnityEngine;

public class MatrixEditorWindow : EditorWindow
{
    private Matrix matrix;
    private Vector2Int nodeKey = Vector2Int.zero;
    private Matrix.Node selectedNode;
    private Vector2 scrollPosition;

    [MenuItem("Window/Darklight/MatrixEditorWindow")]
    public static void ShowWindow()
    {
        GetWindow<MatrixEditorWindow>("Matrix Node Inspector");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Matrix Node Inspector", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Select target Matrix
        matrix = (Matrix)EditorGUILayout.ObjectField("Target Matrix", matrix, typeof(Matrix), true);
        if (matrix != null)
        {
            EditorGUILayout.Space();
            nodeKey = EditorGUILayout.Vector2IntField("Node Key", nodeKey);

            if (GUILayout.Button("Inspect Node"))
            {
                selectedNode = matrix.GetCell(nodeKey);
                if (selectedNode == null)
                {
                    Debug.LogWarning($"Node at {nodeKey} not found.");
                }
            }

            if (selectedNode != null)
            {
                //DisplayNodeDetails();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please assign a target Matrix object.", MessageType.Warning);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (matrix == null) return;

        matrix.SendVisitorToAllCells(SceneGUINodeVisitor);

        // Repaint the scene view to update the handles in real-time
        sceneView.Repaint();
    }

    Matrix.Node.Visitor SceneGUINodeVisitor => new Matrix.Node.Visitor(node =>
    {
        node.GetWorldSpaceValues(out Vector3 position, out Vector2 dimensions, out Vector3 normal);
        CustomGizmos.DrawButtonHandle(position, node.Data.SizeAvg, normal, Color.grey, () =>
            {
                selectedNode = node;
                Debug.Log($"Selected node at {node.Data.Key}");
            }, Handles.RectangleHandleCap);
        return true;
    });


    // Overloaded ShowWindow method to accept a Matrix instance
    public static void ShowWindow(Matrix targetMatrix)
    {
        var window = GetWindow<MatrixEditorWindow>("Matrix Node Inspector");
        window.matrix = targetMatrix;
    }
}
#endif
