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

    #region -- << ABSTRACT CLASS >> : MONOBEHAVIOURGRID2D ------------------------------------ >>
    public abstract class BaseMonoBehaviourGrid : MonoBehaviour
    {
        protected const string ASSET_PATH = "Assets/Resources/Darklight/Grid2D";
        protected const string CONFIG_PATH = ASSET_PATH + "/Config";
        protected const string DATA_PATH = ASSET_PATH + "/Data";

        [SerializeField, Expandable] protected Grid2D_ConfigObject configObj;
        protected virtual void GenerateConfigObj()
        {
            configObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_ConfigObject>(CONFIG_PATH, name);
        }
        [SerializeField, Expandable] protected Grid2D_AbstractDataObject dataObj;
        protected virtual void GenerateDataObj()
        {
            dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_DataObject>(DATA_PATH, name);
        }
        public abstract void InitializeGrid();
        public abstract void UpdateGrid();

        public abstract void SaveGridData();
        public abstract void LoadGridData();
        public abstract void ClearData();

        public abstract void DrawGizmos(bool editMode = false);
    }
    #endregion

    #region -- << GENERIC CLASS >> : MONOBEHAVIOURGRID2D ------------------------------------ >>
    public class GenericMonoBehaviourGrid<TCell, TData> : BaseMonoBehaviourGrid
        where TCell : BaseCell, new()
        where TData : BaseCellData, new()
    {
        [SerializeField] protected GenericGrid<TCell, TData> grid;

        // Initialize the grid with the specific types
        public override void InitializeGrid()
        {
            // If the config object is null, generate a new one
            if (configObj == null)
                GenerateConfigObj();

            // If the data object is null, generate a new one
            if (dataObj == null)
                GenerateDataObj();

            // Create a new grid from the config object
            grid = new GenericGrid<TCell, TData>(configObj.ToConfig());
            LoadGridData();

            Debug.Log($"MonoBehaviourGrid2D initialized.", this);
        }

        public virtual void Update() => UpdateGrid();
        public override void UpdateGrid()
        {
            if (grid == null)
                InitializeGrid();

            // Apply the config values to the grid
            grid.UpdateConfig(configObj.ToConfig());
        }

        public override void SaveGridData()
        {
            if (dataObj == null) return;
            dataObj.SaveGridData(grid);

            Debug.Log($"Saved grid data to {dataObj.name}.", this);
        }

        public override void LoadGridData()
        {
            if (dataObj == null) return;
            if (dataObj is not Grid2D_GenericDataObject<TCell, TData> typedDataObj) return;

            List<TData> dataList = typedDataObj.GetCellData();
            if (dataList == null || dataList.Count == 0) return;

            grid.map.ApplyDataList(dataList);

            Debug.Log($"Loaded {dataList.Count} cells from {dataObj.name}.", this);
        }

        public override void ClearData()
        {
            if (dataObj == null) return;
            dataObj.ClearData();

            grid.map.Clear();
        }


        public override void DrawGizmos(bool editMode = false)
        {
            if (grid == null) return;
            grid.DrawGizmos(editMode);
        }
    }
    #endregion

    public class MonoBehaviourGrid2D : GenericMonoBehaviourGrid<Cell, CellData> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(BaseMonoBehaviourGrid), true)]
    public class MonoBehaviourGrid2DCustomEditor : UnityEditor.Editor
    {
        SerializedObject _serializedObject;
        BaseMonoBehaviourGrid _script;
        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (BaseMonoBehaviourGrid)target;
            _script.InitializeGrid();
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Grid Data"))
            {
                _script.SaveGridData();
            }
            if (GUILayout.Button("Load Grid Data"))
            {
                _script.LoadGridData();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Update Grid"))
            {
                _script.UpdateGrid();
            }
            if (GUILayout.Button("Clear Data"))
            {
                _script.ClearData();
            }
            EditorGUILayout.EndHorizontal();

            CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                Repaint();
            }

            _script.UpdateGrid();
        }

        private void OnSceneGUI()
        {
            _script.DrawGizmos(true);
        }
    }
#endif
}