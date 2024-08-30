using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;

namespace Darklight.UnityExt.Game.Grid
{

    #region -- << ABSRACT CLASS >> : AbstractGrid ------------------------------------ >>
    public abstract class AbstractGrid
    {
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

        public abstract void Initialize(Config config = null);
        public abstract void Update();
        public abstract void Clear();
        public abstract List<TData> GetData<TData>() where TData : BaseCellData;
        public abstract void DrawGizmos();
    }
    #endregion

    #region -- << BASE CLASS >> : BaseGrid ------------------------------------ >>
    [System.Serializable]
    public class BaseGrid<TCell, TData> : AbstractGrid
        where TCell : AbstractCell
        where TData : BaseCellData
    {
        protected Dictionary<Vector2Int, TCell> cellMap = new Dictionary<Vector2Int, TCell>();

        // ===================== >> SERIALIZED FIELDS << ===================== //
        [SerializeField] protected Config config;
        [SerializeField] List<TCell> _cellList = new List<TCell>();
        [SerializeField] List<TData> _dataList = new List<TData>();

        // ===================== >> CONSTRUCTORS << ===================== //
        public BaseGrid() { }
        public BaseGrid(Config config) => Initialize(config);

        // ===================== >> PROTECTED METHODS << ===================== //
        protected void Generate()
        {
            // Function to create a new cell
            bool CreateCell(Vector2Int key)
            {
                if (cellMap.ContainsKey(key))
                    return false;

                TCell cell = (TCell)Activator.CreateInstance(typeof(TCell), key);
                cellMap[key] = cell;
                cell.ApplyConfig(config);
                return true;
            }

            // Iterate through the grid dimensions and create cells
            Vector2Int dimensions = config.gridDimensions;
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);
                    if (!cellMap.ContainsKey(gridKey))
                    {
                        // Create a new cell and add it to the map
                        CreateCell(gridKey);
                    }
                }
            }
        }

        protected void Resize()
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



        // ===================== >> PUBLIC METHODS << ===================== //

        // (( RUNTIME METHODS )) ------------------------------ >>
        public override void Initialize(Config config = null)
        {
            // Create a basic config if none is provided
            if (config == null)
                config = new Config();
            this.config = config;
            Generate();
        }
        public override void Update()
        {
            if (cellMap == null || cellMap.Count == 0) return;

            // Resize the grid if the dimensions have changed
            Resize();

            // Update each cell in the map
            MapFunction(cell =>
            {
                cell.Update();
                return cell;
            });

            // Update the data list
            _cellList = new List<TCell>(cellMap.Values);
            _dataList = GetData<TData>();
        }
        public override void Clear()
        {
            if (cellMap == null || cellMap.Count == 0) return;

            // Clear the data list
            _dataList.Clear();

            // Clear the cell map
            cellMap.Clear();
        }
        public override void DrawGizmos()
        {
            if (cellMap == null || cellMap.Count == 0) return;
            if (config.showGizmos == false) return;
            MapFunction(cell =>
            {
                cell.DrawGizmos(config.showEditorGizmos);
                return cell;
            });
        }

        public void MapFunction(Func<TCell, TCell> mapFunction)
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

        // (( SETTERS )) ------------------------------ >>
        public virtual void SetConfig(Config config)
        {
            if (config == null) return;
            this.config = config;
            MapFunction(cell =>
            {
                cell.ApplyConfig(config);
                return cell;
            });
        }

        public void SetData(List<TData> dataList)
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
                    AbstractCell cell = cellMap[cellData.key];
                    cell.SetData(cellData);
                }
            }
        }

        // (( GETTERS )) ------------------------------ >>
        public override List<T> GetData<T>()
        {
            if (cellMap == null || cellMap.Count == 0) return null;

            List<T> data = new List<T>();
            foreach (AbstractCell cell in cellMap.Values)
            {
                T cellData = cell.GetData<T>();
                if (cellData != null)
                {
                    data.Add(cellData);
                }
            }
            return data;
        }
    }
    #endregion

    public enum Alignment
    {
        TopLeft, TopCenter, TopRight,
        MiddleLeft, Center, MiddleRight,
        BottomLeft, BottomCenter, BottomRight
    }
}
