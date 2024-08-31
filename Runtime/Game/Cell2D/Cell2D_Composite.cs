using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    using ComponentType = ICell2DComponent.TypeKey;

    [System.Serializable]
    public class Cell2D_Composite
    {
        Cell2D _cell;
        ComponentType _componentFlags;
        Dictionary<ComponentType, ICell2DComponent> _componentMap = new();

        // ======== [[ SERIALIZED FIELDS ]] ======================================================= >>>>
        [SerializeReference, NonReorderable] List<ICell2DComponent> _components = new();

        // ======== [[ PROPERTIES ]] ======================================================= >>>>

        // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
        public Cell2D_Composite(Cell2D cell)
        {
            _cell = cell;
            _componentFlags = cell.Config.ComponentFlags;

            _componentMap = new Dictionary<ComponentType, ICell2DComponent>();
            _components = new List<ICell2DComponent>();
        }

        public Cell2D_Composite(Cell2D_Composite originComposite)
        {
            _cell = originComposite._cell;
            _componentFlags = originComposite._componentFlags;

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
            _componentMap = new Dictionary<ComponentType, ICell2DComponent>();
            foreach (ICell2DComponent component in originComponents)
            {
                ICell2DComponent newComponent = Cell2DComponentFactory.CreateComponent(component.Type, _cell);
                newComponent.Copy(component);
                _componentMap.Add(newComponent.Type, newComponent);
            }
            Refresh();
        }

        public void UpdateComponents(Cell2D_Config config)
        {
            if (_componentMap == null)
            {
                _componentMap = new Dictionary<ComponentType, ICell2DComponent>();
                Debug.LogError("Component map is null. Creating new map.");
            }

            bool ShouldHaveComponent(ComponentType type)
            {
                return (config.ComponentFlags & type) == type;
            }

            // Iterate through the component types and update the cell accordingly
            foreach (ComponentType type in System.Enum.GetValues(typeof(ComponentType)))
            {
                bool shouldHaveComponent = ShouldHaveComponent(type);
                if (shouldHaveComponent)
                {
                    // If the cell does not have the component, add it
                    if (!HasComponent(type))
                    {
                        ICell2DComponent component = Cell2DComponentFactory.CreateComponent(type, _cell);
                        if (component != null)
                        {
                            AddComponent(component);
                        }
                    }
                    else
                    {
                        // If the cell has the component, update it
                        _componentMap[type].Update();
                    }
                }
                else
                {
                    // If the cell has the component, remove it
                    if (HasComponent(type))
                    {
                        RemoveComponent(type);
                    }
                }
            }
        }

        public void AddComponent(ICell2DComponent component)
        {
            // If the component is not already in the dictionary, add it.
            if (!_componentMap.ContainsKey(component.Type))
            {
                _componentMap.Add(component.Type, component);
                //Debug.Log($"Added component {component.Name} to cell {_cell.Data.Key}");
            }
            Refresh();
        }

        public void OverrideComponent(ICell2DComponent component)
        {
            // If the component is in the dictionary, update it.
            if (_componentMap.ContainsKey(component.Type))
            {
                _componentMap[component.Type] = component;
            }
            Refresh();
        }

        public void RemoveComponent(ICell2DComponent component)
        {
            // If the component is in the dictionary, remove it.
            if (_componentMap.ContainsKey(component.Type))
            {
                _componentMap.Remove(component.Type);
            }
            Refresh();
        }

        public void RemoveComponent(ComponentType type)
        {
            // If the component is in the dictionary, remove it.
            if (_componentMap.ContainsKey(type))
            {
                _componentMap.Remove(type);
            }
            Refresh();
        }

        public bool HasComponent(ComponentType type)
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