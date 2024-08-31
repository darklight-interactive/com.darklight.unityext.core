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
    public class Grid2D_BaseMonoBehaviour : MonoBehaviour
    {
        protected const string ASSET_PATH = "Assets/Resources/Darklight/Grid2D";
        protected const string CONFIG_PATH = ASSET_PATH + "/Config";
        protected const string DATA_PATH = ASSET_PATH + "/SerializedData";
        protected const string PREFIX = "[GRID2D_MONO]";

        [SerializeField] protected Grid2D grid = new Grid2D();
        [SerializeField, Expandable] protected Grid2D_ConfigDataObject configObj;
        [SerializeField, Expandable] protected Grid2D_SerializedDataObject dataObj;

        protected virtual void GenerateDataObjects()
        {
            configObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_ConfigDataObject>
                (CONFIG_PATH, $"{PREFIX}_Config");

            dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_SerializedDataObject>
                (DATA_PATH, $"{PREFIX}_Data");
        }

        public void Awake() => InitializeGrid();
        public virtual void InitializeGrid()
        {
            GenerateDataObjects();

            // Create a new grid from the config object
            Grid2D_Config config = configObj.ToConfig();
            grid = new Grid2D(config);
            LoadGridData();
            //Debug.Log($"{prefix} initialized grid.", this);
        }
        public void Update() => UpdateGrid();
        public virtual void UpdateGrid()
        {
            if (grid == null)
                InitializeGrid();

            // Assign the grid's config from the config object
            Grid2D_Config config = configObj.ToConfig();
            config.SetGridPosition(transform.position);
            grid.SetConfig(config);

            // Update the grid
            grid.Update();
            //Debug.Log($"{prefix} updated grid.", this);
        }

        #region (( DATA MANAGEMENT )) ------------------ >>
        public virtual void SaveGridData()
        {
            if (dataObj == null) return;
            List<Cell2D_SerializedData> dataList = grid.GetData();
            dataObj.SetData(dataList);
        }

        public virtual void LoadGridData()
        {
            if (dataObj == null) return;

            List<Cell2D_SerializedData> dataList = dataObj.GetData();
            if (dataList == null || dataList.Count == 0) return;

            grid.SetData(dataList);
        }

        public virtual void ClearData()
        {
            if (dataObj == null) return;
            dataObj.ClearData();
            grid.Clear();
        }
        #endregion

        public void OnDrawGizmos()
        {
            if (grid == null) return;
            if (!configObj.showGizmos) return;

            Cell2D_GizmoRenderer gizmoRenderer = new Cell2D_GizmoRenderer();
            grid.MapFunction(cell =>
            {
                cell.Accept(gizmoRenderer);
                return cell;
            });
        }

        public void DrawEditorGizmos()
        {
            if (grid == null) return;
            if (!configObj.showEditorGizmos) return;

            Cell2D_EditorGizmoRenderer editorGizmoRenderer = new Cell2D_EditorGizmoRenderer();
            grid.MapFunction(cell =>
            {
                cell.Accept(editorGizmoRenderer);
                return cell;
            });
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Grid2D_BaseMonoBehaviour), true)]
    public class GridMonoBehaviourEditor : UnityEditor.Editor
    {
        protected SerializedObject _serializedObject;
        Grid2D_BaseMonoBehaviour _script;

        SerializedProperty _gridProp;
        SerializedProperty _configObjProp;
        SerializedProperty _dataObjProp;

        protected virtual void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (Grid2D_BaseMonoBehaviour)target;

            // Cache the serialized properties
            _gridProp = _serializedObject.FindProperty("grid");
            _configObjProp = _serializedObject.FindProperty("configObj");
            _dataObjProp = _serializedObject.FindProperty("dataObj");

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

        void OnSceneGUI()
        {
            _script.DrawEditorGizmos();
        }
    }
#endif
}