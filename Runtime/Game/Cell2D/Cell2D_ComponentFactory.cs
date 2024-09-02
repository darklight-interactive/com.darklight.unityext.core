using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    partial class Cell2D
    {
        public static class ComponentFactory
        {
            private static Dictionary<Cell2D_Component.Type, Func<Cell2D, Cell2D_Component>> _componentFactory = new Dictionary<Cell2D_Component.Type, Func<Cell2D, Cell2D_Component>>();

            // Static constructor to initialize the factory with component registrations
            static ComponentFactory()
            {
                RegisterComponent(Cell2D_Component.Type.BASE, (Cell2D cell) => new Cell2D_BaseComponent(cell));
                RegisterComponent(Cell2D_Component.Type.OVERLAP, (Cell2D cell) => new Cell2D_OverlapComponent(cell));
                RegisterComponent(Cell2D_Component.Type.SHAPE, (Cell2D cell) => new Cell2D_ShapeComponent(cell));
                RegisterComponent(Cell2D_Component.Type.WEIGHT, (Cell2D cell) => new Cell2D_WeightComponent(cell));
                RegisterComponent(Cell2D_Component.Type.SPAWNER, (Cell2D cell) => new Cell2D_SpawnerComponent(cell));
            }

            // Method to register a component creation function
            static void RegisterComponent(Cell2D_Component.Type type, Func<Cell2D, Cell2D_Component> factoryMethod)
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
            public static Cell2D_Component CreateComponent(Cell2D_Component.Type type, Cell2D cell)
            {
                if (_componentFactory.TryGetValue(type, out Func<Cell2D, Cell2D_Component> factoryMethod))
                {
                    return factoryMethod(cell);
                }

                Debug.LogError($"Component type {type} is not registered in the factory.");
                return null;
            }
        }
    }
}
