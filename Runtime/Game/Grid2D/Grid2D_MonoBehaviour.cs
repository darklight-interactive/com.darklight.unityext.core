using UnityEngine;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using Darklight.UnityExt.Game;
using System.Collections.Generic;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [ExecuteAlways]
    public class Grid2D_MonoBehaviour : MonoBehaviour
    {
        // ======== [[ CONSTANTS ]] ======================================================= >>>>
        protected const string ASSET_PATH = "Assets/Resources/Darklight/Grid2D";
        protected const string CONFIG_PATH = ASSET_PATH + "/Config";
        protected const string DATA_PATH = ASSET_PATH + "/SerializedData";
        protected const string PREFIX = "[GRID2D_MONO]";

        // ======== [[ FIELDS ]] ======================================================= >>>>
        [SerializeField] Grid2D _grid = new Grid2D();
        [SerializeField, Expandable] Grid2D_ConfigDataObject _configObj;
        [SerializeField, Expandable] Grid2D_SerializedDataObject _dataObj;

        // ======== [[ PROPERTIES ]] ======================================================= >>>>
        public Grid2D Grid => _grid;


        // ======== [[ METHODS ]] ======================================================= >>>>

        #region  -- (( UNITY RUNTIME )) ------------------ >>
        public void Awake() => InitializeGrid();

        public void Update()
        {
            if (_grid == null)
                InitializeGrid();

            // Assign the grid's config from the config object
            Grid2D_Config config = _configObj.CreateGridConfig();
            if (config.LockToTransform)
            {
                // Set the grid's position and normal to the transform's position and forward
                config.SetGridPosition(transform.position);
                config.SetGridNormal(transform.forward);
            }
            _grid.SetConfig(config);

            // Update the grid
            _grid.Update();
        }

        public void OnDrawGizmos()
        {
            if (_grid == null) return;
            _grid.DrawGizmos();
        }

        public void DrawEditorGizmos()
        {
            if (_grid == null) return;
            _grid.DrawEditor();
        }
        #endregion

        #region -- (( PUBLIC METHODS )) ------------------ >>
        public void InitializeGrid()
        {
            GenerateDataObjects();

            // Create a new grid from the config object
            Grid2D_Config config = _configObj.CreateGridConfig();
            _grid = new Grid2D(config);
            LoadGridData();
            //Debug.Log($"{prefix} initialized grid.", this);
        }

        public virtual void SaveGridData()
        {
            if (_dataObj == null) return;
            _dataObj.SaveCells(_grid.GetCells());

            Debug.Log($"{PREFIX} saved {_grid.GetCells().Count} cells.", this);
        }

        public virtual void LoadGridData()
        {
            if (_dataObj == null) return;
            List<Cell2D> loadedCells = _dataObj.LoadCells();
            if (loadedCells == null || loadedCells.Count == 0) return;

            _grid.SetCells(loadedCells);
            Debug.Log($"{PREFIX} loaded {loadedCells.Count} cells.", this);
        }

        public virtual void ClearData()
        {
            if (_dataObj == null) return;
            _dataObj.ClearData();
            _grid.Clear();
        }
        #endregion

        // -- (( PRIVATE METHODS )) ------------------ >>
        void GenerateDataObjects()
        {
            _configObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_ConfigDataObject>
                (CONFIG_PATH, $"{PREFIX}_Config");

            _dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_SerializedDataObject>
                (DATA_PATH, $"{PREFIX}_Data");
        }



    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Grid2D_MonoBehaviour), true)]
    public class GridMonoBehaviourEditor : UnityEditor.Editor
    {
        protected SerializedObject _serializedObject;
        Grid2D_MonoBehaviour _script;

        SerializedProperty _gridProp;
        SerializedProperty _configObjProp;
        SerializedProperty _dataObjProp;

        protected virtual void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (Grid2D_MonoBehaviour)target;

            // Cache the serialized properties
            _gridProp = _serializedObject.FindProperty("_grid");
            _configObjProp = _serializedObject.FindProperty("_configObj");
            _dataObjProp = _serializedObject.FindProperty("_dataObj");

            _script.InitializeGrid();
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_gridProp);

            CustomInspectorGUI.DrawHorizontalLine(Color.gray, 4, 10);
            EditorGUILayout.PropertyField(_configObjProp);

            CustomInspectorGUI.DrawHorizontalLine(Color.gray, 4, 10);
            DrawDataManagementButtons();
            EditorGUILayout.PropertyField(_dataObjProp);

            // Apply changes if any
            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                Repaint();
            }

            _script.Update();
        }

        void DrawDataManagementButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Data"))
            {
                _script.SaveGridData();
            }
            if (GUILayout.Button("Load Data"))
            {
                _script.LoadGridData();
            }
            if (GUILayout.Button("Clear Data"))
            {
                _script.ClearData();
            }
            EditorGUILayout.EndHorizontal();
        }

        void OnSceneGUI()
        {
            _script.DrawEditorGizmos();
        }
    }
#endif
}