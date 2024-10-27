using System.Collections.Generic;
using System.Linq;

using Darklight.UnityExt.Behaviour;

using NaughtyAttributes;

using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix : MonoBehaviour
    {
        protected const string ASSET_PATH = "Assets/Resources/Darklight/Matrix";

        Dictionary<Vector2Int, Node> _nodeMap = new Dictionary<Vector2Int, Node>();
        [SerializeField] Config _config = new Config();
        [SerializeField] Data _data = new Data();


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
        public Dictionary<Vector2Int, Node> NodeMap
        {
            get
            {
                if (_nodeMap == null)
                    _nodeMap = new Dictionary<Vector2Int, Node>();
                return _nodeMap;
            }
            protected set { _nodeMap = value; }
        }
        public HashSet<Vector2Int> NodeKeys => new HashSet<Vector2Int>(NodeMap.Keys);
        public HashSet<Node> NodeValues => new HashSet<Node>(NodeMap.Values);


        // -- (( VISITORS )) ------------------ >>
        protected Node.Visitor CellUpdateVisitor =>
            new Node.Visitor(cell =>
            {
                cell.RecalculateDataFromGrid(this);
                cell.Refresh();
                return true;
            });

        protected Node.Visitor CellGizmoVisitor =>
            new Node.Visitor(cell =>
            {
                cell.DrawGizmos();
                return true;
            });



        public Node GetCell(Vector2Int key)
        {
            if (NodeMap.ContainsKey(key))
                return NodeMap[key];
            return null;
        }

        public List<Node> GetCells()
        {
            return new List<Node>(NodeMap.Values);
        }


        public Node GetClosestCellTo(Vector2 position)
        {
            Node closestCell = null;
            float closestDistance = float.MaxValue;
            foreach (Node cell in NodeMap.Values)
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

        #region -- (( GETTERS )) -------- )))
        public Config GetConfig()
        {
            if (_config == null)
                _config = new Config();
            return _config;
        }

        #endregion


        #region -- (( VISITOR PATTERN )) -------- )))
        public void SendVisitorToCell(Vector2Int key, IVisitor<Node> visitor)
        {
            if (NodeMap == null)
                return;

            // Skip if the key is not in the map
            if (!NodeMap.ContainsKey(key))
                return;

            // Apply the map function to the cell
            Node cell = NodeMap[key];
            cell.Accept(visitor);
        }

        public void SendVisitorToAllCells(IVisitor<Node> visitor)
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
                Node cell = NodeMap[key];
                cell.Accept(visitor);
            }
        }
        #endregion

        public void SetCells(List<Node> cells)
        {
            if (cells == null || cells.Count == 0)
                return;
            foreach (Node cell in cells)
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
        void OnValidate() => Refresh();
        #endregion


        #region < PRIVATE_METHODS > [[ Internal Runtime ]] ================================================================

        [HideIf("_data.isPreloaded"), Button]
        public void Preload()
        {
            _data.isPreloaded = false;
            _data.isInitialized = false;

            // Create a new config if none exists
            if (_config == null)
            {
                _config = new Config();
            }

            // Create a new cell map
            _nodeMap = new Dictionary<Vector2Int, Node>();

            // Determine if the grid was preloaded
            _data.isPreloaded = true;

            // Invoke the grid preloaded event
            OnGridPreloaded?.Invoke();
        }

        [HideIf("_data.isInitialized"), Button]
        public void Initialize()
        {
            if (!_data.isPreloaded)
                Preload();

            // Generate a new grid from the config
            bool mapGenerated = Generate();

            // Determine if the grid was initialized
            _data.isInitialized = mapGenerated;

            // Return if the grid was not initialized
            if (!_data.isInitialized)
                return;

            // Invoke the grid initialized event
            OnGridInitialized?.Invoke();
        }

        [ShowIf("_data.isInitialized"), Button]
        public void Refresh()
        {
            // Initialize if not already
            if (!_data.isInitialized || _config == null)
            {
                Initialize();
                return;
            }

            // Resize the grid if the dimensions have changed
            Resize();

            _data.nodes = NodeMap.Values.ToArray();
            _data.originKey = CalculateOriginKey(_config);

            // Update the cells
            SendVisitorToAllCells(CellUpdateVisitor);
            OnGridUpdated?.Invoke();
        }

        [ShowIf("_data.isInitialized"), Button]
        public void Reset()
        {
            Clear();

            _config.SetToDefaults();

            Preload();
        }


        [Button]
        public void Clear()
        {
            _data.Reset();

            if (NodeMap != null)
                NodeMap.Clear(); // << Clear the map
        }



        #endregion

        #region < PRIVATE_METHODS > [[ Handle Cells ]] ================================================================
        bool CreateNode(Vector2Int key)
        {
            if (_nodeMap.ContainsKey(key))
                return false;

            Node node = new Node(this, key);
            _nodeMap[key] = node;
            return true;
        }

        bool RemoveNode(Vector2Int key)
        {
            if (!_nodeMap.ContainsKey(key))
                return false;

            _nodeMap.Remove(key);
            return true;
        }

        bool Generate()
        {
            // Skip if already initialized
            if (_data.isInitialized)
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
                    CreateNode(gridKey);
                }
            }

            if (_nodeMap.Count == 0)
                return false;
            return true;
        }

        void Resize()
        {
            if (!_data.isInitialized)
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
                    RemoveNode(key);
            }

            // Add cells that are in bounds
            for (int x = 0; x < newDimensions.x; x++)
            {
                for (int y = 0; y < newDimensions.y; y++)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);
                    CreateNode(gridKey);
                }
            }
        }
        #endregion

        public void Draw()
        {
            if (!_data.isInitialized) return;
            SendVisitorToAllCells(CellGizmoVisitor);
        }


        public static void CalculateCellTransform(
            out Vector3 position,
            out Vector2Int coordinate,
            out Vector3 normal,
            out Vector2 dimensions,
            Node cell,
            Matrix.Config config
        )
        {
            position = CalculatePositionFromKey(cell.Key, config);
            coordinate = CalculateCoordinateFromKey(cell.Key, config);
            normal = config.MatrixNormal;
            dimensions = config.NodeDimensions;
        }

        public static Vector3 CalculatePositionFromKey(Vector2Int key, Config config)
        {
            // Calculate the node position offset in world space based on dimensions
            Vector2 keyOffsetPos = key * config.NodeDimensions;

            // Calculate the origin position offset in world space based on alignment
            Vector2 originOffset = CalculateOriginOffset(config);

            // Calculate the spacing offset and clamp to avoid overlapping cells
            Vector2 spacingOffsetPos = config.NodeSpacing + Vector2.one;
            spacingOffsetPos.x = Mathf.Clamp(spacingOffsetPos.x, 0.5f, float.MaxValue);
            spacingOffsetPos.y = Mathf.Clamp(spacingOffsetPos.y, 0.5f, float.MaxValue);

            // Calculate bonding offsets
            Vector2 bondingOffset = Vector2.zero;
            if (key.y % 2 == 0)
                bondingOffset.x = config.NodeBonding.x;
            if (key.x % 2 == 0)
                bondingOffset.y = config.NodeBonding.y;

            Vector2 cellPosition = keyOffsetPos + originOffset;
            cellPosition *= spacingOffsetPos;
            cellPosition += bondingOffset;


            // Apply a scale transformation that flips the x-axis
            Vector3 scale = new Vector3(-1, 1, 1);  // Inverts the x-axis
            Vector3 transformedPosition = Vector3.Scale(new Vector3(cellPosition.x, cellPosition.y, 0), scale);

            // Apply rotation based on grid's normal and return the final world position
            Quaternion rotation = Quaternion.LookRotation(config.MatrixNormal, Vector3.forward);
            return config.MatrixPosition + (rotation * transformedPosition);
        }

        static Vector2 CalculateOriginOffset(Config config)
        {
            Vector2Int gridDimensions = config.MatrixDimensions - Vector2Int.one;
            Vector2 originOffset = Vector2.zero;

            switch (config.MatrixAlignment)
            {
                case Matrix.Alignment.BottomLeft:
                    originOffset = Vector2.zero;
                    break;
                case Matrix.Alignment.BottomCenter:
                    originOffset = new Vector2(
                        -gridDimensions.x * config.NodeDimensions.x / 2,
                        0
                    );
                    break;
                case Matrix.Alignment.BottomRight:
                    originOffset = new Vector2(
                        -gridDimensions.x * config.NodeDimensions.x,
                        0
                    );
                    break;
                case Matrix.Alignment.MiddleLeft:
                    originOffset = new Vector2(
                        0,
                        -gridDimensions.y * config.NodeDimensions.y / 2
                    );
                    break;
                case Matrix.Alignment.Center:
                    originOffset = new Vector2(
                        -gridDimensions.x * config.NodeDimensions.x / 2,
                        -gridDimensions.y * config.NodeDimensions.y / 2
                    );
                    break;
                case Matrix.Alignment.MiddleRight:
                    originOffset = new Vector2(
                        -gridDimensions.x * config.NodeDimensions.x,
                        -gridDimensions.y * config.NodeDimensions.y / 2
                    );
                    break;
                case Matrix.Alignment.TopLeft:
                    originOffset = new Vector2(
                        0,
                        -gridDimensions.y * config.NodeDimensions.y
                    );
                    break;
                case Matrix.Alignment.TopCenter:
                    originOffset = new Vector2(
                        -gridDimensions.x * config.NodeDimensions.x / 2,
                        -gridDimensions.y * config.NodeDimensions.y
                    );
                    break;
                case Matrix.Alignment.TopRight:
                    originOffset = new Vector2(
                        -gridDimensions.x * config.NodeDimensions.x,
                        -gridDimensions.y * config.NodeDimensions.y
                    );
                    break;
            }

            return originOffset;
        }


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
