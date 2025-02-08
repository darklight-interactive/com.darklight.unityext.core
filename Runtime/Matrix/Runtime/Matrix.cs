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
        protected static readonly Dictionary<Alignment, Vector2> AlignmentOffsets = new Dictionary<
            Alignment,
            Vector2
        >
        {
            { Alignment.BottomLeft, Vector2.zero },
            { Alignment.BottomCenter, new Vector2(-0.5f, 0) },
            { Alignment.BottomRight, new Vector2(-1f, 0) },
            { Alignment.MiddleLeft, new Vector2(0, -0.5f) },
            { Alignment.MiddleCenter, new Vector2(-0.5f, -0.5f) },
            { Alignment.MiddleRight, new Vector2(-1f, -0.5f) },
            { Alignment.TopLeft, new Vector2(0, -1f) },
            { Alignment.TopCenter, new Vector2(-0.5f, -1f) },
            { Alignment.TopRight, new Vector2(-1f, -1f) },
        };

        protected static readonly Dictionary<Alignment, Func<Vector2Int, Vector2Int>> OriginKeys =
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

        public MatrixInfo Info => _info;
        public NodeMap Map => _map;
        public Node.Visitor UpdateNodeContextVisitor =>
            new Node.Visitor(node =>
            {
                node.Refresh();
                return true;
            });

        public State CurrentState => _currentState = _stateMachine.currentStateEnum;
        public Node OriginNode => _map.GetNode(_info.OriginKey);
        public Node TerminalNode => _map.GetNode(_info.TerminalKey);

        public virtual void Preload()
        {
            if (_stateMachine == null)
            {
                _stateMachine = new StateMachine();
                _stateMachine.OnStateChanged += OnStateChanged;
            }

            if (_info == null)
            {
                _info = new MatrixInfo(this.transform);
                Debug.Log("Creating new MatrixInfo");
            }

            if (_info.Grid == null)
            {
                if (GetComponentInChildren<Grid>() != null)
                {
                    _info.Grid = GetComponentInChildren<Grid>();
                    _info.Parent = _info.Grid.transform;
                }
            }

            Debug.Log("Preloading matrix");
            Initialize(_info);

            // Determine if the grid was preloaded
            _stateMachine.GoToState(State.PRELOADED);
        }

        public virtual void Initialize(MatrixInfo info)
        {
            Debug.Log("Initializing matrix");
            _info = info;
            _info.Validate();
            _map = new NodeMap(_info);
            Refresh();
        }

        public virtual void Refresh()
        {
            Debug.Log("Refreshing matrix");
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

        protected static Vector2Int CalculateNodeKey(Vector2Int coordinate, Vector2Int originKey)
        {
            return coordinate + originKey;
        }

        protected static Vector2Int CalculateNodeCoordinate(Vector2Int key, Vector2Int originKey)
        {
            return key - originKey;
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
        /// Calculates the alignment offset based on the alignment and size.
        /// </summary>
        /// <param name="alignment">The alignment to calculate the offset for.</param>
        /// <param name="size">The size of the matrix.</param>
        /// <returns>The alignment offset in local space Vector2.</returns>
        protected static Vector2 CalculateLocalAlignmentOffset(Alignment alignment, Vector2 size)
        {
            Vector2 alignmentOffset = AlignmentOffsets.TryGetValue(alignment, out Vector2 offset)
                ? offset
                : Vector2.zero;

            alignmentOffset *= size;
            return alignmentOffset;
        }

        /// <summary>
        /// Calculates alignment offset taking swizzle into account.
        /// </summary>
        protected static Vector2 CalculateWorldAlignmentOffset(
            Alignment alignment,
            Vector2 size,
            GridLayout.CellSwizzle swizzle
        )
        {
            // Get base alignment offset
            Vector2 baseOffset = AlignmentOffsets.TryGetValue(alignment, out Vector2 offset)
                ? offset
                : Vector2.zero;

            // Adjust offset based on swizzle
            switch (swizzle)
            {
                case GridLayout.CellSwizzle.XYZ:
                    // For XYZ, we need to flip the Y offset
                    return new Vector2(baseOffset.x * size.x, -baseOffset.y * size.y);

                case GridLayout.CellSwizzle.YXZ:
                    // For YXZ, we swap X and Y and flip Y
                    return new Vector2(baseOffset.y * size.y, -baseOffset.x * size.x);

                case GridLayout.CellSwizzle.ZYX:
                    // For ZYX, we flip both X and Y
                    return new Vector2(-baseOffset.x * size.x, -baseOffset.y * size.y);

                case GridLayout.CellSwizzle.YZX:
                    // For YZX, we flip X
                    return new Vector2(-baseOffset.x * size.x, baseOffset.y * size.y);

                case GridLayout.CellSwizzle.ZXY:
                    // For ZXY, we swap X and Y
                    return new Vector2(baseOffset.y * size.y, baseOffset.x * size.x);

                case GridLayout.CellSwizzle.XZY:
                default:
                    // Default XZY behavior (original)
                    return new Vector2(baseOffset.x * size.x, baseOffset.y * size.y);
            }
        }

        /// <summary>
        /// Converts a 2D alignment value to 3D based on the cell swizzle.
        /// </summary>
        protected static Vector3 SwizzleVec2(Vector2 value, GridLayout.CellSwizzle swizzle)
        {
            switch (swizzle)
            {
                case GridLayout.CellSwizzle.XYZ:
                    return new Vector3(value.x, value.y, 0);

                case GridLayout.CellSwizzle.XZY:
                    return new Vector3(value.x, 0, value.y);
                case GridLayout.CellSwizzle.YXZ:
                    return new Vector3(value.y, value.x, 0);
                case GridLayout.CellSwizzle.YZX:
                    return new Vector3(0, value.x, value.y);
                case GridLayout.CellSwizzle.ZXY:
                    return new Vector3(value.y, 0, value.x);
                case GridLayout.CellSwizzle.ZYX:
                    return new Vector3(0, value.y, value.x);
                default:
                    return new Vector3(value.x, 0, value.y); // Default to XZY
            }
        }

        protected static Quaternion CalculateSwizzleRotationOffset(GridLayout.CellSwizzle swizzle)
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

        /// <summary>
        /// Gets the dimensions of the matrix in world space.
        /// </summary>
        /// <returns>The full dimensions of the matrix in local units.</returns>
        protected static Vector2 CalculateMatrixDimensions(MatrixInfo info)
        {
            Vector2 dimensions2D = new Vector2(
                info.Bounds.x * info.NodeSize.x,
                info.Bounds.y * info.NodeSize.y
            );

            dimensions2D *= (info.NodeSpacing + Vector2.one);
            return dimensions2D;
        }

        /// <summary>
        /// Calculates the center position of the matrix in world space using matrix data.
        /// </summary>
        /// <returns>The center position of the matrix in world coordinates.</returns>
        protected static Vector3 CalculateMatrixCenter(MatrixInfo info)
        {
            // Calculate alignment offset
            Vector2 alignmentOffset = CalculateLocalAlignmentOffset(
                info.OriginAlignment,
                info.Dimensions - info.NodeSize
            );
            alignmentOffset -= info.NodeHalfSize;

            Vector2 centerPos2D = info.Dimensions / 2;
            centerPos2D += alignmentOffset;

            // Convert to 3D with proper swizzle
            Vector3 centerPos3D = SwizzleVec2(centerPos2D, info.Swizzle);
            return info.OriginWorldPosition + centerPos3D;
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
            else
            {
                // Calculate the node position offset in world space based on dimensions
                Vector2 keyOffsetPos = key * info.NodeSize;
                Vector2 nodeHalfSize = info.NodeSize * 0.5f;

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
                localPosition2D += CalculateLocalAlignmentOffset(
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
                    (info?.Parent != null ? info.Parent.position : Vector3.zero) + rotatedPosition;

                // Apply the same rotation to each node
                rotation = matrixRotation;
            }
        }

        protected static Vector2 ClampVector2(Vector2 value, float min, float max)
        {
            return new Vector2(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max));
        }

        protected virtual void Awake() => Preload();

        protected virtual void OnValidate() => Refresh();

        protected virtual void OnEnable() => Refresh();

        protected virtual void OnDisable() { }

        protected virtual void OnDestroy() { }

        protected virtual void OnDrawGizmosSelected()
        {
            CustomGizmos.DrawWireRect(_info.Center, _info.Dimensions, _info.Rotation, Color.green);
            Gizmos.DrawSphere(_info.Center, 0.1f);

#if UNITY_EDITOR
            // Debug visualization of orientation
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_info.Center, _info.Rotation * Vector3.forward * 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(_info.Center, _info.Rotation * Vector3.up * 0.5f);
            Gizmos.color = Color.red;

            Gizmos.DrawRay(_info.Center, _info.Rotation * Vector3.right * 0.5f);
#endif
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
