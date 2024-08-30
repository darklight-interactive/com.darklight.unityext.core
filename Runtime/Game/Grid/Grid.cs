using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;

namespace Darklight.UnityExt.Game.Grid
{
    #region -- << INTERFACE >> : IGrid ------------------------------------ >>
    public abstract class AbstractGrid
    {
        public abstract void Initialize(GridMapConfig config = null);
        public abstract void Update();
        public abstract void Clear();
        public abstract void DrawGizmos();

    }
    #endregion

    #region -- << ABSTRACT CLASS >> : BaseGrid ------------------------------------ >>
    [System.Serializable]
    public class BaseGrid<TCell, TData> : AbstractGrid
        where TCell : BaseCell
        where TData : BaseCellData
    {
        protected Dictionary<Vector2Int, TCell> cellMap = new Dictionary<Vector2Int, TCell>();


        // ===================== >> SERIALIZED FIELDS << ===================== //
        [SerializeField] protected GridMapConfig config;
        [SerializeField] List<TData> _dataList = new List<TData>();

        // ===================== >> CONSTRUCTORS << ===================== //
        public BaseGrid() { }
        public BaseGrid(GridMapConfig config) => Initialize(config);



        // ===================== >> PROTECTED METHODS << ===================== //
        protected void Generate()
        {
            // Function to create a new cell
            bool CreateCell(Vector2Int key)
            {
                if (cellMap.ContainsKey(key))
                    return false;

                TCell cell = (TCell)Activator.CreateInstance(typeof(TCell), key);
                cellMap[key] = cell;
                cell.SetConfig(config);
                return true;
            }

            // Iterate through the grid dimensions and create cells
            Vector2Int dimensions = config.gridDimensions;
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);
                    if (!cellMap.ContainsKey(gridKey))
                    {
                        // Create a new cell and add it to the map
                        CreateCell(gridKey);
                    }
                }
            }
        }
        protected void Resize()
        {
            if (cellMap == null) return;
            Vector2Int newDimensions = config.gridDimensions;

            // Remove null cells from the map
            List<Vector2Int> keys = new List<Vector2Int>(cellMap.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                Vector2Int key = keys[i];
                if (key.x >= newDimensions.x || key.y >= newDimensions.y)
                {
                    cellMap.Remove(key);
                }
            }
            Generate();
        }
        protected void MapFunction(Func<TCell, TCell> mapFunction)
        {
            if (cellMap == null) return;

            List<Vector2Int> keys = new List<Vector2Int>(cellMap.Keys);
            foreach (Vector2Int key in keys)
            {
                // Skip if the key is not in the map
                if (!cellMap.ContainsKey(key)) continue;

                // Apply the map function to the cell
                TCell cell = cellMap[key] as TCell;
                cellMap[key] = mapFunction(cell);
            }
        }

        // ===================== >> PUBLIC METHODS << ===================== //

        // (( RUNTIME METHODS )) ------------------------------ >>
        public override void Initialize(GridMapConfig config = null)
        {
            // Create a basic config if none is provided
            if (config == null)
                config = new GridMapConfig();
            this.config = config;
            Generate();
        }
        public override void Update()
        {
            if (cellMap == null || cellMap.Count == 0) return;

            // Resize the grid if the dimensions have changed
            Resize();

            // Update each cell in the map
            MapFunction(cell =>
            {
                cell.Update();
                return cell;
            });

            // Update the data list
            _dataList = GetData();
        }
        public override void Clear()
        {
            if (cellMap == null || cellMap.Count == 0) return;

            // Clear the data list
            _dataList.Clear();

            // Clear the cell map
            cellMap.Clear();
        }
        public override void DrawGizmos()
        {
            if (cellMap == null || cellMap.Count == 0) return;
            if (config.showGizmos == false) return;
            MapFunction(cell =>
            {
                cell.DrawGizmos(config.showEditorGizmos);
                return cell;
            });
        }

        // (( SETTERS )) ------------------------------ >>
        public virtual void SetConfig(GridMapConfig config)
        {
            if (config == null) return;
            this.config = config;
            MapFunction(cell =>
            {
                cell.SetConfig(config);
                return cell;
            });
        }

        public void SetData(List<TData> dataList)
        {
            if (dataList == null) return;
            if (cellMap == null || cellMap.Count == 0) return;

            foreach (TData cellData in dataList)
            {
                // Skip if the cell data is null
                if (cellData == null || cellData.key == null) continue;

                // Check if the key is in the map
                if (cellMap.ContainsKey(cellData.key))
                {
                    BaseCell cell = cellMap[cellData.key];
                    cell.SetData(cellData);
                }
            }
        }

        // (( GETTERS )) ------------------------------ >>
        public List<TData> GetData()
        {
            if (cellMap == null || cellMap.Count == 0) return null;

            List<TData> data = new List<TData>();
            foreach (BaseCell cell in cellMap.Values)
            {
                TData cellData = cell.GetData() as TData;
                if (cellData != null)
                {
                    data.Add(cellData);
                }
            }
            return data;
        }

    }
    #endregion

    #region -- << CLASS >> : Grid ------------------------------------ >>
    public class Grid : BaseGrid<BaseCell, BaseCellData>
    {
        public Grid() { }
        public Grid(GridMapConfig config) : base(config) { }
    }
    #endregion
}
