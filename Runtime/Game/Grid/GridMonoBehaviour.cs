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
    public abstract class AbstractGridMonoBehaviour : MonoBehaviour
    {
        protected const string ASSET_PATH = "Assets/Resources/Darklight/Grid";
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
    public abstract class GenericGridMonoBehaviour<TCell, TData> : AbstractGridMonoBehaviour
        where TCell : AbstractCell, new()
        where TData : BaseCellData, new()
    {
        [SerializeField] protected new BaseGrid<TCell, TData> grid;
        [SerializeField, Expandable] protected GridConfigDataObject configObj;
        [SerializeField, Expandable] protected AbstractGridDataObject dataObj;

        protected override void GenerateConfigObj()
        {
            configObj = ScriptableObject.CreateInstance<GridConfigDataObject>();
            configObj.name = $"{prefix}_Config";
            Debug.Log($"{prefix} generated config object.", this);
        }

        protected override void GenerateDataObj()
        {
            dataObj = ScriptableObject.CreateInstance<GridDataObject>();
            dataObj.name = $"{prefix}_Data";
            Debug.Log($"{prefix} generated data object.", this);
        }

        public override void InitializeGrid()
        {
            GenerateConfigObj();
            GenerateDataObj();

            // Create a new grid from the config object
            AbstractGrid.Config config = configObj.ToConfig();
            grid = new BaseGrid<TCell, TData>(config);
            LoadGridData();
            //Debug.Log($"{prefix} initialized grid.", this);
        }

        public override void UpdateGrid()
        {
            if (grid == null)
                InitializeGrid();

            // Assign the grid's config from the config object
            AbstractGrid.Config config = configObj.ToConfig();

            config.SetGridPosition(transform.position);
            grid.SetConfig(config);
            grid.Update();
            //Debug.Log($"{prefix} updated grid.", this);
        }

        public override void SaveGridData()
        {
            if (dataObj == null) return;
            List<TData> dataList = grid.GetData<TData>();
            dataObj.SetData(dataList);
            Debug.Log($"{prefix} saved grid data.", this);
        }

        public override void LoadGridData()
        {
            if (dataObj == null) return;
            //if (dataObj is not GridDataObject typedDataObj) return;

            List<TData> dataList = dataObj.GetData<TData>();
            if (dataList == null || dataList.Count == 0) return;

            grid.SetData(dataList);

            Debug.Log($"{prefix} loaded grid data.", this);
        }

        public override void ClearData()
        {
            if (dataObj == null) return;
            dataObj.ClearData();
            grid.Clear();
            Debug.Log($"{prefix} cleared grid data.", this);
        }


        public override void DrawGizmos()
        {
            if (grid == null) return;
            grid.DrawGizmos();
        }
    }

    public abstract class GenericGridMonoBehaviour<TCell> : GenericGridMonoBehaviour<TCell, BaseCellData>
        where TCell : AbstractCell, new()
    {
    }

    #endregion

    public class GridMonoBehaviour : GenericGridMonoBehaviour<BaseCell, BaseCellData>
    {
        protected override void GenerateDataObj()
        {
            dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<GridDataObject>(DATA_PATH, name);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AbstractGridMonoBehaviour), true)]
    public class GridMonoBehaviourEditor : UnityEditor.Editor
    {
        protected SerializedObject _serializedObject;
        AbstractGridMonoBehaviour _script;

        SerializedProperty _grid;
        SerializedProperty _configObj;
        SerializedProperty _dataObj;

        protected virtual void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (AbstractGridMonoBehaviour)target;

            // Cache the serialized properties
            _grid = _serializedObject.FindProperty("grid");
            _configObj = _serializedObject.FindProperty("configObj");
            _dataObj = _serializedObject.FindProperty("dataObj");

            _script.InitializeGrid();
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_grid);

            CustomInspectorGUI.DrawHorizontalLine(Color.gray, 4, 10);
            EditorGUILayout.PropertyField(_configObj);

            CustomInspectorGUI.DrawHorizontalLine(Color.gray, 4, 10);
            DrawDataManagementButtons();
            EditorGUILayout.PropertyField(_dataObj);

            // Apply changes if any
            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                Repaint();
            }

            _script.UpdateGrid();
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

        protected virtual void OnSceneGUI()
        {
            _script.DrawGizmos();
        }
    }
#endif
}