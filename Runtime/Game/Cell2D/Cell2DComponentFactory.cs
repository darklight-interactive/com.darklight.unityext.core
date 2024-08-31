using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    using ComponentType = ICell2DComponent.TypeKey;

    public static class Cell2DComponentFactory
    {
        private static Dictionary<ComponentType, Func<Cell2D, ICell2DComponent>> _componentFactory = new Dictionary<ComponentType, Func<Cell2D, ICell2DComponent>>();

        // Static constructor to initialize the factory with component registrations
        static Cell2DComponentFactory()
        {
            RegisterComponent(ComponentType.Base, (cell) => new Base_Cell2DComponent(cell));
            RegisterComponent(ComponentType.Shape, (cell) => new Shape_Cell2DComponent(cell));
            RegisterComponent(ComponentType.Weight, (cell) => new Weighted_Cell2DComponent(cell));
            RegisterComponent(ComponentType.Overlap, (cell) => new Overlap_Cell2DComponent(cell));
        }

        // Method to register a component creation function
        static void RegisterComponent(ComponentType type, Func<Cell2D, ICell2DComponent> factoryMethod)
        {
            if (!_componentFactory.ContainsKey(type))
            {
                _componentFactory[type] = factoryMethod;
            }
            else
            {
                Debug.LogWarning($"Component type {type} is already registered in the factory.");
            }
        }

        // Method to create a component based on the TypeKey
        public static ICell2DComponent CreateComponent(ComponentType type, Cell2D cell)
        {
            if (_componentFactory.TryGetValue(type, out Func<Cell2D, ICell2DComponent> factoryMethod))
            {
                return factoryMethod(cell);
            }

            Debug.LogError($"Component type {type} is not registered in the factory.");
            return null;
        }
    }
}
