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
    [RequireComponent(typeof(Grid2D))]
    public abstract class Grid2D_Component :
        MonoBehaviour,
        IComponent<Grid2D, Grid2D_Component.TypeTag>
    {
        [SerializeField, ShowOnly] protected int guid;
        [SerializeField, ShowOnly] protected TypeTag _type;
        protected Grid2D baseGrid;

        public virtual void InitializeComponent(Grid2D baseObj)
        {
            guid = System.Guid.NewGuid().GetHashCode();
            _type = GetTypeTag();

            baseGrid = baseObj;
        }

        public abstract void UpdateComponent();
        public abstract void DrawGizmos();
        public abstract void DrawEditorGizmos();
        public abstract TypeTag GetTypeTag();

        // ======== [[ NESTED TYPES ]] ================================== >>>>
        public enum TypeTag
        {
            BASE = 0,
            CONFIG = 1
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Grid2D_Component), true)]
    public class Grid2D_ComponentCustomEditor : UnityEditor.Editor
    {
        SerializedObject _serializedObject;
        Grid2D_Component _script;

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            // < DEFAULT INSPECTOR > ------------------------------------ >>
            CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif


}
