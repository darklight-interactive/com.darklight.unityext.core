using UnityEngine;
using Darklight.UnityExt.Behaviour.Interface;
using Darklight.UnityExt.Editor;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{

    /// <summary>
    /// The base MonoBehaviour class for all Grid2D components. <br/>
    /// <para>Grid2D components are used to extend the functionality of a Grid2D object.</para>
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Grid2D))]
    public abstract class Grid2D_Component :
        MonoBehaviour,
        IComponent<Grid2D, Grid2D_Component.TypeTag>
    {
        [SerializeField, ShowOnly] protected int guid;
        [SerializeField, ShowOnly] protected TypeTag type;
        protected Grid2D baseGrid;

        // (( VISITORS )) -------- )))
        Cell2D.Visitor _gizmoVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            cell.ComponentReg.DrawComponentGizmos();
        });

        Cell2D.Visitor _editorGizmoVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            cell.ComponentReg.DrawComponentEditorGizmos();
        });

        // ======== [[ METHODS ]] ================================== >>>>
        // -- (( UNITY METHODS )) -------- ))
        public void Awake()
        {
            InitializeComponent(GetComponent<Grid2D>());
        }

        public void Update()
        {
            UpdateComponent();
        }

        public void OnDrawGizmos()
        {
            DrawGizmos();
        }

        // -- (( INTERFACE METHODS )) -------- ))
        public virtual void InitializeComponent(Grid2D baseObj)
        {
            guid = System.Guid.NewGuid().GetHashCode();
            type = GetTypeTag();
            baseGrid = baseObj;
        }

        public abstract void UpdateComponent();
        public virtual void DrawGizmos() => baseGrid.SendVisitorToAllCells(_gizmoVisitor);
        public virtual void DrawEditorGizmos() => baseGrid.SendVisitorToAllCells(_editorGizmoVisitor);
        public abstract TypeTag GetTypeTag();

        // ======== [[ NESTED TYPES ]] ================================== >>>>
        public enum TypeTag
        {
            BASE = 0,
            CONFIG = 1,
            OVERLAP = 2,
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Grid2D_Component), true)]
    public class Grid2D_ComponentCustomEditor : UnityEditor.Editor
    {
        SerializedObject _serializedObject;
        Grid2D_Component _script;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (Grid2D_Component)target;
            _script.Awake();
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            // < DEFAULT INSPECTOR > ------------------------------------ >>
            CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                _script.UpdateComponent();
            }
        }
    }
#endif


}
