using System.Collections.Generic;
using UnityEngine;
using Darklight.UnityExt.Editor;

using NaughtyAttributes;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game
{
    [System.Serializable]
    public class Grid2D
    {
        #region -- << CLASS >> : CONFIG ------------------------------------ >>
        [System.Serializable]
        public class Config
        {
            // -- Dimensions ---- >>
            [SerializeField, ShowOnly]
            Vector2Int _dimensions = new Vector2Int(3, 3);
            public Vector2Int dimensions { get => _dimensions; set => _dimensions = value; }
            public int numRows => _dimensions.y;
            public int numColumns => _dimensions.x;

            // -- Transform ---- >>
            [SerializeField, ShowOnly]
            Vector3 _worldPosition = Vector3.zero;
            [SerializeField, ShowOnly]
            Vector3 _worldDirection = Vector3.up;
            public Vector3 worldPosition { get => _worldPosition; set => _worldPosition = value; }
            public Vector3 worldDirection { get => _worldDirection; set => _worldDirection = value; }

            // -- Origin ---- >>
            [SerializeField, ShowOnly]
            Vector2Int _originOffset = Vector2Int.zero;
            public Vector2Int originOffset { get => _originOffset; set => _originOffset = value; }

            // -- Cell Dimensions ---- >>
            [SerializeField, ShowOnly]
            Vector2 _cellDimensions = new Vector2(1, 1);
            public Vector2 cellDimensions { get => _cellDimensions; set => _cellDimensions = value; }
            public float cellWidth => _cellDimensions.x;
            public float cellHeight => _cellDimensions.y;

            // -- Spacing ---- >>
            [SerializeField, ShowOnly]
            Vector2 _cellSpacing = new Vector2(0, 0);
            public Vector2 cellSpacing { get => _cellSpacing; set => _cellSpacing = value; }
        }
        #endregion

        #region -- << CLASS >> : CELL ------------------------------------ >>
        /// <summary>
        /// Definition of the Grid2D_CellData class. This class is used by the Grid2D class to store the data for each grid cell.
        /// </summary>
        [System.Serializable]
        public class Cell
        {
            #region -- << CLASS >> : DATA ------------------------------------ >>
            public class Data
            {
                // -- Identifiers ---- >>
                Grid2D _grid; // Reference to the parent grid
                Vector2Int _key; // The position key of the cell in the grid
                Color _color = Color.white; // Color of the cell fpr visualization

                // -- Dimensions ---- >>
                Vector2 _dimensions = Vector2.one; // Dimensions of the cell

                // -- Transform ---- >>
                Vector3 _cellPosition = Vector3.zero; // World position of the cell
                Vector3 _cellNormal = Vector3.up; // Normal direction of the cell

                // -- States ---- >>
                bool _disabled = false; // Is the cell active or not
                public bool disabled => _disabled;

                public Data(Grid2D grid, Vector2Int key)
                {
                    _grid = grid;
                    _key = key;
                    _dimensions = grid.config.cellDimensions;
                    _cellPosition = CalculateWorldPosition();
                    _cellNormal = grid.config.worldDirection;
                }

                Vector3 CalculateWorldPosition()
                {
                    Vector2Int key = _key;
                    Grid2D.Config config = _grid.config;

                    // Start with the grid's origin position in world space
                    Vector3 basePosition = config.worldPosition;

                    // Calculate the offset for the grid origin key
                    Vector2 originOffset = (Vector2)config.originOffset * config.cellDimensions * -1;

                    // Calculate the offset for the current key position
                    Vector2 keyOffset = (Vector2)key * config.cellDimensions;

                    // Combine origin offset and key offset
                    Vector2 gridOffset = (originOffset + keyOffset) * config.cellSpacing;

                    // Create a rotation matrix based on the grid's direction
                    Quaternion rotation = Quaternion.LookRotation(config.worldDirection, Vector3.up);

                    // Apply the rotation to the grid offset to get the final world offset
                    Vector3 worldOffset = rotation * new Vector3(gridOffset.x, gridOffset.y, 0);

                    // Combine the base position with the calculated world offset
                    return basePosition + worldOffset;
                }

                public virtual Color GetColor()
                {
                    return _disabled ? Color.white : Color.black;
                }
            }
            #endregion

            // -- Protected Data ---- >>
            protected Data data; // Cell data

            public Cell(Grid2D grid, Vector2Int key)
            {
                data = new Data(grid, key);
            }
        }
        #endregion

        #region -- << CLASS >> : MAP ------------------------------------ >>
        protected class Map<TCell> where TCell : Cell
        {
            Grid2D _grid; // Reference to the parent grid
            Dictionary<Vector2Int, TCell> _cellMap = new Dictionary<Vector2Int, TCell>(); // Dictionary to store cells

            // Indexer to access cells by their key
            public TCell this[Vector2Int key]
            {
                get => _cellMap[key];
                set => _cellMap[key] = value;
            }

            // Constructors
            public Map() { }
            public Map(Grid2D grid) => Initialize(grid);

            // Initialization
            public void Initialize(Grid2D grid2D)
            {
                _grid = grid2D;
                InitializeDataMap(grid2D.config.dimensions);
            }

            void InitializeDataMap(Vector2Int dimensions)
            {
                _cellMap.Clear();
                for (int x = 0; x < dimensions.x; x++)
                {
                    for (int y = 0; y < dimensions.y; y++)
                    {
                        Vector2Int gridKey = new Vector2Int(x, y);
                        if (!_cellMap.ContainsKey(gridKey))
                        {
                            TCell newCell = (TCell)Activator.CreateInstance(typeof(TCell), _grid, gridKey);
                            _cellMap[gridKey] = newCell;
                        }
                    }
                }
            }
        }

        #endregion

        [SerializeField] protected Config config = new Config();
        protected Map<Cell> cellMap = new Map<Cell>();

        public Grid2D() => Initialize();
        public Grid2D(Config config)
        {
            this.config = config;
            Initialize();
        }

        /// <summary>
        /// Initializes the data map with cells created by the derived class.
        /// </summary>
        protected virtual void Initialize()
        {
            // ( Rebuild the cell map )
            cellMap.Initialize(this);
        }


    }

    public class Grid2D<TCell> : Grid2D where TCell : Grid2D.Cell
    {
        // Override the cell map to use the generic cell type
        protected new Map<TCell> cellMap = new Map<TCell>();

        public Grid2D() : base() => Initialize();
        public Grid2D(Config config) : base(config) => Initialize();

        protected override void Initialize()
        {
            // ( Rebuild the cell map )
            cellMap.Initialize(this);
        }
    }
}
