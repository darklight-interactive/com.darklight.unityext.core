using System.Collections.Generic;
using UnityEngine;

namespace Darklight.UnityExt.Game
{
    /// <summary>
    /// Abstract class for a 2D grid data object that inherits from ScriptableObject.
    /// </summary>
    public abstract class Grid2D_AbstractDataObject : ScriptableObject
    {
        public abstract void SaveGridData(AbstractGrid2D grid);
    }

    /// <summary>
    /// Abstract non-generic class for a 2D grid data object that inherits from Grid2D_AbstractDataObject.
    /// </summary>
    /// <typeparam name="TCell"></typeparam>
    public class Grid2D_DataObject<TCell> : Grid2D_AbstractDataObject where TCell : Cell2D, new()
    {
        public Grid2D.Config config;
        public List<TCell> cells = new List<TCell>();

        public override void SaveGridData(AbstractGrid2D grid)
        {
            config = grid.config;
            if (grid is Grid2D<TCell>)
            {
                SaveCellData((grid as Grid2D<TCell>).cellMap.cellList);
            }
        }

        public virtual void SaveCellData(List<TCell> cells)
        {
            this.cells = cells;
        }
    }

    public class Grid2D_DataObject : Grid2D_DataObject<Cell2D> { }

}
