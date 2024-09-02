using Darklight.UnityExt.Behaviour.Interface;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public abstract class Cell2D_Component : IComponent<Cell2D, Cell2D_Component.Type>
    {
        bool _initialized = false;
        [SerializeField, ShowOnly] int _guid = System.Guid.NewGuid().GetHashCode();
        [SerializeField, ShowOnly] Type _type;
        Cell2D _baseCell;

        // ======== [[ PROPERTIES ]] ================================== >>>>
        protected Cell2D baseCell => _baseCell;

        // ======== [[ CONSTRUCTORS ]] ================================== >>>>
        public Cell2D_Component(Cell2D baseObj) => InitializeComponent(baseObj);

        // ======== [[ METHODS ]] ================================== >>>>
        public virtual void InitializeComponent(Cell2D baseObj)
        {
            _guid = System.Guid.NewGuid().GetHashCode();
            _baseCell = baseObj;
            _type = Type.BASE;
        }
        public abstract void UpdateComponent();
        public abstract Type GetTypeTag();

        public virtual void DrawGizmos()
        {
            _baseCell.DrawDefaultGizmos();
        }
        public virtual void DrawEditorGizmos() { }

        // ======== [[ NESTED TYPES ]] ================================== >>>>
        /// <summary>
        /// Enum to represent the different types of components that can be attached to a cell.
        /// Intended to be used as a bit mask to determine which components are present on a cell.
        /// </summary>
        public enum Type
        {
            BASE = 0,
            OVERLAP = 1,
            SHAPE = 2,
            WEIGHT = 3,
            SPAWNER = 4,
        }
    }
}