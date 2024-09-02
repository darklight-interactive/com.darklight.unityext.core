using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    partial class Cell2D
    {

        [System.Serializable]
        public class ComponentRegistry
        {
            Cell2D _cell;
            Dictionary<Cell2D_Component.Type, Cell2D_Component> _componentMap = new();

            // ======== [[ SERIALIZED FIELDS ]] ======================================================= >>>>
            [SerializeReference, NonReorderable] List<Cell2D_Component> _components = new();

            // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
            public ComponentRegistry(Cell2D cell)
            {
                _cell = cell;
                _componentMap = new Dictionary<Cell2D_Component.Type, Cell2D_Component>();
                _components = new List<Cell2D_Component>();
            }

            public ComponentRegistry(ComponentRegistry originComposite)
            {
                _cell = originComposite._cell;

                LoadComponents(originComposite._components);
            }

            // ======== [[ METHODS ]] ============================================================ >>>>
            public void RegisterComponent(Cell2D_Component.Type type)
            {
                if (!HasComponent(type))
                {
                    Cell2D_Component component = ComponentFactory.CreateComponent(type, _cell);
                    _componentMap.Add(type, component);
                    Refresh();
                }
            }

            public Cell2D_Component GetComponent(Cell2D_Component.Type type)
            {
                if (_componentMap.ContainsKey(type))
                {
                    return _componentMap[type];
                }
                return null;
            }

            public TComponent GetComponent<TComponent>() where TComponent : Cell2D_Component
            {
                foreach (Cell2D_Component component in _components)
                {
                    if (component is TComponent)
                    {
                        return (TComponent)component;
                    }
                }
                return default;
            }

            public void RemoveComponent(Cell2D_Component.Type type)
            {
                // If the component is in the dictionary, remove it.
                if (_componentMap.ContainsKey(type))
                {
                    _componentMap.Remove(type);
                }
                Refresh();
            }

            public void LoadComponents(List<Cell2D_Component> originComponents)
            {
                _componentMap = new Dictionary<Cell2D_Component.Type, Cell2D_Component>();
                foreach (Cell2D_Component component in originComponents)
                {
                    Cell2D_Component.Type type = component.GetTypeTag();
                    Cell2D_Component newComponent = ComponentFactory.CreateComponent(type, _cell);
                    _componentMap.Add(type, newComponent);
                }
                Refresh();
            }

            public bool HasComponent(Cell2D_Component.Type type)
            {
                if (_componentMap.ContainsKey(type))
                {
                    if (_componentMap[type] != null)
                    {
                        return true;
                    }
                }
                return false;
            }

            void Refresh()
            {
                _components = _componentMap.Values.ToList();
            }
        }
    }
}