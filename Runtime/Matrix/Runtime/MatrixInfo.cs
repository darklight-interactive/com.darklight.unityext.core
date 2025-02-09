using System;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        public enum Alignment
        {
            TopLeft,
            TopCenter,
            TopRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            BottomLeft,
            BottomCenter,
            BottomRight
        }

        public struct Info
        {
            #region < PRIVATE_CONST > ================================================================
            const float MIN_NODE_SPACING = -0.5f;
            const float MAX_NODE_SPACING = 0.5f;
            const float MIN_NODE_BONDING = -1f;
            const float MAX_NODE_BONDING = 1f;

            const int DEFAULT_PARTITION_SIZE = 5;
            const float DEFAULT_NODE_SIZE = 1f;
            const float DEFAULT_NODE_SPACING = 0f;
            const float DEFAULT_NODE_BONDING = 0f;
            #endregion

            Matrix _matrix;

            #region < PUBLIC_PROPERTIES > ================================================================

            public Transform Parent { get; set; }
            public Grid Grid { get; set; }
            public Alignment OriginAlignment { get; set; }
            public Vector2Int Bounds { get; set; }
            public Vector2 NodeSize { get; set; }
            public Vector2 NodeSpacing { get; set; }
            public Vector2 NodeBonding { get; set; }
            public GridLayout.CellSwizzle Swizzle { get; set; }

            public int PartitionSize { get; set; }

            public int ColumnCount => Bounds.x;
            public int RowCount => Bounds.y;
            public int Capacity => Bounds.x * Bounds.y;
            public Vector2 Dimensions => CalculateMatrixDimensions(this);
            public Vector2Int TerminalKey => new Vector2Int(Bounds.x - 1, Bounds.y - 1);
            public Vector2Int OriginKey
            {
                get
                {
                    Vector2Int maxIndices = TerminalKey;
                    return OriginKeys.TryGetValue(OriginAlignment, out var originFunc)
                        ? originFunc(maxIndices)
                        : Vector2Int.zero;
                }
            }

            public Vector3 OriginPosition => Parent != null ? Parent.position : Vector3.zero;
            public Vector3 CenterPosition => CalculateMatrixCenter(this);
            public Quaternion Rotation => CalculateMatrixRotation(Swizzle);
            public Vector3 RightDirection => Rotation * Vector3.right;
            public Vector3 UpDirection => Rotation * Vector3.up;
            public Vector3 ForwardDirection => Rotation * Vector3.forward;
            public Vector2 NodeHalfSize => NodeSize / 2;
            public float NodeAvgSize => (NodeSize.x + NodeSize.y) / 2;
            public bool IsValid => _matrix != null;
            #endregion

            public Info(Matrix matrix)
            {
                _matrix = matrix;
                Parent = matrix.transform;
                Grid = matrix.GetGrid();
                OriginAlignment = Alignment.BottomLeft;
                PartitionSize = DEFAULT_PARTITION_SIZE;
                Bounds = Vector2Int.one * 5;
                NodeSize = new Vector2(DEFAULT_NODE_SIZE, DEFAULT_NODE_SIZE);
                NodeSpacing = new Vector2(DEFAULT_NODE_SPACING, DEFAULT_NODE_SPACING);
                NodeBonding = new Vector2(DEFAULT_NODE_BONDING, DEFAULT_NODE_BONDING);
                Swizzle = GridLayout.CellSwizzle.XYZ;
            }

            public Info(Info info)
            {
                _matrix = info._matrix;
                Parent = info.Parent;
                Grid = info.Grid;
                OriginAlignment = info.OriginAlignment;
                Bounds = info.Bounds;
                NodeSize = info.NodeSize;
                NodeSpacing = info.NodeSpacing;
                NodeBonding = info.NodeBonding;
                Swizzle = info.Swizzle;
                PartitionSize = info.PartitionSize;
            }

            public void Validate()
            {
                if (Grid != null)
                {
                    Parent = Grid.transform;
                    OriginAlignment = Alignment.TopLeft;
                    NodeSize = Grid.cellSize;
                    NodeSpacing = Grid.cellGap;
                    Swizzle = Grid.cellSwizzle;
                }

                // << CLAMP VALUES >>
                int clampedX = Mathf.Max(1, Bounds.x);
                int clampedY = Mathf.Max(1, Bounds.y);
                Bounds = new Vector2Int(clampedX, clampedY);
                PartitionSize = Mathf.Max(PartitionSize, 5);

                NodeSpacing = Utility.ClampVector2(NodeSpacing, MIN_NODE_SPACING, MAX_NODE_SPACING);
                NodeBonding = Utility.ClampVector2(NodeBonding, MIN_NODE_BONDING, MAX_NODE_BONDING);
            }

            #region < PRIVATE_METHODS > [[ Calculations ]] ==================================================================================
            /// <summary>
            /// Calculates the center position of the matrix in world space using matrix data.
            /// </summary>
            /// <returns>The center position of the matrix in world coordinates.</returns>
            static Vector3 CalculateMatrixCenter(Info info)
            {
                // Calculate alignment offset
                Vector2 alignmentOffset = Utility.CalculateAlignmentOffset(
                    info.OriginAlignment,
                    info.Dimensions - info.NodeSize
                );
                alignmentOffset -= info.NodeHalfSize;

                Vector2 centerPos2D = info.Dimensions / 2;
                centerPos2D += alignmentOffset;

                if (info.Grid != null)
                {
                    centerPos2D.x += info.NodeHalfSize.x;
                    centerPos2D.y -= info.NodeHalfSize.y;
                }

                // Convert to 3D with proper swizzle
                Vector3 centerPos3D = Utility.SwizzleVec2(centerPos2D, info.Swizzle);
                return info.OriginPosition + centerPos3D;
            }

            /// <summary>
            /// Gets the dimensions of the matrix in world space.
            /// </summary>
            /// <returns>The full dimensions of the matrix in local units.</returns>
            static Vector2 CalculateMatrixDimensions(Info info)
            {
                Vector2 dimensions2D = new Vector2(
                    info.Bounds.x * info.NodeSize.x,
                    info.Bounds.y * info.NodeSize.y
                );

                dimensions2D *= (info.NodeSpacing + Vector2.one);
                return dimensions2D;
            }

            Quaternion CalculateMatrixRotation(GridLayout.CellSwizzle swizzle)
            {
                Vector3 up = Vector3.up;
                Vector3 forward = Vector3.forward;

                switch (swizzle)
                {
                    case GridLayout.CellSwizzle.XYZ:
                        // Default Unity 2D orientation (vertical plane)
                        up = Vector3.back;
                        forward = Vector3.up;
                        break;

                    case GridLayout.CellSwizzle.XZY:
                        // Default Unity 3D orientation (horizontal plane)
                        // up = Vector3.up;
                        // forward = Vector3.forward;
                        break;

                    case GridLayout.CellSwizzle.YXZ:
                        // Vertical plane, rotated 90° counter-clockwise around Z
                        up = Vector3.back;
                        forward = Vector3.right;
                        break;

                    case GridLayout.CellSwizzle.YZX:
                        // Vertical plane, facing right
                        up = Vector3.right;
                        forward = Vector3.forward;
                        break;

                    case GridLayout.CellSwizzle.ZXY:
                        // Horizontal plane, rotated 90° counter-clockwise around Y
                        up = Vector3.up;
                        forward = Vector3.right;
                        break;

                    case GridLayout.CellSwizzle.ZYX:
                        // Vertical plane, rotated 90° clockwise around X
                        up = Vector3.back;
                        forward = Vector3.right;
                        break;
                }

                // Create rotation from the up and forward vectors
                return Quaternion.LookRotation(forward, up);
            }
            #endregion

#if UNITY_EDITOR
            public void OnGUI_DrawValues()
            {
                EditorGUILayout.LabelField("Bounds", Bounds.ToString());
                EditorGUILayout.LabelField("Origin Alignment", OriginAlignment.ToString());
                EditorGUILayout.LabelField("Node Size", NodeSize.ToString());
                EditorGUILayout.LabelField("Node Spacing", NodeSpacing.ToString());
                EditorGUILayout.LabelField("Node Bonding", NodeBonding.ToString());
                EditorGUILayout.LabelField("Swizzle", Swizzle.ToString());
                EditorGUILayout.LabelField("Partition Size", PartitionSize.ToString());
            }
#endif
        }
    }
}
