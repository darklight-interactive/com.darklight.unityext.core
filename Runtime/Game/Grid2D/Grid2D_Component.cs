using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    [ExecuteAlways]
    [RequireComponent(typeof(Grid2D))]
    public class Grid2D_Component : MonoBehaviour
    {
        Cell2D.Visitor _componentInitializer => new Cell2D.Visitor(cell =>
        {
            cell.InitializeComponents(_cellComponentFlags);
        });

        [SerializeField, EnumFlags] Cell2D.ComponentFlags _cellComponentFlags = Cell2D.ComponentFlags.Shape;

        protected Grid2D grid => GetComponent<Grid2D>();

        [Button]
        public void Initialize()
        {
            grid.SendVisitorToAllCells(_componentInitializer);
        }

    }
}
