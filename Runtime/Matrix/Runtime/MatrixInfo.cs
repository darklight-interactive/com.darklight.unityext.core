using System;
using System.Collections.Generic;

using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Utility;

using NaughtyAttributes;

using NUnit.Framework.Internal;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        [Serializable]
        public class Info
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

            static readonly Dictionary<Alignment, Func<Vector2, Vector2>> AlignmentOffsets = new Dictionary<Alignment, Func<Vector2, Vector2>>
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

            static readonly Dictionary<Alignment, Func<Vector2Int, Vector2Int>> OriginKeys = new Dictionary<Alignment, Func<Vector2Int, Vector2Int>>
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

            #region < PUBLIC_FIELDS > ================================================================

            public Transform ParentTransform;
            [DisableIf("HasParentTransform"), AllowNesting] public Vector3 OriginWorldPosition;
            [DisableIf("HasParentTransform"), AllowNesting] public Vector3 OriginWorldRotation;

            [Space(10)]
            [Range(1, MAX_MAP_KEY)] public int MatrixColumnCount = 1;
            [Range(1, MAX_MAP_KEY)] public int MatrixRowCount = 1;
            [ShowOnly] public Vector2 Dimensions;

            /// <summary> The final key of the matrix, representing the number of columns and rows. </summary>
            [ShowOnly] public Vector2Int TerminalNodeKey = Vector2Int.zero;
            [ShowOnly] public int NodeCapacity;

            [Space(10)]
            public Alignment MapAlignment;

            /// <summary> The origin key of the matrix, determined by the alignment of the map to the position. </summary>
            [ShowOnly] public Vector2Int OriginNodeKey = Vector2Int.zero;

            [Space(10), Header("Node Values")]
            public Vector2 NodeDimensions;
            public Vector2 NodeSpacing;
            public Vector2 NodeBonding;

            public bool HasParentTransform => ParentTransform != null;
            #endregion

            // ======== [[ CONSTRUCTOR ]] ======================================================= >>>>
            public Info() => Validate();

            #region < PRIVATE_METHODS > ================================================================

            Vector2 ClampVector2(Vector2 value, float min, float max)
            {
                return new Vector2(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max));
            }

            Vector2 CalculateMatrixDimensions()
            {
                return new Vector2(MatrixColumnCount, MatrixRowCount) * NodeDimensions;
            }

            /// <summary>
            /// Calculates the alignment offset of the matrix in world space,
            /// based on the alignment setting and the dimensions of the matrix.
            /// </summary>
            Vector2 CalculateMatrixAlignmentOffset()
            {
                Vector2 matrixDimensions = CalculateMatrixDimensions() - NodeDimensions;
                return AlignmentOffsets.TryGetValue(MapAlignment, out var offsetFunc)
                    ? offsetFunc(matrixDimensions)
                    : Vector2.zero;
            }

            /// <summary>
            /// Calculates the origin node key of the matrix, based on the alignment setting.
            /// </summary>
            Vector2Int CalculateMatrixOriginKey()
            {
                Vector2Int maxIndices = TerminalNodeKey;
                return OriginKeys.TryGetValue(MapAlignment, out var originFunc)
                    ? originFunc(maxIndices)
                    : Vector2Int.zero;
            }

            Vector2Int CalculateTerminalKey()
            {
                return new Vector2Int(MatrixColumnCount - 1, MatrixRowCount - 1);
            }

            #endregion

            #region < PUBLIC_METHODS > ================================================================
            public void Validate()
            {
                // << CLAMP INDEX VALUES >>
                MatrixColumnCount = Mathf.Clamp(MatrixColumnCount, MIN_MAP_KEY, MAX_MAP_KEY);
                MatrixRowCount = Mathf.Clamp(MatrixRowCount, MIN_MAP_KEY, MAX_MAP_KEY);

                // << CLAMP NODE VALUES >>
                NodeDimensions = ClampVector2(NodeDimensions, MIN_NODE_DIMENSION, MAX_NODE_DIMENSION);
                NodeSpacing = ClampVector2(NodeSpacing, MIN_NODE_SPACING, MAX_NODE_SPACING);
                NodeBonding = ClampVector2(NodeBonding, MIN_NODE_BONDING, MAX_NODE_BONDING);

                // << CALCULATE TRANSFORM VALUES >>
                if (ParentTransform != null)
                {
                    OriginWorldPosition = ParentTransform.position;
                    OriginWorldRotation = ParentTransform.rotation.eulerAngles;
                }

                // << CALCULATE DERIVED KEYS >>
                OriginNodeKey = CalculateMatrixOriginKey();
                TerminalNodeKey = CalculateTerminalKey();

                // << CALCULATE DERIVED VALUES >>
                NodeCapacity = MatrixColumnCount * MatrixRowCount;
                Dimensions = CalculateMatrixDimensions();

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

                for (int x = 0; x <= TerminalNodeKey.x; x++)
                {
                    key.x = x;
                    for (int y = 0; y <= TerminalNodeKey.y; y++)
                    {
                        key.y = y;
                        yield return key;
                    }
                }
            }

            public bool IsKeyInBounds(Vector2Int key)
            {
                return key.x >= 0 
                    && key.x < TerminalNodeKey.x 
                    && key.y >= 0 
                    && key.y < TerminalNodeKey.y;
            }



            /// <summary>
            /// Calculates the world position and rotation of a node based on its key.
            /// </summary>
            public void CalculateNodeTransformFromKey(Vector2Int key, out Vector3 position, out Quaternion rotation)
            {
                // Calculate the node position offset in world space based on dimensions
                Vector2 keyOffsetPos = key * NodeDimensions;

                // Calculate the origin position offset in world space based on alignment
                Vector2 originOffset = CalculateMatrixAlignmentOffset();

                // Calculate the spacing offset and clamp to avoid overlapping cells
                Vector2 spacingOffsetPos = NodeSpacing + Vector2.one;
                spacingOffsetPos.x = Mathf.Max(spacingOffsetPos.x, 0.5f);
                spacingOffsetPos.y = Mathf.Max(spacingOffsetPos.y, 0.5f);

                // Calculate bonding offsets
                Vector2 bondingOffset = Vector2.zero;
                if (key.y % 2 == 0)
                    bondingOffset.x = NodeBonding.x;
                if (key.x % 2 == 0)
                    bondingOffset.y = NodeBonding.y;

                // Combine offsets and apply spacing
                Vector2 localPosition2D = keyOffsetPos + originOffset;
                localPosition2D *= spacingOffsetPos;
                localPosition2D += bondingOffset;

                // Convert the 2D local position to 3D and apply matrix rotation
                Vector3 localPosition = new Vector3(localPosition2D.x, 0, localPosition2D.y);
                Quaternion matrixRotation = Quaternion.Euler(OriginWorldRotation);
                Vector3 rotatedPosition = matrixRotation * localPosition;

                // Final world position by adding rotated local position to MatrixPosition
                position = OriginWorldPosition + rotatedPosition;

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

            /// <summary>
            /// Static method to get a default instance of MapInfo with optional parameters.
            /// </summary>
            public static Info GetDefault(Transform parent = null)
            {
                return new Info()
                {
                    ParentTransform = parent,
                    MatrixColumnCount = DEFAULT_MAP_KEY,
                    MatrixRowCount = DEFAULT_MAP_KEY,
                    NodeDimensions = new Vector2(DEFAULT_NODE_DIMENSION, DEFAULT_NODE_DIMENSION),
                    NodeSpacing = new Vector2(DEFAULT_NODE_SPACING, DEFAULT_NODE_SPACING),
                    NodeBonding = new Vector2(DEFAULT_NODE_BONDING, DEFAULT_NODE_BONDING),
                };
            }
            #endregion
        }
    }
}