using System.Collections.Generic;
using UnityEngine;

namespace Darklight.UnityExt.Game
{
    public abstract class Grid2D_AbstractDataObject : ScriptableObject
    {
        public abstract void Initialize(AbstractGrid2D grid);
        public abstract void SaveData();
    }

    public abstract class Grid2D_DataObject<TCell> : Grid2D_AbstractDataObject where TCell : Cell2D, new()
    {
        Grid2D<TCell> grid;
        public List<TCell> cells = new List<TCell>();
        public override void Initialize(AbstractGrid2D grid)
        {
            this.grid = grid as Grid2D<TCell>;
            cells = this.grid.cellMap.cellList;
        }

        public override void SaveData()
        {
            cells = grid.cellMap.cellList;
        }
    }

    public class Grid2D_DataObject : Grid2D_DataObject<Cell2D> { }

}
