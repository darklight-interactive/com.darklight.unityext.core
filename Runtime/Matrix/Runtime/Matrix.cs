using System;
using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Matrix
{
    [ExecuteAlways]
    public partial class Matrix : MonoBehaviour
    {
        #region < STATIC_FIELDS > ======================================================================
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
        #endregion

        [SerializeField]
        StateMachine _stateMachine = new StateMachine();

        [SerializeField]
        Info _info;

        [SerializeField]
        Map _map;

        public delegate bool VisitNodeEvent(Node node);
        public delegate bool VisitPartitionEvent(Partition partition);

        public Info GetInfo() => _info;

        public Map GetMap() => _map;

        #region < PROTECTED_METHODS > [[ Initializer Methods ]] ==================================================================================

        protected virtual void Initialize(Info info = null)
        {
            _info = info;
            if (_info == null)
                _info = new Info(this.transform);
            _info.Validate();

            _map = new Map(this);
            _stateMachine = new StateMachine();
        }

        protected virtual void Refresh()
        {
            _map.Refresh();
            SendVisitorToAllNodes(
                new Node.Visitor(node =>
                {
                    node.Refresh();
                    return true;
                })
            );
        }

        #endregion



        #region < PROTECTED_METHODS > [[ Send Visitor Methods ]] ==================================================================================
        protected void SendVisitorToNode(Vector2Int key, IVisitor<Node> visitor)
        {
            if (visitor == null)
                return;
            _map.GetNodeByKey(key)?.AcceptVisitor(visitor);
        }

        protected void SendVisitorToNodes(List<Vector2Int> keys, IVisitor<Node> visitor)
        {
            if (visitor == null)
                return;
            foreach (Vector2Int key in keys)
                _map.GetNodeByKey(key)?.AcceptVisitor(visitor);
        }

        protected void SendVisitorToAllNodes(IVisitor<Node> visitor)
        {
            if (visitor == null)
                return;
            foreach (Node node in _map.GetAllNodes())
                node.AcceptVisitor(visitor);
        }
        #endregion

        #region < PROTECTED_METHODS > [[ Unity Methods ]] ==================================================================================
        protected virtual void Awake() => Initialize();
        #endregion


#if UNITY_EDITOR


        [CustomEditor(typeof(Matrix), true)]
        public class MatrixCustomEditor : UnityEditor.Editor
        {
            SerializedObject _serializedObject;
            Matrix _script;

            // Fields to store the last-known transform state
            private Vector3 _lastPosition;
            private Quaternion _lastRotation;

            private bool _showMatrixInfo = false;

            private void OnEnable()
            {
                _serializedObject = new SerializedObject(target);
                _script = (Matrix)target;

                // Initialize transform state and set up change listeners
                if (_script != null)
                {
                    _lastPosition = _script.transform.position;
                    _lastRotation = _script.transform.rotation;
                }

                EditorApplication.update += CheckTransformChanges;
                Undo.undoRedoPerformed += OnUndoRedo;

                _script.Initialize(_script.GetInfo());
            }

            private void OnDisable()
            {
                EditorApplication.update -= CheckTransformChanges;
                Undo.undoRedoPerformed -= OnUndoRedo;
            }

            #region < PRIVATE_METHODS > [[ Internal Handlers ]] ================================================================
            private void CheckTransformChanges()
            {
                if (_script == null)
                    return;

                // Check for changes in the position, rotation, or scale
                if (
                    _script.transform.position != _lastPosition
                    || _script.transform.rotation != _lastRotation
                )
                {
                    // Update the last-known state
                    _lastPosition = _script.transform.position;
                    _lastRotation = _script.transform.rotation;

                    // Respond to the change
                    //Debug.Log("Transform has changed!");
                    _script.Refresh();

                    // Refresh the editor if needed
                    Repaint();
                }
            }

            private void OnUndoRedo()
            {
                if (_script != null)
                {
                    // Handle undo/redo for the transform changes
                    //Debug.Log("Transform changed due to undo/redo!");
                    _script.Refresh();

                    // Update last-known transform state in case it has changed
                    _lastPosition = _script.transform.position;
                    _lastRotation = _script.transform.rotation;

                    // Refresh the editor if needed
                    Repaint();
                }
            }
            #endregion

            protected virtual void DrawButtons()
            {
                // Add a button to open the Matrix Editor Window
                if (GUILayout.Button("Open Matrix Editor"))
                {
                    // Open the MatrixEditorWindow and pass the current Matrix instance
                    MatrixEditorWindow.ShowWindow(_script);
                }
            }

            public override void OnInspectorGUI()
            {
                DrawButtons();

                EditorGUI.BeginChangeCheck();

                base.OnInspectorGUI();

                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();
                    _script.Refresh();
                }
            }
        }
#endif
    }
}
