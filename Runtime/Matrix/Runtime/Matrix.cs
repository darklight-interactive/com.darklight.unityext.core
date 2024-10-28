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
    [ExecuteAlways]
    public partial class Matrix : MonoBehaviour
    {
        const string ASSET_PATH = "Assets/Resources/Darklight/Matrix";

        StateMachine _stateMachine = new StateMachine();

        [SerializeField, ShowOnly] State _currentState;
        [SerializeField, ShowOnly] Vector3 _position;
        [SerializeField, ShowOnly] Quaternion _rotation;
        [SerializeField, ShowOnly] Vector3 _normal;


        [Header("Context")]
        [SerializeField, DisableIf("HasConfigPreset"), AllowNesting]
        Context _context = new Context(Alignment.MiddleCenter, 3, 3);
        [SerializeField, Expandable] MatrixContextPreset _contextPreset;

        [Header("Map")]
        [SerializeField] NodeMap _map;

        public bool HasConfigPreset => _contextPreset != null;

        Node.Visitor UpdateNodeVisitor => new Node.Visitor(node =>
        {
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


        #region < PRIVATE_METHODS > [[ Unity Runtime ]] ================================================================
        void Awake() => Preload();
        void Start() => Initialize();
        void Update() => Refresh();
        void OnDrawGizmos() => SendVisitorToAllNodes(DrawGizmosVisitor);
        void OnDrawGizmosSelected() => SendVisitorToAllNodes(DrawGizmosSelectedVisitor);
        void OnValidate() => Refresh();
        #endregion

        #region < PROTECTED_METHODS > [[ Internal Runtime ]] ================================================================
        protected void Preload()
        {
            if (_stateMachine == null)
            {
                _stateMachine = new StateMachine();
                _stateMachine.OnStateChanged += OnStateChanged;
            }
            _stateMachine.GoToState(State.INVALID);

            // Create a new cell map
            _map = new NodeMap(_context);


            // Determine if the grid was preloaded
            _stateMachine.GoToState(State.PRELOADED);
        }

        protected void Initialize()
        {
            _stateMachine.GoToState(State.INITIALIZED);
        }

        protected void OnStateChanged(State state)
        {
            _currentState = state;
        }
        #endregion

        #region < PUBLIC_METHODS > [[ Matrix Handlers ]] ================================================================ 

        public Context GetContext()
        {
            _context.MatrixNormal = transform.up;
            return _context;
        }

        public void ExtractConfigToPreset(string name)
        {
            _contextPreset = ScriptableObjectUtility.CreateOrLoadScriptableObject<MatrixContextPreset>(ASSET_PATH, name);
            _contextPreset.SetData(_context);
        }

        public void Refresh()
        {
            if (_stateMachine.CurrentState == State.INVALID)
            {
                Preload();
                return;
            }
            else if (_stateMachine.CurrentState == State.PRELOADED)
            {
                Initialize();
                return;
            }

            if (_contextPreset != null)
                _context = _contextPreset.ToData();

            // Resize the grid if the dimensions have changed
            _map.Refresh();

            // Update the cells
            SendVisitorToAllNodes(UpdateNodeVisitor);
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



        public enum State { INVALID, PRELOADED, INITIALIZED }
        class StateMachine : SimpleStateMachine<State>
        {
            public StateMachine() : base(State.INVALID) { }
        }

        public enum Alignment
        {
            TopLeft, TopCenter, TopRight,
            MiddleLeft, MiddleCenter, MiddleRight,
            BottomLeft, BottomCenter, BottomRight
        }

    }
}
