using System;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public partial class Grid2D
    {
        public class ComponentRegistry
        {
            Grid2D _grid;
            [SerializeField, ShowOnly, NonReorderable]
            List<Grid2D_Component> _components = new List<Grid2D_Component>();
            public ComponentRegistry(Grid2D grid)
            {
                _grid = grid;
                grid.OnGridInitialized += InitializeComponents;
                grid.OnGridUpdated += UpdateComponents;
            }

            void InitializeComponents()
            {
                _components.Clear();
                _grid.GetComponentsInChildren(_components);

                MapComponents((Grid2D_Component component) =>
                {
                    component.InitializeComponent(_grid);
                    return component;
                });
            }

            void UpdateComponents()
            {
                MapComponents((Grid2D_Component component) =>
                {
                    component.UpdateComponent();
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
        }
    }

}