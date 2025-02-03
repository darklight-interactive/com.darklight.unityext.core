using System;
using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Utility;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

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
        class StateMachine : SimpleStateMachine<State>
        {
            public StateMachine()
                : base(State.INVALID) { }
        }

        StateMachine _stateMachine = new StateMachine();

        [Header("SerializeData")]
        [SerializeField, ShowOnly]
        State _currentState;

        [SerializeField, AllowNesting]
        Info _info;

        [SerializeField]
        NodeMap _map;

        public Node.Visitor UpdateNodeContextVisitor =>
            new Node.Visitor(node =>
            {
                node.Refresh();
                return true;
            });

        public Node.Visitor DrawGizmosVisitor;
        public Node.Visitor DrawGizmosSelectedVisitor = new Node.Visitor(node =>
        {
            CustomGizmos.DrawWireRect(node.Position, node.Dimensions, node.Rotation, Color.white);
            CustomGizmos.DrawLabel(
                node.Coordinate.ToString(),
                node.Position,
                CustomGUIStyles.CenteredStyle
            );
            return true;
        });

        public State CurrentState => _currentState = _stateMachine.currentStateEnum;

        #region < PRIVATE_METHODS > [[ Unity Runtime ]] ================================================================
        void Awake() => Preload();

        void Start() { }

        void Update() { }

        void OnDrawGizmos() => SendVisitorToAllNodes(DrawGizmosVisitor);

        void OnDrawGizmosSelected() => SendVisitorToAllNodes(DrawGizmosSelectedVisitor);

        void OnValidate() { }

        void OnEnable() => Refresh();

        void OnDisable() { }

        void OnDestroy() { }
        #endregion

        #region < PRIVATE_METHODS > [[ State Machine ]] ================================================================
        void OnStateChanged(State state)
        {
            _currentState = state;
        }
        #endregion

        public void Preload()
        {
            if (_stateMachine == null)
            {
                _stateMachine = new StateMachine();
                _stateMachine.OnStateChanged += OnStateChanged;
            }

            // Create a new cell map
            if (_info == null)
                _info = Info.CreateDefault(transform);
            _map = new NodeMap(this);

            // Determine if the grid was preloaded
            _stateMachine.GoToState(State.PRELOADED);
        }

        public void Refresh()
        {
            _map.Refresh();
            SendVisitorToAllNodes(UpdateNodeContextVisitor);
        }

        #region < PUBLIC_METHODS > [[ Getters ]] ================================================================

        public Info GetInfo()
        {
            return _info;
        }

        #endregion

        #region < PUBLIC_METHODS > [[ Visitor Handlers ]] ================================================================
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
        #endregion
    }
}
