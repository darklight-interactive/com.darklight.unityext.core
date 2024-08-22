using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;

namespace Darklight.UnityExt.Game
{
    #region -- << INTERFACE >> : IGRID2D ------------------------------------ >>
    public interface IGrid2D
    {
        /// <summary>
        /// Generates a new grid based on the given configuration.
        /// </summary>
        /// <param name="config">
        ///     The base config type for the grid.
        /// </param>
        void Initialize(AbstractGrid2D.Config config);

        /// <summary>
        ///     Updates the grid to match the given configuration data.
        /// </summary>
        void UpdateConfig(AbstractGrid2D.Config config);

        /// <summary>
        /// Draws the gizmos for the grid.
        /// </summary>
        /// <param name="editMode">
        ///     If true, the edit mode gizmos will be drawn.
        /// </param>
        void DrawGizmos(bool editMode = false);
    }
    #endregion

    #region -- << ABSTRACT CLASS >> : ABSTRACT GRID2D ------------------------------------ >>
    /// <summary>
    ///     Abstract class for a 2D grid. Creates a Grid2D with Cell2D objects.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractGrid2D : IGrid2D
    {
        #region -- << INTERNAL CLASS >> : CONFIG ------------------------------------ >>
        [System.Serializable]
        public class Config
        {
            // -- States ---- >>
            [SerializeField, ShowOnly] bool _showGizmos = true;
            public bool showGizmos { get => _showGizmos; set => _showGizmos = value; }

            // -- Dimensions ---- >>
            [SerializeField, ShowOnly] Vector2Int _dimensions = new Vector2Int(3, 3);
            public Vector2Int dimensions { get => _dimensions; set => _dimensions = value; }
            public int numRows => _dimensions.y;
            public int numColumns => _dimensions.x;

            // -- Transform ---- >>
            [SerializeField, ShowOnly] Vector3 _position = Vector3.zero;
            [SerializeField, ShowOnly] Vector3 _normal = Vector3.up;
            public Vector3 position { get => _position; set => _position = value; }
            public Vector3 normal { get => _normal; set => _normal = value; }

            // -- Origin ---- >>
            [SerializeField, ShowOnly] Vector2Int _originOffset = Vector2Int.zero;
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

        #region -- << INTERNAL CLASS >> : CELL MAP ------------------------------------ >>

        /// <summary>
        ///     Handles the cells in the grid. Mainly handles location and access to cells.
        ///     Also provides methods to modify cells in the grid.
        /// </summary>
        /// <typeparam name="TCell"></typeparam>
        [System.Serializable]
        public class CellMap<TCell> where TCell : Cell2D
        {
            [SerializeField, ShowOnly] bool _initialized;
            Grid2D<TCell> _grid;
            Dictionary<Vector2Int, TCell> _cellMap = new Dictionary<Vector2Int, TCell>(); // Dictionary to store cells
            [SerializeField] List<TCell> _cellList = new List<TCell>();
            public List<TCell> cellList => _cellList;

            // Indexer to access cells by their key
            public TCell this[Vector2Int key]
            {
                get => _cellMap[key];
                set => _cellMap[key] = value;
            }

            public CellMap(Grid2D<TCell> grid)
            {
                _grid = grid;
                if (_grid == null)
                {
                    Debug.LogError("CellMap: Cannot initialize cell map with null grid.");
                    return;
                }

                _initialized = true;
                Generate();
            }

            void Generate()
            {
                if (!_initialized) return;

                _cellMap.Clear();
                Vector2Int dimensions = _grid._config.dimensions;
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

                _cellList = _cellMap.Values.ToList();
            }

            public void MapFunction(Func<TCell, TCell> mapFunction)
            {
                if (!_initialized) return;
                if (_cellMap == null) return;
                foreach (var key in _cellMap.Keys.ToList())
                {
                    _cellMap[key] = mapFunction(_cellMap[key]);
                }

                RefreshData();
            }

            public void RefreshData()
            {
                _cellList.Clear();
                foreach (TCell cell in _cellMap.Values)
                {
                    _cellList.Add(cell);
                }
            }
        }

        #endregion

        // ===================== >> PROTECTED DATA << ===================== //
        [SerializeField, ShowOnly] bool _initialized;
        [SerializeField] Config _config = new Config();
        public bool initialized { get => _initialized; protected set => _initialized = value; }
        public Config config { get => _config; protected set => _config = value; }

        // ===================== >> INITIALIZATION << ===================== //
        public AbstractGrid2D() { }
        public AbstractGrid2D(Config config) => Initialize(config);
        public abstract void Initialize(Config config);
        public abstract void UpdateConfig(Config config);
        public abstract void DrawGizmos(bool editMode);
    }
    #endregion

    [System.Serializable]
    public class Grid2D<TCell> : AbstractGrid2D, IGrid2D where TCell : Cell2D
    {
        // Override the cell map to use the generic type TCell
        public CellMap<TCell> cellMap;

        // -- Constructors ---- >>
        public Grid2D(Config config) => Initialize(config);
        public override void Initialize(Config config)
        {
            if (config == null)
            {
                Debug.LogError("Grid2D: Cannot initialize grid with null config.");
                initialized = false;
                return;
            }

            // ( Set the config )
            this.config = config;

            // ( Rebuild the cell map )
            cellMap = new CellMap<TCell>(this);
            initialized = true;
        }

        public override void UpdateConfig(Config config)
        {
            // ( Set the config )
            this.config = config;

            // ( Rebuild the cell map )
            cellMap = new CellMap<TCell>(this);
        }

        public override void DrawGizmos(bool editMode)
        {
            if (!config.showGizmos) return;
            cellMap.MapFunction(cell => { cell.DrawGizmos(); return cell; });
        }


        // ===================== >> HANDLER METHODS << ===================== //
        public virtual void SetTransformParent(Transform transform)
        {
            config.position = transform.position;
            config.normal = transform.forward;
        }

        public virtual void ResetTransform()
        {
            config.position = Vector3.zero;
            config.normal = Vector3.up;
        }


    }
}
