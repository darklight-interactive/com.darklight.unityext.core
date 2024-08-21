using Darklight.UnityExt.Game;
using UnityEngine;

namespace Darklight.UnityExt.Game
{
    public class WeightedCell2D : Grid2D.Cell
    {
        int _weight = 0;
        public WeightedCell2D(Grid2D grid, Vector2Int key) : base(grid, key)
        {
            _weight = 1; // Default weight
        }
    }
}