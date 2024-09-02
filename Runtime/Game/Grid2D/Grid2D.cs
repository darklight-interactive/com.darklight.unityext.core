using System.Collections.Generic;
using System.Linq;

using Darklight.UnityExt.Behaviour.Interface;
using Darklight.UnityExt.Editor;

using UnityEngine;
using System;
using Sherbert.Framework.Generic;




#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [ExecuteAlways]
    public partial class Grid2D : MonoBehaviour, IUnityEditorListener
    {
        // ======== [[ CONSTANTS ]] ======================================================= >>>>
        protected const string ASSET_PATH = "Assets/Resources/Darklight/Grid2D";
        protected const string CONFIG_PATH = ASSET_PATH + "/Config";
        protected const string DATA_PATH = ASSET_PATH + "/SerializedData";
        protected const string CONSOLE_PREFIX = "[GRID2D]";

        // ======== [[ FIELDS ]] ======================================================= >>>>
        ConsoleGUI _console = new ConsoleGUI();
        [SerializeField, ShowOnly] bool _isLoaded = false;
        [SerializeField, ShowOnly] bool _isInitialized = false;

        [Space(10), Header("Internal Data")]
        [SerializeField] Config _config;

        Dictionary<Vector2Int, Cell2D> _map;
        [SerializeField] List<Cell2D> _cellsInMap;
        [SerializeField] ComponentRegistry _componentSystem;


        // ======== [[ PROPERTIES ]] ======================================================= >>>>
        protected Config config
        {
            get
            {
                if (_config == null)
                    _config = new Config();
                return _config;
            }
            set { _config = value; }
        }
        protected Dictionary<Vector2Int, Cell2D> map
        {
            get
            {
                if (_map == null)
                    _map = new Dictionary<Vector2Int, Cell2D>();
                return _map;
            }
            set { _map = value; }
        }
        protected ConsoleGUI internalConsole => _console;

        // -- (( VISITORS )) ------------------ >>
        protected Cell2D.Visitor cellUpdateVisitor => new Cell2D.Visitor(cell =>
        {
            cell.RecalculateDataFromGrid(this);
            cell.Update();
        });

        // ======== [[ EVENTS ]] ======================================================= >>>>
        public delegate void GridEvent();
        public event GridEvent OnGridPreloaded;
        public event GridEvent OnGridInitialized;
        public event GridEvent OnGridUpdated;

        // ======== [[ METHODS ]] ============================================================ >>>>
        public void OnEditorReloaded()
        {
            _console.Clear();
            Awake();
        }

        #region -- (( UNITY )) -------- )))
        public void Awake() => Preload();

        public void Start() => Initialize();

        public void Update() => Refresh();
        #endregion

        #region -- (( RUNTIME )) -------- )))
        void Preload()
        {
            _isLoaded = false;
            _isInitialized = false;

            // Create a new config if none exists
            if (_config == null)
                _config = new Config();

            // Create a new cell map
            _map = new Dictionary<Vector2Int, Cell2D>();

            // Create a new component system
            _componentSystem = new ComponentRegistry(this);

            // Determine if the grid was preloaded
            _isLoaded = true;

            // Invoke the grid preloaded event
            OnGridPreloaded?.Invoke();
            internalConsole.Log($"Preloaded: {_isLoaded}");
        }

        void Initialize()
        {
            // Generate a new grid from the config
            bool mapGenerated = GenerateCellMap();

            // Determine if the grid was initialized
            _isInitialized = mapGenerated;

            // Return if the grid was not initialized
            if (!_isInitialized) return;

            // Invoke the grid initialized event
            OnGridInitialized?.Invoke();
            internalConsole.Log($"Initialized: {_isInitialized}");
        }

        void Refresh()
        {
            // Initialize if not already
            if (!_isInitialized)
            {
                Initialize();
                return;
            }

            // Resize the grid if the dimensions have changed
            ResizeCellMap();

            _cellsInMap = new List<Cell2D>(map.Values);

            // Update the cells
            SendVisitorToAllCells(cellUpdateVisitor);
            OnGridUpdated?.Invoke();
        }

        void Clear()
        {
            _isLoaded = false;
            _isInitialized = false;

            if (map != null)
                map.Clear(); // << Clear the map

            internalConsole.Log("Cleared");
        }
        #endregion

        #region -- (( VISITOR PATTERN )) -------- )))
        public void SendVisitorToAllCells(Cell2D.Visitor visitor)
        {
            if (map == null) return;

            List<Vector2Int> keys = new List<Vector2Int>(map.Keys);
            foreach (Vector2Int key in keys)
            {
                // Skip if the key is not in the map
                if (!map.ContainsKey(key)) continue;

                // Apply the map function to the cell
                Cell2D cell = map[key];
                cell.Accept(visitor);
            }
        }
        #endregion

        // -- (( GETTERS )) -------- )))
        public Config GetConfig()
        {
            return config;
        }

        public List<Cell2D> GetCells()
        {
            return new List<Cell2D>(map.Values);
        }

        // (( SETTERS )) -------- )))
        public void SetConfig(Config config)
        {
            if (config == null) return;

            // Check if the grid should lock to the transform
            if (config.LockToTransform)
            {
                // Set the grid's position and normal to the transform's position and forward
                config.SetGridPosition(transform.position);
                config.SetGridNormal(transform.forward);
            }

            // Assign the new config
            this.config = config;
        }

        public void SetCells(List<Cell2D> cells)
        {
            if (cells == null || cells.Count == 0) return;
            foreach (Cell2D cell in cells)
            {
                if (cell == null) continue;
                if (map.ContainsKey(cell.Key))
                    map[cell.Key] = cell;
                else
                    map.Add(cell.Key, cell);
            }
        }

        // ======== [[ PROTECTED METHODS ]] ======================================================= >>>>
        bool CreateCell(Vector2Int key)
        {
            if (_map.ContainsKey(key))
                return false;

            Cell2D cell = (Cell2D)Activator.CreateInstance(typeof(Cell2D), key);
            _map[key] = cell;
            return true;
        }

        bool RemoveCell(Vector2Int key)
        {
            if (!_map.ContainsKey(key))
                return false;

            _map.Remove(key);
            return true;
        }

        bool GenerateCellMap()
        {
            // Skip if already initialized
            if (_isInitialized) return false;

            // Clear the map
            _map.Clear();

            // Iterate through the grid dimensions and create cells
            Vector2Int dimensions = config.GridDimensions;
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    Vector2Int gridKey = new Vector2Int(x, y);
                    CreateCell(gridKey);
                }
            }

            if (_map.Count == 0) return false;
            return true;
        }

        void ResizeCellMap()
        {
            if (!_isInitialized) return;
            Vector2Int newDimensions = config.GridDimensions;

            // Check if the dimensions have changed
            int newGridArea = newDimensions.x * newDimensions.y;
            int oldGridArea = map.Count;
            if (newGridArea == oldGridArea) return;

            // Remove cells that are out of bounds
            List<Vector2Int> keys = new List<Vector2Int>(map.Keys);
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

        // ======== [[ NESTED TYPES ]] ======================================================= >>>>
        public enum Alignment
        {
            TopLeft, TopCenter, TopRight,
            MiddleLeft, Center, MiddleRight,
            BottomLeft, BottomCenter, BottomRight
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(Grid2D), true)]
        public class Grid2D_Editor : UnityEditor.Editor
        {
            protected SerializedObject _serializedObject;
            Grid2D _script;

            protected virtual void OnEnable()
            {
                _serializedObject = new SerializedObject(target);
                _script = (Grid2D)target;

                _script.internalConsole.Clear();
                _script.Awake();
            }

            public override void OnInspectorGUI()
            {
                _serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Space();


                // < DEFAULT INSPECTOR > ------------------ >>
                CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

                // < CUSTOM INSPECTOR > ------------------ >>
                CustomInspectorGUI.DrawHorizontalLine(Color.gray, 4, 10);
                if (GUILayout.Button("Initialize")) { _script.Initialize(); }

                // < CONSOLE > ------------------ >>
                CustomInspectorGUI.DrawHorizontalLine(Color.gray, 4, 10);
                _script._console.DrawInEditor();

                // Apply changes if any
                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();
                    _script.Refresh();
                    EditorUtility.SetDirty(target);
                    Repaint();
                }

            }

            void OnSceneGUI()
            {
                _script.Refresh();
            }
        }
#endif
    }


}
