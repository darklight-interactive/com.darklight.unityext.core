using Darklight.UnityExt.Game;
using UnityEngine;

namespace Darklight.UnityExt.Game
{
    public class WeightedCell2D : Cell2D
    {
        int _weight = 0;

        public WeightedCell2D(Grid2DSettings settings, Vector2Int gridKey, Vector3 gridPosition) : base(settings, gridKey, gridPosition)
        {
        }

        public WeightedCell2D(Grid2DSettings settings, Vector2Int gridKey, Vector3 gridPosition, int weight) : base(settings, gridKey, gridPosition)
        {
            _weight = weight;
        }
    }
}