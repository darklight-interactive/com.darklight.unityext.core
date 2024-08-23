using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;

namespace Darklight.UnityExt.Game
{

    #region -- << ABSTRACT CLASS >> : GRID2D ------------------------------------ >>
    /// <summary>
    ///     Abstract class for a 2D grid. Creates a Grid2D with Cell2D objects.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractGrid2D
    {
        protected static TCell CreateCell<TCell>(Vector2Int key, Config config) where TCell : Cell
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
        public static Vector3 CalculateWorldPositionFromKey(Vector2Int key, Config config)
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

        #region -- << INTERNAL CLASS >> : CONFIG ------------------------------------ >>
        [System.Serializable]
        public class Config
        {
            // -- Transform ---- >>
            [SerializeField, ShowOnly] bool _lockToTransform = true;
            Transform _transformParent;
            [SerializeField, ShowOnly] Vector3 _position = Vector3.zero;
            [SerializeField, ShowOnly] Vector3 _normal = Vector3.up;
            public Vector3 position { get => _position; set => _position = value; }
            public Vector3 normal { get => _normal; set => _normal = value; }

            // -- Origin ---- >>
            [SerializeField, ShowOnly] Vector2Int _originOffset = Vector2Int.zero;
            public Vector2Int originOffset { get => _originOffset; set => _originOffset = value; }

            // -- Grid Dimensions ---- >>
            [SerializeField, ShowOnly] Vector2Int _dimensions = new Vector2Int(3, 3);
            public Vector2Int dimensions { get => _dimensions; set => _dimensions = value; }
            public int numRows => _dimensions.y;
            public int numColumns => _dimensions.x;

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

            // -- Gizmos ---- >>
            [SerializeField, ShowOnly] bool _showGizmos = true;
            public bool showGizmos { get => _showGizmos; set => _showGizmos = value; }

            public void LockToTransform(Transform transform)
            {
                _lockToTransform = true;
                _transformParent = transform;
            }

            public void UnlockFromTransform()
            {
                _lockToTransform = false;
                _transformParent = null;
            }

            public void GetWorldSpaceData(out Vector3 position, out Vector2 dimensions, out Vector3 normal)
            {
                dimensions = new Vector2(numColumns, numRows);

                if (_lockToTransform)
                {
                    position = _transformParent.position;
                    normal = _transformParent.up;
                }
                else
                {
                    position = _position;
                    normal = _normal;
                }
            }

        }
        #endregion

        #region -- << INTERNAL CLASS >> : CELL MAP ------------------------------------ >>

        /// <summary>
        ///     Handles the cells in the grid. Mainly handles location and access to cells.
        ///     Also provides methods to modify cells in the grid.
        /// </summary>
        /// <typeparam name="TCell"></typeparam>
        [System.Serializable]
        public class CellMap<TCell, TData>
            where TCell : Cell
            where TData : Cell.Data
        {
            Config _config; // Config object for the grid
            Grid2D_AbstractDataObject _dataObj; // Data object for the grid

            Dictionary<Vector2Int, TCell> _cellMap = new Dictionary<Vector2Int, TCell>(); // Dictionary to store cells
            [SerializeField] List<TData> _dataList = new List<TData>(); // List to store cell data

            // Indexer to access cells by their key
            public TCell this[Vector2Int key]
            {
                get => _cellMap[key];
                set => _cellMap[key] = value;
            }

            #region (( Initialization )) --------- >>
            public CellMap(Config config) => Initialize(config);
            public CellMap(Config config, Grid2D_AbstractDataObject dataObj) => Initialize(config, dataObj);
            void Initialize(Config config, Grid2D_AbstractDataObject dataObj = null)
            {
                this._config = config;
                _dataObj = dataObj;
                Generate();
            }

            void Generate()
            {
                _cellMap.Clear();
                Vector2Int dimensions = _config.dimensions;
                for (int x = 0; x < dimensions.x; x++)
                {
                    for (int y = 0; y < dimensions.y; y++)
                    {
                        Vector2Int gridKey = new Vector2Int(x, y);
                        if (!_cellMap.ContainsKey(gridKey))
                        {
                            TCell newCell = CreateCell<TCell>(gridKey, _config);
                            _cellMap[gridKey] = newCell;
                        }
                    }
                }
            }

            public void RefreshData()
            {
                _dataList.Clear();
                foreach (TCell cell in _cellMap.Values)
                {
                    _dataList.Add(cell.GetData() as TData);
                }
            }


            #endregion

            #region (( MapFunction Methods )) --------- >>
            public void MapFunction(Func<TCell, TCell> mapFunction)
            {
                if (_cellMap == null) return;
                foreach (var key in _cellMap.Keys.ToList())
                {
                    _cellMap[key] = mapFunction(_cellMap[key]);
                }

                RefreshData();
            }

            public void ApplyConfigToMap(Config config)
            {
                MapFunction(cell =>
                {
                    cell.ApplyConfigToData(_config);
                    return cell;
                });
            }


            #endregion

            #region (( Getter Methods )) --------- >>
            public List<TData> GetCellData()
            {
                RefreshData();
                return _dataList;
            }

            #endregion

            #region (( Loas & Save Methods )) --------- >>

            public void LoadData(List<TData> dataList)
            {
                if (dataList == null) return;
                foreach (TData cellData in dataList)
                {
                    if (_cellMap.ContainsKey(cellData.key))
                    {
                        TCell cell = _cellMap[cellData.key];
                        cell.SetData(cellData);
                    }
                }
                RefreshData();
            }
            #endregion
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
        public abstract void SetConfig(Config config);
        public abstract void DrawGizmos(bool editMode);
    }
    #endregion

    #region -- << GENERIC CLASS >> : GRID2D ------------------------------------ >>
    [System.Serializable]
    public class GenericGrid2D<TCell, TData> : AbstractGrid2D
        where TCell : Cell
        where TData : Cell.Data
    {
        // Override the cell map to use the generic type TCell
        public CellMap<TCell, TData> cellMap;

        // -- Constructor ---- >>
        public GenericGrid2D(Config config)
        {
            Initialize(config);
        }

        #region (( IGrid2D Methods )) --------- >>
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
            cellMap = new CellMap<TCell, TData>(config);
            initialized = true;
        }

        public override void SetConfig(Config config)
        {
            // ( Set the config )
            this.config = config;

            // ( Update the cell map )
            cellMap.ApplyConfigToMap(config);
        }

        public override void DrawGizmos(bool editMode)
        {
            if (!config.showGizmos) return;
            cellMap.MapFunction(cell =>
            {
                cell.DrawGizmos(editMode);
                return cell;
            });
        }
        #endregion
    }
    #endregion
}
