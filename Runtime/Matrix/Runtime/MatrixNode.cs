using System;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
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
            bool _isValid = false;
            bool _isEnabled = true;

            Info _info;

            [SerializeField, ShowOnly]
            Vector2Int _key = Vector2Int.zero;

            [SerializeField, ShowOnly]
            Vector2Int _coordinate = Vector2Int.zero;

            [Header("World Space Values")]
            [SerializeField, ShowOnly]
            Vector3 _position = Vector3.zero;

            [SerializeField, ShowOnly]
            Vector2 _dimensions = Vector2.one;

            public Info MatrixInfo => _info;
            public bool Enabled
            {
                get => _isEnabled;
                set => _isEnabled = value;
            }
            public Vector2Int Key => _key;
            public Vector2Int Coordinate => _coordinate;
            public Vector3Int Coordinate_Vec3 => Utility.SwizzleVec2Int(Coordinate, _info.Swizzle);
            public Vector3 Position => _position;
            public Vector2 Dimensions => _dimensions;
            public int PartitionKey => CalculatePartitionKey(_info, _key);

            #region ---- < PUBLIC_PROPERTIES > ( Span ) ---------------------------------
            public float DiagonalSpan =>
                Mathf.Sqrt(Mathf.Pow(_dimensions.x, 2) + Mathf.Pow(_dimensions.y, 2));
            public float AverageSpan => (_dimensions.x + _dimensions.y) * 0.5f;
            public float MaxSpan => Mathf.Max(_dimensions.x, _dimensions.y);
            public float MinSpan => Mathf.Min(_dimensions.x, _dimensions.y);
            #endregion

            // ======== [[ CONSTRUCTOR ]] ======================================================= >>>>
            public Node(Info info, Vector2Int key)
            {
                if (info == null)
                {
                    Debug.LogError("MatrixInfo is null on Node: " + key);
                    return;
                }
                _isValid = true;

                _info = info;
                _key = key;

                Refresh();
            }

            // (( INTERFACE )) : IVisitable -------- ))
            public void AcceptVisitor(IVisitor<Node> visitor)
            {
                visitor.Visit(this);
            }

            public void Refresh()
            {
                _dimensions = _info.NodeSize;
            }

            public static void ConvertKeyToCoordinate(
                Info info,
                Vector2Int key,
                out Vector2Int coordinate
            )
            {
                coordinate = key - info.OriginKey;
            }

            public static void ConvertCoordinateToKey(
                Info info,
                Vector2Int coordinate,
                out Vector2Int key
            )
            {
                key = coordinate + info.OriginKey;
            }

            public static void CalculatePosition(Info info, Vector2Int key, out Vector3 position)
            {
                if (info.Grid != null)
                {
                    ConvertKeyToCoordinate(info, key, out Vector2Int coordinate);
                    position = info.Grid.CellToWorld(new Vector3Int(coordinate.x, coordinate.y, 0));
                    position.x += info.NodeHalfSize.x;
                    position.y -= info.NodeHalfSize.y;
                    return;
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
                        (info?.Parent != null ? info.Parent.position : Vector3.zero)
                        + rotatedPosition;
                }
            }

            public static int CalculatePartitionKey(Info info, Vector2Int key)
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
        }
    }
}
