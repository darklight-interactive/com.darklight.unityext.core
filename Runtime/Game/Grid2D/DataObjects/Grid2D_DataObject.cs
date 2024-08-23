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
        public abstract List<TData> GetGridData<TData>() where TData : Cell.Data;
    }

    /// <summary>
    /// Abstract generic class for a 2D grid data object.
    /// </summary>
    /// <typeparam name="TCell">Type of the cell.</typeparam>
    /// <typeparam name="TData">Type of the data contained within the cell.</typeparam>
    public class Grid2D_DataObject<TCell, TData> : Grid2D_AbstractDataObject
        where TCell : Cell, new()
        where TData : Cell.Data, new()
    {
        public List<TData> savedData = new List<TData>();
        public override void SaveGridData(AbstractGrid2D grid)
        {
            if (grid == null)
                return;

            if (grid is GenericGrid2D<TCell, TData> typedGrid)
            {
                // Save data from the typed grid
                SaveCellData(typedGrid.cellMap.GetCellData());
            }
        }

        public virtual void SaveCellData(List<TData> cells)
        {
            this.savedData = cells;
        }

        public override List<T> GetGridData<T>()
        {
            return savedData as List<T>;
        }
    }

    public class Grid2D_DataObject : Grid2D_DataObject<Cell, Cell.Data> { }

}
