#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Darklight.UnityExt.Matrix;
using Darklight.UnityExt.Editor;
using System;
using Darklight.UnityExt.Utility;
using System.Collections.Generic;

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        /// <summary>
        /// Static class responsible for drawing Matrix Node GUI elements in both Scene and Inspector views
        /// </summary>
        public static class GUI
        {
            static GUIStyle labelStyle = new GUIStyle()
            {
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
            };

            public enum NodeFilter
            {
                NONE, // Draw all nodes
                SELECTED, // Draw only selected node
                ENABLED, // Draw only enabled nodes
                DISABLED // Draw only disabled nodes
            }

            public static void ResetToDefaults()
            {
                Preferences.ResetToDefaults();
            }

            public static void DrawGUI(Matrix matrix, ref Node selectedNode)
            {
                CustomInspectorGUI.DrawButton("Reset to Defaults", ResetToDefaults);

                CustomInspectorGUI.DrawHorizontalLine(Color.grey, 5);

                OnGUI_DrawSelectedNode(ref selectedNode);

                CustomInspectorGUI.DrawHorizontalLine(Color.grey, 5);
                CustomInspectorGUI.DrawHeader("Matrix Properties");

                // << MATRIX PROPERTIES >>
                Preferences.GroupPrefs.ShowMatrixProperties =
                    CustomInspectorGUI.DrawFoldoutPropertyGroup(
                        "Matrix Properties",
                        Preferences.GroupPrefs.ShowMatrixProperties,
                        () =>
                        {
                            matrix._info.Bounds = EditorGUILayout.Vector2IntField(
                                "Bounds",
                                matrix._info.Bounds
                            );

                            // ( SWIZZLE PROPERTY ) - Disabled if Grid is not set
                            EditorGUI.BeginDisabledGroup(matrix._info.Grid != null);
                            matrix._info.Swizzle = (GridLayout.CellSwizzle)
                                EditorGUILayout.EnumPopup("Swizzle", matrix._info.Swizzle);
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField(
                                "Dimensions",
                                matrix._info.Dimensions.ToString()
                            );
                            EditorGUILayout.LabelField(
                                "Origin Key",
                                matrix._info.OriginKey.ToString()
                            );

                            EditorGUILayout.LabelField(
                                "Terminal Key",
                                matrix._info.TerminalKey.ToString()
                            );
                            EditorGUILayout.LabelField(
                                "Origin Alignment",
                                matrix._info.OriginAlignment.ToString()
                            );
                        }
                    );

                // << NODE PROPERTIES >>
                Preferences.GroupPrefs.ShowNodeProperties =
                    CustomInspectorGUI.DrawFoldoutPropertyGroup(
                        "Node Properties",
                        Preferences.GroupPrefs.ShowNodeProperties,
                        () =>
                        {
                            EditorGUILayout.LabelField(
                                "Node Size",
                                matrix._info.NodeSize.ToString()
                            );
                            EditorGUILayout.LabelField(
                                "Node Spacing",
                                matrix._info.NodeSpacing.ToString()
                            );
                            EditorGUILayout.LabelField(
                                "Node Bonding",
                                matrix._info.NodeBonding.ToString()
                            );
                            EditorGUILayout.LabelField(
                                "Total Nodes",
                                matrix._map.NodeCount.ToString()
                            );

                            EditorGUILayout.LabelField(
                                "Enabled Nodes",
                                matrix._map.GetEnabledNodes().Count.ToString()
                            );

                            EditorGUILayout.LabelField(
                                "Disabled Nodes",
                                matrix._map.GetDisabledNodes().Count.ToString()
                            );
                        }
                    );

                // << PARTITION PROPERTIES >>
                Preferences.GroupPrefs.ShowPartitionProperties =
                    CustomInspectorGUI.DrawFoldoutPropertyGroup(
                        "Partition Properties",
                        Preferences.GroupPrefs.ShowPartitionProperties,
                        () =>
                        {
                            matrix._info.PartitionSize = EditorGUILayout.IntField(
                                "Partition Size",
                                matrix._info.PartitionSize
                            );
                            EditorGUILayout.LabelField(
                                "Total Partitions",
                                matrix._map.PartitionCount.ToString()
                            );

                            EditorGUILayout.LabelField(
                                "Origin Key",
                                matrix._info.OriginKey.ToString()
                            );
                            EditorGUILayout.LabelField(
                                "Terminal Key",
                                matrix._info.TerminalKey.ToString()
                            );

                            EditorGUILayout.LabelField(
                                "Alignment",
                                matrix._info.OriginAlignment.ToString()
                            );
                        }
                    );

                CustomInspectorGUI.DrawHorizontalLine(Color.grey, 5);
                CustomInspectorGUI.DrawHeader("Gizmos");

                // << MATRIX GIZMOS >>
                Preferences.GroupPrefs.DrawMatrixGizmos =
                    CustomInspectorGUI.DrawTogglePropertyGroup(
                        "Draw Matrix Gizmos",
                        Preferences.GroupPrefs.DrawMatrixGizmos,
                        () =>
                        {
                            var results = CustomInspectorGUI.DrawToggleGroup(
                                new Dictionary<string, (bool, string)>
                                {
                                    {
                                        "Draw Matrix Bounds",
                                        (
                                            Preferences.MatrixPrefs.DrawBounds,
                                            "Display the bounding box of the matrix"
                                        )
                                    },
                                    {
                                        "Draw Matrix Origin",
                                        (
                                            Preferences.MatrixPrefs.DrawOrigin,
                                            "Display the origin point and axes of the matrix"
                                        )
                                    },
                                    {
                                        "Draw Matrix Rotation Axis",
                                        (
                                            Preferences.MatrixPrefs.DrawRotation,
                                            "Display the rotation axis and angle"
                                        )
                                    },
                                    {
                                        "Draw Matrix Direction Vectors",
                                        (
                                            Preferences.MatrixPrefs.DrawDirections,
                                            "Display the direction vectors of the matrix"
                                        )
                                    }
                                }
                            );

                            Preferences.MatrixPrefs.DrawBounds = results["Draw Matrix Bounds"];
                            Preferences.MatrixPrefs.DrawOrigin = results["Draw Matrix Origin"];
                            Preferences.MatrixPrefs.DrawRotation = results[
                                "Draw Matrix Rotation Axis"
                            ];
                            Preferences.MatrixPrefs.DrawDirections = results[
                                "Draw Matrix Direction Vectors"
                            ];
                        }
                    );

                // << NODE GIZMOS >>
                Preferences.GroupPrefs.DrawNodeGizmos = CustomInspectorGUI.DrawTogglePropertyGroup(
                    "Draw Node Gizmos",
                    Preferences.GroupPrefs.DrawNodeGizmos,
                    () =>
                    {
                        // Add filter dropdown
                        Preferences.NodePrefs.Filter = (NodeFilter)
                            EditorGUILayout.EnumPopup("Node Filter", Preferences.NodePrefs.Filter);

                        Preferences.NodePrefs.labelContent = (Node.GUI.LabelContent)
                            EditorGUILayout.EnumPopup(
                                "Label Content",
                                Preferences.NodePrefs.labelContent
                            );

                        Preferences.NodePrefs.DrawButtons = CustomInspectorGUI.DrawToggleLeft(
                            "Draw Node Buttons",
                            Preferences.NodePrefs.DrawButtons,
                            "Draw buttons on nodes"
                        );

                        // Add new toggle for dimensions
                        Preferences.NodePrefs.DrawDimensions = CustomInspectorGUI.DrawToggleLeft(
                            "Draw Node Dimensions",
                            Preferences.NodePrefs.DrawDimensions,
                            "Draw wireframe showing node dimensions"
                        );
                    }
                );

                // << PARTITION GIZMOS >>
                Preferences.GroupPrefs.DrawPartitionGizmos =
                    CustomInspectorGUI.DrawTogglePropertyGroup(
                        "Draw Partition Gizmos",
                        Preferences.GroupPrefs.DrawPartitionGizmos,
                        () =>
                        {
                            var results = CustomInspectorGUI.DrawToggleGroup(
                                new Dictionary<string, (bool, string)>
                                {
                                    {
                                        "Draw Partition Bounds",
                                        (
                                            Preferences.PartitionPrefs.DrawBounds,
                                            "Display the bounding box of each partition"
                                        )
                                    },
                                    {
                                        "Draw Partition Centers",
                                        (
                                            Preferences.PartitionPrefs.DrawCenters,
                                            "Display the center point of each partition"
                                        )
                                    },
                                    {
                                        "Draw Partition Labels",
                                        (
                                            Preferences.PartitionPrefs.DrawLabels,
                                            "Display partition key labels"
                                        )
                                    }
                                }
                            );

                            Preferences.PartitionPrefs.DrawBounds = results[
                                "Draw Partition Bounds"
                            ];
                            Preferences.PartitionPrefs.DrawCenters = results[
                                "Draw Partition Centers"
                            ];
                            Preferences.PartitionPrefs.DrawLabels = results[
                                "Draw Partition Labels"
                            ];
                        }
                    );

                EditorGUILayout.Space();
            }

            static void OnGUI_DrawSelectedNode(ref Node selectedNode)
            {
                CustomInspectorGUI.DrawHeader("Selected Node");
                if (selectedNode == null || !selectedNode.IsValid)
                {
                    EditorGUILayout.HelpBox("No node selected", MessageType.Info);
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Focus"))
                        Node.GUI.Focus(selectedNode);

                    if (GUILayout.Button("Clear"))
                    {
                        selectedNode.IsSelected = false;
                        selectedNode = null;
                    }
                }

                Node.GUI.OnInspectorGUI(selectedNode, "Selected Node Info");
            }

            /// <summary>
            /// Draws the matrix visualization in the scene view based on toggle states
            /// </summary>
            /// <param name="matrix">The matrix to visualize</param>
            /// <param name="onClick">Callback when a node is selected</param>
            public static void DrawSceneGUI(Matrix matrix, Action<Matrix.Node> onClick)
            {
                if (!matrix)
                    return;

                if (Preferences.GroupPrefs.DrawMatrixGizmos)
                {
                    // Draw Matrix Bounds
                    if (Preferences.MatrixPrefs.DrawBounds)
                        OnSceneGUI_DrawMatrixBounds(matrix);

                    // Draw Matrix Origin
                    if (Preferences.MatrixPrefs.DrawOrigin)
                        OnSceneGUI_DrawMatrixOrigin(matrix);

                    // Draw Matrix Rotation Axis
                    if (Preferences.MatrixPrefs.DrawRotation)
                        OnSceneGUI_DrawMatrixRotationAxis(matrix);

                    // Draw Matrix Direction Vectors
                    if (Preferences.MatrixPrefs.DrawDirections)
                        OnSceneGUI_DrawMatrixDirectionVectors(matrix);
                }

                // Draw Matrix Nodes with filter
                if (Preferences.GroupPrefs.DrawNodeGizmos)
                {
                    matrix.SendVisitorToAllNodes(
                        new Node.Visitor(node =>
                        {
                            bool shouldDraw = false;

                            switch (Preferences.NodePrefs.Filter)
                            {
                                case NodeFilter.NONE:
                                    shouldDraw = false;
                                    break;
                                case NodeFilter.SELECTED:
                                    shouldDraw = node.IsSelected;
                                    break;
                                case NodeFilter.ENABLED:
                                    shouldDraw = node.IsEnabled;

                                    break;
                                case NodeFilter.DISABLED:
                                    shouldDraw = !node.IsEnabled;
                                    break;
                            }

                            if (shouldDraw)
                            {
                                Node.GUI.OnSceneGUI(node, onClick);
                            }
                            return true;
                        })
                    );
                }

                // Draw Matrix Partitions
                if (Preferences.GroupPrefs.DrawPartitionGizmos)
                {
                    var partitions = matrix.GetMap().GetAllPartitions();
                    foreach (var partition in partitions)
                    {
                        Partition.GUI.DrawSceneGUI(matrix, partition);
                    }
                }
            }

            static void OnSceneGUI_DrawMatrixBounds(Matrix matrix)
            {
                Handles.DrawWireCube(
                    matrix.GetInfo().CenterPosition,
                    Matrix.Utility.SwizzleVec2(
                        matrix.GetInfo().Dimensions,
                        matrix.GetInfo().Swizzle
                    )
                );
            }

            static void OnSceneGUI_DrawMatrixOrigin(Matrix matrix)
            {
                Vector3 position = matrix.GetInfo().CenterPosition;

                // Draw Center Label
                Handles.color = Color.white;
                Handles.Label(position + Vector3.down * 0.2f, "Origin", labelStyle);
            }

            static void OnSceneGUI_DrawMatrixDirectionVectors(Matrix matrix)
            {
                Vector3 position = matrix.GetInfo().CenterPosition;
                float size = 1f;
                Color xAxisColor = CustomGUIColors.xAxis;
                Color yAxisColor = CustomGUIColors.yAxis;
                Color zAxisColor = CustomGUIColors.zAxis;

                Vector3 right = matrix.GetInfo().RightDirection;
                Vector3 up = matrix.GetInfo().UpDirection;
                Vector3 forward = matrix.GetInfo().ForwardDirection;

                // Draw Right Direction (X)
                Handles.color = xAxisColor;
                Handles.DrawLine(position, position + right * size);
                Handles.Label(
                    position + right * (size + 0.1f),
                    "Right",
                    new GUIStyle(labelStyle) { normal = { textColor = xAxisColor } }
                );

                // Draw Up Direction (Y)
                Handles.color = yAxisColor;
                Handles.DrawLine(position, position + up * size);
                Handles.Label(
                    position + up * (size + 0.1f),
                    "Up",
                    new GUIStyle(labelStyle) { normal = { textColor = yAxisColor } }
                );

                // Draw Forward Direction (Z)
                Handles.color = zAxisColor;
                Handles.DrawLine(position, position + forward * size);
                Handles.Label(
                    position + forward * (size + 0.1f),
                    "Forward",
                    new GUIStyle(labelStyle) { normal = { textColor = zAxisColor } }
                );
            }

            static void OnSceneGUI_DrawMatrixRotationAxis(Matrix matrix)
            {
                float size = 1f;

                Vector3 position = matrix.GetInfo().CenterPosition;
                Quaternion rotation = matrix.GetInfo().Rotation;
                Vector3 right = matrix.GetInfo().RightDirection;

                // Draw Rotation Visualization
                float angle;
                Vector3 axis;
                rotation.ToAngleAxis(out angle, out axis);

                if (angle != 0 && !float.IsNaN(angle))
                {
                    Color rotationColor = Color.white;
                    Handles.color = rotationColor;

                    // Draw rotation axis
                    Handles.DrawDottedLine(position - axis * size, position + axis * size, 5f);
                    Handles.Label(position + axis * (size + 0.1f), "Rotation Axis", labelStyle);
                }
            }
        }

        public static class Preferences
        {
            private const string PREFIX = "Darklight.Matrix.GUI.";

            // Property Groups
            private static string SHOW_MATRIX_PROPERTIES = PREFIX + "ShowMatrixProperties";
            private static string SHOW_NODE_PROPERTIES = PREFIX + "ShowNodeProperties";
            private static string SHOW_PARTITION_PROPERTIES = PREFIX + "ShowPartitionProperties";
            private static string SHOW_MATRIX_GIZMOS = PREFIX + "ShowMatrixGizmos";
            private static string SHOW_NODE_GIZMOS = PREFIX + "ShowNodeGizmos";
            private static string SHOW_PARTITION_GIZMOS = PREFIX + "ShowPartitionGizmos";

            // Matrix Gizmos
            private static string DRAW_MATRIX_BOUNDS = PREFIX + "DrawMatrixBounds";
            private static string DRAW_MATRIX_ORIGIN = PREFIX + "DrawMatrixOrigin";
            private static string DRAW_MATRIX_ROTATION = PREFIX + "DrawMatrixRotation";
            private static string DRAW_MATRIX_DIRECTIONS = PREFIX + "DrawMatrixDirections";

            // Node Gizmos
            private static string NODE_LABEL_CONTENT = PREFIX + "NodeLabelContent";
            private static string NODE_DRAW_BUTTONS = PREFIX + "NodeDrawButtons";
            private static string NODE_DRAW_DIMENSIONS = PREFIX + "NodeDrawDimensions";
            private static string NODE_FILTER = PREFIX + "NodeFilter";

            // Partition Gizmos
            private static string PARTITION_DRAW_BOUNDS = PREFIX + "PartitionDrawBounds";
            private static string PARTITION_DRAW_CENTERS = PREFIX + "PartitionDrawCenters";
            private static string PARTITION_DRAW_LABELS = PREFIX + "PartitionDrawLabels";

            public static class GroupPrefs
            {
                public static bool ShowMatrixProperties
                {
                    get => EditorPrefs.GetBool(SHOW_MATRIX_PROPERTIES, false);
                    set => EditorPrefs.SetBool(SHOW_MATRIX_PROPERTIES, value);
                }

                public static bool ShowNodeProperties
                {
                    get => EditorPrefs.GetBool(SHOW_NODE_PROPERTIES, false);
                    set => EditorPrefs.SetBool(SHOW_NODE_PROPERTIES, value);
                }

                public static bool ShowPartitionProperties
                {
                    get => EditorPrefs.GetBool(SHOW_PARTITION_PROPERTIES, false);
                    set => EditorPrefs.SetBool(SHOW_PARTITION_PROPERTIES, value);
                }

                public static bool DrawMatrixGizmos
                {
                    get => EditorPrefs.GetBool(SHOW_MATRIX_GIZMOS, false);
                    set => EditorPrefs.SetBool(SHOW_MATRIX_GIZMOS, value);
                }

                public static bool DrawNodeGizmos
                {
                    get => EditorPrefs.GetBool(SHOW_NODE_GIZMOS, false);
                    set => EditorPrefs.SetBool(SHOW_NODE_GIZMOS, value);
                }

                public static bool DrawPartitionGizmos
                {
                    get => EditorPrefs.GetBool(SHOW_PARTITION_GIZMOS, false);
                    set => EditorPrefs.SetBool(SHOW_PARTITION_GIZMOS, value);
                }
            }

            public static class MatrixPrefs
            {
                public static bool DrawBounds
                {
                    get => EditorPrefs.GetBool(DRAW_MATRIX_BOUNDS, false);
                    set => EditorPrefs.SetBool(DRAW_MATRIX_BOUNDS, value);
                }

                public static bool DrawOrigin
                {
                    get => EditorPrefs.GetBool(DRAW_MATRIX_ORIGIN, false);
                    set => EditorPrefs.SetBool(DRAW_MATRIX_ORIGIN, value);
                }

                public static bool DrawRotation
                {
                    get => EditorPrefs.GetBool(DRAW_MATRIX_ROTATION, false);
                    set => EditorPrefs.SetBool(DRAW_MATRIX_ROTATION, value);
                }

                public static bool DrawDirections
                {
                    get => EditorPrefs.GetBool(DRAW_MATRIX_DIRECTIONS, false);
                    set => EditorPrefs.SetBool(DRAW_MATRIX_DIRECTIONS, value);
                }
            }

            public static class NodePrefs
            {
                public static Node.GUI.LabelContent labelContent
                {
                    get => (Node.GUI.LabelContent)EditorPrefs.GetInt(NODE_LABEL_CONTENT, 0);
                    set => EditorPrefs.SetInt(NODE_LABEL_CONTENT, (int)value);
                }

                public static bool DrawButtons
                {
                    get => EditorPrefs.GetBool(NODE_DRAW_BUTTONS, false);
                    set => EditorPrefs.SetBool(NODE_DRAW_BUTTONS, value);
                }

                public static bool DrawDimensions
                {
                    get => EditorPrefs.GetBool(NODE_DRAW_DIMENSIONS, false);
                    set => EditorPrefs.SetBool(NODE_DRAW_DIMENSIONS, value);
                }

                public static GUI.NodeFilter Filter
                {
                    get => (GUI.NodeFilter)EditorPrefs.GetInt(NODE_FILTER, 0);
                    set => EditorPrefs.SetInt(NODE_FILTER, (int)value);
                }
            }

            public static class PartitionPrefs
            {
                public static bool DrawBounds
                {
                    get => EditorPrefs.GetBool(PARTITION_DRAW_BOUNDS, false);
                    set => EditorPrefs.SetBool(PARTITION_DRAW_BOUNDS, value);
                }

                public static bool DrawCenters
                {
                    get => EditorPrefs.GetBool(PARTITION_DRAW_CENTERS, false);
                    set => EditorPrefs.SetBool(PARTITION_DRAW_CENTERS, value);
                }

                public static bool DrawLabels
                {
                    get => EditorPrefs.GetBool(PARTITION_DRAW_LABELS, false);
                    set => EditorPrefs.SetBool(PARTITION_DRAW_LABELS, value);
                }
            }

            public static void ResetToDefaults()
            {
                // Reset Groups
                GroupPrefs.ShowMatrixProperties = false;
                GroupPrefs.ShowNodeProperties = false;
                GroupPrefs.ShowPartitionProperties = false;
                GroupPrefs.DrawMatrixGizmos = true;
                GroupPrefs.DrawNodeGizmos = false;
                GroupPrefs.DrawPartitionGizmos = false;

                // Reset Matrix Gizmos
                MatrixPrefs.DrawBounds = true;
                MatrixPrefs.DrawOrigin = true;
                MatrixPrefs.DrawRotation = false;
                MatrixPrefs.DrawDirections = false;

                // Reset Node Gizmos
                NodePrefs.labelContent = Node.GUI.LabelContent.KEY;
                NodePrefs.DrawButtons = false;
                NodePrefs.DrawDimensions = false;
                NodePrefs.Filter = GUI.NodeFilter.NONE;

                // Reset Partition Gizmos
                PartitionPrefs.DrawBounds = true;
                PartitionPrefs.DrawCenters = false;
                PartitionPrefs.DrawLabels = true;
            }
        }
    }
}
#endif
