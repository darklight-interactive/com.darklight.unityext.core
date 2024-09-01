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
            ComponentFlags _flags;
            Dictionary<ComponentFlags, ICell2DComponent> _componentMap = new();

            // ======== [[ SERIALIZED FIELDS ]] ======================================================= >>>>
            [SerializeReference, NonReorderable] List<ICell2DComponent> _components = new();

            // ======== [[ PROPERTIES ]] ======================================================= >>>>

            // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
            public Composite(Cell2D cell)
            {
                _cell = cell;
                _flags = cell.config.ComponentFlags;

                _componentMap = new Dictionary<ComponentFlags, ICell2DComponent>();
                _components = new List<ICell2DComponent>();
            }

            public Composite(Composite originComposite)
            {
                _cell = originComposite._cell;
                _flags = originComposite._flags;

                LoadComponents(originComposite._components);
            }

            // ======== [[ METHODS ]] ============================================================ >>>>
            public void MapFunction(System.Action<ICell2DComponent> function)
            {
                foreach (ICell2DComponent component in _components)
                {
                    function(component);
                }
            }

            public void LoadComponents(List<ICell2DComponent> originComponents)
            {
                _componentMap = new Dictionary<ComponentFlags, ICell2DComponent>();
                foreach (ICell2DComponent component in originComponents)
                {
                    ICell2DComponent newComponent = Cell2DComponentFactory.CreateComponent(component.Flag, _cell);
                    newComponent.Copy(component);
                    _componentMap.Add(newComponent.Flag, newComponent);
                }
                Refresh();
            }

            public void UpdateComponents(Config config)
            {
                if (_componentMap == null)
                {
                    _componentMap = new Dictionary<ComponentFlags, ICell2DComponent>();
                    Debug.LogError("Component map is null. Creating new map.");
                }

                bool ShouldHaveComponent(ComponentFlags type)
                {
                    return (config.ComponentFlags & type) == type;
                }

                // Iterate through the component types and update the cell accordingly
                foreach (ComponentFlags flag in System.Enum.GetValues(typeof(ComponentFlags)))
                {
                    bool shouldHaveComponent = ShouldHaveComponent(flag);
                    if (shouldHaveComponent)
                    {
                        // If the cell does not have the component, add it
                        if (!HasComponent(flag))
                        {
                            ICell2DComponent component = Cell2DComponentFactory.CreateComponent(flag, _cell);
                            if (component != null)
                            {
                                AddComponent(component);
                            }
                        }
                        else
                        {
                            // If the cell has the component, update it
                            _componentMap[flag].Update();
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

            public void AddComponent(ICell2DComponent component)
            {
                // If the component is not already in the dictionary, add it.
                if (!_componentMap.ContainsKey(component.Flag))
                {
                    _componentMap.Add(component.Flag, component);
                    //Debug.Log($"Added component {component.Name} to cell {_cell.Data.Key}");
                }
                Refresh();
            }

            public void OverrideComponent(ICell2DComponent component)
            {
                // If the component is in the dictionary, update it.
                if (_componentMap.ContainsKey(component.Flag))
                {
                    _componentMap[component.Flag] = component;
                }
                Refresh();
            }

            public void RemoveComponent(ICell2DComponent component)
            {
                // If the component is in the dictionary, remove it.
                if (_componentMap.ContainsKey(component.Flag))
                {
                    _componentMap.Remove(component.Flag);
                }
                Refresh();
            }

            public void RemoveComponent(ComponentFlags type)
            {
                // If the component is in the dictionary, remove it.
                if (_componentMap.ContainsKey(type))
                {
                    _componentMap.Remove(type);
                }
                Refresh();
            }

            public bool HasComponent(ComponentFlags type)
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