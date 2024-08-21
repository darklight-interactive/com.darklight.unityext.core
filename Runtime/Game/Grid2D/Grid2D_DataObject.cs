using System.Collections.Generic;
using UnityEngine;

namespace Darklight.UnityExt.Game
{
    public abstract class Grid2D_AbstractDataObject : ScriptableObject
    {
        public abstract void Initialize(Grid2D grid);
        public abstract void SaveData();
    }

    public abstract class Grid2D_DataObject<TCell> : Grid2D_AbstractDataObject where TCell : Grid2D.Cell, new()
    {
        Grid2D<TCell> grid;
        public List<TCell> cells = new List<TCell>();
        public override void Initialize(Grid2D grid)
        {
            this.grid = grid as Grid2D<TCell>;
            cells = this.grid.cellMap.cellList;
        }

        public override void SaveData()
        {

        }
    }

    public class Grid2D_DataObject : Grid2D_DataObject<Grid2D.Cell> { }

}
