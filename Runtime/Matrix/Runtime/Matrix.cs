using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.World;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Matrix
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

    public enum State
    {
        INVALID,
        PRELOADED,
        INITIALIZED
    }

    [ExecuteAlways]
    public partial class Matrix : MonoBehaviour
    {
        StateMachine _stateMachine = new StateMachine();

        [SerializeField, ShowOnly]
        State _currentState;

        [SerializeField, AllowNesting]
        MatrixInfo _info;

        [SerializeField]
        MatrixNodeMap _map;

        public MatrixInfo Info => _info;
        public MatrixNodeMap Map => _map;
        public MatrixNode.Visitor UpdateNodeContextVisitor =>
            new MatrixNode.Visitor(node =>
            {
                node.Refresh();
                return true;
            });

        public State CurrentState => _currentState = _stateMachine.currentStateEnum;

        public void Preload()
        {
            if (_stateMachine == null)
            {
                _stateMachine = new StateMachine();
                _stateMachine.OnStateChanged += OnStateChanged;
            }

            // Create a new cell map
            if (_info == null)
                _info = MatrixInfo.CreateDefault(transform);
            _map = new MatrixNodeMap(this);

            // Determine if the grid was preloaded
            _stateMachine.GoToState(State.PRELOADED);
        }

        public void Refresh()
        {
            _map.Refresh();
            SendVisitorToAllNodes(UpdateNodeContextVisitor);
        }

        public MatrixInfo GetInfo()
        {
            return _info;
        }

        public void SendVisitorToNode(Vector2Int key, IVisitor<MatrixNode> visitor)
        {
            if (visitor == null)
                return;
            _map.GetNode(key)?.Accept(visitor);
        }

        public void SendVisitorToNodes(List<Vector2Int> keys, IVisitor<MatrixNode> visitor)
        {
            if (visitor == null)
                return;
            foreach (Vector2Int key in keys)
                _map.GetNode(key)?.Accept(visitor);
        }

        public void SendVisitorToAllNodes(IVisitor<MatrixNode> visitor)
        {
            if (visitor == null)
                return;
            foreach (MatrixNode node in _map.Nodes)
                node.Accept(visitor);
        }

        void Awake() => Preload();

        void OnValidate() => Refresh();

        void OnEnable() => Refresh();

        void OnDisable() { }

        void OnDestroy() { }

        void OnDrawGizmosSelected()
        {
            CustomGizmos.DrawWireRect(
                _info.OriginWorldPosition,
                _info.Dimensions,
                _info.OriginWorldRotation,
                Color.white
            );
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
