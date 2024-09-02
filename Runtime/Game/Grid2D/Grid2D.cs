using System;
using System.Collections.Generic;
using System.Linq;

using Darklight.UnityExt.Behaviour.Interface;
using Darklight.UnityExt.Editor;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [ExecuteAlways]
    public partial class Grid2D : MonoBehaviour, IPreload
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
        [SerializeField] Config _config;
        [SerializeField] Dictionary<Vector2Int, Cell2D> _cellMap;

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
        protected Dictionary<Vector2Int, Cell2D> cellMap
        {
            get
            {
                if (_cellMap == null)
                    _cellMap = new Dictionary<Vector2Int, Cell2D>();
                return _cellMap;
            }
            set { _cellMap = value; }
        }
        protected ConsoleGUI console => _console;


        // -- (( VISITORS )) ------------------ >>
        protected Cell2D.Visitor cellUpdateVisitor => new Cell2D.Visitor(cell =>
        {
            cell.RecalculateDataFromGrid(this);
            cell.Update();
        });
        protected Cell2D.Visitor cellGizmoVisitor => new Cell2D.Visitor(cell => cell.DrawGizmos());
        protected Cell2D.Visitor cellEditorVisitor => new Cell2D.Visitor(cell => cell.DrawEditor());

        // ======== [[ METHODS ]] ============================================================ >>>>
        #region -- (( UNITY RUNTIME )) -------- )))
        public void Awake() => Preload();

        public void Start() => Initialize();

        public void OnDrawGizmos()
        {
            if (cellMap == null || cellMap.Count == 0) return;
            SendVisitorToAllCells(cellGizmoVisitor);
        }

        public void OnDrawEditor()
        {
            if (cellMap == null || cellMap.Count == 0) return;
            SendVisitorToAllCells(cellEditorVisitor);
        }
        #endregion

        #region -- (( IPreload )) -------- )))
        public virtual void Preload()
        {
            _isInitialized = false;

            if (config == null)
                config = new Config();
            cellMap = new Dictionary<Vector2Int, Cell2D>();

            _isLoaded = true;
            console.Log($"{CONSOLE_PREFIX} preloaded.");
        }

        public virtual void Initialize()
        {
            // Generate a new grid from the config
            bool mapGenerated = GenerateCellMap();


            _isInitialized = mapGenerated;
            console.Log($"{CONSOLE_PREFIX} initialized.");
        }

        public virtual void Refresh()
        {
            // Initialize if not already
            if (!_isInitialized)
            {
                Initialize();
                return;
            }

            // Resize the grid if the dimensions have changed
            ResizeCellMap();

            // Update the cells
            SendVisitorToAllCells(cellUpdateVisitor);
        }

        public virtual void Clear()
        {
            if (cellMap == null || cellMap.Count == 0) return;

            // Clear the cell map
            cellMap.Clear();

            _isInitialized = false;
            console.Log($"{CONSOLE_PREFIX} cleared.");
        }
        #endregion

        // -- (( VISITOR PATTERN )) -------- )))
        public void SendVisitorToAllCells(Cell2D.Visitor visitor)
        {
            if (cellMap == null) return;

            List<Vector2Int> keys = new List<Vector2Int>(cellMap.Keys);
            foreach (Vector2Int key in keys)
            {
                // Skip if the key is not in the map
                if (!cellMap.ContainsKey(key)) continue;

                // Apply the map function to the cell
                Cell2D cell = cellMap[key];
                cell.Accept(visitor);
            }
        }

        // -- (( GETTERS )) -------- )))
        public Config GetConfig()
        {
            return config;
        }

        public List<Cell2D> GetCells()
        {
            return new List<Cell2D>(cellMap.Values);
        }

        // (( SETTERS )) -------- )))
        public void SetConfig(Config config)
        {
            if (config == null) return;
            this.config = config;
        }

        public void SetCells(List<Cell2D> cells)
        {
            if (cells == null || cells.Count == 0) return;
            foreach (Cell2D cell in cells)
            {
                if (cell == null) continue;
                if (cellMap.ContainsKey(cell.Key))
                    cellMap[cell.Key] = cell;
                else
                    cellMap.Add(cell.Key, cell);
            }
        }

        // ======== [[ PROTECTED METHODS ]] ======================================================= >>>>
        bool CreateCell(Vector2Int key)
        {
            if (_cellMap.ContainsKey(key))
                return false;

            Cell2D cell = (Cell2D)Activator.CreateInstance(typeof(Cell2D), key);
            _cellMap[key] = cell;
            return true;
        }

        bool GenerateCellMap()
        {
            _cellMap = new Dictionary<Vector2Int, Cell2D>();

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

            if (_cellMap.Count == 0)
            {
                console.Log($"{CONSOLE_PREFIX} failed to generate cells.", 0, LogSeverity.Error);
                return false;
            }

            Debug.Log($"{CONSOLE_PREFIX} generated {_cellMap.Count} cells.");
            console.Log($"{CONSOLE_PREFIX} generated {cellMap.Count} cells.", 1);
            return true;
        }

        void ResizeCellMap()
        {
            if (!_isInitialized) return;
            Vector2Int newDimensions = config.GridDimensions;

            // Remove null cells from the map
            int removedCount = 0;
            List<Vector2Int> keys = new List<Vector2Int>(cellMap.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                Vector2Int key = keys[i];
                if (key.x >= newDimensions.x || key.y >= newDimensions.y)
                {
                    cellMap.Remove(key);
                    removedCount++;
                }
            }

            // Generate new cells if needed
            if (removedCount > 0)
                GenerateCellMap();
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
                _script.Awake();
            }

            public override void OnInspectorGUI()
            {
                _serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Space();

                int cellCount = _script.cellMap.Count;
                EditorGUILayout.LabelField($"Cell Count: {cellCount}");

                if (GUILayout.Button("Initialize")) { _script.Initialize(); }

                // < DEFAULT INSPECTOR > ------------------ >>
                CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

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
                _script.OnDrawEditor();
            }
        }
#endif


    }


}
