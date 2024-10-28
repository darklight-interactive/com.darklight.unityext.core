using System.Collections.Generic;
using System.Linq;

using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Utility;

using NaughtyAttributes;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Darklight.UnityExt.Matrix
{
    [ExecuteAlways]
    public partial class Matrix : MonoBehaviour
    {
        protected const string ASSET_PATH = "Assets/Resources/Darklight/Matrix";
        protected Dictionary<Vector2Int, Node> _map = new Dictionary<Vector2Int, Node>();

        [Header("Matrix Data")]
        [SerializeField] Data _data;

        [Header("Matrix Config")]
        [SerializeField, Expandable] MatrixConfigPreset _configPreset;
        [SerializeField, DisableIf("HasConfigPreset"), AllowNesting] Config _config = new Config();

        [ShowOnly] public bool isPreloaded;
        [ShowOnly] public bool isInitialized;

        public bool HasConfigPreset => _configPreset != null;

        public Vector3 Position {
            get{
                if (_config.LockPosToTransform)
                    return transform.position;
                else
                    return _config.MatrixPosition;
            }
        }

        Node.Visitor UpdateNodeVisitor => new Node.Visitor(node =>
        {
            node.Refresh();
            return true;
        });

        Node.Visitor DrawDefaultGizmosVisitor = new Node.Visitor(node =>
        {
            node.GetWorldSpaceValues(out Vector3 position, out Vector2 dimensions, out Vector3 normal);
            CustomGizmos.DrawWireRect(position, dimensions, normal, Color.grey);
            return true;
        });


        #region < PRIVATE_METHODS > [[ Unity Runtime ]] ================================================================
        void Awake() => Preload();
        void Start() => Initialize();
        void Update() => Refresh();
        void OnDrawGizmos() => DrawGizmos();
        void OnValidate() => Refresh();
        #endregion

        #region < PROTECTED_METHODS > [[ Internal Runtime ]] ================================================================
        protected void Preload()
        {
            isPreloaded = false;
            isInitialized = false;

            // Create a new config if none exists
            if (_config == null)
                _config = new Config();

            // Create a new cell map
            _map = new Dictionary<Vector2Int, Node>();

            // Determine if the grid was preloaded
            isPreloaded = true;
        }

        protected void Initialize()
        {
            if (!isPreloaded)
                Preload();

            // Generate a new grid from the config
            bool mapGenerated = Generate();

            // Determine if the grid was initialized
            isInitialized = mapGenerated;

            // Return if the grid was not initialized
            if (!isInitialized)
                return;
        }
        #endregion

        #region < PRIVATE_METHODS > [[ Node Generation ]] ================================================================
        bool CreateNode(Vector2Int key)
        {
            if (_map.ContainsKey(key))
                return false;

            Node node = new Node(this, key);
            _map[key] = node;
            return true;
        }

        bool RemoveNode(Vector2Int key)
        {
            if (!_map.ContainsKey(key))
                return false;

            _map.Remove(key);
            return true;
        }

        bool Generate()
        {
            // Skip if already initialized
            if (isInitialized)
                return false;

            // Clear the map
            _map.Clear();

            // Iterate through the grid dimensions and create cells
            for (int x = 0; x < _config.MatrixColumns; x++)
            {
                for (int y = 0; y < _config.MatrixRows; y++)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);
                    CreateNode(gridKey);
                }
            }

            if (_map.Count == 0)
                return false;
            return true;
        }

        void Resize()
        {
            if (!isInitialized)
                return;

            // Check if the dimensions have changed
            int newGridArea = _config.MatrixColumns * _config.MatrixRows;
            int oldGridArea = _map.Count;
            if (newGridArea == oldGridArea)
                return;

            // Remove cells that are out of bounds
            List<Vector2Int> keys = new List<Vector2Int>(_map.Keys);
            foreach (Vector2Int key in keys)
            {
                if (key.x >= _config.MatrixColumns || key.y >= _config.MatrixRows)
                    RemoveNode(key);
            }

            // Add cells that are in bounds
            for (int x = 0; x < _config.MatrixColumns; x++)
            {
                for (int y = 0; y < _config.MatrixRows; y++)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);
                    CreateNode(gridKey);
                }
            }
        }

        #endregion

        #region < PROTECTED_METHODS > [[ Node Calculations ]] ================================================================
        protected void CalculateNodeWorldSpaceFromKey(Vector2Int key, out Vector3 position, out Vector2Int coordinate, out Vector3 normal, out Vector2 dimensions)
        {
            position = CalculateNodePositionFromKey(key);
            coordinate = CalculateCoordinateFromKey(key);
            normal = _config.MatrixNormal;
            dimensions = _config.NodeDimensions;
        }

        protected Vector3 CalculateNodePositionFromKey(Vector2Int key)
        {
            // Calculate the node position offset in world space based on dimensions
            Vector2 keyOffsetPos = key * _config.NodeDimensions;

            // Calculate the origin position offset in world space based on alignment
            Vector2 originOffset = CalculateOriginPositionOffset();

            // Calculate the spacing offset and clamp to avoid overlapping cells
            Vector2 spacingOffsetPos = _config.NodeSpacing + Vector2.one;
            spacingOffsetPos.x = Mathf.Clamp(spacingOffsetPos.x, 0.5f, float.MaxValue);
            spacingOffsetPos.y = Mathf.Clamp(spacingOffsetPos.y, 0.5f, float.MaxValue);

            // Calculate bonding offsets
            Vector2 bondingOffset = Vector2.zero;
            if (key.y % 2 == 0)
                bondingOffset.x = _config.NodeBonding.x;
            if (key.x % 2 == 0)
                bondingOffset.y = _config.NodeBonding.y;

            Vector2 cellPosition = keyOffsetPos + originOffset;
            cellPosition *= spacingOffsetPos;
            cellPosition += bondingOffset;


            // Apply a scale transformation that flips the x-axis
            Vector3 scale = new Vector3(-1, 1, 1);  // Inverts the x-axis
            Vector3 transformedPosition = Vector3.Scale(new Vector3(cellPosition.x, cellPosition.y, 0), scale);

            // Apply rotation based on grid's normal and return the final world position
            Quaternion rotation = Quaternion.LookRotation(_config.MatrixNormal, Vector3.forward);
            return _config.MatrixPosition + (rotation * transformedPosition);
        }

        protected Vector2Int CalculateCoordinateFromKey(Vector2Int key)
        {
            Vector2Int originKey = CalculateOriginKey();
            return key - originKey;
        }

        protected Vector2Int CalculateOriginKey()
        {
            int rows = _config.MatrixRows - 1;
            int columns = _config.MatrixColumns - 1;

            Vector2Int originKey = Vector2Int.zero;

            switch (_config.MatrixAlignment)
            {
                case Matrix.Alignment.BottomLeft:
                    originKey = new Vector2Int(0, 0);
                    break;
                case Matrix.Alignment.BottomCenter:
                    originKey = new Vector2Int(Mathf.FloorToInt(columns / 2), 0);
                    break;
                case Matrix.Alignment.BottomRight:
                    originKey = new Vector2Int(Mathf.FloorToInt(columns), 0);
                    break;
                case Matrix.Alignment.MiddleLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(rows / 2));
                    break;
                case Matrix.Alignment.Center:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(columns / 2),
                        Mathf.FloorToInt(rows / 2)
                    );
                    break;
                case Matrix.Alignment.MiddleRight:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(columns),
                        Mathf.FloorToInt(rows / 2)
                    );
                    break;
                case Matrix.Alignment.TopLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(rows));
                    break;
                case Matrix.Alignment.TopCenter:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(columns / 2),
                        Mathf.FloorToInt(rows)
                    );
                    break;
                case Matrix.Alignment.TopRight:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(columns),
                        Mathf.FloorToInt(rows)
                    );
                    break;
            }

            return originKey;
        }

        protected Vector2 CalculateOriginPositionOffset()
        {
            int rows = _config.MatrixRows - 1;
            int columns = _config.MatrixColumns - 1; Vector2 originOffset = Vector2.zero;

            switch (_config.MatrixAlignment)
            {
                case Matrix.Alignment.BottomLeft:
                    originOffset = Vector2.zero;
                    break;
                case Matrix.Alignment.BottomCenter:
                    originOffset = new Vector2(
                        -columns * _config.NodeDimensions.x / 2,
                        0
                    );
                    break;
                case Matrix.Alignment.BottomRight:
                    originOffset = new Vector2(
                        -columns * _config.NodeDimensions.x,
                        0
                    );
                    break;
                case Matrix.Alignment.MiddleLeft:
                    originOffset = new Vector2(
                        0,
                        -rows * _config.NodeDimensions.y / 2
                    );
                    break;
                case Matrix.Alignment.Center:
                    originOffset = new Vector2(
                        -columns * _config.NodeDimensions.x / 2,
                        -rows * _config.NodeDimensions.y / 2
                    );
                    break;
                case Matrix.Alignment.MiddleRight:
                    originOffset = new Vector2(
                        -columns * _config.NodeDimensions.x,
                        -rows * _config.NodeDimensions.y / 2
                    );
                    break;
                case Matrix.Alignment.TopLeft:
                    originOffset = new Vector2(
                        0,
                        -rows * _config.NodeDimensions.y
                    );
                    break;
                case Matrix.Alignment.TopCenter:
                    originOffset = new Vector2(
                        -columns * _config.NodeDimensions.x / 2,
                        -rows * _config.NodeDimensions.y
                    );
                    break;
                case Matrix.Alignment.TopRight:
                    originOffset = new Vector2(
                        -columns * _config.NodeDimensions.x,
                        -rows * _config.NodeDimensions.y
                    );
                    break;
            }

            return originOffset;
        }

        #endregion

        #region < PUBLIC_METHODS > [[ Matrix Handlers ]] ================================================================ 

        public void ExtractConfigToPreset(string name)
        {
            _configPreset = ScriptableObjectUtility.CreateOrLoadScriptableObject<MatrixConfigPreset>(ASSET_PATH, name);
            _configPreset.SetData(_config);
        }

        public void Refresh()
        {
            if (!isPreloaded)
            {
                Preload();
                return;
            }

            // Initialize if not already
            if (!isInitialized || _config == null)
            {
                Initialize();
                return;
            }

            if (_configPreset != null)
                _config = _configPreset.ToData();

            // Resize the grid if the dimensions have changed
            Resize();

            _data.nodes = _map.Values.ToArray();

            // Update the cells
            SendVisitorToAllCells(UpdateNodeVisitor);
        }

        public void Reset()
        {
            _config = new Config();
            _data = new Data(this);

            Preload();
        }

        #endregion

        #region < PUBLIC_METHODS > [[ Visitor Handlers ]] ================================================================ 
        public void SendVisitorToCell(Vector2Int key, IVisitor<Node> visitor)
        {
            if (_map == null)
                return;

            // Skip if the key is not in the map
            if (!_map.ContainsKey(key))
                return;

            // Apply the map function to the cell
            Node cell = _map[key];
            cell.Accept(visitor);
        }

        public void SendVisitorToCells(List<Vector2Int> keys, IVisitor<Node> visitor)
        {
            if (_map == null)
                return;

            foreach (Vector2Int key in keys)
            {
                // Skip if the key is not in the map
                if (!_map.ContainsKey(key))
                    continue;

                // Apply the map function to the cell
                Node cell = _map[key];
                cell.Accept(visitor);
            }
        }

        public void SendVisitorToAllCells(IVisitor<Node> visitor)
        {
            if (_map == null)
                return;

            List<Vector2Int> keys = new List<Vector2Int>(_map.Keys);
            foreach (Vector2Int key in keys)
            {
                // Skip if the key is not in the map
                if (!_map.ContainsKey(key))
                    continue;

                // Apply the map function to the cell
                Node cell = _map[key];
                cell.Accept(visitor);
            }
        }
        #endregion

        #region < PUBLIC_METHODS > [[ Getters ]] ================================================================ 
        public Node GetNode(Vector2Int key)
        {
            if (_map.ContainsKey(key))
                return _map[key];
            return null;
        }

        public Node GetClosestNodeTo(Vector2 position)
        {
            Node closestCell = null;
            float closestDistance = float.MaxValue;
            foreach (Node cell in _map.Values)
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
        #endregion

        #region < PUBLIC_METHODS > [[ Setters ]] ================================================================ 
        public void SetCells(List<Node> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return;
            foreach (Node node in nodes)
            {
                if (node == null)
                    continue;
                if (_map.ContainsKey(node.Key))
                    _map[node.Key] = node;
                else
                    _map.Add(node.Key, node);
            }
        }
        #endregion

        #region < PUBLIC_METHODS > [[ Draw Gizmos ]] ================================================================
        public void DrawGizmos()
        {
            if (!isInitialized) return;
            SendVisitorToAllCells(DrawDefaultGizmosVisitor);
        }
        #endregion

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
    }
}
