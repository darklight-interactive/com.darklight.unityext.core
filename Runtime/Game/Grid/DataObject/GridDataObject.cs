using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public abstract class AbstractGridDataObject : ScriptableObject
    {
        [SerializeField] protected List<BaseCellData> savedData = new List<BaseCellData>();


        public void SaveData(AbstractGrid grid)
        {
            if (grid == null) return;

            List<BaseCellData> data = GetDataCopy<BaseCellData>();
            SaveCellData(data);
        }

        public void SaveCellData(List<BaseCellData> data)
        {
            savedData.Clear();
            savedData = GetDataCopy<BaseCellData>();

            Debug.Log($"Saved {savedData.Count} cells.", this);
        }

        public void ClearData()
        {
            savedData.Clear();
        }

        // (( STATIC METHODS )) ------------------------------ >>
        public List<TData> GetDataCopy<TData>() where TData : BaseCellData, new()
        {
            if (savedData == null) return new List<TData>();

            List<TData> copy = new List<TData>();
            foreach (TData d in savedData)
            {
                TData newData = new TData();
                newData.CopyFrom(d);
                copy.Add(newData);
            }
            return copy;
        }
    }

    /// <summary>
    /// Abstract generic class for a 2D grid data object.
    /// </summary>
    /// <typeparam name="TCell">Type of the cell.</typeparam>
    /// <typeparam name="TData">Type of the data contained within the cell.</typeparam>
    public class BaseGridDataObject<TCell, TData> : AbstractGridDataObject
        where TCell : BaseCell, new()
        where TData : BaseCellData, new()
    {
        // (( PROTECTED FIELDS )) ------------------------------ >>


        // (( PUBLIC METHODS )) ------------------------------ >>




    }

    public class GridDataObject : BaseGridDataObject<Cell, BaseCellData> { }
}
