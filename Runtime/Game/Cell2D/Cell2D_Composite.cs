using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    partial class Cell2D
    {

        [System.Serializable]
        public class Composite
        {
            Cell2D _cell;
            Component.TypeTag _types;
            Dictionary<Component.TypeTag, Component> _componentMap = new();

            // ======== [[ SERIALIZED FIELDS ]] ======================================================= >>>>
            [SerializeReference, NonReorderable] List<Component> _components = new();

            // ======== [[ PROPERTIES ]] ======================================================= >>>>

            // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
            public Composite(Cell2D cell)
            {
                _cell = cell;
                _componentMap = new Dictionary<Component.TypeTag, Component>();
                _components = new List<Component>();
            }

            public Composite(Composite originComposite)
            {
                _cell = originComposite._cell;
                _types = originComposite._types;

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

            public void SetComponentFlags(Component.TypeTag flags)
            {
                _types = flags;
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
                if (_componentMap == null)
                {
                    _componentMap = new Dictionary<Component.TypeTag, Component>();
                    Debug.LogError("Component map is null. Creating new map.");
                }

                bool ShouldHaveComponent(Component.TypeTag type)
                {
                    return (_types & type) == type;
                }

                // Iterate through the component types and update the cell accordingly
                foreach (Component.TypeTag flag in System.Enum.GetValues(typeof(Component.TypeTag)))
                {
                    bool shouldHaveComponent = ShouldHaveComponent(flag);
                    if (shouldHaveComponent)
                    {
                        // If the cell does not have the component, add it
                        if (!HasComponent(flag))
                        {
                            Component component = ComponentFactory.CreateComponent(flag, _cell);
                            if (component != null)
                            {
                                AddComponent(component);
                            }
                        }
                        else
                        {
                            // If the cell has the component, update it
                            _componentMap[flag].UpdateComponent();
                        }
                    }
                    else
                    {
                        // If the cell has the component, remove it
                        if (HasComponent(flag))
                        {
                            RemoveComponent(flag);
                        }
                    }
                }
            }

            public void AddComponent(Component component)
            {
                // If the component is not already in the dictionary, add it.
                if (!_componentMap.ContainsKey(component.Type))
                {
                    _componentMap.Add(component.Type, component);
                    //Debug.Log($"Added component {component.Name} to cell {_cell.Data.Key}");
                }
                Refresh();
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