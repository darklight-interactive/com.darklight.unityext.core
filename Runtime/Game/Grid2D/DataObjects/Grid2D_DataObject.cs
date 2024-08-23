using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace Darklight.UnityExt.Game
{
    /// <summary>
    /// Abstract class for a 2D grid data object that inherits from ScriptableObject.
    /// </summary>
    public abstract class Grid2D_AbstractDataObject : ScriptableObject
    {


        public abstract void SaveGridData(AbstractGrid2D grid);
        public abstract void ClearData();
    }

    /// <summary>
    /// Abstract generic class for a 2D grid data object.
    /// </summary>
    /// <typeparam name="TCell">Type of the cell.</typeparam>
    /// <typeparam name="TData">Type of the data contained within the cell.</typeparam>
    public class Grid2D_GenericDataObject<TCell, TData> : Grid2D_AbstractDataObject
        where TCell : Cell2D, new()
        where TData : AbstractCellData, new()
    {

        // (( STATIC METHODS )) ------------------------------ >>
        public static List<TData> GetDataCopy(List<TData> data)
        {
            List<TData> copy = new List<TData>();
            foreach (TData d in data)
            {
                TData newData = new TData();
                newData.CopyFrom(d);
                copy.Add(newData);
            }
            return copy;
        }


        [SerializeField] protected List<TData> savedData = new List<TData>();
        public override void SaveGridData(AbstractGrid2D grid)
        {
            if (grid == null) return;
            if (grid is not GenericGrid2D<TCell, TData> genericGrid) return;

            List<TData> data = genericGrid.cellMap.GetCellData();
            SaveCellData(data);
        }

        public void SaveCellData(List<TData> data)
        {
            savedData.Clear();
            savedData = GetDataCopy(data);

            Debug.Log($"Saved {savedData.Count} cells.", this);
        }

        public List<TData> GetCellData()
        {
            return GetDataCopy(savedData);
        }

        public override void ClearData()
        {
            savedData.Clear();
        }
    }

    public class Grid2D_DataObject : Grid2D_GenericDataObject<Cell2D, Cell2D.Data> { }
}
