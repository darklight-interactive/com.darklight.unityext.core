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
        // ===================== >> PROTECTED DATA << ===================== //
        [SerializeField] protected GridMapConfig config;
        protected Dictionary<Vector2Int, BaseCell> cellMap = new Dictionary<Vector2Int, BaseCell>();


        public BaseGridMap() => Initialize();
        public BaseGridMap(GridMapConfig config) => Initialize(config);
        public abstract void Initialize(GridMapConfig config = null);
        public abstract void Update();
        public abstract void Clear();
        protected abstract void Generate();
        protected abstract void Resize();
        public abstract void SetConfig(GridMapConfig config);

        protected virtual TCell CreateCell<TCell>(Vector2Int key) where TCell : BaseCell
        {
            if (cellMap.ContainsKey(key)) return cellMap[key] as TCell;
            TCell cell = (TCell)Activator.CreateInstance(typeof(TCell), key);
            cellMap[key] = cell;
            return cell;
        }

        protected virtual TCell CreateCell<TCell>(Vector2Int key, GridMapConfig config) where TCell : BaseCell
        {
            if (cellMap.ContainsKey(key)) return cellMap[key] as TCell;
            TCell cell = (TCell)Activator.CreateInstance(typeof(TCell), key);
            cell.SetConfig(config);
            cellMap[key] = cell;
            return cell;
        }

        protected void MapFunction<TCell>(Func<TCell, TCell> mapFunction) where TCell : BaseCell
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
    }

    [System.Serializable]
    public class GenericGridMap<TCell, TData> : BaseGridMap
        where TCell : BaseCell
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

            // Resize the grid if the dimensions have changed
            Resize();

            // Update each cell in the map
            MapFunction<TCell>(cell =>
            {
                cell.Update();
                return cell;
            });

            // Update the data list
            _dataList = GetData();
        }

        protected override void Generate()
        {
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

        protected override void Resize()
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

        public void DrawGizmos()
        {
            if (cellMap == null || cellMap.Count == 0) return;
            if (config.showGizmos == false) return;
            MapFunction<TCell>(cell =>
            {
                cell.DrawGizmos(config.showEditorGizmos);
                return cell;
            });
        }


    }
}