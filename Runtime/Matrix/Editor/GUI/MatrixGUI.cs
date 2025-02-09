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

            public static void ResetToDefaults()
            {
                Preferences.ResetToDefaults();
            }

            public static void DrawGUI(Matrix matrix)
            {
                Matrix.Info info = matrix.GetInfo();
                Matrix.Map map = matrix.GetMap();

                CustomInspectorGUI.DrawButton("Reset to Defaults", ResetToDefaults);

                // << MATRIX PROPERTIES >>
                Preferences.GroupPrefs.ShowMatrixProperties =
                    CustomInspectorGUI.DrawFoldoutPropertyGroup(
                        "Matrix Properties",
                        Preferences.GroupPrefs.ShowMatrixProperties,
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
                Preferences.GroupPrefs.ShowNodeProperties =
                    CustomInspectorGUI.DrawFoldoutPropertyGroup(
                        "Node Properties",
                        Preferences.GroupPrefs.ShowNodeProperties,
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
                Preferences.GroupPrefs.ShowPartitionProperties =
                    CustomInspectorGUI.DrawFoldoutPropertyGroup(
                        "Partition Properties",
                        Preferences.GroupPrefs.ShowPartitionProperties,
                        () =>
                        {
                            EditorGUILayout.LabelField(
                                "Partition Size",
                                info.PartitionSize.ToString()
                            );

                            EditorGUILayout.LabelField(
                                "Total Partitions",
                                map.PartitionCount.ToString()
                            );
                            EditorGUILayout.LabelField("Origin Key", info.OriginKey.ToString());
                            EditorGUILayout.LabelField("Terminal Key", info.TerminalKey.ToString());
                            EditorGUILayout.LabelField(
                                "Alignment",
                                info.OriginAlignment.ToString()
                            );
                        }
                    );

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

                // Draw Matrix Nodes
                if (Preferences.GroupPrefs.DrawNodeGizmos)
                {
                    matrix.SendVisitorToAllNodes(
                        new Node.Visitor(node =>
                        {
                            Node.GUI.OnSceneGUI(node);
                            return true;
                        })
                    );
                }
            }

            static void OnSceneGUI_DrawMatrixBounds(Matrix matrix)
            {
                Handles.DrawWireCube(
                    matrix.GetInfo().Center,
                    Matrix.Utility.SwizzleVec2(matrix.GetInfo().Bounds, matrix.GetInfo().Swizzle)
                );
            }

            static void OnSceneGUI_DrawMatrixOrigin(Matrix matrix)
            {
                Vector3 position = matrix.GetInfo().Center;

                // Draw Center Label
                Handles.color = Color.white;
                Handles.Label(position + Vector3.down * 0.2f, "Origin", labelStyle);
            }

            static void OnSceneGUI_DrawMatrixDirectionVectors(Matrix matrix)
            {
                Vector3 position = matrix.GetInfo().Center;
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

                Vector3 position = matrix.GetInfo().Center;
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

            // Matrix Gizmos
            private static string DRAW_MATRIX_BOUNDS = PREFIX + "DrawMatrixBounds";
            private static string DRAW_MATRIX_ORIGIN = PREFIX + "DrawMatrixOrigin";
            private static string DRAW_MATRIX_ROTATION = PREFIX + "DrawMatrixRotation";
            private static string DRAW_MATRIX_DIRECTIONS = PREFIX + "DrawMatrixDirections";

            // Node Gizmos
            private static string NODE_LABEL_CONTENT = PREFIX + "NodeLabelContent";
            private static string NODE_DRAW_BUTTONS = PREFIX + "NodeDrawButtons";

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
            }

            public static void ResetToDefaults()
            {
                // Reset Groups
                GroupPrefs.ShowMatrixProperties = false;
                GroupPrefs.ShowNodeProperties = false;
                GroupPrefs.ShowPartitionProperties = false;
                GroupPrefs.DrawMatrixGizmos = true;
                GroupPrefs.DrawNodeGizmos = false;

                // Reset Matrix Gizmos
                MatrixPrefs.DrawBounds = true;
                MatrixPrefs.DrawOrigin = true;
                MatrixPrefs.DrawRotation = false;
                MatrixPrefs.DrawDirections = false;

                // Reset Node Gizmos
                NodePrefs.labelContent = Node.GUI.LabelContent.KEY;
                NodePrefs.DrawButtons = false;
            }
        }
    }
}
#endif
