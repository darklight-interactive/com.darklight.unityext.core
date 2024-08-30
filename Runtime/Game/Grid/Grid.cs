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

    [System.Serializable]
    public class Grid
    {
        public enum Alignment
        {
            TopLeft, TopCenter, TopRight,
            MiddleLeft, Center, MiddleRight,
            BottomLeft, BottomCenter, BottomRight
        }

        #region -- << DATA CLASS >> : Config ------------------------------------ >>    
        [System.Serializable]
        public class Config
        {
            // ======================= [[ SERIALIZED FIELDS ]] ======================= //
            [SerializeField, ShowOnly] bool _showGizmos = true;
            [SerializeField, ShowOnly] bool _showEditorGizmos = true;
            [SerializeField, ShowOnly] bool _lockToTransform = true;

            [SerializeField, ShowOnly] Alignment _gridAlignment = Alignment.Center;
            [SerializeField, ShowOnly] Vector3 _gridPosition = new Vector3(0, 0, 0);
            [SerializeField, ShowOnly] Vector3 _gridNormal = Vector3.up;
            [SerializeField, ShowOnly] Vector2Int _gridDimensions = new Vector2Int(3, 3);

            [SerializeField, ShowOnly] Vector2 _cellDimensions = new Vector2(1, 1);
            [SerializeField, ShowOnly] Vector2 _cellSpacing = new Vector2(1, 1);
            [SerializeField, ShowOnly] Vector2 _cellBonding = new Vector2(0, 0);

            // ======================= [[ PUBLIC REFERENCE PROPERTIES ]] ======================= //
            public bool showGizmos => _showGizmos;
            public bool showEditorGizmos => _showEditorGizmos;
            public bool SetGizmos(bool showGizmos, bool showEditorGizmos = false)
            {
                _showGizmos = showGizmos;
                _showEditorGizmos = showEditorGizmos;
                return _showGizmos;
            }

            public bool lockToTransform => _lockToTransform;
            public void SetLockToTransform(bool lockToTransform) => _lockToTransform = lockToTransform;

            public Alignment gridAlignment => _gridAlignment;
            public void SetGridAlignment(Alignment gridAlignment) => _gridAlignment = gridAlignment;

            public Vector3 gridPosition => _gridPosition;
            public Vector3 gridNormal => _gridNormal;
            public Vector2Int gridDimensions => _gridDimensions;
            public void SetGridPosition(Vector3 originPosition) => _gridPosition = originPosition;
            public void SetGridNormal(Vector3 gridNormal) => _gridNormal = gridNormal;
            public void SetGridDimensions(Vector2Int gridDimensions) => _gridDimensions = gridDimensions;

            public Vector2 cellDimensions => _cellDimensions;
            public Vector2 cellSpacing => _cellSpacing;
            public Vector2 cellBonding => _cellBonding;
            public void SetCellDimensions(Vector2 cellDimensions) => _cellDimensions = cellDimensions;
            public void SetCellSpacing(Vector2 cellSpacing) => _cellSpacing = cellSpacing;
            public void SetCellBonding(Vector2 cellBonding) => _cellBonding = cellBonding;


            // ======================= [[ CONSTRUCTORS ]] ======================= //
            public Config() { }


            #region (( Calculation Methods )) --------- >>
            public Vector3 CalculatePositionFromKey(Vector2Int key)
            {
                Config config = this;

                // Get the origin key of the grid
                Vector2Int originKey = config.CalculateOriginKey();

                // Calculate the spacing offset && clamp it to avoid overlapping cells
                Vector2 spacingOffsetPos = config.cellSpacing + Vector2.one; // << Add 1 to allow for values of 0
                spacingOffsetPos.x = Mathf.Clamp(spacingOffsetPos.x, 0.5f, float.MaxValue);
                spacingOffsetPos.y = Mathf.Clamp(spacingOffsetPos.y, 0.5f, float.MaxValue);

                // Calculate bonding offsets
                Vector2 bondingOffset = Vector2.zero;
                if (key.y % 2 == 0)
                    bondingOffset.x = config.cellBonding.x;
                if (key.x % 2 == 0)
                    bondingOffset.y = config.cellBonding.y;

                // Calculate the offset of the cell from the grid origin
                Vector2 originOffsetPos = originKey * config.cellDimensions;
                Vector2 keyOffsetPos = key * config.cellDimensions;

                // Calculate the final position of the cell
                Vector2 cellPosition = (keyOffsetPos - originOffsetPos); // << Calculate the position offset
                cellPosition *= spacingOffsetPos; // << Multiply the spacing offset
                cellPosition += bondingOffset; // << Add the bonding offset

                // Create a rotation matrix based on the grid's normal
                Quaternion rotation = Quaternion.LookRotation(config.gridNormal, Vector3.forward);

                // Apply the rotation to the grid offset and return the final world position
                return gridPosition + (rotation * new Vector3(cellPosition.x, cellPosition.y, 0));
            }

            public Vector2Int CalculateCoordinateFromKey(Vector2Int key)
            {
                Config config = this;
                Vector2Int originKey = config.CalculateOriginKey();
                return key - originKey;
            }

            Vector2Int CalculateOriginKey()
            {
                Config config = this;
                Vector2Int gridDimensions = config.gridDimensions - Vector2Int.one;
                Vector2Int originKey = Vector2Int.zero;

                switch (config.gridAlignment)
                {
                    case Alignment.TopLeft:
                        originKey = new Vector2Int(0, 0);
                        break;
                    case Alignment.TopCenter:
                        originKey = new Vector2Int(Mathf.FloorToInt(gridDimensions.x / 2), 0);
                        break;
                    case Alignment.TopRight:
                        originKey = new Vector2Int(Mathf.FloorToInt(gridDimensions.x), 0);
                        break;
                    case Alignment.MiddleLeft:
                        originKey = new Vector2Int(0, Mathf.FloorToInt(gridDimensions.y / 2));
                        break;
                    case Alignment.Center:
                        originKey = new Vector2Int(
                            Mathf.FloorToInt(gridDimensions.x / 2),
                            Mathf.FloorToInt(gridDimensions.y / 2)
                            );
                        break;
                    case Alignment.MiddleRight:
                        originKey = new Vector2Int(
                            Mathf.FloorToInt(gridDimensions.x),
                            Mathf.FloorToInt(gridDimensions.y / 2)
                            );
                        break;
                    case Alignment.BottomLeft:
                        originKey = new Vector2Int(0, Mathf.FloorToInt(gridDimensions.y));
                        break;
                    case Alignment.BottomCenter:
                        originKey = new Vector2Int(
                            Mathf.FloorToInt(gridDimensions.x / 2),
                            Mathf.FloorToInt(gridDimensions.y)
                            );
                        break;
                    case Alignment.BottomRight:
                        originKey = new Vector2Int(
                            Mathf.FloorToInt(gridDimensions.x),
                            Mathf.FloorToInt(gridDimensions.y)
                            );
                        break;
                }

                return originKey;
            }


            #endregion
        }
        #endregion
        Dictionary<Vector2Int, BaseCell> _cellMap = new Dictionary<Vector2Int, BaseCell>();
        [SerializeField] Config _config;
        [SerializeField] List<BaseCell> _cells = new List<BaseCell>();

        public Grid() => Initialize(null);
        public Grid(Config config) => Initialize(config);
        public virtual void Initialize(Config config)
        {
            // Create a basic config if none is provided
            if (config == null)
                config = new Config();
            this._config = config;
            Generate();
        }

        public virtual void Update()
        {
            if (_cellMap == null || _cellMap.Count == 0) return;

            // Resize the grid if the dimensions have changed
            Resize();

            // Update the cells
            CellUpdater cellUpdater = new CellUpdater(_config);
            MapFunction(cell =>
            {
                cell.Accept(cellUpdater);
                return cell;
            });

            _cells = new List<BaseCell>(_cellMap.Values.ToList());
        }

        public virtual void Clear()
        {
            if (_cellMap == null || _cellMap.Count == 0) return;

            // Clear the cell map
            _cellMap.Clear();
        }

        public void SetConfig(Config config)
        {
            if (config == null) return;
            this._config = config;
        }

        public void MapFunction(Func<BaseCell, BaseCell> mapFunction)
        {
            if (_cellMap == null) return;

            List<Vector2Int> keys = new List<Vector2Int>(_cellMap.Keys);
            foreach (Vector2Int key in keys)
            {
                // Skip if the key is not in the map
                if (!_cellMap.ContainsKey(key)) continue;

                // Apply the map function to the cell
                BaseCell cell = _cellMap[key];
                _cellMap[key] = mapFunction(cell);
            }
        }

        public List<BaseCellData> GetData()
        {
            List<BaseCellData> dataList = new List<BaseCellData>();
            List<Vector2Int> keys = new List<Vector2Int>(_cellMap.Keys);
            foreach (Vector2Int key in keys)
            {
                if (!_cellMap.ContainsKey(key)) continue;
                dataList.Add(_cellMap[key].Data);
            }
            return dataList;
        }

        public void SetData(List<BaseCellData> dataList)
        {
            if (dataList == null || dataList.Count == 0) return;
            foreach (Vector2Int key in _cellMap.Keys)
            {
                if (!_cellMap.ContainsKey(key)) continue;
                ICell cell = _cellMap[key];

                // Find the data with the same key
                BaseCellData data = dataList.Find(d => d.key == key);
                if (data == null) continue;

                // Set the cell's data
                cell.SetData(data);
            }
        }

        // Function to create a new cell
        bool CreateCell(Vector2Int key)
        {
            if (_cellMap.ContainsKey(key))
                return false;

            BaseCell cell = (BaseCell)Activator.CreateInstance(typeof(BaseCell), key);
            _cellMap[key] = cell;
            return true;
        }

        void Generate()
        {
            // Iterate through the grid dimensions and create cells
            Vector2Int dimensions = _config.gridDimensions;
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
            Vector2Int newDimensions = _config.gridDimensions;

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
    }
}
