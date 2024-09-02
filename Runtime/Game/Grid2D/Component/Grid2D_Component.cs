using UnityEngine;
using Darklight.UnityExt.Behaviour.Interface;
using Darklight.UnityExt.Editor;


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
        [SerializeField, ShowOnly] int _guid;
        Grid2D _base;
        [SerializeField, ShowOnly] TypeTag _type;

        public virtual void InitializeComponent(Grid2D baseObj)
        {
            _guid = System.Guid.NewGuid().GetHashCode();
            _base = baseObj;
            _type = GetTypeTag();
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

        public static class Factory
        {

        }

    }


}
