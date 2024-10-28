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

        [SerializeField] Context _context;

        [Header("Data")]
        [SerializeField, ShowOnly] Vector3 _position;
        [SerializeField, ShowOnly] Quaternion _rotation;
        [SerializeField, ShowOnly] Vector3 _normal;


        [Header("Matrix Config")]
        [SerializeField, Expandable] MatrixContextPreset _contextPreset;
        [SerializeField, HideIf("HasConfigPreset"), AllowNesting] SerializedConfig _config = new SerializedConfig();

        [Header("Map")]
        [SerializeField] NodeMap _map;


        [ShowOnly] public bool isPreloaded;
        [ShowOnly] public bool isInitialized;


        public bool HasConfigPreset => _contextPreset != null;
        public SerializedConfig InternalConfig => _config;

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
            isPreloaded = false;
            isInitialized = false;

            // Create a new config if none exists
            if (_config == null)
                _config = new SerializedConfig();

            // Create a new cell map
            _map = new NodeMap(new Context(this));

            // Determine if the grid was preloaded
            isPreloaded = true;
        }

        protected void Initialize()
        {
            if (!isPreloaded)
                Preload();

            isInitialized = true;
        }
        #endregion

        #region < PUBLIC_METHODS > [[ Matrix Handlers ]] ================================================================ 

        public Context GetContext()
        {
            _context = new Context(this);
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
            if (!isPreloaded)
            {
                Preload();
                return;
            }

            // Initialize if not already
            if (!isInitialized || _config == null)
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
            _config = new SerializedConfig();
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
        public enum Alignment
        {
            TopLeft, TopCenter, TopRight,
            MiddleLeft, MiddleCenter, MiddleRight,
            BottomLeft, BottomCenter, BottomRight
        }

        [Serializable]
        public struct Context
        {
            public Matrix Matrix;
            public Alignment MatrixAlignment;
            public int MatrixRows;
            public int MatrixColumns;
            public Vector3 MatrixPosition => Matrix.transform.position;
            public Vector3 MatrixNormal;
            public Vector2 NodeDimensions;
            public Vector2 NodeSpacing;
            public Vector2 NodeBonding;

            public Context(Matrix matrix)
            {
                Matrix = matrix;

                MatrixAlignment = Alignment.MiddleCenter;

                MatrixRows = 3;
                MatrixColumns = 3;

                MatrixNormal = Vector2.up;

                NodeDimensions = new Vector2(1, 1);
                NodeSpacing = new Vector2(0, 0);
                NodeBonding = new Vector2(0, 0);
            }
        }
    }
}
