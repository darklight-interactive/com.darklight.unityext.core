using UnityEngine;

namespace Darklight.UnityExt.Game
{
    /// <summary>
    /// A simple 2D grid that stores Cell2D objects.
    /// </summary>
    [ExecuteAlways]
    public class SimpleGrid2D : MonoBehaviourGrid2D<SimpleGrid2D.SimpleCell>
    {
        [System.Serializable]
        public class SimpleCell : Grid2D.Cell
        {
            [SerializeField] Data _data;

            public SimpleCell() { }
            public SimpleCell(Grid2D grid, Vector2Int key) : base(grid, key) { }
            public override void Initialize()
            {
                base.Initialize();
                _data = data;
            }
        }
    }
}