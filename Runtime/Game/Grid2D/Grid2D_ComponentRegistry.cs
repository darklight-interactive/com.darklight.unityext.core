using System;
using System.Collections.Generic;
using System.ComponentModel;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public partial class Grid2D
    {
        /// <summary>
        /// The type of Grid2D component.
        /// </summary>
        /// <remarks>
        /// <para>Each Grid2D component has a type tag that can be used to identify the component.</para>
        /// </remarks>
        public enum ComponentTypeKey
        {
            BASE = 0,
            CONFIG = 1,
            OVERLAP = 2,
            WEIGHT = 3,
            WEIGHTED_SPAWNER = 4,
        }

        public class ComponentRegistry
        {
            // ======== [[ STATIC FIELDS ]] ================================== >>>>
            static Dictionary<ComponentTypeKey, Type> _componentTypeMap = new Dictionary<ComponentTypeKey, Type>()
            {
                { ComponentTypeKey.BASE, typeof(Grid2D_BaseComponent) },
                { ComponentTypeKey.CONFIG, typeof(Grid2D_ConfigComponent) },
                { ComponentTypeKey.OVERLAP, typeof(Grid2D_OverlapComponent) },
                { ComponentTypeKey.WEIGHT, typeof(Grid2D_WeightComponent) },
                { ComponentTypeKey.WEIGHTED_SPAWNER, typeof(Grid2D_WeightedSpawnerComponent) },
            };

            Grid2D _grid;
            [SerializeField, ShowOnly, NonReorderable]
            List<Grid2D_Component> _components = new List<Grid2D_Component>();
            public ComponentRegistry(Grid2D grid)
            {
                _grid = grid;
                grid.OnGridInitialized += InitializeComponents;
                grid.OnGridUpdated += UpdateComponents;
            }

            // ======== [[ METHODS ]] ================================== >>>>
            void InitializeComponents()
            {
                _components.Clear();
                _grid.GetComponentsInChildren(_components);

                MapComponents((Grid2D_Component component) =>
                {
                    component.Initialize(_grid);
                    return component;
                });
            }

            void UpdateComponents()
            {
                MapComponents((Grid2D_Component component) =>
                {
                    component.Update();
                    return component;
                });
            }

            void MapComponents(Func<Grid2D_Component, Grid2D_Component> func)
            {
                foreach (var component in _components)
                {
                    func.Invoke(component);
                }
            }

            // ---- (( STATIC METHODS )) -------- ))
            public static ComponentTypeKey GetTypeKey<TComponent>() where TComponent : Grid2D_Component
            {
                foreach (var pair in _componentTypeMap)
                {
                    if (pair.Value == typeof(TComponent))
                    {
                        return pair.Key;
                    }
                }
                throw new InvalidEnumArgumentException(
                    $"Component type {typeof(TComponent)} is not registered in the factory.");
            }

            public static ComponentTypeKey GetTypeKey(Grid2D_Component component)
            {
                foreach (var pair in _componentTypeMap)
                {
                    if (pair.Value == component.GetType())
                    {
                        return pair.Key;
                    }
                }
                throw new InvalidEnumArgumentException(
                    $"Component type {component.GetType()} is not registered in the factory.");
            }

        }
    }

}