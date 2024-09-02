using Darklight.UnityExt.Behaviour.Interface;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{

    public partial class Cell2D
    {

        [System.Serializable]
        public abstract class Component :
            IComponent<Cell2D, ComponentTypeKey>,
            IVisitable<Component>
        {
            Cell2D _baseCell;
            [SerializeField, ShowOnly] int _guid = System.Guid.NewGuid().GetHashCode();
            [SerializeField, ShowOnly] Cell2D.ComponentTypeKey _type;
            bool _initialized = false;
            // ======== [[ PROPERTIES ]] ================================== >>>>
            public Cell2D Cell => _baseCell;
            public int GUID => _guid;
            public ComponentTypeKey Type => _type;
            public bool Initialized => _initialized;


            // ======== [[ CONSTRUCTORS ]] ================================== >>>>
            public Component(Cell2D baseObj) => Initialize(baseObj);

            // ======== [[ METHODS ]] ================================== >>>>
            public virtual void Initialize(Cell2D baseObj)
            {
                _guid = System.Guid.NewGuid().GetHashCode();
                _baseCell = baseObj;
                _type = Cell2D.ComponentTypeKey.BASE;
                _initialized = true;
            }
            public abstract void Updater();
            public abstract Cell2D.ComponentTypeKey GetTypeKey();
            public virtual void DrawGizmos()
            {
                // Draw the base cell gizmos by default
                _baseCell.DrawDefaultGizmos();
            }
            public virtual void DrawEditorGizmos() { }

            // ---- (( VISITOR METHODS )) ---- >>
            public virtual void Accept(IVisitor<Component> visitor)
            {
                visitor.Visit(this);
            }
        }
    }
}