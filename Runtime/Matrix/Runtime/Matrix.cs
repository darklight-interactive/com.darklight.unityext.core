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
        const string ASSET_PATH = "Assets/Resources/Darklight/Matrix";

        class StateMachine : SimpleStateMachine<State>
        {
            public StateMachine() : base(State.INVALID) { }
        }

        StateMachine _stateMachine = new StateMachine();

        [Header("Data")]
        [SerializeField, ShowOnly] State _currentState;
        [SerializeField, ShowOnly] Vector3 _position;
        [SerializeField, ShowOnly] Quaternion _rotation;
        [SerializeField, ShowOnly] Vector3 _normal;

        [Header("Context")]
        [SerializeField, HideIf("HasContextPreset"), AllowNesting] Context _context = new Context(Alignment.MiddleCenter, 3, 3);
        [SerializeField, Expandable] MatrixContextPreset _contextPreset;

        [Header("Map")]
        [SerializeField] NodeMap _map;

        Node.Visitor UpdateNodeContextVisitor => new Node.Visitor(node =>
        {
            node.UpdateContext(GetContext());
            return true;
        });

        Node.Visitor DrawGizmosVisitor;
        Node.Visitor DrawGizmosSelectedVisitor = new Node.Visitor(node =>
        {
            node.GetWorldSpaceValues(out Vector3 position, out Vector2 dimensions, out Vector3 normal);
            CustomGizmos.DrawWireRect(position, dimensions, normal, Color.white);
            CustomGizmos.DrawLabel(node.Key.ToString(), position, CustomGUIStyles.CenteredStyle);
            return true;
        });

        public State CurrentState => _currentState = _stateMachine.CurrentState;
        public bool HasContextPreset => _contextPreset != null;


        #region < PRIVATE_METHODS > [[ Unity Runtime ]] ================================================================
        void Awake() => Preload();
        void Start() => Initialize();
        void Update() => Refresh();
        void OnDrawGizmos() => SendVisitorToAllNodes(DrawGizmosVisitor);
        void OnDrawGizmosSelected() => SendVisitorToAllNodes(DrawGizmosSelectedVisitor);
        void OnValidate() => Refresh();
        #endregion

        #region < NONPUBLIC_METHODS > [[ Internal Runtime ]] ================================================================
        protected void OnStateChanged(State state)
        {
            _currentState = state;
            Debug.Log($"OnStateChanged: Current State: {state}");
        }

        public void Preload()
        {
            if (_stateMachine == null)
            {
                _stateMachine = new StateMachine();
                _stateMachine.OnStateChanged += OnStateChanged;
            }



            // Create a new cell map
            _map = new NodeMap(this);

            // Determine if the grid was preloaded
            _stateMachine.GoToState(State.PRELOADED);
        }

        public void Initialize()
        {
            _stateMachine.GoToState(State.INITIALIZED);
        }
        #endregion

        #region < PUBLIC_METHODS > [[ Matrix Handlers ]] ================================================================ 

        public Context GetContext()
        {
            if (_contextPreset != null && !_context.Equals(_contextPreset.ToData()))
                _context = _contextPreset.ToData();

            if (_context.IsValid() == false)
                _context.Validate();

            return _context;
        }

        public void ExtractConfigToPreset(string name)
        {
            _contextPreset = ScriptableObjectUtility.CreateOrLoadScriptableObject<MatrixContextPreset>(ASSET_PATH, name);
            _contextPreset.SetData(_context);
        }

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
