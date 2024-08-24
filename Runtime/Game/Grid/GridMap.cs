using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public abstract class BaseGridMap
    {
        #region (( Static Methods )) --------- >>
        protected static TCell CreateCell<TCell>(Vector2Int key, GridMapConfig config)
            where TCell : AbstractCell
        {
            if (config == null)
            {
                Debug.LogError("Grid2D: Cannot create cell with null config.");
                return null;
            }

            TCell newCell = (TCell)Activator.CreateInstance(typeof(TCell), key);
            return newCell;
        }
        #endregion

        // ===================== >> PROTECTED DATA << ===================== //
        protected GridMapConfig config;
        protected Dictionary<Vector2Int, AbstractCell> cellMap = new Dictionary<Vector2Int, AbstractCell>();
        protected void MapFunction<TCell>(Func<TCell, TCell> mapFunction) where TCell : AbstractCell
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

        public BaseGridMap() => Initialize();
        public BaseGridMap(GridMapConfig config) => Initialize(config);
        public abstract void Initialize(GridMapConfig config = null);
        public abstract void Update();
        public abstract void Clear();
        protected abstract void Generate();
        public abstract void SetConfig(GridMapConfig config);
    }

    [System.Serializable]
    public class GenericGridMap<TCell, TData> : BaseGridMap
        where TCell : AbstractCell
        where TData : BaseCellData
    {
        [SerializeField] List<TData> _dataList = new List<TData>();

        public GenericGridMap() => Initialize();
        public GenericGridMap(GridMapConfig config) => Initialize(config);
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
            MapFunction<TCell>(cell =>
            {
                cell.Update();
                return cell;
            });

            _dataList = GetData();
        }

        protected override void Generate()
        {
            cellMap.Clear();
            Vector2Int dimensions = config.gridDimensions;
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);
                    if (!cellMap.ContainsKey(gridKey))
                    {
                        // Create a new cell and add it to the map
                        TCell newCell = CreateCell<TCell>(gridKey, config);
                        cellMap[gridKey] = newCell;
                    }
                }
            }
        }

        public override void SetConfig(GridMapConfig config)
        {
            if (config == null) return;
            this.config = config;
            MapFunction<TCell>(cell =>
            {
                cell.SetConfig(config);
                return cell;
            });
        }

        public List<TData> GetData()
        {
            if (cellMap == null || cellMap.Count == 0) return null;

            List<TData> data = new List<TData>();
            foreach (TCell cell in cellMap.Values)
            {
                TData cellData = cell.GetData() as TData;
                if (cellData != null)
                {
                    data.Add(cellData);
                }
            }
            return data;
        }

        public void SetData(List<TData> dataList)
        {
            if (dataList == null) return;
            _dataList = dataList;
            ApplyDataToMap(dataList);
        }

        public override void Clear()
        {
            cellMap.Clear();
            _dataList.Clear();
        }



        public void ApplyDataToMap(List<TData> dataList)
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
                    TCell cell = cellMap[cellData.key] as TCell;
                    cell.SetData(cellData);
                }
            }
        }

        public void DrawGizmos(bool editMode = false)
        {
            if (cellMap == null || cellMap.Count == 0) return;
            MapFunction<TCell>(cell =>
            {
                cell.DrawGizmos(editMode);
                return cell;
            });
        }


    }
}