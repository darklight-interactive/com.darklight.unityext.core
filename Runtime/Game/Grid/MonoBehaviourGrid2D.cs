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
    [ExecuteAlways]
    public abstract class AbstractMonoBehaviourGrid : MonoBehaviour
    {
        protected const string ASSET_PATH = "Assets/Resources/Darklight/Grid2D";
        protected const string CONFIG_PATH = ASSET_PATH + "/Config";
        protected const string DATA_PATH = ASSET_PATH + "/Data";

        protected string prefix => name;

        protected AbstractGrid grid;
        protected abstract void GenerateConfigObj();
        protected abstract void GenerateDataObj();

        public void Awake() => InitializeGrid();
        public abstract void InitializeGrid();

        public void Update() => UpdateGrid();
        public abstract void UpdateGrid();

        public abstract void SaveGridData();
        public abstract void LoadGridData();
        public abstract void ClearData();

        public abstract void DrawGizmos();
    }
    #endregion

    #region -- << GENERIC CLASS >> : MONOBEHAVIOURGRID2D ------------------------------------ >>
    public class GenericMonoBehaviourGrid<TCell, TData> : AbstractMonoBehaviourGrid
        where TCell : BaseCell, new()
        where TData : BaseCellData, new()
    {
        [Header("Internal Grid Class")]
        [SerializeField] protected new GenericGrid<TCell, TData> grid;

        [Header("Configuration ScriptableObject")]
        [SerializeField, Expandable] protected GridMapConfig_DataObject configObj;
        protected override void GenerateConfigObj()
        {
            configObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<GridMapConfig_DataObject>(CONFIG_PATH, name);
        }

        [Header("Preset Data ScriptableObject")]
        [SerializeField, Expandable] protected AbstractGrid_DataObject dataObj;
        protected override void GenerateDataObj()
        {
            dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid_DataObject>(DATA_PATH, name);
        }

        public override void InitializeGrid()
        {
            // If the config object is null, generate a new one
            if (configObj == null)
                GenerateConfigObj();

            // If the data object is null, generate a new one
            if (dataObj == null)
                GenerateDataObj();

            // Create a new grid from the config object
            GridMapConfig config = configObj.ToConfig();
            grid = new GenericGrid<TCell, TData>(config);
            LoadGridData();
            //Debug.Log($"{prefix} initialized grid.", this);
        }

        public override void UpdateGrid()
        {
            if (grid == null)
                InitializeGrid();

            // Assign the grid's config from the config object
            GridMapConfig config = configObj.ToConfig();
            if (config.lockToTransform)
                config.SetOriginPosition(transform.position);
            grid.SetConfig(config);

            grid.Update();
            //Debug.Log($"{prefix} updated grid.", this);
        }

        public override void SaveGridData()
        {
            if (dataObj == null) return;
            dataObj.SaveGridData(grid);
            Debug.Log($"{prefix} saved grid data.", this);
        }

        public override void LoadGridData()
        {
            if (dataObj == null) return;
            if (dataObj is not GenericGrid_DataObject<TCell, TData> typedDataObj) return;

            List<TData> dataList = typedDataObj.GetCellData();
            if (dataList == null || dataList.Count == 0) return;

            grid.SetData(dataList);

            Debug.Log($"{prefix} loaded grid data.", this);
        }

        public override void ClearData()
        {
            if (dataObj == null) return;
            dataObj.ClearData();
            grid.ClearData();
            Debug.Log($"{prefix} cleared grid data.", this);
        }


        public override void DrawGizmos()
        {
            if (grid == null) return;
            grid.DrawGizmos();
        }
    }
    #endregion

    public class MonoBehaviourGrid2D : GenericMonoBehaviourGrid<Cell, CellData> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(AbstractMonoBehaviourGrid), true)]
    public class MonoBehaviourGrid2DCustomEditor : UnityEditor.Editor
    {
        SerializedObject _serializedObject;
        AbstractMonoBehaviourGrid _script;
        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (AbstractMonoBehaviourGrid)target;
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
            _script.DrawGizmos();
        }
    }
#endif
}