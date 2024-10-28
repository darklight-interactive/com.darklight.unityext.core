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
        TopLeft, TopCenter, TopRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        BottomLeft, BottomCenter, BottomRight
    }

    public enum State { INVALID, PRELOADED, INITIALIZED }

    [ExecuteAlways]
    public partial class Matrix : MonoBehaviour
    {
        class StateMachine : SimpleStateMachine<State>
        {
            public StateMachine() : base(State.INVALID) { }
        }

        StateMachine _stateMachine = new StateMachine();

        [Header("SerializeData")]
        [SerializeField, ShowOnly] State _currentState;
        [SerializeField, ShowOnly] Vector3 _position;
        [SerializeField, ShowOnly] Vector3 _rotation;
        [SerializeField, ShowOnly] Vector3 _normal;

        [Space(5)]
        [SerializeField, ShowOnly] Vector2 _alignmentOffset;

        [Space(5)]
        [SerializeField, ShowOnly] Vector2Int _originKey;
        [SerializeField, ShowOnly] Vector3 _originPosition;



        [Header("Map")]
        [SerializeField] Map _map;

        public Node.Visitor UpdateNodeContextVisitor => new Node.Visitor(node =>
        {
            node.UpdateContext(_map.Info);
            return true;
        });

        public Node.Visitor DrawGizmosVisitor;
        public Node.Visitor DrawGizmosSelectedVisitor = new Node.Visitor(node =>
        {
            CustomGizmos.DrawWireRect(node.Position, node.Dimensions, node.Rotation, Color.white);
            CustomGizmos.DrawLabel(node.Key.ToString(), node.Position, CustomGUIStyles.CenteredStyle);
            return true;
        });

        public State CurrentState => _currentState = _stateMachine.CurrentState;
        public MapInfo MapInfo => _map.Info;

        #region < PRIVATE_METHODS > [[ Unity Runtime ]] ================================================================
        void Awake() => Preload();
        void Start() => Initialize();
        void Update() => Refresh();
        void OnDrawGizmos() => SendVisitorToAllNodes(DrawGizmosVisitor);
        void OnDrawGizmosSelected() => SendVisitorToAllNodes(DrawGizmosSelectedVisitor);
        void OnValidate() => Refresh();
        void OnEnable() {}
        void OnDisable() {}
        void OnDestroy() {}
        #endregion

        #region < PUBLIC_METHODS > [[ Internal Runtime ]] ================================================================
        void OnStateChanged(State state)
        {
            _currentState = state;
        }

        public void Preload()
        {
            if (_stateMachine == null)
            {
                _stateMachine = new StateMachine();
                _stateMachine.OnStateChanged += OnStateChanged;
            }

            // Create a new cell map
            _map = new Map();

            // Determine if the grid was preloaded
            _stateMachine.GoToState(State.PRELOADED);
        }

        public void Initialize()
        {
            _stateMachine.GoToState(State.INITIALIZED);
        }
        #endregion

        #region < PUBLIC_METHODS > [[ Matrix Handlers ]] ================================================================ 



        public void Refresh()
        {
            _map.Refresh();
        }

        public void Reset()
        {
            Preload();
        }

        #endregion

        #region < PUBLIC_METHODS > [[ Visitor Handlers ]] ================================================================ 
        public void SendVisitorToNode(Vector2Int key, IVisitor<Node> visitor)
        {
            if (visitor == null) return;
            _map.GetNode(key)?.Accept(visitor);
        }

        public void SendVisitorToNodes(List<Vector2Int> keys, IVisitor<Node> visitor)
        {
            if (visitor == null) return;
            foreach (Vector2Int key in keys)
                _map.GetNode(key)?.Accept(visitor);
        }

        public void SendVisitorToAllNodes(IVisitor<Node> visitor)
        {
            if (visitor == null) return;
            foreach (Node node in _map.Nodes)
                node.Accept(visitor);
        }
        #endregion

        public static void SendVisitorToNode(Node node, IVisitor<Node> visitor)
        {
            if (node == null) return;
            if (visitor == null) return;

            node.Accept(visitor);
        }

        public static void SendVisitorToNodes(List<Node> nodes, IVisitor<Node> visitor)
        {
            if (nodes == null || nodes.Count == 0) return;
            if (visitor == null) return;

            foreach (Node node in nodes) node.Accept(visitor);
        }
    }
}