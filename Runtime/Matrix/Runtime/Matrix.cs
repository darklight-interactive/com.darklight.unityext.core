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
        Grid _grid;

        [SerializeField]
        protected StateMachine _stateMachine = new StateMachine();

        [SerializeField]
        protected Info _info;

        [SerializeField]
        protected Map _map;

        public delegate bool VisitNodeEvent(Node node);
        public delegate bool VisitPartitionEvent(Partition partition);

        public Info GetInfo() => _info;

        public Map GetMap() => _map;

        public Grid GetGrid()
        {
            if (_grid == null)
                _grid = GetComponent<Grid>();
            return _grid;
        }

        #region < PROTECTED_METHODS > [[ Initializer Methods ]] ==================================================================================

        protected virtual void Initialize(Info info = default)
        {
            if (_grid == null)
                _grid = GetComponent<Grid>();

            _info = info;
            if (!_info.IsValid)
                _info = new Info(this);
            _info.Validate();

            _map = new Map(this);
            _stateMachine = new StateMachine();
        }

        protected virtual void Refresh()
        {
            _map.Refresh();
        }

        #endregion



        #region < PROTECTED_METHODS > [[ Send Visitor Methods ]] ==================================================================================
        protected void SendVisitorToNode(Vector2Int key, IVisitor<Node> visitor)
        {
            if (visitor == null)
                return;
            _map.GetNodeByKey(key).AcceptVisitor(visitor);
        }

        protected void SendVisitorToNodes(List<Vector2Int> keys, IVisitor<Node> visitor)
        {
            if (visitor == null)
                return;
            foreach (Vector2Int key in keys)
                _map.GetNodeByKey(key).AcceptVisitor(visitor);
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
            SerializedProperty _infoProperty;

            // Fields to store the last-known transform and info state
            private Vector3 _lastPosition;
            private Quaternion _lastRotation;
            private SerializedProperty _lastGridValues;

            private void OnEnable()
            {
                _serializedObject = new SerializedObject(target);
                _script = (Matrix)target;
                _infoProperty = _serializedObject.FindProperty("_info");

                // Initialize transform state
                if (_script != null)
                {
                    _lastPosition = _script.transform.position;
                    _lastRotation = _script.transform.rotation;
                    _lastGridValues = _infoProperty.Copy();
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

            private void CheckTransformChanges()
            {
                if (_script == null)
                    return;

                bool hasChanged = false;

                // Check for changes in transform
                if (
                    _script.transform.position != _lastPosition
                    || _script.transform.rotation != _lastRotation
                )
                {
                    _lastPosition = _script.transform.position;
                    _lastRotation = _script.transform.rotation;
                    hasChanged = true;
                }

                // Check for changes in grid values
                if (!SerializedProperty.EqualContents(_infoProperty, _lastGridValues))
                {
                    _lastGridValues = _infoProperty.Copy();
                    hasChanged = true;
                }

                if (hasChanged)
                {
                    _script.Refresh();
                    SceneView.RepaintAll();
                }
            }

            private void OnUndoRedo()
            {
                if (_script != null)
                {
                    _lastPosition = _script.transform.position;
                    _lastRotation = _script.transform.rotation;
                    _lastGridValues = _infoProperty.Copy();
                    _script.Refresh();
                    SceneView.RepaintAll();
                }
            }

            protected virtual void DrawButtons()
            {
                // Add a button to open the Matrix Editor Window
                if (GUILayout.Button("Open Matrix Editor"))
                {
                    // Open the MatrixEditorWindow and pass the current Matrix instance
                    MatrixEditorWindow.ShowWindow(_script);
                }

                if (GUILayout.Button("Refresh"))
                {
                    _script.Refresh();
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
