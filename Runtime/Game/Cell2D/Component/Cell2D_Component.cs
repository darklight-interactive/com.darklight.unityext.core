using Darklight.UnityExt.Behaviour.Interface;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    partial class Cell2D
    {
        [System.Serializable]
        public abstract class Component : IComponent<Cell2D, Component.TypeTag>
        {
            bool _initialized = false;
            [SerializeField, ShowOnly] int _guid = System.Guid.NewGuid().GetHashCode();
            TypeTag _type;
            Cell2D _baseCell;

            // ======== [[ PROPERTIES ]] ================================== >>>>
            public int GUID { get => _guid; }
            public TypeTag Type { get => _type; }
            public Cell2D Base { get => _baseCell; }

            // ======== [[ CONSTRUCTORS ]] ================================== >>>>
            public Component(Cell2D baseObj) => InitializeComponent(baseObj);

            // ======== [[ METHODS ]] ================================== >>>>
            public virtual void InitializeComponent(Cell2D baseObj)
            {
                _guid = System.Guid.NewGuid().GetHashCode();
                _baseCell = baseObj;
                _type = TypeTag.BASE;
            }
            public abstract void UpdateComponent();
            public abstract void DrawGizmos();
            public abstract void DrawEditorGizmos();
            public abstract TypeTag GetTypeTag();

            // ======== [[ NESTED TYPES ]] ================================== >>>>
            /// <summary>
            /// Enum to represent the different types of components that can be attached to a cell.
            /// Intended to be used as a bit mask to determine which components are present on a cell.
            /// </summary>
            public enum TypeTag
            {
                BASE = 0,
                OVERLAP = 1,
                SHAPE = 1,
                WEIGHT = 1
            }
        }
    }
}