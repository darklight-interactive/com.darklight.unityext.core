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

    #region < PUBLIC_STRUCT > [[ Context ]] ================================================================ 

    [Serializable]
    public class MapInfo
    {
        #region < PRIVATE_CONST > ================================================================ 
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
        public Vector3 MapPosition;
        public Vector3 MapRotation;

        public Alignment MapAlignment;

        /// <summary> The final key of the matrix, representing the number of columns and rows. </summary>
        public Vector2Int TerminalKey;

        public Vector2 NodeDimensions;
        public Vector2 NodeSpacing;
        public Vector2 NodeBonding;

        public int Capacity => TerminalKey.x * TerminalKey.y;
        public Vector2 MapDimensions => TerminalKey * NodeDimensions;
        public int NumColumns => TerminalKey.x;
        public int NumRows => TerminalKey.y;

        #endregion

        #region  < CONSTRUCTORS > ================================================================
        public MapInfo() { }

        /// <summary>
        /// Private constructor used only by the static factory methods to ensure proper initialization.
        /// </summary>
        private MapInfo(Transform parentTransform, Vector2Int terminalKey, Alignment mapAlignment, Vector3 mapPosition, Vector3 mapRotation, Vector2 nodeDimensions, Vector2 nodeSpacing, Vector2 nodeBonding)
        {
            ParentTransform = parentTransform;
            MapPosition = mapPosition;
            MapRotation = mapRotation;
            MapAlignment = mapAlignment;

            TerminalKey = ClampTerminalKey(terminalKey);
            NodeDimensions = ClampNodeDimensions(nodeDimensions);
            NodeSpacing = ClampNodeSpacing(nodeSpacing);
            NodeBonding = ClampNodeBonding(nodeBonding);
        }
        #endregion

        #region < PRIVATE_METHODS > ================================================================

        private static Vector2Int ClampTerminalKey(Vector2Int terminalKey) =>
            new Vector2Int(
                Mathf.Clamp(terminalKey.x, 0, MAX_MAP_KEY), 
                Mathf.Clamp(terminalKey.y, 0, MAX_MAP_KEY));

        private static Vector2 ClampNodeDimensions(Vector2 dimensions) =>
            new Vector2(
                Mathf.Clamp(dimensions.x, MIN_NODE_DIMENSION, MAX_NODE_DIMENSION), 
                Mathf.Clamp(dimensions.y, MIN_NODE_DIMENSION, MAX_NODE_DIMENSION));

        private static Vector2 ClampNodeSpacing(Vector2 spacing) =>
            new Vector2(
                Mathf.Clamp(spacing.x, MIN_NODE_SPACING, MAX_NODE_SPACING), 
                Mathf.Clamp(spacing.y, MIN_NODE_SPACING, MAX_NODE_SPACING));

        private static Vector2 ClampNodeBonding(Vector2 bonding) =>
            new Vector2(
                Mathf.Clamp(bonding.x, MIN_NODE_BONDING, MAX_NODE_BONDING), 
                Mathf.Clamp(bonding.y, MIN_NODE_BONDING, MAX_NODE_BONDING));

        #endregion

        #region < PUBLIC_METHODS > ================================================================
        public void Validate()
        {
            if (ParentTransform != null)
            {
                MapPosition = ParentTransform.position;
                MapRotation = ParentTransform.rotation.eulerAngles;
            }

            TerminalKey = ClampTerminalKey(TerminalKey);
            NodeDimensions = ClampNodeDimensions(NodeDimensions);
            NodeSpacing = ClampNodeSpacing(NodeSpacing);
            NodeBonding = ClampNodeBonding(NodeBonding);
        }

        public void CopyFrom(MapInfo other)
        {
            ParentTransform = other.ParentTransform;
            MapPosition = other.MapPosition;
            MapRotation = other.MapRotation;
            MapAlignment = other.MapAlignment;
            TerminalKey = other.TerminalKey;
            NodeDimensions = other.NodeDimensions;
            NodeSpacing = other.NodeSpacing;
            NodeBonding = other.NodeBonding;
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

        /// <summary>
        /// Calculates the alignment offset of the matrix in world space,
        /// based on the alignment setting and the dimensions of the matrix.
        /// </summary>
        public Vector2 CalculateMatrixAlignmentOffset()
        {
            Vector2 matrixDimensions = MapDimensions - NodeDimensions;
            return AlignmentOffsets.TryGetValue(MapAlignment, out var offsetFunc)
                ? offsetFunc(matrixDimensions)
                : Vector2.zero;
        }

        /// <summary>
        /// Calculates the origin node key of the matrix, based on the alignment setting.
        /// </summary>
        public Vector2Int CalculateMatrixOriginKey()
        {
            Vector2Int maxIndices = new Vector2Int(TerminalKey.x - 1, TerminalKey.y - 1);
            return OriginKeys.TryGetValue(MapAlignment, out var originFunc)
                ? originFunc(maxIndices)
                : Vector2Int.zero;
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
            Quaternion matrixRotation = Quaternion.Euler(MapRotation);
            Vector3 rotatedPosition = matrixRotation * localPosition;

            // Final world position by adding rotated local position to MatrixPosition
            position = MapPosition + rotatedPosition;

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
        public static MapInfo GetDefault(Transform parent = null, Vector2Int? terminalKey = null, Alignment alignment = Alignment.MiddleCenter)
        {
            Debug.Log("GetDefault");

            return new MapInfo(
                parentTransform: parent,
                terminalKey: terminalKey ?? new Vector2Int(DEFAULT_MAP_KEY, DEFAULT_MAP_KEY),
                mapPosition: parent != null ? parent.position : Vector3.zero,
                mapRotation: parent != null ? parent.rotation.eulerAngles : Vector3.zero,
                mapAlignment: alignment,
                nodeDimensions: new Vector2(DEFAULT_NODE_DIMENSION, DEFAULT_NODE_DIMENSION),
                nodeSpacing: new Vector2(DEFAULT_NODE_SPACING, DEFAULT_NODE_SPACING),
                nodeBonding: new Vector2(DEFAULT_NODE_BONDING, DEFAULT_NODE_BONDING)
            );
        }
        #endregion
    }
    #endregion

}