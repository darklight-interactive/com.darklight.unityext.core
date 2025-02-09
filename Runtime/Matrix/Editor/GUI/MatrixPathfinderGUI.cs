#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Darklight.UnityExt.Matrix
{
    /// <summary>
    /// Static class responsible for drawing Matrix Pathfinder GUI elements in both Scene and Inspector views
    /// </summary>
    public static class MatrixPathfinderGUI
    {
        /// <summary>
        /// Draws the pathfinder controls in the inspector
        /// </summary>
        public static void DrawPathfinderControls(
            Matrix matrix,
            MatrixPathfinder pathfinder,
            Matrix.Node selectedNode,
            Matrix.Node pathStartNode,
            Matrix.Node pathEndNode,
            List<Matrix.Node> currentPath,
            bool findPathWithActiveNodes,
            Color pathColor,
            System.Action onFindPath,
            System.Action onClearPath,
            ref bool showPathfinderControls
        )
        {
            if (pathfinder == null)
                return;

            EditorGUILayout.Space();
            showPathfinderControls = EditorGUILayout.Foldout(
                showPathfinderControls,
                "Pathfinder",
                true
            );

            if (showPathfinderControls)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.Toggle("Find Path With Active Nodes", findPathWithActiveNodes);

                    // Start Node
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(
                        "Start Node:",
                        pathStartNode.IsValid ? pathStartNode.Coordinate.ToString() : "Not Set"
                    );
                    GUI.enabled = selectedNode.IsValid;
                    if (GUILayout.Button("Set Selected As Start", GUILayout.Width(150)))
                    {
                        onFindPath?.Invoke();
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();

                    // End Node
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(
                        "End Node:",
                        pathEndNode.IsValid ? pathEndNode.Coordinate.ToString() : "Not Set"
                    );
                    GUI.enabled = selectedNode.IsValid;
                    if (GUILayout.Button("Set Selected As End", GUILayout.Width(150)))
                    {
                        onClearPath?.Invoke();
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();

                    // Find Path Button
                    EditorGUILayout.Space();
                    GUI.enabled = pathStartNode.IsValid && pathEndNode.IsValid;
                    if (GUILayout.Button("Find Path"))
                    {
                        onFindPath?.Invoke();
                    }

                    GUI.enabled = true;

                    // Path Color
                    EditorGUILayout.ColorField("Path Color", pathColor);

                    // Clear Path Button
                    if (currentPath != null && currentPath.Count > 0)
                    {
                        if (GUILayout.Button("Clear Path"))
                        {
                            onClearPath?.Invoke();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the pathfinder visualization in the scene view
        /// </summary>
        public static void DrawPathfinderSceneGUI(
            List<Matrix.Node> currentPath,
            Matrix.Node pathStartNode,
            Matrix.Node pathEndNode,
            Color pathColor
        )
        {
            // Draw path if exists
            if (currentPath != null && currentPath.Count > 1)
            {
                Handles.color = pathColor;
                for (int i = 0; i < currentPath.Count - 1; i++)
                {
                    var startPos = currentPath[i].Center;
                    var endPos = currentPath[i + 1].Center;

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
                    currentPath[currentPath.Count - 1].Center,
                    Quaternion.identity,
                    0.2f,
                    EventType.Repaint
                );
            }

            // Highlight start and end nodes
            if (pathStartNode.IsValid)
            {
                Handles.color = Color.green;
                Handles.DrawWireCube(pathStartNode.Center, Vector3.one * 0.5f);
            }

            if (pathEndNode.IsValid)
            {
                Handles.color = Color.red;
                Handles.DrawWireCube(pathEndNode.Center, Vector3.one * 0.5f);
            }
        }
    }
}
#endif
