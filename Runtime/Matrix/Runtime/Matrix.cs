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

        [Header("Context")]
        [SerializeField, HideIf("HasContextPreset"), AllowNesting] Context _context;
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
            CustomGizmos.DrawWireRect(node.Position, node.Dimensions, node.Rotation, Color.white);
            CustomGizmos.DrawLabel(node.Key.ToString(), node.Position, CustomGUIStyles.CenteredStyle);
            return true;
        });

        public State CurrentState => _currentState = _stateMachine.CurrentState;
        public NodeMap Map => _map;
        public bool HasContextPreset => _contextPreset != null;


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

            // Set the context parent
            _context = new Context(this.transform);

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

            _position = _context.MatrixPosition;
            _rotation = _context.MatrixRotation;
            _normal = _context.MatrixNormal;

            _alignmentOffset = _context.CalculateMatrixAlignmentOffset();

            _originKey = _context.CalculateMatrixOriginKey();
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
