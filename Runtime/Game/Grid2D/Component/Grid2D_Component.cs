using UnityEngine;
using Darklight.UnityExt.Behaviour.Interface;
using Darklight.UnityExt.Editor;
using UnityEngine.Events;

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
        IComponent<Grid2D, Grid2D.ComponentTypeKey>
    {
        // ======== [[ FIELDS ]] ================================== >>>>
        [SerializeField, ShowOnly] int _guid;
        [SerializeField, ShowOnly] Grid2D.ComponentTypeKey _typeKey;
        Grid2D _baseGrid;


        // ======== [[ PROPERTIES ]] ================================== >>>>
        public int GUID => _guid;
        public Grid2D.ComponentTypeKey TypeKey => _typeKey;
        protected Grid2D BaseGrid
        {
            get
            {
                if (_baseGrid == null) _baseGrid = GetComponent<Grid2D>();
                return _baseGrid;
            }
            private set => _baseGrid = value;
        }
        protected abstract Cell2D.ComponentVisitor CellComponentVisitor { get; }


        // ======== [[ METHODS ]] ================================== >>>>
        // -- (( UNITY METHODS )) -------- ))
        public void Awake() => Initialize(BaseGrid);
        public void Update() => Updater();
        public void OnDrawGizmos() => DrawGizmos();

        // -- (( INTERFACE METHODS )) -------- ))
        public virtual void Initialize(Grid2D baseObj)
        {
            _guid = System.Guid.NewGuid().GetHashCode();
            _typeKey = GetTypeKey();
            BaseGrid = baseObj;
        }
        public virtual void Updater() { }
        public virtual void DrawGizmos() { }
        public virtual void DrawEditorGizmos() { }
        public virtual Grid2D.ComponentTypeKey GetTypeKey() => Grid2D.ComponentRegistry.GetTypeKey(this);


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
                if (_serializedObject == null) return;
                _serializedObject.Update();

                EditorGUI.BeginChangeCheck();

                // < DEFAULT INSPECTOR > ------------------------------------ >>
                CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();
                    _script.Update();
                }
            }

            private void OnSceneGUI()
            {
                _script.DrawEditorGizmos();
            }
        }
#endif
    }

    // == < CLASS > : Grid2D_BaseComponent ============================================ >>>>
    public class Grid2D_BaseComponent : Grid2D_Component
    {
        protected override Cell2D.ComponentVisitor CellComponentVisitor
            => new Cell2D.ComponentVisitor(Cell2D.ComponentTypeKey.BASE);

    }
}