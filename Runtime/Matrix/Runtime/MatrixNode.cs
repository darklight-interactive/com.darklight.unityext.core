using System;
using Codice.Client.Common.TreeGrouper;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        [System.Serializable]
        public class Node : IVisitable<Node>
        {
            Matrix _matrix;
            Vector2Int _key;

            public Vector2Int Key => _key;
            public Vector2Int Coordinate =>
                IsValid ? ConvertKeyToCoordinate(_matrix._info, Key) : Vector2Int.zero;
            public Vector3Int Coordinate_Vec3 =>
                IsValid
                    ? Utility.SwizzleVec2Int(Coordinate, _matrix._info.Swizzle)
                    : Vector3Int.zero;
            public Vector3 Center => IsValid ? CalculatePosition(_matrix._info, Key) : Vector3.zero;
            public Vector3 NormalDir => IsValid ? _matrix._info.UpDirection : Vector3.up;
            public Vector2 Size => IsValid ? _matrix._info.NodeSize : Vector2.zero;
            public float AvgSize => IsValid ? _matrix._info.NodeAvgSize : 0f;
            public int PartitionKey => IsValid ? CalculatePartitionKey(_matrix._info, Key) : -1;
            public bool IsValid
            {
                get => _matrix != null && _key.x != -1 && _key.y != -1;
            }
            public bool IsEnabled { get; set; } = false;
            public bool IsSelected { get; set; } = false;
            public Color DebugColor
            {
                get
                {
                    if (!IsValid)
                        return CustomGUIColors.black.WithAlpha(0.35f);
                    if (IsSelected)
                        return CustomGUIColors.selected;
                    if (!IsEnabled)
                        return CustomGUIColors.disabled;
                    return Color.white;
                }
            }

            // ======== [[ CONSTRUCTOR ]] ======================================================= >>>>
            public Node(Matrix matrix, Vector2Int key)
            {
                _matrix = matrix;
                _key = key;
            }

            // (( INTERFACE )) : IVisitable -------- ))
            public void AcceptVisitor(IVisitor<Node> visitor)
            {
                visitor.Visit(this);
            }

            public bool IsEqual(Node other)
            {
                return _matrix == other._matrix && _key == other._key;
            }

            public static void ConvertKeyToCoordinate(
                Info info,
                Vector2Int key,
                out Vector2Int coordinate
            )
            {
                coordinate = key - info.OriginKey;
            }

            public static Vector2Int ConvertKeyToCoordinate(Info info, Vector2Int key)
            {
                Vector2Int coordinate = key - info.OriginKey;
                return coordinate;
            }

            public static void ConvertCoordinateToKey(
                Info info,
                Vector2Int coordinate,
                out Vector2Int key
            )
            {
                key = coordinate + info.OriginKey;
            }

            public static Vector2Int ConvertCoordinateToKey(Info info, Vector2Int coordinate)
            {
                Vector2Int key = coordinate + info.OriginKey;
                return key;
            }

            public static Vector3 CalculatePosition(Info info, Vector2Int key)
            {
                Vector3 position = Vector3.zero;
                if (info.Grid != null)
                {
                    ConvertKeyToCoordinate(info, key, out Vector2Int coordinate);
                    position = info.Grid.CellToWorld(new Vector3Int(coordinate.x, coordinate.y, 0));
                    Vector2 offset = new Vector2(info.NodeHalfSize.x, -info.NodeHalfSize.y);
                    position += Utility.SwizzleVec2(offset, info.Swizzle);
                }
                else
                {
                    // Calculate the node position offset in world space based on dimensions
                    Vector2 keyOffsetPos = key * info.NodeSize;

                    // Calculate the spacing offset and clamp to avoid overlapping cells
                    Vector2 spacingOffsetPos = info.NodeSpacing + Vector2.one;
                    spacingOffsetPos.x = Mathf.Max(spacingOffsetPos.x, 0.5f);
                    spacingOffsetPos.y = Mathf.Max(spacingOffsetPos.y, 0.5f);

                    // Calculate bonding offsets
                    Vector2 bondingOffset = Vector2.zero;
                    if (key.y % 2 == 0)
                        bondingOffset.x = info.NodeBonding.x;
                    if (key.x % 2 == 0)
                        bondingOffset.y = info.NodeBonding.y;

                    // Combine offsets and apply spacing
                    Vector2 localPosition2D = keyOffsetPos;
                    localPosition2D *= spacingOffsetPos;
                    localPosition2D += bondingOffset;
                    localPosition2D += Utility.CalculateAlignmentOffset(
                        info.OriginAlignment,
                        info.Dimensions - info.NodeSize
                    ); // offset from the origin

                    // Convert the 2D local position to 3D and apply matrix rotation
                    Vector3 localPosition = new Vector3(localPosition2D.x, 0, localPosition2D.y);

                    // Apply the matrix rotation
                    Quaternion matrixRotation = info.Rotation;
                    Vector3 rotatedPosition = matrixRotation * localPosition;

                    // Final world position by adding rotated local position to MatrixPosition
                    position =
                        (info.Parent != null ? info.Parent.position : Vector3.zero)
                        + rotatedPosition;
                }
                return position;
            }

            static Vector3 CalculateNormalDirection(Info info)
            {
                return info.UpDirection;
            }

            static int CalculatePartitionKey(Info info, Vector2Int key)
            {
                int partitionX = Mathf.FloorToInt(key.x / (float)info.PartitionSize);
                int partitionY = Mathf.FloorToInt(key.y / (float)info.PartitionSize);

                // Using a more robust hash function for partition key
                // This handles negative coordinates better
                const int PRIME = 31;
                int hash = 17;
                hash = hash * PRIME + partitionX;
                hash = hash * PRIME + partitionY;
                return hash;
            }

            /// <summary>
            /// Calculates the pivot point for a node based on its alignment and inset.
            /// /// </summary>
            /// <param name="node">The node to calculate the pivot for.</param>
            /// <param name="alignment">The alignment of the node.</param>
            /// <param name="inset">The inset of the node.</param>
            /// <returns>The pivot point of the node.</returns>
            public static Vector3 CalculatePivot(Node node, Alignment alignment, float inset = 0f)
            {
                // Clamp inset to valid range
                inset = Mathf.Clamp(inset, 0, 0.5f);

                // Calculate base offset
                Vector2 offset = Utility.CalculateAlignmentOffset(alignment, node.Size);
                offset += node.Size / 2;

                // Apply inset based on alignment
                Vector2 insetOffset = Vector2.zero;
                switch (alignment)
                {
                    case Alignment.TopLeft:
                        insetOffset = new Vector2(inset, inset);
                        break;
                    case Alignment.TopCenter:
                        insetOffset = new Vector2(0, inset);
                        break;
                    case Alignment.TopRight:
                        insetOffset = new Vector2(inset, inset);
                        break;
                    case Alignment.MiddleLeft:
                        insetOffset = new Vector2(inset, 0);

                        break;
                    case Alignment.MiddleCenter:
                        break;
                    case Alignment.MiddleRight:
                        insetOffset = new Vector2(inset, 0);
                        break;
                    case Alignment.BottomLeft:
                        insetOffset = new Vector2(inset, inset);

                        break;
                    case Alignment.BottomCenter:
                        insetOffset = new Vector2(0, inset);
                        break;
                    case Alignment.BottomRight:
                        insetOffset = new Vector2(inset, inset);
                        break;
                }

                offset += insetOffset * node.Size;

                Vector3 pivot =
                    node.Center - Utility.SwizzleVec2(offset, node._matrix._info.Swizzle);
                return pivot;
            }

            #region < PUBLIC_CLASS > [[ Visitor ]] ================================================================


            public class Visitor : IVisitor<Node>
            {
                VisitNodeEvent _visitFunction;

                public Visitor(VisitNodeEvent visitFunction)
                {
                    _visitFunction = visitFunction;
                }

                public virtual void Visit(Node cell)
                {
                    _visitFunction(cell);
                }
            }
            #endregion

#if UNITY_EDITOR
            public static class GUI
            {
                public enum LabelContent
                {
                    NONE,
                    KEY,
                    COORDINATE,
                    POSITION,
                    DIMENSIONS,
                    PARTITION_KEY
                }

                static bool _isExpanded = false;

                public static void OnInspectorGUI(Node node, string header = "Node Info")
                {
                    if (node == null)
                        return;

                    _isExpanded = CustomInspectorGUI.DrawFoldoutPropertyGroup(
                        header,
                        _isExpanded,
                        () =>
                        {
                            using (new EditorGUI.DisabledGroupScope(true))
                            {
                                EditorGUILayout.LabelField("Key", node.Key.ToString());
                                EditorGUILayout.LabelField(
                                    "Coordinate",
                                    node.Coordinate.ToString()
                                );
                                EditorGUILayout.LabelField("Position", node.Center.ToString());
                                EditorGUILayout.LabelField("Dimensions", node.Size.ToString());
                                EditorGUILayout.LabelField(
                                    "Partition Key",
                                    node.PartitionKey.ToString()
                                );
                                EditorGUILayout.LabelField("IsValid", node.IsValid.ToString());
                                EditorGUILayout.LabelField("IsEnabled", node.IsEnabled.ToString());
                            }
                        }
                    );
                }

                public static void OnSceneGUI(Node node, Action<Node> onClick)
                {
                    if (node == null || !node.IsValid)
                        return;

                    DrawNodeLabel(node);

                    if (Preferences.NodePrefs.DrawDimensions)
                    {
                        DrawNodeDimensions(node);
                    }

                    if (Preferences.NodePrefs.DrawButtons)
                        DrawNodeButton(node, () => onClick?.Invoke(node));
                }

                public static void Focus(Node node)
                {
                    if (SceneView.lastActiveSceneView != null)
                    {
                        SceneView.lastActiveSceneView.LookAt(
                            node.Center,
                            SceneView.lastActiveSceneView.rotation,
                            node.AvgSize
                        );
                    }
                }

                static void DrawNodeDimensions(Node node)
                {
                    if (node == null || !node.IsValid || node._matrix == null)
                        return;

                    Vector3 center = node.Center;
                    Vector2 size = node.Size;
                    Vector3 normalDir = node.NormalDir;

                    if (size == Vector2.zero)
                        return;

                    // Draw wireframe rectangle using CustomGizmos
                    CustomGizmos.DrawWireRect(center, size, normalDir, node.DebugColor);
                }

                static void DrawNodeButton(Node node, Action onClick)
                {
                    if (node == null || !node.IsValid)
                        return;

                    CustomGizmos.DrawButtonHandle(
                        node.Center,
                        node.AvgSize / 2,
                        node.NormalDir,
                        node.DebugColor,
                        onClick,
                        Handles.CubeHandleCap
                    );
                }

                static void DrawNodeLabel(Node node)
                {
                    if (
                        node == null
                        || !node.IsValid
                        || Preferences.NodePrefs.labelContent == LabelContent.NONE
                    )
                        return;

                    string label = "";
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
                    {
                        wordWrap = true,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 8,
                        normal = { textColor = Color.white }
                    };

                    switch (Preferences.NodePrefs.labelContent)
                    {
                        case LabelContent.KEY:
                            label = node.Key.ToString();
                            break;
                        case LabelContent.COORDINATE:
                            label = node.Coordinate.ToString();
                            break;
                        case LabelContent.POSITION:
                            label = node.Center.ToString();
                            break;
                        case LabelContent.DIMENSIONS:
                            label = node.Size.ToString();
                            break;
                        case LabelContent.PARTITION_KEY:
                            label = node.PartitionKey.ToString();
                            break;
                    }

                    Handles.Label(node.Center, label, labelStyle);
                }
            }
#endif
        }
    }
}
