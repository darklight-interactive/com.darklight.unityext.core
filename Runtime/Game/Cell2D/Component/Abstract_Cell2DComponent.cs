using System;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{

    [System.Serializable]
    public abstract class Abstract_Cell2DComponent
    {
        [SerializeField, ShowOnly] string _name = "AbstractCellComponent";
        [SerializeField, ShowOnly] int _guid = System.Guid.NewGuid().GetHashCode();
        Cell2D _cell;
        ICell2DComponent.TypeKey _type;
        bool _initialized = false;

        public string Name { get => _name; protected set => _name = value; }
        public int Guid { get => _guid; }
        public Cell2D Cell { get => _cell; protected set => _cell = value; }
        public ICell2DComponent.TypeKey Type { get => _type; protected set => _type = value; }
        public bool initialized { get => _initialized; protected set => _initialized = value; }

        public virtual void Initialize(Cell2D cell)
        {
            if (cell == null)
            {
                Debug.LogError("Cannot initialize component with null cell.");
                return;
            }

            this.Cell = cell;
            this.initialized = true;
        }
    }
}