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

        [Serializable]
        public struct Info
        {
            #region < PRIVATE_CONST > ================================================================
            const float MIN_NODE_DIMENSION = 0.125f;
            const float MAX_NODE_DIMENSION = 10f;
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
            Transform _parent;
            Grid _grid;

            [SerializeField]
            Vector2Int _bounds;

            [SerializeField]
            Alignment _alignment;

            [SerializeField, Range(5, 100)]
            int _partitionSize;

            [HorizontalLine]
            [SerializeField, DisableIf("HasGrid"), AllowNesting]
            private GridLayout.CellSwizzle _swizzle;

            [SerializeField, DisableIf("HasGrid"), AllowNesting]
            private Vector2 _nodeSize;

            [SerializeField, DisableIf("HasGrid"), AllowNesting]
            private Vector2 _nodeSpacing;

            [SerializeField, HideIf("HasGrid")]
            private Vector2 _nodeBonding;

            #region < PUBLIC_PROPERTIES > ================================================================
            public Grid Grid
            {
                get => _grid;
                set
                {
                    _grid = value;
                    if (_grid != null)
                    {
                        _parent = value.transform;
                        _swizzle = value.cellSwizzle;
                    }
                }
            }

            public Transform Parent
            {
                get => _parent;
                set => _parent = value;
            }

            public Vector3 OriginWorldPosition => _parent != null ? _parent.position : Vector3.zero;
            public Alignment OriginAlignment
            {
                get => _alignment;
                set => _alignment = value;
            }
            public int PartitionSize => _partitionSize;
            public Vector2Int Bounds
            {
                get => _bounds;
                set => _bounds = value;
            }
            public int ColumnCount => _bounds.x;
            public int RowCount => _bounds.y;
            public int Capacity => _bounds.x * _bounds.y;
            public Vector2 Dimensions => CalculateMatrixDimensions(this);
            public Vector2Int TerminalKey => new Vector2Int(_bounds.x - 1, _bounds.y - 1);
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

            public Vector3 Center => CalculateMatrixCenter(this);
            public Quaternion Rotation => CalculateMatrixRotation(_swizzle);
            public Vector3 RightDirection => Rotation * Vector3.right;
            public Vector3 UpDirection => Rotation * Vector3.up;
            public Vector3 ForwardDirection => Rotation * Vector3.forward;

            public Vector2 NodeSize => _nodeSize;
            public Vector2 NodeHalfSize => _nodeSize / 2;
            public float NodeAvgSize => (_nodeSize.x + _nodeSize.y) / 2;

            public Vector2 NodeSpacing => _nodeSpacing;
            public Vector2 NodeBonding => _nodeBonding;

            public GridLayout.CellSwizzle Swizzle => _swizzle;

            public bool HasParent => _parent != null;
            public bool HasGrid => _grid != null;
            public bool IsValid => _matrix != null;
            #endregion

            public Info(Matrix matrix)
            {
                _matrix = matrix;
                _parent = matrix.transform;
                _grid = matrix.GetGrid();

                _bounds = Vector2Int.one * 5;

                _alignment = Matrix.Alignment.MiddleCenter;
                _partitionSize = DEFAULT_PARTITION_SIZE;
                _swizzle = GridLayout.CellSwizzle.XYZ;

                _nodeSize = new Vector2(DEFAULT_NODE_SIZE, DEFAULT_NODE_SIZE);
                _nodeSpacing = new Vector2(DEFAULT_NODE_SPACING, DEFAULT_NODE_SPACING);
                _nodeBonding = new Vector2(DEFAULT_NODE_BONDING, DEFAULT_NODE_BONDING);

                Validate();
            }

            public void Validate()
            {
                // << CALCULATE TRANSFORM VALUES >>
                if (_grid != null)
                {
                    _parent = _grid.transform;
                    _swizzle = _grid.cellSwizzle;
                    _nodeSize = _grid.cellSize;
                    _nodeSpacing = _grid.cellGap;
                }

                // << CLAMP VALUES >>
                int clampedX = Mathf.Max(1, _bounds.x);
                int clampedY = Mathf.Max(1, _bounds.y);
                _bounds = new Vector2Int(clampedX, clampedY);
                _partitionSize = Mathf.Max(_partitionSize, 3);

                _nodeSize = Utility.ClampVector2(_nodeSize, MIN_NODE_DIMENSION, MAX_NODE_DIMENSION);
                _nodeSpacing = Utility.ClampVector2(
                    _nodeSpacing,
                    MIN_NODE_SPACING,
                    MAX_NODE_SPACING
                );
                _nodeBonding = Utility.ClampVector2(
                    _nodeBonding,
                    MIN_NODE_BONDING,
                    MAX_NODE_BONDING
                );
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

                if (info.HasGrid)
                {
                    centerPos2D.x += info.NodeHalfSize.x;
                    centerPos2D.y -= info.NodeHalfSize.y;
                }

                // Convert to 3D with proper swizzle
                Vector3 centerPos3D = Utility.SwizzleVec2(centerPos2D, info.Swizzle);
                return info.OriginWorldPosition + centerPos3D;
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
        }
    }
}
