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

            [SerializeField, ReadOnly]
            Grid _grid;

            [SerializeField, ReadOnly]
            Transform _parent;

            [SerializeField]
            BoundsInt _bounds;

            [SerializeField]
            Alignment _alignment;

            [SerializeField, Range(5, 100)]
            int _partitionSize;

            [HorizontalLine]
            [SerializeField]
            /// <summary> The origin key of the matrix, determined by the alignment of the map to the position. </summary>
            Vector2Int _originKey;

            [SerializeField, ShowOnly]
            Vector2 _originAlignmentOffset;

            [SerializeField, ShowOnly]
            Vector3 _originWorldPosition;

            [SerializeField, ShowOnly]
            Vector3 _originWorldRotation;

            /// <summary> The final key of the matrix, representing the number of columns and rows. </summary>
            Vector2Int _terminalKey;

            [Space(10), Header("Node Values")]
            [SerializeField]
            private bool _centerNodes;

            [SerializeField]
            private Vector2 _nodeSize;

            [SerializeField]
            private Vector2 _nodeSpacing;

            [SerializeField]
            private Vector2 _nodeBonding;

            #region < PUBLIC_PROPERTIES > ================================================================
            public Grid Grid => _grid;
            public Transform Parent
            {
                get => _parent;
                set => _parent = value;
            }
            public Alignment Alignment => _alignment;
            public BoundsInt Bounds => _bounds;
            public int PartitionSize => _partitionSize;

            public Vector3 OriginWorldPosition => _originWorldPosition;
            public Vector3 OriginWorldRotation => _originWorldRotation;
            public Vector2 OriginAlignmentOffset => _originAlignmentOffset;

            public int ColumnCount => _bounds.size.x + 1;
            public int RowCount => _bounds.size.y + 1;
            public int Capacity => ColumnCount * RowCount;
            public Vector2 Dimensions => (Capacity * _nodeSize) - _nodeSize;

            public Vector2Int OriginKey => _originKey;
            public Vector2Int TerminalKey => _terminalKey;

            public bool CenterNodes => _centerNodes;
            public Vector2 NodeSize => _nodeSize;
            public Vector2 NodeSpacing => _nodeSpacing;
            public Vector2 NodeBonding => _nodeBonding;

            #endregion

            // ======== [[ CONSTRUCTOR ]] ======================================================= >>>>
            public MatrixInfo(Transform parent = null)
            {
                _grid = null;
                _parent = parent;
                _bounds = new BoundsInt(Vector3Int.zero, Vector3Int.one);

                _alignment = Matrix.Alignment.MiddleCenter;
                _partitionSize = DEFAULT_MAP_KEY;

                _originKey = Vector2Int.zero;
                _originAlignmentOffset = Vector2.zero;
                _originWorldPosition = Vector3.zero;
                _originWorldRotation = Vector3.zero;

                _terminalKey = Vector2Int.zero;

                _centerNodes = false;
                _nodeSize = new Vector2(DEFAULT_NODE_DIMENSION, DEFAULT_NODE_DIMENSION);
                _nodeSpacing = new Vector2(DEFAULT_NODE_SPACING, DEFAULT_NODE_SPACING);
                _nodeBonding = new Vector2(DEFAULT_NODE_BONDING, DEFAULT_NODE_BONDING);

                Validate();
            }

            public static MatrixInfo GetMatrixInfo(Grid grid, BoundsInt bounds)
            {
                return new MatrixInfo
                {
                    _parent = grid.transform,
                    _bounds = bounds,
                    _nodeSize = grid.cellSize,
                    _nodeSpacing = grid.cellGap,
                    _nodeBonding = Vector2.zero,
                    _centerNodes = false,
                    _alignment = Matrix.Alignment.BottomLeft,
                };
            }

            public void Validate()
            {
                if (_bounds != null)
                {
                    // << CLAMP BOUNDS >>
                    if (_bounds.size.x <= 0 || _bounds.size.y <= 0 || _bounds.size.z <= 0)
                    {
                        int clampedX = Mathf.Max(1, _bounds.size.x);
                        int clampedY = Mathf.Max(1, _bounds.size.y);
                        int clampedZ = Mathf.Max(1, _bounds.size.z);
                        _bounds.size = new Vector3Int(clampedX, clampedY, clampedZ);
                    }
                    SetBounds(_bounds);
                }

                if (_grid != null)
                {
                    _nodeSize = _grid.cellSize;
                    _nodeSpacing = _grid.cellGap;
                }

                // << CLAMP NODE VALUES >>
                _nodeSize = ClampVector2(_nodeSize, MIN_NODE_DIMENSION, MAX_NODE_DIMENSION);

                _nodeSpacing = ClampVector2(_nodeSpacing, MIN_NODE_SPACING, MAX_NODE_SPACING);
                _nodeBonding = ClampVector2(_nodeBonding, MIN_NODE_BONDING, MAX_NODE_BONDING);

                // << CALCULATE TRANSFORM VALUES >>
                if (_parent != null)
                {
                    _originWorldPosition = _parent.position;
                    _originWorldRotation = _parent.rotation.eulerAngles;
                }

                // << CALCULATE DERIVED KEYS >>
                _originKey = CalculateOriginKey(_terminalKey, _alignment);
                _terminalKey = CalculateTerminalKey(_bounds);
            }

            public void SetBounds(BoundsInt bounds)
            {
                _bounds = bounds;
                _alignment = Matrix.Alignment.MiddleCenter;
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
            /// Calculates the node coordinate of a node based on its key.
            /// </summary>
        }
    }
}
