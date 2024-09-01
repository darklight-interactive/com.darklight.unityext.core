using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{

    partial class Cell2D
    {
        public interface ICell2DComponent
        {
            string Name { get; }
            int Guid { get; }
            Cell2D Cell { get; }
            ComponentFlags Flag { get; }

            void Initialize(Cell2D cell);
            void Update();
            void DrawGizmos();
            void DrawEditorGizmos();
            void Copy(ICell2DComponent component);
        }

        [System.Serializable]
        public class Component : ICell2DComponent
        {
            [SerializeField, ShowOnly] string _name = "BaseComponent";
            [SerializeField, ShowOnly] int _guid = System.Guid.NewGuid().GetHashCode();
            [SerializeField, ShowOnly] bool _initialized = false;

            Cell2D _cell;
            ComponentFlags _flags;

            public string Name { get => _name; protected set => _name = value; }
            public int Guid { get => _guid; }
            public Cell2D Cell { get => _cell; protected set => _cell = value; }
            public ComponentFlags Flag { get => _flags; protected set => _flags = value; }
            public bool initialized { get => _initialized; protected set => _initialized = value; }
            public Component(Cell2D cell) => Initialize(cell);
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

            public virtual void Update()
            {
                if (!initialized) return;
            }

            public virtual void DrawGizmos()
            {
                if (!initialized) return;

                Cell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
                CustomGizmos.DrawWireSquare(position, radius, normal, Color.grey);
            }

            public virtual void DrawEditorGizmos()
            {
                if (!initialized) return;
            }

            public virtual void Copy(ICell2DComponent component)
            {
                if (!initialized) return;
            }
        }
    }
}