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
        public class Info
        {
            [SerializeField]
            private Grid _grid;

            [SerializeField, Expandable]
            private WorldSpaceBounds _worldSpaceBounds;

            [SerializeField, ShowIf("IsGridNull"), AllowNesting]
            private Alignment _alignment;

            [SerializeField, ShowIf("IsGridNull"), AllowNesting]
            private Transform _parentTransform;

            [SerializeField, ShowIf("IsParentNull"), AllowNesting]
            private Vector3 _originWorldPosition;

            [SerializeField, ShowIf("IsParentNull"), AllowNesting]
            private Vector3 _originWorldRotation;

            [SerializeField, ShowIf("IsWorldSpaceBoundsNull"), AllowNesting]
            [Range(1, MAX_MAP_KEY)]
            private int _columnCount = 1;

            [SerializeField, ShowIf("IsWorldSpaceBoundsNull"), AllowNesting]
            [Range(1, MAX_MAP_KEY)]
            private int _rowCount = 1;

            [SerializeField, ShowOnly]
            private Vector2 _dimensions;

            /// <summary> The final key of the matrix, representing the number of columns and rows. </summary>
            [SerializeField, ShowOnly]
            private Vector2Int _terminalKey = Vector2Int.zero;

            [SerializeField, ShowOnly]
            private int _nodeCapacity;

            /// <summary> The origin key of the matrix, determined by the alignment of the map to the position. </summary>
            [SerializeField, ShowOnly]
            private Vector2Int _originNodeKey = Vector2Int.zero;

            [Space(10), Header("Node Values")]
            [SerializeField]
            private bool _centerNodes = true;

            [SerializeField]
            private Vector2 _nodeDimensions;

            [SerializeField]
            private Vector2 _nodeSpacing;

            [SerializeField]
            private Vector2 _nodeBonding;

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

            static readonly Dictionary<Alignment, Func<Vector2, Vector2>> AlignmentOffsets =
                new Dictionary<Alignment, Func<Vector2, Vector2>>
                {
                    { Alignment.BottomLeft, _ => Vector2.zero },
                    { Alignment.BottomCenter, dims => new Vector2(-dims.x / 2, 0) },
                    { Alignment.BottomRight, dims => new Vector2(-dims.x, 0) },
                    { Alignment.MiddleLeft, dims => new Vector2(0, -dims.y / 2) },
                    { Alignment.MiddleCenter, dims => new Vector2(-dims.x / 2, -dims.y / 2) },
                    { Alignment.MiddleRight, dims => new Vector2(-dims.x, -dims.y / 2) },
                    { Alignment.TopLeft, dims => new Vector2(0, -dims.y) },
                    { Alignment.TopCenter, dims => new Vector2(-dims.x / 2, -dims.y) },
                    { Alignment.TopRight, dims => new Vector2(-dims.x, -dims.y) },
                };

            static readonly Dictionary<Alignment, Func<Vector2Int, Vector2Int>> OriginKeys =
                new Dictionary<Alignment, Func<Vector2Int, Vector2Int>>
                {
                    { Alignment.BottomLeft, _ => new Vector2Int(0, 0) },
                    { Alignment.BottomCenter, max => new Vector2Int(max.x / 2, 0) },
                    { Alignment.BottomRight, max => new Vector2Int(max.x, 0) },
                    { Alignment.MiddleLeft, max => new Vector2Int(0, max.y / 2) },
                    { Alignment.MiddleCenter, max => new Vector2Int(max.x / 2, max.y / 2) },
                    { Alignment.MiddleRight, max => new Vector2Int(max.x, max.y / 2) },
                    { Alignment.TopLeft, max => new Vector2Int(0, max.y) },
                    { Alignment.TopCenter, max => new Vector2Int(max.x / 2, max.y) },
                    { Alignment.TopRight, max => new Vector2Int(max.x, max.y) },
                };
            #endregion

            #region < PUBLIC_PROPERTIES > ================================================================
            public Alignment Alignment => _alignment;
            public Transform ParentTransform => _parentTransform;
            public Grid Grid => _grid;

            public int ColumnCount => _columnCount;
            public int RowCount => _rowCount;

            public Vector2 Dimensions => _dimensions;
            public Vector2Int TerminalKey => _terminalKey;
            public int NodeCapacity => _nodeCapacity;
            public Vector2Int OriginNodeKey => _originNodeKey;

            public Vector2 NodeDimensions => _nodeDimensions;
            public Vector2 NodeSpacing => _nodeSpacing;
            public Vector2 NodeBonding => _nodeBonding;

            public bool IsGridNull => _grid != null;
            public bool IsParentNull => _parentTransform == null;
            public bool IsWorldSpaceBoundsNull => _worldSpaceBounds == null;

            #endregion

            // ======== [[ CONSTRUCTOR ]] ======================================================= >>>>
            public Info() => Validate();

            /// <summary>
            /// Static method to get a default instance of MapInfo with optional parameters.
            /// </summary>
            public static Info CreateDefault(Transform parent = null)
            {
                return new Info()
                {
                    _parentTransform = parent,
                    _columnCount = DEFAULT_MAP_KEY,
                    _rowCount = DEFAULT_MAP_KEY,
                    _nodeDimensions = new Vector2(DEFAULT_NODE_DIMENSION, DEFAULT_NODE_DIMENSION),
                    _nodeSpacing = new Vector2(DEFAULT_NODE_SPACING, DEFAULT_NODE_SPACING),
                    _nodeBonding = new Vector2(DEFAULT_NODE_BONDING, DEFAULT_NODE_BONDING),
                };
            }

            #region < PUBLIC_METHODS > ================================================================
            public void Validate()
            {
                if (_worldSpaceBounds != null)
                {
                    SetToWorldSpaceBounds(_worldSpaceBounds);
                }

                if (_grid != null)
                {
                    SetToGrid(_grid, new Vector2Int(_columnCount, _rowCount));
                }

                // << CLAMP INDEX VALUES >>
                _columnCount = Mathf.Clamp(_columnCount, MIN_MAP_KEY, MAX_MAP_KEY);
                _rowCount = Mathf.Clamp(_rowCount, MIN_MAP_KEY, MAX_MAP_KEY);

                // << CLAMP NODE VALUES >>
                _nodeDimensions = ClampVector2(
                    _nodeDimensions,
                    MIN_NODE_DIMENSION,
                    MAX_NODE_DIMENSION
                );
                _nodeSpacing = ClampVector2(_nodeSpacing, MIN_NODE_SPACING, MAX_NODE_SPACING);
                _nodeBonding = ClampVector2(_nodeBonding, MIN_NODE_BONDING, MAX_NODE_BONDING);

                // << CALCULATE TRANSFORM VALUES >>
                if (_parentTransform != null)
                {
                    _originWorldPosition = _parentTransform.position;
                    _originWorldRotation = _parentTransform.rotation.eulerAngles;
                }

                // << CALCULATE DERIVED KEYS >>
                _originNodeKey = CalculateMatrixOriginKey();
                _terminalKey = CalculateTerminalKey();

                // << CALCULATE DERIVED VALUES >>
                _nodeCapacity = _columnCount * _rowCount;
                _dimensions = CalculateMatrixDimensions();
            }

            public void SetToGrid(Grid grid, Vector2Int area)
            {
                _grid = grid;
                _columnCount = area.x;
                _rowCount = area.y;
                _nodeDimensions = grid.cellSize;
                _nodeSpacing = grid.cellGap;

                _alignment = Alignment.MiddleCenter;
                _parentTransform = _grid.transform;
            }

            public void SetToWorldSpaceBounds(WorldSpaceBounds bounds)
            {
                _worldSpaceBounds = bounds;
                _alignment = Alignment.MiddleCenter;
                _columnCount = (int)bounds.Width + 1;
                _rowCount = (int)bounds.Height + 1;
                _originWorldPosition = bounds.Center;
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

                for (int x = 0; x <= _terminalKey.x; x++)
                {
                    key.x = x;
                    for (int y = 0; y <= _terminalKey.y; y++)
                    {
                        key.y = y;
                        yield return key;
                    }
                }
            }

            public bool IsKeyInBounds(Vector2Int key)
            {
                return key.x >= 0 && key.x < _terminalKey.x && key.y >= 0 && key.y < _terminalKey.y;
            }

            /// <summary>
            /// Calculates the world position and rotation of a node based on its key.
            /// </summary>
            public void CalculateNodeTransformFromKey(
                Vector2Int key,
                out Vector3 position,
                out Quaternion rotation
            )
            {
                // Calculate the node position offset in world space based on dimensions
                Vector2 keyOffsetPos = key * _nodeDimensions;

                // Calculate the origin position offset in world space based on alignment
                Vector2 originOffset = CalculateMatrixAlignmentOffset();
                if (_centerNodes)
                {
                    if (_columnCount % 2 == 0)
                    {
                        originOffset.x += _nodeDimensions.x * 0.5f;
                    }
                    if (_rowCount % 2 == 0)
                    {
                        originOffset.y += _nodeDimensions.y * 0.5f;
                    }
                }

                // Calculate the spacing offset and clamp to avoid overlapping cells
                Vector2 spacingOffsetPos = _nodeSpacing + Vector2.one;
                spacingOffsetPos.x = Mathf.Max(spacingOffsetPos.x, 0.5f);
                spacingOffsetPos.y = Mathf.Max(spacingOffsetPos.y, 0.5f);

                // Calculate bonding offsets
                Vector2 bondingOffset = Vector2.zero;
                if (key.y % 2 == 0)
                    bondingOffset.x = _nodeBonding.x;
                if (key.x % 2 == 0)
                    bondingOffset.y = _nodeBonding.y;

                // Combine offsets and apply spacing
                Vector2 localPosition2D = keyOffsetPos + originOffset;
                localPosition2D *= spacingOffsetPos;
                localPosition2D += bondingOffset;

                // Convert the 2D local position to 3D and apply matrix rotation
                Vector3 localPosition = new Vector3(localPosition2D.x, 0, localPosition2D.y);
                Quaternion matrixRotation = Quaternion.Euler(_originWorldRotation);
                Vector3 rotatedPosition = matrixRotation * localPosition;

                // Final world position by adding rotated local position to MatrixPosition
                position = _originWorldPosition + rotatedPosition;

                // Apply the same rotation to each node
                rotation = matrixRotation;
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

            /// <summary>
            /// Calculates the node coordinate of a node based on its key.
            /// </summary>
            public Vector2Int CalculateNodeCoordinateFromKey(Vector2Int key)
            {
                Vector2Int originKey = CalculateMatrixOriginKey();
                return key - originKey;
            }
            #endregion

            #region < PRIVATE_METHODS > ================================================================

            Vector2 ClampVector2(Vector2 value, float min, float max)
            {
                return new Vector2(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max));
            }

            Vector2 CalculateMatrixDimensions()
            {
                return new Vector2(_columnCount, _rowCount) * _nodeDimensions;
            }

            /// <summary>
            /// Calculates the alignment offset of the matrix in world space,
            /// based on the alignment setting and the dimensions of the matrix.
            /// </summary>
            Vector2 CalculateMatrixAlignmentOffset()
            {
                Vector2 matrixDimensions = CalculateMatrixDimensions() - _nodeDimensions;
                return AlignmentOffsets.TryGetValue(_alignment, out var offsetFunc)
                    ? offsetFunc(matrixDimensions)
                    : Vector2.zero;
            }

            /// <summary>
            /// Calculates the origin node key of the matrix, based on the alignment setting.
            /// </summary>
            Vector2Int CalculateMatrixOriginKey()
            {
                Vector2Int maxIndices = _terminalKey;
                return OriginKeys.TryGetValue(_alignment, out var originFunc)
                    ? originFunc(maxIndices)
                    : Vector2Int.zero;
            }

            Vector2Int CalculateTerminalKey()
            {
                return new Vector2Int(_columnCount - 1, _rowCount - 1);
            }
            #endregion
        }
    }
}
