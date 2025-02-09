#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Darklight.UnityExt.Matrix;
using Darklight.UnityExt.Editor;
using System;
using Darklight.UnityExt.Utility;

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        /// <summary>
        /// Static class responsible for drawing Matrix Node GUI elements in both Scene and Inspector views
        /// </summary>
        public static class GUI
        {
            private static bool _showMatrixProperties = false;
            private static bool _showNodeProperties = false;
            private static bool _showPartitionProperties = false;
            private static bool _showMatrixGizmos = false;

            static bool _drawMatrixBounds = true;
            static bool _drawMatrixOrigin = true;
            static bool _drawMatrixNodes = true;

            static bool _showNodeKeys = false;
            static bool _showNodeCoordinates = false;
            static bool _showNodePositions = false;
            static bool _drawNodeButtons = true;

            public static void DrawGUI(Matrix matrix)
            {
                Matrix.Info info = matrix.GetInfo();
                Matrix.Map map = matrix.GetMap();

                // << MATRIX PROPERTIES >>
                CustomInspectorGUI.DrawFoldoutPropertyGroup(
                    "Matrix Properties",
                    ref _showMatrixProperties,
                    () =>
                    {
                        EditorGUILayout.LabelField("Bounds", info.Bounds.ToString());
                        EditorGUILayout.LabelField("Dimensions", info.Dimensions.ToString());
                        EditorGUILayout.LabelField("Swizzle", info.Swizzle.ToString());
                        EditorGUILayout.LabelField("Origin Key", info.OriginKey.ToString());
                        EditorGUILayout.LabelField("Terminal Key", info.TerminalKey.ToString());
                        EditorGUILayout.LabelField(
                            "Origin Alignment",
                            info.OriginAlignment.ToString()
                        );
                    }
                );

                // << NODE PROPERTIES >>
                CustomInspectorGUI.DrawFoldoutPropertyGroup(
                    "Node Properties",
                    ref _showNodeProperties,
                    () =>
                    {
                        EditorGUILayout.LabelField("Node Size", info.NodeSize.ToString());

                        EditorGUILayout.LabelField("Node Spacing", info.NodeSpacing.ToString());
                        EditorGUILayout.LabelField("Node Bonding", info.NodeBonding.ToString());
                        EditorGUILayout.LabelField("Total Nodes", map.NodeCount.ToString());
                        EditorGUILayout.LabelField(
                            "Enabled Nodes",
                            map.GetEnabledNodes().Count.ToString()
                        );
                        EditorGUILayout.LabelField(
                            "Disabled Nodes",
                            map.GetDisabledNodes().Count.ToString()
                        );
                    }
                );

                // << PARTITION PROPERTIES >>
                CustomInspectorGUI.DrawFoldoutPropertyGroup(
                    "Partition Properties",
                    ref _showPartitionProperties,
                    () =>
                    {
                        EditorGUILayout.LabelField("Partition Size", info.PartitionSize.ToString());

                        EditorGUILayout.LabelField(
                            "Total Partitions",
                            map.PartitionCount.ToString()
                        );
                        EditorGUILayout.LabelField("Origin Key", info.OriginKey.ToString());
                        EditorGUILayout.LabelField("Terminal Key", info.TerminalKey.ToString());
                        EditorGUILayout.LabelField("Alignment", info.OriginAlignment.ToString());
                    }
                );

                // << MATRIX GIZMOS >>
                CustomInspectorGUI.DrawFoldoutPropertyGroup(
                    "Matrix Gizmos",
                    ref _showMatrixGizmos,
                    () =>
                    {
                        _drawMatrixBounds = EditorGUILayout.Toggle(
                            "Draw Matrix Bounds",
                            _drawMatrixBounds
                        );
                        _drawMatrixOrigin = EditorGUILayout.Toggle(
                            "Draw Matrix Origin",
                            _drawMatrixOrigin
                        );

                        _drawMatrixNodes = EditorGUILayout.Toggle(
                            "Draw Matrix Nodes",
                            _drawMatrixNodes
                        );
                    }
                );

                EditorGUILayout.Space();
            }

            /// <summary>
            /// Draws the matrix visualization in the scene view based on toggle states
            /// </summary>
            /// <param name="matrix">The matrix to visualize</param>
            /// <param name="onNodeSelected">Callback when a node is selected</param>
            public static void DrawSceneGUI(Matrix matrix, Action<Matrix.Node> onNodeSelected)
            {
                if (!matrix)
                    return;

                // Draw Matrix Bounds
                if (_drawMatrixBounds)
                    DrawMatrixBounds_SceneGUI(matrix);

                // Draw Matrix Origin
                if (_drawMatrixOrigin)
                    DrawMatrixOrigin_SceneGUI(matrix);

                // Draw Matrix Nodes
                if (_drawMatrixNodes) { }
            }

            static void DrawMatrixBounds_SceneGUI(Matrix matrix)
            {
                Handles.DrawWireCube(
                    matrix.GetInfo().Center,
                    Matrix.Utility.SwizzleVec2(matrix.GetInfo().Bounds, matrix.GetInfo().Swizzle)
                );
            }

            static void DrawMatrixOrigin_SceneGUI(Matrix matrix)
            {
                float size = 1f;
                Vector3 position = matrix.GetInfo().Center;
                Quaternion rotation = matrix.GetInfo().Rotation;
                Quaternion forward = matrix.GetInfo().ForwardDirection;
                Quaternion up = matrix.GetInfo().UpDirection;

                Color xAxisColor = Color.red;
                Color yAxisColor = Color.green;
                Color zAxisColor = Color.blue;

                // Draw main rotation axes

                // Draw X axis (right)
                Handles.color = xAxisColor;
                Handles.DrawLine(position, position + Vector3.right * size);
                Handles.ConeHandleCap(
                    0,
                    position + Vector3.right * size,
                    Quaternion.LookRotation(Vector3.right),
                    size * 0.1f,
                    EventType.Repaint
                );

                // Draw Y axis (up)
                Handles.color = yAxisColor;
                Handles.DrawLine(position, position + Vector3.up * size);
                Handles.ConeHandleCap(
                    0,
                    position + Vector3.up * size,
                    Quaternion.LookRotation(Vector3.up),
                    size * 0.1f,
                    EventType.Repaint
                );

                // Draw Z axis (forward)
                Handles.color = CustomGUIColors.zAxis;
                Handles.DrawLine(position, position + Vector3.forward * size);

                Handles.ConeHandleCap(
                    0,
                    position + Vector3.forward * size,
                    Quaternion.LookRotation(Vector3.forward),
                    size * 0.1f,
                    EventType.Repaint
                );

                Handles.color = CustomGUIColors.yAxis;
                Handles.DrawWireArc(position, Vector3.up, Vector3.forward, 360, size * 0.8f);

                Handles.DrawLine(position, position + Vector3.forward * size);
                Handles.ConeHandleCap(
                    0,
                    position + Vector3.forward * size,
                    Quaternion.LookRotation(Vector3.forward),
                    size * 0.1f,
                    EventType.Repaint
                );

                Handles.color = yAxisColor.WithAlpha(0.5f);
                Handles.DrawWireArc(position, Vector3.right, Vector3.up, 360, size * 0.6f);
                Handles.DrawLine(position, position + Vector3.up * size);
                Handles.ConeHandleCap(
                    0,
                    position + Vector3.up * size,
                    Quaternion.LookRotation(Vector3.up),
                    size * 0.1f,
                    EventType.Repaint
                );
            }
        }
    }
}
#endif
