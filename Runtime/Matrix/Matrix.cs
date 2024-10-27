using System;
using System.Collections.Generic;

using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;

using NaughtyAttributes;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Darklight.UnityExt.Matrix
{
    [ExecuteAlways]
    [System.Serializable]
    public partial class Matrix : MonoBehaviour
    {
        protected const string ASSET_PATH = "Assets/Resources/Darklight/Matrix";

        Dictionary<Vector2Int, MatrixNode> _nodeMap;
        ComponentRegistry _componentRegistry;


        [Space(5), Header("Cells")]
        [SerializeField] List<MatrixNode> _nodeList;

        [Space(5), Header("Config")]
        [SerializeField] Config _config;


        [Header("States")]
        [SerializeField, ShowOnly] bool _isInitialized = false;
        [SerializeField, ShowOnly] bool _isPreloaded = false;

        // ======== [[ EVENTS ]] ======================================================= >>>>
        public delegate void GridEvent();
        public event GridEvent OnGridPreloaded;
        public event GridEvent OnGridInitialized;
        public event GridEvent OnGridUpdated;


        // ======== [[ NESTED TYPES ]] ======================================================= >>>>
        public enum Alignment
        {
            TopLeft,
            TopCenter,
            TopRight,
            MiddleLeft,
            Center,
            MiddleRight,
            BottomLeft,
            BottomCenter,
            BottomRight
        }

        // ======== [[ PROPERTIES ]] ======================================================= >>>>
        public HashSet<Vector2Int> CellKeys => new HashSet<Vector2Int>(NodeMap.Keys);
        public Dictionary<Vector2Int, MatrixNode> NodeMap
        {
            get
            {
                if (_nodeMap == null)
                    _nodeMap = new Dictionary<Vector2Int, MatrixNode>();
                return _nodeMap;
            }
            protected set { _nodeMap = value; }
        }
        public HashSet<MatrixNode> CellValues => new HashSet<MatrixNode>(NodeMap.Values);

        // -- (( VISITORS )) ------------------ >>
        protected MatrixNode.Visitor CellUpdateVisitor =>
            new MatrixNode.Visitor(cell =>
            {
                cell.RecalculateDataFromGrid(this);
                cell.Refresh();
                return true;
            });

        protected MatrixNode.Visitor CellGizmoVisitor =>
            new MatrixNode.Visitor(cell =>
            {
                cell.DrawGizmos();
                return true;
            });

        public static void CalculateCellTransform(
            out Vector3 position,
            out Vector2Int coordinate,
            out Vector3 normal,
            out Vector2 dimensions,
            MatrixNode cell,
            Matrix.Config config
        )
        {
            position = CalculatePositionFromKey(cell.Key, config);
            coordinate = CalculateCoordinateFromKey(cell.Key, config);
            normal = config.MatrixNormal;
            dimensions = config.NodeDimensions;
        }

        public static Vector3 CalculatePositionFromKey(Vector2Int key, Matrix.Config config)
        {
            // Get the origin key of the grid
            Vector2Int originKey = CalculateOriginKey(config);

            // Calculate the spacing offset && clamp it to avoid overlapping cells
            Vector2 spacingOffsetPos = config.NodeSpacing + Vector2.one; // << Add 1 to allow for values of 0
            spacingOffsetPos.x = Mathf.Clamp(spacingOffsetPos.x, 0.5f, float.MaxValue);
            spacingOffsetPos.y = Mathf.Clamp(spacingOffsetPos.y, 0.5f, float.MaxValue);

            // Calculate bonding offsets
            Vector2 bondingOffset = Vector2.zero;
            if (key.y % 2 == 0)
                bondingOffset.x = config.NodeBonding.x;
            if (key.x % 2 == 0)
                bondingOffset.y = config.NodeBonding.y;

            // Calculate the offset of the cell from the grid origin
            Vector2 originOffsetPos = originKey * config.NodeDimensions;
            Vector2 keyOffsetPos = key * config.NodeDimensions;

            // Calculate the final position of the cell
            Vector2 cellPosition = (keyOffsetPos - originOffsetPos); // << Calculate the position offset
            cellPosition *= spacingOffsetPos; // << Multiply the spacing offset
            cellPosition += bondingOffset; // << Add the bonding offset

            // Create a rotation matrix based on the grid's normal
            Quaternion rotation = Quaternion.LookRotation(config.MatrixNormal, Vector3.forward);

            // Apply the rotation to the grid offset and return the final world position
            return config.MatrixPosition + (rotation * new Vector3(cellPosition.x, cellPosition.y, 0));
        }


        public MatrixNode GetCell(Vector2Int key)
        {
            if (NodeMap.ContainsKey(key))
                return NodeMap[key];
            return null;
        }

        public List<MatrixNode> GetCells()
        {
            return new List<MatrixNode>(NodeMap.Values);
        }

        public List<MatrixNode> GetCellsByComponentType(ComponentTypeKey type)
        {
            List<MatrixNode> cells = new List<MatrixNode>();
            foreach (MatrixNode cell in NodeMap.Values)
            {
                if (cell.ComponentReg.HasComponent(type))
                {
                    cells.Add(cell);
                }
            }
            return cells;
        }

        public MatrixNode GetClosestCellTo(Vector2 position)
        {
            MatrixNode closestCell = null;
            float closestDistance = float.MaxValue;
            foreach (MatrixNode cell in NodeMap.Values)
            {
                float distance = Vector2.Distance(cell.Position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCell = cell;
                }
            }
            return closestCell;
        }

        public List<TComponent> GetComponentsByType<TComponent>()
            where TComponent : MatrixNode.Component
        {
            List<TComponent> components = new List<TComponent>();
            foreach (MatrixNode cell in NodeMap.Values)
            {
                TComponent component = cell.ComponentReg.GetComponent<TComponent>();
                if (component != null)
                {
                    components.Add(component);
                }
            }
            return components;
        }

        #region -- (( GETTERS )) -------- )))
        public Config GetConfig()
        {
            if (_config == null)
                _config = new Config();
            return _config;
        }

        #endregion


        #region -- (( VISITOR PATTERN )) -------- )))
        public void SendVisitorToCell(Vector2Int key, IVisitor<MatrixNode> visitor)
        {
            if (NodeMap == null)
                return;

            // Skip if the key is not in the map
            if (!NodeMap.ContainsKey(key))
                return;

            // Apply the map function to the cell
            MatrixNode cell = NodeMap[key];
            cell.Accept(visitor);
        }

        public void SendVisitorToAllCells(IVisitor<MatrixNode> visitor)
        {
            if (NodeMap == null)
                return;

            List<Vector2Int> keys = new List<Vector2Int>(NodeMap.Keys);
            foreach (Vector2Int key in keys)
            {
                // Skip if the key is not in the map
                if (!NodeMap.ContainsKey(key))
                    continue;

                // Apply the map function to the cell
                MatrixNode cell = NodeMap[key];
                cell.Accept(visitor);
            }
        }
        #endregion

        public void SetCells(List<MatrixNode> cells)
        {
            if (cells == null || cells.Count == 0)
                return;
            foreach (MatrixNode cell in cells)
            {
                if (cell == null)
                    continue;
                if (NodeMap.ContainsKey(cell.Key))
                    NodeMap[cell.Key] = cell;
                else
                    NodeMap.Add(cell.Key, cell);
            }
        }


        #region < PRIVATE_METHODS > [[ Unity Runtime ]] ================================================================
        void Awake() => Preload();
        void Start() => Initialize();
        void Update() => Refresh();
        void OnDrawGizmos() => Draw();
        #endregion


        #region < PRIVATE_METHODS > [[ Internal Runtime ]] ================================================================

        [HideIf("_isPreloaded"), Button]
        public void Preload()
        {
            _isPreloaded = false;
            _isInitialized = false;

            // Create a new config if none exists
            if (_config == null)
            {
                _config = new Config();
            }

            // Create a new cell map
            _nodeMap = new Dictionary<Vector2Int, MatrixNode>();

            // Create a new component system
            _componentRegistry = new ComponentRegistry(this);

            // Determine if the grid was preloaded
            _isPreloaded = true;

            // Invoke the grid preloaded event
            OnGridPreloaded?.Invoke();
        }

        [HideIf("_isInitialized"), Button]
        public void Initialize()
        {
            if (!_isPreloaded)
                Preload();

            // Generate a new grid from the config
            bool mapGenerated = GenerateCellMap();

            // Determine if the grid was initialized
            _isInitialized = mapGenerated;

            // Return if the grid was not initialized
            if (!_isInitialized)
                return;

            // Invoke the grid initialized event
            OnGridInitialized?.Invoke();
        }

        [ShowIf("_isInitialized"), Button]
        public void Refresh()
        {
            // Initialize if not already
            if (!_isInitialized || _config == null)
            {
                Initialize();
                return;
            }

            // Resize the grid if the dimensions have changed
            ResizeCellMap();

            _nodeList = new List<MatrixNode>(NodeMap.Values);

            // Update the cells
            SendVisitorToAllCells(CellUpdateVisitor);
            OnGridUpdated?.Invoke();
        }

        [ShowIf("_isInitialized"), Button]
        public void Reset()
        {
            Clear();
            Preload();
        }


        [Button]
        public void Clear()
        {
            _isPreloaded = false;
            _isInitialized = false;

            if (NodeMap != null)
                NodeMap.Clear(); // << Clear the map
            _nodeList.Clear(); // << Clear the list
        }

        public void Draw()
        {
            if (!_isInitialized) return;
            SendVisitorToAllCells(CellGizmoVisitor);
        }

        #endregion

        #region < PRIVATE_METHODS > [[ Handle Cells ]] ================================================================
        bool CreateCell(Vector2Int key)
        {
            if (_nodeMap.ContainsKey(key))
                return false;

            MatrixNode cell = (MatrixNode)Activator.CreateInstance(typeof(MatrixNode), key);
            _nodeMap[key] = cell;
            return true;
        }

        bool GenerateCellMap()
        {
            // Skip if already initialized
            if (_isInitialized)
                return false;

            // Clear the map
            _nodeMap.Clear();

            // Iterate through the grid dimensions and create cells
            Vector2Int dimensions = _config.MatrixDimensions;
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);
                    CreateCell(gridKey);
                }
            }

            if (_nodeMap.Count == 0)
                return false;
            return true;
        }

        bool RemoveCell(Vector2Int key)
        {
            if (!_nodeMap.ContainsKey(key))
                return false;

            _nodeMap.Remove(key);
            return true;
        }

        void ResizeCellMap()
        {
            if (!_isInitialized)
                return;
            Vector2Int newDimensions = _config.MatrixDimensions;

            // Check if the dimensions have changed
            int newGridArea = newDimensions.x * newDimensions.y;
            int oldGridArea = NodeMap.Count;
            if (newGridArea == oldGridArea)
                return;

            // Remove cells that are out of bounds
            List<Vector2Int> keys = new List<Vector2Int>(NodeMap.Keys);
            foreach (Vector2Int key in keys)
            {
                if (key.x >= newDimensions.x || key.y >= newDimensions.y)
                    RemoveCell(key);
            }

            // Add cells that are in bounds
            for (int x = 0; x < newDimensions.x; x++)
            {
                for (int y = 0; y < newDimensions.y; y++)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);
                    CreateCell(gridKey);
                }
            }
        }
        #endregion


        static Vector2Int CalculateCoordinateFromKey(Vector2Int key, Matrix.Config config)
        {
            Vector2Int originKey = CalculateOriginKey(config);
            return key - originKey;
        }

        static Vector2Int CalculateOriginKey(Matrix.Config config)
        {
            Vector2Int gridDimensions = config.MatrixDimensions - Vector2Int.one;
            Vector2Int originKey = Vector2Int.zero;

            switch (config.MatrixAlignment)
            {
                case Matrix.Alignment.BottomLeft:
                    originKey = new Vector2Int(0, 0);
                    break;
                case Matrix.Alignment.BottomCenter:
                    originKey = new Vector2Int(Mathf.FloorToInt(gridDimensions.x / 2), 0);
                    break;
                case Matrix.Alignment.BottomRight:
                    originKey = new Vector2Int(Mathf.FloorToInt(gridDimensions.x), 0);
                    break;
                case Matrix.Alignment.MiddleLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(gridDimensions.y / 2));
                    break;
                case Matrix.Alignment.Center:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(gridDimensions.x / 2),
                        Mathf.FloorToInt(gridDimensions.y / 2)
                    );
                    break;
                case Matrix.Alignment.MiddleRight:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(gridDimensions.x),
                        Mathf.FloorToInt(gridDimensions.y / 2)
                    );
                    break;
                case Matrix.Alignment.TopLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(gridDimensions.y));
                    break;
                case Matrix.Alignment.TopCenter:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(gridDimensions.x / 2),
                        Mathf.FloorToInt(gridDimensions.y)
                    );
                    break;
                case Matrix.Alignment.TopRight:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(gridDimensions.x),
                        Mathf.FloorToInt(gridDimensions.y)
                    );
                    break;
            }

            return originKey;
        }
    }
}
