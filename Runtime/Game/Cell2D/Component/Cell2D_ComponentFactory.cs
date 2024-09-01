using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    partial class Cell2D
    {
        public static class Cell2DComponentFactory
        {
            private static Dictionary<ComponentFlags, Func<Cell2D, ICell2DComponent>> _componentFactory = new Dictionary<ComponentFlags, Func<Cell2D, ICell2DComponent>>();

            // Static constructor to initialize the factory with component registrations
            static Cell2DComponentFactory()
            {
                RegisterComponent(ComponentFlags.Base, (cell) => new Component(cell));
                RegisterComponent(ComponentFlags.Shape, (cell) => new Cell2D_OverlapComponent(cell));
                RegisterComponent(ComponentFlags.Weight, (cell) => new Cell2D_WeightComponent(cell));
                RegisterComponent(ComponentFlags.Overlap, (cell) => new Cell2D_OverlapComponent(cell));
            }

            // Method to register a component creation function
            static void RegisterComponent(ComponentFlags type, Func<Cell2D, ICell2DComponent> factoryMethod)
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
            public static ICell2DComponent CreateComponent(ComponentFlags type, Cell2D cell)
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
}
