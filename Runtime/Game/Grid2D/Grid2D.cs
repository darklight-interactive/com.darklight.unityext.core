using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game
{
    public interface IGrid2D
    {
        /// <summary>
        /// Generates a new grid based on the current configuration.
        /// Overwrites the current grid data.
        /// </summary>
        void InitializeGrid();

        /// <summary>
        /// Refreshes the grid data based on the current configuration.
        /// Assumes the grid has already been initialized.
        /// </summary>
        void RefreshData();

        /// <summary>
        /// Draws the gizmos for the grid.
        /// </summary>
        /// <param name="editMode">
        ///     If true, the edit mode gizmos will be drawn.
        /// </param>
        void DrawGizmos(bool editMode = false);
    }

    [System.Serializable]
    public abstract class Grid2D
    {
        #region -- << CLASS >> : CONFIG ------------------------------------ >>
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

        #region -- << CLASS >> : CELL ------------------------------------ >>
        /// <summary>
        /// Definition of the Grid2D_CellData class. This class is used by the Grid2D class to store the data for each grid cell.
        /// </summary>
        [System.Serializable]
        public class Cell
        {
            #region -- << CLASS >> : DATA ------------------------------------ >>
            [System.Serializable]
            public class Data
            {
                // -- Identifiers ---- >>
                Grid2D _grid; // Reference to the parent grid
                [SerializeField, ShowOnly] string _name = "Cell"; // Name of the cell
                [SerializeField, ShowOnly] Vector2Int _key; // The position key of the cell in the grid
                [SerializeField] Color _color = Color.grey; // Color of the cell fpr visualization
                public string name => _name;
                public Vector2Int key => _key;
                public Color color { get => _color; protected set => _color = value; }

                // -- Dimensions ---- >>
                Vector2 _dimensions = Vector2.one; // Dimensions of the cell
                public Vector2 dimensions => _dimensions;

                // -- Transform ---- >>
                Vector3 _position = Vector3.zero; // World position of the cell
                Vector3 _normal = Vector3.up; // Normal direction of the cell
                public Vector3 position
                {
                    get
                    {
                        _position = CalculateWorldPosition();
                        return _position;
                    }
                }
                public Vector3 normal
                {
                    get
                    {
                        _normal = GetNormal();
                        return _normal;
                    }
                }

                // -- States ---- >>
                [SerializeField, ShowOnly] bool _disabled = false; // Is the cell active or not
                public bool disabled { get => _disabled; set => _disabled = value; }

                public Data(Grid2D grid, Vector2Int key)
                {
                    _grid = grid;
                    _key = key;
                    _name = $"Cell {key}";
                    _dimensions = grid.config.cellDimensions;
                    _position = CalculateWorldPosition();
                    _normal = grid.config.normal;
                }

                public virtual void Refresh()
                {
                    SetColor();
                }

                protected virtual void SetColor()
                {
                    if (_disabled)
                    {
                        _color = Color.grey;
                        return;
                    }

                    _color = Color.white;
                }

                /// <summary>
                /// Internal method to calculate the world position of the cell based on its key and the grid configuration.
                /// </summary>
                /// <returns></returns>
                Vector3 CalculateWorldPosition()
                {
                    Vector2Int key = _key;
                    Grid2D.Config gridConfig = _grid.config;

                    // Start with the grid's origin position in world space
                    Vector3 basePosition = gridConfig.position;

                    // Calculate the offset for the grid origin key
                    Vector2 originOffset = (Vector2)gridConfig.originOffset * gridConfig.cellDimensions * -1;

                    // Calculate the offset for the current key position
                    Vector2 keyOffset = (Vector2)key * gridConfig.cellDimensions;

                    // Calculate the spacing offset && clamp it to avoid overlapping cells
                    Vector2 spacingOffset = gridConfig.cellSpacing;
                    spacingOffset.x = Mathf.Clamp(spacingOffset.x, 1, float.MaxValue);
                    spacingOffset.y = Mathf.Clamp(spacingOffset.y, 1, float.MaxValue);

                    // Combine origin offset and key offset, then apply spacing
                    Vector2 gridOffset = (originOffset + keyOffset) * spacingOffset;

                    // Create a rotation matrix based on the grid's direction
                    Quaternion rotation = Quaternion.LookRotation(gridConfig.normal, Vector3.up);

                    // Apply the rotation to the grid offset to get the final world offset
                    Vector3 worldOffset = rotation * new Vector3(gridOffset.x, gridOffset.y, 0);

                    // Combine the base position with the calculated world offset
                    return basePosition + worldOffset;
                }

                Vector3 GetNormal() => _grid.config.normal;
            }
            #endregion
            public Data data { get; protected set; }

            public Cell() { }
            public Cell(Grid2D grid, Vector2Int key)
            {
                data = new Data(grid, key);
            }
            public virtual void Initialize() { }
            public virtual void DrawGizmos(bool editMode = false)
            {
                // Draw the cell square
                CustomGizmos.DrawWireRect(data.position, data.dimensions, data.normal, data.color);

                string label = $"{data.key}";
                CustomGizmos.DrawLabel(label, data.position, CustomGUIStyles.CenteredStyle);

                if (editMode)
                {
                    float size = Mathf.Min(data.dimensions.x, data.dimensions.y);
                    size *= 0.75f;

                    // Draw the button handle only if the grid is in edit mode
                    CustomGizmos.DrawButtonHandle(data.position, size, data.normal, data.color, () =>
                    {
                        data.disabled = !data.disabled;
                    }, Handles.RectangleHandleCap);
                }
            }
        }

        public class Cell<TData> : Cell where TData : Cell.Data
        {
            public new TData data;

            public Cell() { }
            public Cell(Grid2D grid, Vector2Int key) : base(grid, key)
            {
                data = (TData)Activator.CreateInstance(typeof(TData), grid, key);
            }

            public override void Initialize() { }
        }
        #endregion

        #region -- << CLASS >> : CELL MAP ------------------------------------ >>
        [System.Serializable]
        public class CellMap<TCell> where TCell : Cell
        {
            Grid2D _grid; // Reference to the parent grid
            Dictionary<Vector2Int, TCell> _cellMap = new Dictionary<Vector2Int, TCell>(); // Dictionary to store cells
            [SerializeField] List<TCell> _cellList = new List<TCell>();
            public List<TCell> cellList => _cellList;

            // Indexer to access cells by their key
            public TCell this[Vector2Int key]
            {
                get => _cellMap[key];
                set => _cellMap[key] = value;
            }

            // Constructors
            public CellMap() { }
            public CellMap(Grid2D grid) => Initialize(grid);

            // Initialization
            public void Initialize(Grid2D grid2D)
            {
                _grid = grid2D;
                Generate(grid2D.config.dimensions);
            }

            void Generate(Vector2Int dimensions)
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

                RefreshData();
            }

            public void ModifyCells(Func<TCell, TCell> mapFunction)
            {
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

            public void DrawGizmos(bool editMode = false)
            {
                foreach (var cell in _cellMap.Values)
                {
                    cell.DrawGizmos(editMode);
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
        public Grid2D() => InitializeGrid();
        public Grid2D(Config config)
        {
            _config = config;
            InitializeGrid();
        }
        public abstract void InitializeGrid();
        public abstract void RefreshData();
        public abstract void DrawGizmos(bool editMode);
    }

    [System.Serializable]
    public class Grid2D<TCell> : Grid2D, IGrid2D where TCell : Grid2D.Cell
    {
        [SerializeField] CellMap<TCell> _cellMap = new CellMap<TCell>();
        public CellMap<TCell> cellMap { get => _cellMap; protected set => _cellMap = value; }
        public Grid2D() : base() { }
        public Grid2D(Config config) : base(config) { }
        public override void InitializeGrid()
        {
            // ( Rebuild the cell map )
            cellMap.Initialize(this);
            initialized = true;
        }

        public override void RefreshData()
        {
            cellMap.RefreshData();
        }

        public override void DrawGizmos(bool editMode)
        {
            if (!config.showGizmos) return;
            cellMap.DrawGizmos(editMode);
        }


        // ===================== >> HANDLER METHODS << ===================== //
        public virtual void SetTransform(Transform transform)
        {
            config.position = transform.position;
            config.normal = transform.forward;
        }


    }
}
