using System;
using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.World;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    [ExecuteAlways]
    public partial class Matrix : MonoBehaviour
    {
        static readonly Dictionary<Matrix.Alignment, Func<Vector2, Vector2>> AlignmentOffsets =
            new Dictionary<Matrix.Alignment, Func<Vector2, Vector2>>
            {
                { Matrix.Alignment.BottomLeft, _ => Vector2.zero },
                { Matrix.Alignment.BottomCenter, dims => new Vector2(-dims.x / 2, 0) },
                { Matrix.Alignment.BottomRight, dims => new Vector2(-dims.x, 0) },
                { Matrix.Alignment.MiddleLeft, dims => new Vector2(0, -dims.y / 2) },
                { Matrix.Alignment.MiddleCenter, dims => new Vector2(-dims.x / 2, -dims.y / 2) },
                { Matrix.Alignment.MiddleRight, dims => new Vector2(-dims.x, -dims.y / 2) },
                { Matrix.Alignment.TopLeft, dims => new Vector2(0, -dims.y) },
                { Matrix.Alignment.TopCenter, dims => new Vector2(-dims.x / 2, -dims.y) },
                { Matrix.Alignment.TopRight, dims => new Vector2(-dims.x, -dims.y) },
            };

        static readonly Dictionary<Matrix.Alignment, Func<Vector2Int, Vector2Int>> OriginKeys =
            new Dictionary<Matrix.Alignment, Func<Vector2Int, Vector2Int>>
            {
                { Matrix.Alignment.BottomLeft, _ => new Vector2Int(0, 0) },
                { Matrix.Alignment.BottomCenter, max => new Vector2Int(max.x / 2, 0) },
                { Matrix.Alignment.BottomRight, max => new Vector2Int(max.x, 0) },
                { Matrix.Alignment.MiddleLeft, max => new Vector2Int(0, max.y / 2) },
                { Matrix.Alignment.MiddleCenter, max => new Vector2Int(max.x / 2, max.y / 2) },
                { Matrix.Alignment.MiddleRight, max => new Vector2Int(max.x, max.y / 2) },
                { Matrix.Alignment.TopLeft, max => new Vector2Int(0, max.y) },
                { Matrix.Alignment.TopCenter, max => new Vector2Int(max.x / 2, max.y) },
                { Matrix.Alignment.TopRight, max => new Vector2Int(max.x, max.y) },
            };
        StateMachine _stateMachine = new StateMachine();

        [SerializeField, ShowOnly]
        State _currentState;

        [SerializeField, AllowNesting]
        MatrixInfo _info;

        [SerializeField]
        NodeMap _map;

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

        public enum State
        {
            INVALID,
            PRELOADED,
            INITIALIZED
        }

        public MatrixInfo Info { get; protected set; }
        public NodeMap Map { get; protected set; }
        public Node.Visitor UpdateNodeContextVisitor =>
            new Node.Visitor(node =>
            {
                node.Refresh();
                return true;
            });

        public State CurrentState => _currentState = _stateMachine.currentStateEnum;

        public virtual void Preload()
        {
            if (_stateMachine == null)
            {
                _stateMachine = new StateMachine();

                _stateMachine.OnStateChanged += OnStateChanged;
            }

            // Create a new cell map
            if (_info.Parent == null)
                _info.Parent = transform;
            Initialize(_info);

            // Determine if the grid was preloaded
            _stateMachine.GoToState(State.PRELOADED);
        }

        public virtual void Initialize(MatrixInfo info)
        {
            _info = info;
            _info.Validate();
            _map = new NodeMap(_info);
        }

        public virtual void Refresh()
        {
            _info.Validate();
            _map.Refresh();
            SendVisitorToAllNodes(UpdateNodeContextVisitor);
        }

        public void SendVisitorToNode(Vector2Int key, IVisitor<Node> visitor)
        {
            if (visitor == null)
                return;
            _map.GetNode(key)?.Accept(visitor);
        }

        public void SendVisitorToNodes(List<Vector2Int> keys, IVisitor<Node> visitor)
        {
            if (visitor == null)
                return;
            foreach (Vector2Int key in keys)
                _map.GetNode(key)?.Accept(visitor);
        }

        public void SendVisitorToAllNodes(IVisitor<Node> visitor)
        {
            if (visitor == null)
                return;
            foreach (Node node in _map.Nodes)
                node.Accept(visitor);
        }

        protected virtual void Awake() => Preload();

        protected virtual void OnValidate() => Refresh();

        protected virtual void OnEnable() => Refresh();

        protected virtual void OnDisable() { }

        protected virtual void OnDestroy() { }

        protected virtual void OnDrawGizmosSelected()
        {
            CustomGizmos.DrawWireRect(
                _info.OriginWorldPosition,
                _info.Dimensions,
                _info.OriginWorldRotation,
                Color.white
            );
        }

        protected static Vector2Int CalculateNodeKey(Vector2Int coordinate, Vector2Int originKey)
        {
            return coordinate + originKey;
        }

        protected static Vector2Int CalculateNodeCoordinate(Vector2Int key, Vector2Int originKey)
        {
            return key - originKey;
        }

        protected static Vector2Int CalculateTerminalKey(BoundsInt bounds)
        {
            return new Vector2Int(bounds.size.x - 1, bounds.size.y - 1);
        }

        protected static int CalculateCellPartition(Vector2Int key, int partitionSize)
        {
            int partitionX = Mathf.FloorToInt(key.x / (float)partitionSize);
            int partitionY = Mathf.FloorToInt(key.y / (float)partitionSize);

            // Using a more robust hash function for partition key
            // This handles negative coordinates better
            const int PRIME = 31;
            int hash = 17;
            hash = hash * PRIME + partitionX;
            hash = hash * PRIME + partitionY;
            return hash;
        }

        /// <summary>
        /// Calculates the origin node key of the matrix, based on the alignment setting.
        /// </summary>
        protected static Vector2Int CalculateOriginKey(Vector2Int terminalKey, Alignment alignment)
        {
            Vector2Int maxIndices = terminalKey;

            return OriginKeys.TryGetValue(alignment, out var originFunc)
                ? originFunc(maxIndices)
                : Vector2Int.zero;
        }

        /// <summary>
        /// Calculates the alignment offset of the matrix in world space,
        /// based on the alignment setting and the dimensions of the matrix.
        /// </summary>
        protected static Vector2 CalculateMatrixAlignmentOffset(
            Vector2 matrixDimensions,
            Alignment alignment
        )
        {
            return AlignmentOffsets.TryGetValue(alignment, out var offsetFunc)
                ? offsetFunc(matrixDimensions)
                : Vector2.zero;
        }

        /// <summary>
        /// Calculates the world position and rotation of a node based on its key.
        /// </summary>
        protected static void CalculateNodeValues(
            Vector2Int key,
            MatrixInfo info,
            out Vector2Int coordinate,
            out Vector3 position,
            out Quaternion rotation,
            out int partition
        )
        {
            coordinate = CalculateNodeCoordinate(key, info.OriginKey);

            // Calculate the partition of the node
            partition = CalculateCellPartition(key, info.PartitionSize);

            if (info.Grid != null)
            {
                position = info.Grid.CellToWorld(new Vector3Int(coordinate.x, coordinate.y, 0));
                rotation = info.Grid.transform.rotation;
                return;
            }

            // Calculate the node position offset in world space based on dimensions
            Vector2 keyOffsetPos = key * info.NodeSize;

            // Calculate the origin position offset in world space based on alignment
            Vector2 originOffset = info.OriginAlignmentOffset;
            if (info.CenterNodes)
            {
                if (info.ColumnCount % 2 == 0)
                {
                    originOffset.x += info.NodeSize.x * 0.5f;
                }

                if (info.RowCount % 2 == 0)
                {
                    originOffset.y += info.NodeSize.y * 0.5f;
                }
            }

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
            Vector2 localPosition2D = keyOffsetPos + originOffset;
            localPosition2D *= spacingOffsetPos;
            localPosition2D += bondingOffset;

            // Convert the 2D local position to 3D and apply matrix rotation
            Vector3 localPosition = new Vector3(localPosition2D.x, 0, localPosition2D.y);
            Quaternion matrixRotation = Quaternion.Euler(info.OriginWorldRotation);
            Vector3 rotatedPosition = matrixRotation * localPosition;

            // Final world position by adding rotated local position to MatrixPosition
            position = info.OriginWorldPosition + rotatedPosition;

            // Apply the same rotation to each node
            rotation = matrixRotation;
        }

        protected static Vector2 ClampVector2(Vector2 value, float min, float max)
        {
            return new Vector2(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max));
        }

        void OnStateChanged(State state)
        {
            _currentState = state;
        }

        class StateMachine : SimpleStateMachine<State>
        {
            public StateMachine()
                : base(State.INVALID) { }
        }
    }
}
