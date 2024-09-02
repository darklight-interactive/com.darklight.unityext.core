using Darklight.UnityExt.Behaviour.Interface;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    partial class Cell2D
    {
        [System.Serializable]
        public abstract class Component : IComponent<Cell2D, Component.Type>
        {
            [SerializeField, ShowOnly] int _guid = System.Guid.NewGuid().GetHashCode();
            [SerializeField, ShowOnly] bool _initialized = false;

            Cell2D _baseCell;
            Type _tag;

            // ======== [[ PROPERTIES ]] ================================== >>>>
            public int Guid { get => _guid; }
            public Cell2D Base { get => _baseCell; }
            public Type Tag { get => _tag; protected set => _tag = value; }

            // ======== [[ CONSTRUCTORS ]] ================================== >>>>
            public Component(Cell2D cell)
            {
                this._baseCell = cell;
                this._tag = Type.BASE;
                Initialize();
            }

            // ======== [[ METHODS ]] ================================== >>>>
            public abstract void Initialize();
            public abstract void Update();
            public abstract void DrawGizmos();
            public abstract void DrawEditorGizmos();

            // ======== [[ NESTED TYPES ]] ================================== >>>>
            /// <summary>
            /// Enum to represent the different types of components that can be attached to a cell.
            /// Intended to be used as a bit mask to determine which components are present on a cell.
            /// </summary>
            public enum Type
            {
                BASE = 0,
                OVERLAP = 1,
                SHAPE = 1,
                WEIGHT = 1
            }
        }
    }
}