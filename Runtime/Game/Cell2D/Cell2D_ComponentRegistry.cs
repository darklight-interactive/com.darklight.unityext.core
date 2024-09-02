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

            // ======== [[ PROPERTIES ]] ======================================================= >>>>

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

            public Component RegisterComponent(Component.TypeTag type)
            {
                if (!HasComponent(type))
                {
                    Component component = ComponentFactory.CreateComponent(type, _cell);
                    _componentMap.Add(type, component);
                    Refresh();
                }
                return _componentMap[type];
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

            public void UpdateComponents()
            {
                foreach (Component component in _components)
                {
                    component.UpdateComponent();
                }
            }

            public void OverrideComponent(Component component)
            {
                // If the component is in the dictionary, update it.
                if (_componentMap.ContainsKey(component.Type))
                {
                    _componentMap[component.Type] = component;
                }
                Refresh();
            }

            public void RemoveComponent(Component component)
            {
                // If the component is in the dictionary, remove it.
                if (_componentMap.ContainsKey(component.Type))
                {
                    _componentMap.Remove(component.Type);
                }
                Refresh();
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