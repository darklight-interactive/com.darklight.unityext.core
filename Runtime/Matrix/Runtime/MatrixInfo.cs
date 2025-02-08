using System;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.World;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        [Serializable]
        public class MatrixInfo
        {
            #region < PRIVATE_CONST > ================================================================
            const int MIN_MAP_KEY = 1;
            const int MAX_MAP_KEY = 25;
            const float MIN_NODE_DIMENSION = 0.125f;
            const float MAX_NODE_DIMENSION = 10f;
            const float MIN_NODE_SPACING = -0.5f;
            const float MAX_NODE_SPACING = 0.5f;
            const float MIN_NODE_BONDING = -1f;
            const float MAX_NODE_BONDING = 1f;

            const int DEFAULT_MAP_KEY = 3;
            const float DEFAULT_NODE_DIMENSION = 1f;
            const float DEFAULT_NODE_SPACING = 0f;
            const float DEFAULT_NODE_BONDING = 0f;
            #endregion

            ///
            [SerializeField, ReadOnly]
            Grid _grid;

            [SerializeField, HideIf("HasGrid"), AllowNesting]
            Transform _parent;

            [SerializeField]
            Vector2Int _bounds;

            [SerializeField]
            Alignment _alignment;

            [SerializeField, Range(5, 100)]
            int _partitionSize;

            [HorizontalLine]
            [Space(10), Header("Node Values")]
            [SerializeField]
            private bool _centerNodes;

            [SerializeField, DisableIf("HasGrid"), AllowNesting]
            private GridLayout.CellSwizzle _swizzle = GridLayout.CellSwizzle.XZY;

            [SerializeField, DisableIf("HasGrid"), AllowNesting]
            private Vector2 _nodeSize;

            [SerializeField, DisableIf("HasGrid"), AllowNesting]
            private Vector2 _nodeSpacing;

            [SerializeField]
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
            public bool HasGrid => _grid != null;

            public Transform Parent
            {
                get => _parent;
                set => _parent = value;
            }
            public bool HasParent => _parent != null;

            public Alignment OriginAlignment => _alignment;
            public int PartitionSize => _partitionSize;
            public Vector2Int Bounds => _bounds;
            public int ColumnCount => _bounds.x;
            public int RowCount => _bounds.y;
            public int Capacity => _bounds.x * _bounds.y;
            public Vector2 Dimensions => CalculateMatrixDimensions();
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
            public Vector3 Center => CalculateMatrixCenter();
            public Quaternion Rotation
            {
                get
                {
                    Quaternion swizzleRot = Matrix.CalculateSwizzleRotationOffset(_swizzle);
                    if (HasParent)
                    {
                        return swizzleRot * _parent.rotation;
                    }
                    else
                    {
                        return swizzleRot;
                    }
                }
            }

            public bool CenterNodes => _centerNodes;
            public Vector2 NodeSize => _nodeSize;
            public Vector2 NodeSpacing => _nodeSpacing;
            public Vector2 NodeBonding => _nodeBonding;

            public GridLayout.CellSwizzle Swizzle => _swizzle;

            #endregion

            // ======== [[ CONSTRUCTOR ]] ======================================================= >>>>
            public MatrixInfo(Transform parent = null)
            {
                _grid = null;
                _parent = parent;
                _bounds = Vector2Int.one;

                _alignment = Matrix.Alignment.MiddleCenter;
                _partitionSize = DEFAULT_MAP_KEY;

                _centerNodes = true;
                _nodeSize = new Vector2(DEFAULT_NODE_DIMENSION, DEFAULT_NODE_DIMENSION);
                _nodeSpacing = new Vector2(DEFAULT_NODE_SPACING, DEFAULT_NODE_SPACING);
                _nodeBonding = new Vector2(DEFAULT_NODE_BONDING, DEFAULT_NODE_BONDING);

                Validate();
            }

            public static MatrixInfo GetMatrixInfo(Grid grid, BoundsInt bounds)
            {
                var info = new MatrixInfo
                {
                    _grid = grid,
                    _parent = grid.transform,
                    _bounds = new Vector2Int(bounds.size.x, bounds.size.y),
                    _nodeSize = grid.cellSize,
                    _nodeSpacing = grid.cellGap,
                    _nodeBonding = Vector2.zero,
                    _centerNodes = false,
                    _alignment = Matrix.Alignment.BottomLeft,
                    _swizzle = grid.cellSwizzle
                };

                info.Validate();
                return info;
            }

            public static MatrixInfo GetDefault() => new MatrixInfo();

            public void Validate()
            {
                // << CALCULATE TRANSFORM VALUES >>
                if (_grid != null)
                {
                    _parent = _grid.transform;

                    _nodeSize = _grid.cellSize;
                    _nodeSpacing = _grid.cellGap;
                    _swizzle = _grid.cellSwizzle;
                }

                // << CLAMP BOUNDS >>
                int clampedX = Mathf.Max(1, _bounds.x);
                int clampedY = Mathf.Max(1, _bounds.y);
                _bounds = new Vector2Int(clampedX, clampedY);

                // << CLAMP NODE VALUES >>
                _nodeSize = ClampVector2(_nodeSize, MIN_NODE_DIMENSION, MAX_NODE_DIMENSION);

                _nodeSpacing = ClampVector2(_nodeSpacing, MIN_NODE_SPACING, MAX_NODE_SPACING);
                _nodeBonding = ClampVector2(_nodeBonding, MIN_NODE_BONDING, MAX_NODE_BONDING);
            }

            /// <summary>
            /// Generates all `Vector2Int` keys within the bounds of the matrix, iterating from (0,0)
            /// to the specified `TerminalKey`. This method uses a single `Vector2Int` instance,
            /// updating its values to reduce memory allocations while yielding each position lazily.
            /// </summary>

            /// <returns>
            /// An `IEnumerable<Vector2Int>` sequence representing all grid keys within the bounds
            /// of (0,0) to `TerminalKey`.
            /// </returns>
            public IEnumerable<Vector2Int> GetKeys()
            {
                Vector2Int key = new Vector2Int(0, 0);

                for (int x = 0; x <= TerminalKey.x; x++)
                {
                    key.x = x;
                    for (int y = 0; y <= TerminalKey.y; y++)
                    {
                        key.y = y;
                        yield return key;
                    }
                }
            }

            public bool IsKeyInBounds(Vector2Int key)
            {
                return key.x >= 0 && key.x < TerminalKey.x && key.y >= 0 && key.y < TerminalKey.y;
            }

            /*
                    /// <summary>
                    /// Calculates the normal vector from the current MatrixRotation.
                    /// </summary>
                    public Vector3 CalculateNormal()
                    {
                        // Convert the Euler rotation to a Quaternion
                        Quaternion rotation = Quaternion.Euler(MapRotation);
    
                        // Use the rotation to transform the default "up" direction
                        Vector3 normal = rotation * Vector3.up;
    
                        return normal;
                    }
            */



            public Color GenerateColorFromPartitionKey(int partitionKey)
            {
                // MurmurHash3-inspired mixing for better distribution
                uint h = (uint)partitionKey;
                h ^= h >> 16;
                h *= 0x85ebca6b;
                h ^= h >> 13;
                h *= 0xc2b2ae35;
                h ^= h >> 16;

                // Use different prime multipliers for each component
                float hue = (h * 0xcc9e2d51) % 360f / 360f;
                float saturation = 0.6f + (((h * 0x1b873593) % 40f) / 100f); // Range 0.6-1.0
                float value = 0.8f + (((h * 0xe6546b64) % 20f) / 100f); // Range 0.8-1.0

                return Color.HSVToRGB(hue, saturation, value);
            }

            /// <summary>
            /// Gets the dimensions of the matrix in world space.
            /// </summary>
            /// <returns>The full dimensions of the matrix in world units.</returns>
            public Vector3 CalculateMatrixDimensions()
            {
                Vector2 dimensions2D = new Vector2(
                    _bounds.x * _nodeSize.x,
                    _bounds.y * _nodeSize.y
                );

                dimensions2D *= (_nodeSpacing + Vector2.one);
                return SwizzleVec2(dimensions2D, _swizzle);
            }

            /// <summary>
            /// Calculates the center position of the matrix in world space using matrix data.
            /// </summary>
            /// <returns>The center position of the matrix in world coordinates.</returns>
            public Vector3 CalculateMatrixCenter()
            {
                if (_grid != null)
                {
                    // Use grid's cell-to-world conversion for center calculation
                    Vector2Int centerCoord = new Vector2Int(_bounds.x / 2, _bounds.y / 2);
                    Vector3 gridCenter = _grid.CellToWorld(
                        new Vector3Int(centerCoord.x, centerCoord.y, 0)
                    );

                    Vector3 originOffset = CalculateLocalAlignmentOffset(_alignment, Dimensions);

                    return gridCenter + originOffset;
                }

                // Calculate the base position in 2D space
                Vector2 centerPos2D = Vector2.zero;
                Vector2 nodeHalfSize = _nodeSize * 0.5f;
                if (_nodeSize.x < 1)
                    nodeHalfSize.x = _nodeSize.x * 2;
                if (_nodeSize.y < 1)
                    nodeHalfSize.y = _nodeSize.y * 2;

                // Calculate dimensions and half sizes
                Vector2 fullDimensions =
                    new Vector2(_bounds.x * _nodeSize.x, _bounds.y * _nodeSize.y)
                    * (_nodeSpacing + Vector2.one);

                // Add half of total dimensions for center point
                centerPos2D = fullDimensions * 0.5f;

                // Apply spacing
                centerPos2D *= (_nodeSpacing + Vector2.one);

                // Apply alignment offset
                Vector2 alignmentOffset = CalculateWorldAlignmentOffset(
                    _alignment,
                    Dimensions,
                    _swizzle
                );
                centerPos2D += alignmentOffset;

                // Convert to 3D with proper swizzle
                Vector3 centerPos3D = SwizzleVec2(centerPos2D, _swizzle);

                // Get base world position and rotation
                Vector3 worldPos = _parent != null ? _parent.position : Vector3.zero;
                Quaternion worldRot =
                    _parent != null
                        ? _parent.rotation * CalculateSwizzleRotationOffset(_swizzle)
                        : CalculateSwizzleRotationOffset(_swizzle);

                return worldPos + (worldRot * centerPos3D);
            }
        }
    }
}
