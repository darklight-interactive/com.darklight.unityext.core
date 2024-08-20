using System.Collections.Generic;
using UnityEngine;
using Darklight.UnityExt.Editor;

using NaughtyAttributes;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game
{

    /// <summary>
    /// Definition of the Grid2D MonoBehaviour class. This class is used to create and manage a 2D grid of cells.
    /// </summary>
    public class Grid2D : MonoBehaviour
    {
        public ConsoleGUI consoleGUI { get; private set; } = new ConsoleGUI();

        #region << DATA >> : Map ------------------------------------------- >>
        [System.Serializable]
        public class Map
        {
            Grid2D _grid;
            Dictionary<Vector2Int, Cell2D> _dataMap = new();
            [SerializeField] List<Cell2D> _dataList = new();

            public Map(Grid2D grid)
            {
                _grid = grid;
                InitializeDataMap();
            }

            /// <summary>
            /// Initializes the data map with default grid data values
            /// </summary>
            void InitializeDataMap()
            {
                _grid.consoleGUI.Log("Initializing Data Map");

                if (_grid == null || _grid.settings == null) return;

                _dataMap.Clear();
                for (int x = 0; x < _grid.settings.gridWidth; x++)
                {
                    for (int y = 0; y < _grid.settings.gridHeight; y++)
                    {
                        // Create a new data object & initialize it
                        Vector2Int gridKey = new Vector2Int(x, y);

                        // Check if there is existing data for this cell
                        Cell2D existingData = GetCellData(gridKey);
                        if (existingData == null)
                        {
                            Cell2D newData = new Cell2D(_grid, gridKey);
                            AddCellData(gridKey, newData);
                            _grid.consoleGUI.Log($"New Cell2D: {gridKey}");
                        }
                    }
                }

                _dataList = new List<Cell2D>(_dataMap.Values); // Update the data list
                _grid.consoleGUI.Log("Data Map Initialized");
            }

            void AddCellData(Vector2Int key, Cell2D data)
            {
                if (_dataMap.ContainsKey(key))
                    _dataMap[key] = data;
                else
                    _dataMap.Add(key, data);
            }

            public virtual Cell2D GetCellData(Vector2Int key)
            {
                _dataMap.TryGetValue(key, out Cell2D data);
                return data;
            }

            public void UpdateData()
            {
                _dataList = new List<Cell2D>(_dataMap.Values);
            }

            public void RefreshData()
            {
                InitializeDataMap();
            }

            public virtual void ClearData()
            {
                _dataMap.Clear();
            }


#if UNITY_EDITOR

            public void DrawGizmos()
            {
                foreach (Cell2D data in _dataList)
                {
                    data.DrawGizmos();
                }
            }

#endif

        }
        #endregion

        [SerializeField, Expandable] Grid2DSettings _settings;
        public Grid2DSettings settings => _settings;

        [SerializeField] Map _map;
        public Map map => _map;

        public void Awake()
        {
            _map = new Map(this);
        }

        public void Update()
        {
            _map.UpdateData();
        }

        public void Refresh()
        {
            _map.RefreshData();
        }

        void OnDrawGizmos()
        {
            _map.DrawGizmos();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Grid2D))]
    public class Grid2DCustomEditor : UnityEditor.Editor
    {
        SerializedObject _serializedObject;
        Grid2D _script;
        bool _showInternalConsole = false;
        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (Grid2D)target;
            _script.Awake();
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            CustomInspectorGUI.CreateFoldout(ref _showInternalConsole, "Internal Console", () =>
            {
                _script.consoleGUI.DrawInEditor();
            });

            CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                _script.Refresh();
                EditorUtility.SetDirty(_script);
            }
        }
    }
#endif
}