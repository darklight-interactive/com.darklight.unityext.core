using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public partial class Grid2D
    {
        public class ComponentSystem
        {
            Grid2D _grid;

            public ComponentSystem(Grid2D grid)
            {
                _grid = grid;
            }
        }
    }

}