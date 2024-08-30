using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public abstract class AbstractGridDataObject : ScriptableObject
    {
        public abstract List<TData> GetData<TData>() where TData : BaseCellData, new();
        public abstract void SetData<TData>(List<TData> data) where TData : BaseCellData, new();
        public abstract void ClearData();


    }

    /// <summary>
    /// Abstract generic class for a 2D grid data object.
    /// </summary>
    /// <typeparam name="TCell">Type of the cell.</typeparam>
    /// <typeparam name="TData">Type of the data contained within the cell.</typeparam>
    public class BaseGridDataObject<TData> : AbstractGridDataObject
        where TData : BaseCellData, new()
    {
        [SerializeField] protected List<TData> savedData = new List<TData>();

        public override List<T> GetData<T>()
        {
            if (savedData == null) return new List<T>();

            List<T> copy = new List<T>();
            foreach (TData d in savedData)
            {
                T newData = new T();
                newData.CopyFrom(d);
                copy.Add(newData);
            }
            return copy;
        }

        public override void SetData<T>(List<T> data)
        {
            savedData.Clear();
            foreach (T newData in data)
            {
                savedData.Add(newData as TData);
            }
        }

        public override void ClearData()
        {
            savedData.Clear();
        }
    }

    public class GridDataObject : BaseGridDataObject<BaseCellData> { }
}
