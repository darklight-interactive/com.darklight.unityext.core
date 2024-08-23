using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public abstract class BaseGridMap
    {
        #region (( Static Methods )) --------- >>
        protected static TCell CreateCell<TCell>(Vector2Int key, GridConfig config) where TCell : BaseCell
        {
            if (config == null)
            {
                Debug.LogError("Grid2D: Cannot create cell with null config.");
                return null;
            }

            TCell newCell = (TCell)Activator.CreateInstance(typeof(TCell), key, config);
            return newCell;
        }

        /// <summary>
        /// Internal method to calculate the world position of the cell based on its key and the grid configuration.
        /// </summary>
        /// <returns></returns>
        public static Vector3 CalculateWorldPositionFromKey(Vector2Int key, GridConfig config)
        {
            // Start with the grid's origin position in world space
            Vector3 basePosition = config.position;

            // Calculate the offset for the grid origin key
            Vector2 originOffset = (Vector2)config.originOffset * config.cellDimensions * -1;

            // Calculate the offset for the current key position
            Vector2 keyOffset = (Vector2)key * config.cellDimensions;

            // Calculate the spacing offset && clamp it to avoid overlapping cells
            Vector2 spacingOffset = config.cellSpacing;
            spacingOffset.x = Mathf.Clamp(spacingOffset.x, 1, float.MaxValue);
            spacingOffset.y = Mathf.Clamp(spacingOffset.y, 1, float.MaxValue);

            // Combine origin offset and key offset, then apply spacing
            Vector2 gridOffset = (originOffset + keyOffset) * spacingOffset;

            // Create a rotation matrix based on the grid's direction
            Quaternion rotation = Quaternion.LookRotation(config.normal, Vector3.up);

            // Apply the rotation to the grid offset to get the final world offset
            Vector3 worldOffset = rotation * new Vector3(gridOffset.x, gridOffset.y, 0);

            // Combine the base position with the calculated world offset
            return basePosition + worldOffset;
        }
        #endregion

        // ===================== >> PROTECTED DATA << ===================== //
        protected GridConfig config; // Config object for the grid
    }

    [System.Serializable]
    public class GenericGridMap<TCell, TData> : BaseGridMap
        where TCell : BaseCell
        where TData : BaseCellData
    {
        protected Dictionary<Vector2Int, TCell> cellMap = new Dictionary<Vector2Int, TCell>();
        protected List<TData> dataList = new List<TData>();

        // Indexer to access cells by their key
        public TCell this[Vector2Int key]
        {
            get => cellMap[key];
            set => cellMap[key] = value;
        }

        public GenericGridMap(GridConfig config) { }

        public void MapFunction(Func<TCell, TCell> mapFunction)
        {
            if (cellMap == null) return;

            List<Vector2Int> keys = new List<Vector2Int>(cellMap.Keys);
            foreach (Vector2Int key in keys)
            {
                // Skip if the key is not in the map
                if (!cellMap.ContainsKey(key)) continue;

                // Apply the map function to the cell
                TCell cell = cellMap[key];
                cellMap[key] = mapFunction(cell);
            }

            RefreshData();
        }

        public void ApplyConfigToMap(GridConfig config)
        {
            this.config = config;
            MapFunction(cell =>
            {
                cell.Data.SetPosition(CalculateWorldPositionFromKey(cell.Data.Key, config));
                cell.Data.SetDimensions(config.cellDimensions);
                cell.Data.SetNormal(config.normal);
                return cell;
            });
        }


        void Generate()
        {
            cellMap.Clear();
            Vector2Int dimensions = config.dimensions;
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);
                    if (!cellMap.ContainsKey(gridKey))
                    {
                        TCell newCell = CreateCell<TCell>(gridKey, config);
                        cellMap[gridKey] = newCell;
                    }
                }
            }
            dataList = GetDataList();
        }

        public void RefreshData()
        {
            dataList = GetDataList();
        }

        #region (( Getter Methods )) --------- >>
        public List<TData> GetDataList()
        {
            List<TData> data = new List<TData>();
            foreach (TCell cell in cellMap.Values)
            {
                TData cellData = cell.Data as TData;
                if (cellData != null)
                {
                    data.Add(cellData);
                }
            }
            return data;
        }
        #endregion

        #region (( Setter Methods )) --------- >>
        public void ApplyDataList(List<TData> dataList)
        {
            this.dataList = dataList;
            ApplyDataToMap(dataList);
        }
        #endregion


        #region (( Load & Save Methods )) --------- >>

        public void ApplyDataToMap(List<TData> dataList)
        {
            if (dataList == null) return;
            if (cellMap == null || cellMap.Count == 0) return;

            foreach (TData cellData in dataList)
            {
                // Skip if the cell data is null
                if (cellData == null || cellData.Key == null) continue;

                // Check if the key is in the map
                if (cellMap.ContainsKey(cellData.Key))
                {
                    TCell cell = cellMap[cellData.Key];
                    cell.SetData(cellData);
                }
            }

            RefreshData();
        }
        #endregion

        public void Clear()
        {
            cellMap.Clear();
            dataList.Clear();
        }
    }
}