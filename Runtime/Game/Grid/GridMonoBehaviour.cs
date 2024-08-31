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
    public class GridMonoBehaviour : MonoBehaviour
    {
        protected const string ASSET_PATH = "Assets/Resources/Darklight/Grid";
        protected const string CONFIG_PATH = ASSET_PATH + "/Config";
        protected const string COMPONENT_PATH = ASSET_PATH + "/Components";
        protected const string DATA_PATH = ASSET_PATH + "/Data";
        protected string Prefix => name;

        [SerializeField] protected Grid grid = new Grid();
        [SerializeField, Expandable] protected GridConfigDataObject configObj;
        [SerializeField, Expandable] protected GridComponentDataObject componentObj;
        [SerializeField, Expandable] protected AbstractGridDataObject dataObj;

        protected virtual void GenerateDataObjects()
        {
            configObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<GridConfigDataObject>
                (CONFIG_PATH, $"{Prefix}_Config");

            componentObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<GridComponentDataObject>
                (COMPONENT_PATH, $"{Prefix}_Components");

            dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<GridDataObject>
                (DATA_PATH, $"{Prefix}_Data");
        }

        public void Awake() => InitializeGrid();
        public virtual void InitializeGrid()
        {
            GenerateDataObjects();

            // Create a new grid from the config object
            Grid.Config config = configObj.ToConfig();
            grid = new Grid(config);
            LoadGridData();
            //Debug.Log($"{prefix} initialized grid.", this);
        }
        public void Update() => UpdateGrid();
        public virtual void UpdateGrid()
        {
            if (grid == null)
                InitializeGrid();

            // Assign the grid's config from the config object
            Grid.Config config = configObj.ToConfig();
            config.SetGridPosition(transform.position);
            grid.SetConfig(config);

            // Add components to the grid
            grid.MapFunction(cell =>
            {
                componentObj.UpdateComponents(cell);
                return cell;
            });

            // Update the grid
            grid.Update();
            //Debug.Log($"{prefix} updated grid.", this);
        }
        public virtual void SaveGridData()
        {
            if (dataObj == null) return;
            List<BaseCellData> dataList = grid.GetData();
            dataObj.SetData(dataList);
            Debug.Log($"{Prefix} saved grid data.", this);
        }

        public virtual void LoadGridData()
        {
            if (dataObj == null) return;

            List<BaseCellData> dataList = dataObj.GetData<BaseCellData>();
            if (dataList == null || dataList.Count == 0) return;

            grid.SetData(dataList);

            Debug.Log($"{Prefix} loaded grid data.", this);
        }

        public virtual void ClearData()
        {
            if (dataObj == null) return;
            dataObj.ClearData();
            grid.Clear();
            Debug.Log($"{Prefix} cleared grid data.", this);
        }


        public void OnDrawGizmos()
        {
            if (grid == null) return;
            if (!configObj.showGizmos) return;

            CellGizmoRenderer gizmoRenderer = new CellGizmoRenderer();
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

            CellEditorGizmoRenderer editorGizmoRenderer = new CellEditorGizmoRenderer();
            grid.MapFunction(cell =>
            {
                cell.Accept(editorGizmoRenderer);
                return cell;
            });
        }
    }
    #endregion


#if UNITY_EDITOR
    [CustomEditor(typeof(GridMonoBehaviour), true)]
    public class GridMonoBehaviourEditor : UnityEditor.Editor
    {
        protected SerializedObject _serializedObject;
        GridMonoBehaviour _script;

        SerializedProperty _gridProp;
        SerializedProperty _configObjProp;
        SerializedProperty _componentObjProp;
        SerializedProperty _dataObjProp;

        protected virtual void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (GridMonoBehaviour)target;

            // Cache the serialized properties
            _gridProp = _serializedObject.FindProperty("grid");
            _configObjProp = _serializedObject.FindProperty("configObj");
            _componentObjProp = _serializedObject.FindProperty("componentObj");
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
            EditorGUILayout.PropertyField(_componentObjProp);

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