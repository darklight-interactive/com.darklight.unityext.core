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
            private static Dictionary<Component.TypeTag, Func<Cell2D, Component>> _componentFactory = new Dictionary<Component.TypeTag, Func<Cell2D, Component>>();

            // Static constructor to initialize the factory with component registrations
            static ComponentFactory()
            {
                RegisterComponent(Component.TypeTag.BASE, (cell) => new Cell2D_BaseComponent(cell));
                RegisterComponent(Component.TypeTag.SHAPE, (cell) => new Cell2D_OverlapComponent(cell));
                RegisterComponent(Component.TypeTag.WEIGHT, (cell) => new Cell2D_WeightComponent(cell));
                RegisterComponent(Component.TypeTag.OVERLAP, (cell) => new Cell2D_OverlapComponent(cell));
            }

            // Method to register a component creation function
            static void RegisterComponent(Component.TypeTag type, Func<Cell2D, Component> factoryMethod)
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
            public static Component CreateComponent(Component.TypeTag type, Cell2D cell)
            {
                if (_componentFactory.TryGetValue(type, out Func<Cell2D, Component> factoryMethod))
                {
                    return factoryMethod(cell);
                }

                Debug.LogError($"Component type {type} is not registered in the factory.");
                return null;
            }
        }
    }
}
