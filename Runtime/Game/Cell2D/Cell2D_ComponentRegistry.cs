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
            Dictionary<Component.TypeTag, Component> _componentMap = new();

            // ======== [[ SERIALIZED FIELDS ]] ======================================================= >>>>
            [SerializeReference, NonReorderable] List<Component> _components = new();

            // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
            public ComponentRegistry(Cell2D cell)
            {
                _cell = cell;
                _componentMap = new Dictionary<Component.TypeTag, Component>();
                _components = new List<Component>();
            }

            public ComponentRegistry(ComponentRegistry originComposite)
            {
                _cell = originComposite._cell;

                LoadComponents(originComposite._components);
            }

            // ======== [[ METHODS ]] ============================================================ >>>>
            public void MapFunction(System.Action<Component> function)
            {
                foreach (Component component in _components)
                {
                    function(component);
                }
            }

            public void RegisterComponent(Component.TypeTag type)
            {
                if (!HasComponent(type))
                {
                    Component component = ComponentFactory.CreateComponent(type, _cell);
                    _componentMap.Add(type, component);
                    Refresh();
                }
            }

            public Component GetComponent(Component.TypeTag type)
            {
                if (_componentMap.ContainsKey(type))
                {
                    return _componentMap[type];
                }
                return null;
            }

            public TComponent GetComponent<TComponent>() where TComponent : Component
            {
                foreach (Component component in _components)
                {
                    if (component is TComponent)
                    {
                        return (TComponent)component;
                    }
                }
                return default;
            }

            public void RemoveComponent(Component.TypeTag type)
            {
                // If the component is in the dictionary, remove it.
                if (_componentMap.ContainsKey(type))
                {
                    _componentMap.Remove(type);
                }
                Refresh();
            }

            public void LoadComponents(List<Component> originComponents)
            {
                _componentMap = new Dictionary<Component.TypeTag, Component>();
                foreach (Component component in originComponents)
                {
                    Component newComponent = ComponentFactory.CreateComponent(component.Type, _cell);
                    _componentMap.Add(newComponent.Type, newComponent);
                }
                Refresh();
            }

            public bool HasComponent(Component.TypeTag type)
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