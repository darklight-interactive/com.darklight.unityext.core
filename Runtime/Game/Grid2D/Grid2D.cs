using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [Serializable]
    public class Grid2D
    {
        Dictionary<Vector2Int, Cell2D> _cellMap = new Dictionary<Vector2Int, Cell2D>();

        // ======== [[ SERIALIZED FIELDS ]] ======================================================= >>>>
        [SerializeField] Grid2D_Config _config;
        [SerializeField] List<Cell2D> _cells = new List<Cell2D>();

        // ======== [[ PROPERTIES ]] ======================================================= >>>>
        public Grid2D_Config Config => _config;

        // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
        public Grid2D() => Initialize(null);
        public Grid2D(Grid2D_Config config) => Initialize(config);

        // ======== [[ PUBLIC METHODS ]] ============================================================ >>>>
        #region (( RUNTIME )) -------- )))
        public void Initialize(Grid2D_Config config)
        {
            // Create a basic config if none is provided
            if (config == null)
                config = new Grid2D_Config();
            this._config = config;
            Generate();
        }

        public virtual void Update()
        {
            if (_cellMap == null || _cellMap.Count == 0) return;

            // Resize the grid if the dimensions have changed
            Resize();

            // Update the cells
            Cell2DUpdater cellUpdater = new Cell2DUpdater(this);
            MapFunction(cell =>
            {
                cell.Accept(cellUpdater);
                return cell;
            });

            _cells = new List<Cell2D>(_cellMap.Values.ToList());
        }

        public virtual void Clear()
        {
            if (_cellMap == null || _cellMap.Count == 0) return;

            // Clear the cell map
            _cellMap.Clear();
        }
        #endregion

        // (( MAP FUNCTION )) -------- )))
        public void MapFunction(Func<Cell2D, Cell2D> mapFunction)
        {
            if (_cellMap == null) return;

            List<Vector2Int> keys = new List<Vector2Int>(_cellMap.Keys);
            foreach (Vector2Int key in keys)
            {
                // Skip if the key is not in the map
                if (!_cellMap.ContainsKey(key)) continue;

                // Apply the map function to the cell
                Cell2D cell = _cellMap[key];
                _cellMap[key] = mapFunction(cell);
            }
        }

        // (( GETTERS )) -------- )))
        public List<Cell2D_Data> GetData()
        {
            List<Cell2D_Data> dataList = new List<Cell2D_Data>();
            List<Vector2Int> keys = new List<Vector2Int>(_cellMap.Keys);
            foreach (Vector2Int key in keys)
            {
                if (!_cellMap.ContainsKey(key)) continue;
                dataList.Add(_cellMap[key].Data);
            }
            return dataList;
        }

        // (( SETTERS )) -------- )))
        public void SetConfig(Grid2D_Config config)
        {
            if (config == null) return;
            this._config = config;
        }

        public void SetData(List<Cell2D_Data> dataList)
        {
            if (dataList == null || dataList.Count == 0) return;
            foreach (Vector2Int key in _cellMap.Keys)
            {
                if (!_cellMap.ContainsKey(key)) continue;
                Cell2D cell = _cellMap[key];

                // Find the data with the same key
                Cell2D_Data data = dataList.Find(d => d.Key == key);
                if (data == null) continue;

                // Set the cell's data
                cell.SetData(data);
            }
        }

        // ======== [[ PRIVATE METHODS ]] ======================================================= >>>>
        bool CreateCell(Vector2Int key)
        {
            if (_cellMap.ContainsKey(key))
                return false;

            Cell2D cell = (Cell2D)Activator.CreateInstance(typeof(Cell2D), key);
            _cellMap[key] = cell;
            return true;
        }

        void Generate()
        {
            // Iterate through the grid dimensions and create cells
            Vector2Int dimensions = _config.GridDimensions;
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);
                    if (!_cellMap.ContainsKey(gridKey))
                    {
                        // Create a new cell and add it to the map
                        CreateCell(gridKey);
                    }
                }
            }
        }

        void Resize()
        {
            if (_cellMap == null) return;
            Vector2Int newDimensions = _config.GridDimensions;

            // Remove null cells from the map
            List<Vector2Int> keys = new List<Vector2Int>(_cellMap.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                Vector2Int key = keys[i];
                if (key.x >= newDimensions.x || key.y >= newDimensions.y)
                {
                    _cellMap.Remove(key);
                }
            }
            Generate();
        }

        // ======== [[ NESTED TYPES ]] ======================================================= >>>>
        public enum Alignment
        {
            TopLeft, TopCenter, TopRight,
            MiddleLeft, Center, MiddleRight,
            BottomLeft, BottomCenter, BottomRight
        }
    }
}
