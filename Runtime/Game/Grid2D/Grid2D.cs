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
        [SerializeField] Grid2D_Config _config;
        Dictionary<Vector2Int, Cell2D> _cellMap = new Dictionary<Vector2Int, Cell2D>();
        [SerializeField] List<Cell2D> _cells = new List<Cell2D>();

        // ======== [[ PROPERTIES ]] ======================================================= >>>>
        public Grid2D_Config Config => _config;
        Cell2D.Visitor cellUpdateVisitor => new Cell2D.Visitor(cell =>
        {
            cell.RecalculateDataFromGrid(this);
            cell.Update();
        });
        Cell2D.Visitor cellGizmoVisitor => new Cell2D.Visitor(cell => cell.DrawGizmos());
        Cell2D.Visitor cellEditorVisitor => new Cell2D.Visitor(cell => cell.DrawEditor());

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
            SendVisitorToAllCells(cellUpdateVisitor);

            _cells = new List<Cell2D>(_cellMap.Values.ToList());
        }

        public virtual void Clear()
        {
            if (_cellMap == null || _cellMap.Count == 0) return;

            // Clear the cell map
            _cellMap.Clear();
        }

        public void DrawGizmos()
        {
            if (_cellMap == null || _cellMap.Count == 0) return;
            SendVisitorToAllCells(cellGizmoVisitor);
        }

        public void DrawEditor()
        {
            if (_cellMap == null || _cellMap.Count == 0) return;
            SendVisitorToAllCells(cellEditorVisitor);
        }
        #endregion

        // (( MAP FUNCTION )) -------- )))
        public void SendVisitorToAllCells(Cell2D.Visitor visitor)
        {
            if (_cellMap == null) return;

            List<Vector2Int> keys = new List<Vector2Int>(_cellMap.Keys);
            foreach (Vector2Int key in keys)
            {
                // Skip if the key is not in the map
                if (!_cellMap.ContainsKey(key)) continue;

                // Apply the map function to the cell
                Cell2D cell = _cellMap[key];
                cell.Accept(visitor);
            }
        }

        // (( GETTERS )) -------- )))
        public List<Cell2D> GetCells()
        {
            return new List<Cell2D>(_cellMap.Values);
        }

        // (( SETTERS )) -------- )))
        public void SetConfig(Grid2D_Config config)
        {
            if (config == null) return;
            this._config = config;
        }

        public void SetCells(List<Cell2D> cells)
        {
            if (cells == null || cells.Count == 0) return;
            foreach (Cell2D cell in cells)
            {
                if (cell == null) continue;
                if (_cellMap.ContainsKey(cell.Key))
                    _cellMap[cell.Key] = cell;
                else
                    _cellMap.Add(cell.Key, cell);
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
