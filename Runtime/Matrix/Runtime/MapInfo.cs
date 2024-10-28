using System;
using System.Collections.Generic;

using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Utility;

using NaughtyAttributes;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{

    #region < PUBLIC_STRUCT > [[ Context ]] ================================================================ 

    [Serializable]
    public struct MapInfo
    {
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

        /// <summary>
        /// The final key of the matrix
        /// </summary>
        public Vector2Int TerminalKey { get; }
        public int Capacity => TerminalKey.x * TerminalKey.y;

        public Alignment MapAlignment { get; }
        public Vector3 MapPosition { get; }
        public Vector3 MapRotation { get; }
        public Vector2 MapDimensions => TerminalKey * NodeDimensions;
        public int NumColumns => TerminalKey.x;
        public int NumRows => TerminalKey.y;

        public Vector2 NodeDimensions { get; }
        public Vector2 NodeSpacing { get; }
        public Vector2 NodeBonding { get; }

        /// <summary>
        /// Private constructor used only by the static factory methods to ensure proper initialization.
        /// </summary>
        private MapInfo(Vector2Int terminalKey, Alignment mapAlignment, Vector3 mapPosition, Vector3 mapRotation, Vector2 nodeDimensions, Vector2 nodeSpacing, Vector2 nodeBonding)
        {
            // Clamp TerminalKey within (0, 0) and (MAP_EXTENT, MAP_EXTENT)
            TerminalKey = new Vector2Int(
                Mathf.Clamp(terminalKey.x, 0, MAX_MAP_KEY),
                Mathf.Clamp(terminalKey.y, 0, MAX_MAP_KEY)
            );

            MapAlignment = mapAlignment;
            MapPosition = mapPosition;
            MapRotation = mapRotation;

            // Applying constraints to node dimensions, spacing, and bonding
            NodeDimensions = new Vector2(
                Mathf.Clamp(nodeDimensions.x, MIN_NODE_DIMENSION, MAX_NODE_DIMENSION),
                Mathf.Clamp(nodeDimensions.y, MIN_NODE_DIMENSION, MAX_NODE_DIMENSION)
            );

            NodeSpacing = new Vector2(
                Mathf.Clamp(nodeSpacing.x, MIN_NODE_SPACING, MAX_NODE_SPACING),
                Mathf.Clamp(nodeSpacing.y, MIN_NODE_SPACING, MAX_NODE_SPACING)
            );

            NodeBonding = new Vector2(
                Mathf.Clamp(nodeBonding.x, MIN_NODE_BONDING, MAX_NODE_BONDING),
                Mathf.Clamp(nodeBonding.y, MIN_NODE_BONDING, MAX_NODE_BONDING)
            );
        }

        /// <summary>
        /// Static method to get a default instance of MapInfo with optional parameters.
        /// </summary>
        public static MapInfo GetDefault(Vector2Int? terminalKey = null, Vector3? position = null, Vector3? rotation = null, Alignment alignment = Alignment.MiddleCenter)
        {
            return new MapInfo(
                terminalKey: terminalKey ?? new Vector2Int(DEFAULT_MAP_KEY, DEFAULT_MAP_KEY),
                mapAlignment: alignment,
                mapPosition: position ?? Vector3.zero,
                mapRotation: rotation ?? Vector3.zero,
                nodeDimensions: new Vector2(DEFAULT_NODE_DIMENSION, DEFAULT_NODE_DIMENSION),
                nodeSpacing: new Vector2(DEFAULT_NODE_SPACING, DEFAULT_NODE_SPACING),
                nodeBonding: new Vector2(DEFAULT_NODE_BONDING, DEFAULT_NODE_BONDING)
            );
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

        #region ---- < PUBLIC_METHODS > ( Calculations ) --------------------------------- 

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
            Vector2 matrixDimensions = MapDimensions - NodeDimensions; // Subtract the origin node dimensions
            Vector2 alignmentOffset = Vector2.zero;
            switch (MapAlignment)
            {
                case Alignment.BottomLeft: alignmentOffset = Vector2.zero; break;
                case Alignment.BottomCenter: alignmentOffset = new Vector2(-matrixDimensions.x / 2, 0); break;
                case Alignment.BottomRight: alignmentOffset = new Vector2(-matrixDimensions.x, 0); break;
                case Alignment.MiddleLeft: alignmentOffset = new Vector2(0, -matrixDimensions.y / 2); break;
                case Alignment.MiddleCenter: alignmentOffset = new Vector2(-matrixDimensions.x / 2, -matrixDimensions.y / 2); break;
                case Alignment.MiddleRight: alignmentOffset = new Vector2(-matrixDimensions.x, -matrixDimensions.y / 2); break;
                case Alignment.TopLeft: alignmentOffset = new Vector2(0, -matrixDimensions.y); break;
                case Alignment.TopCenter: alignmentOffset = new Vector2(-matrixDimensions.x / 2, -matrixDimensions.y); break;
                case Alignment.TopRight: alignmentOffset = new Vector2(-matrixDimensions.x, -matrixDimensions.y); break;
            }
            return alignmentOffset;
        }

        /// <summary>
        /// Calculates the origin node key of the matrix, based on the alignment setting.
        /// </summary>
        public Vector2Int CalculateMatrixOriginKey()
        {
            Vector2Int originKey = Vector2Int.zero;
            switch (MapAlignment)
            {
                case Alignment.BottomLeft:
                    originKey = new Vector2Int(0, 0);
                    break;
                case Alignment.BottomCenter:
                    originKey = new Vector2Int(Mathf.FloorToInt(TerminalKey.x / 2), 0);
                    break;
                case Alignment.BottomRight:
                    originKey = new Vector2Int(Mathf.FloorToInt(TerminalKey.x), 0);
                    break;
                case Alignment.MiddleLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(TerminalKey.y / 2));
                    break;
                case Alignment.MiddleCenter:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(TerminalKey.x / 2),
                        Mathf.FloorToInt(TerminalKey.y / 2)
                    );
                    break;
                case Alignment.MiddleRight:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(TerminalKey.x),
                        Mathf.FloorToInt(TerminalKey.y / 2)
                    );
                    break;
                case Alignment.TopLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(TerminalKey.y));
                    break;
                case Alignment.TopCenter:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(TerminalKey.x / 2),
                        Mathf.FloorToInt(TerminalKey.y)
                    );
                    break;
                case Alignment.TopRight:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(TerminalKey.x),
                        Mathf.FloorToInt(TerminalKey.y)
                    );
                    break;
            }
            return originKey;
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
        #endregion
    }
    #endregion

}