using UnityEngine;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using Darklight.UnityExt.Game;
using System.Collections.Generic;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MonoBehaviourGrid2D : MonoBehaviour, IGrid2D
{
    protected const string ASSET_PATH = "Assets/Resources/Darklight/Grid2D";
    protected const string CONFIG_PATH = ASSET_PATH + "/Config";
    protected const string DATA_PATH = ASSET_PATH + "/Data";

    [SerializeField] Grid2D<Cell2D> _grid;
    public Grid2D<Cell2D> grid { get => _grid; protected set => _grid = value; }

    [HorizontalLine(4)]
    [SerializeField, Expandable] Grid2D_ConfigObject _configObj;
    public virtual void GenerateConfigObj()
    {
        _configObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_ConfigObject>(CONFIG_PATH, name);
    }

    [SerializeField, Expandable] Grid2D_AbstractDataObject _dataObj;
    public Grid2D_AbstractDataObject dataObj { get => _dataObj; protected set => _dataObj = value; }
    public virtual void GenerateDataObj()
    {
        _dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_DataObject>(DATA_PATH, name);
    }

    [HorizontalLine(2)]
    [SerializeField] bool _editMode;
    [SerializeField] bool _lockToTransform;


    public void Awake() => Initialize();
    void Initialize()
    {
        // Create the grid based on the config object
        if (_configObj == null)
            GenerateConfigObj();

        Initialize(_configObj.ToConfig());

        if (_dataObj == null)
            GenerateDataObj();

        UpdateConfig();
    }

    public virtual void Initialize(AbstractGrid2D.Config config)
    {
        grid = new Grid2D<Cell2D>(config);
    }

    public void UpdateConfig() => UpdateConfig(_configObj.ToConfig());
    public virtual void UpdateConfig(AbstractGrid2D.Config config)
    {
        if (grid == null)
            Initialize(config);

        if (_lockToTransform)
            config.SetWorldSpaceData(transform.position, transform.forward);
        else
            config.SetWorldSpaceData(Vector3.zero, Vector3.up);

        if (_configObj)
            _configObj.showTransformValues = !_lockToTransform;

        if (grid == null)
        {
            Debug.LogError("Grid2D: Cannot update config with null grid.");
            return;
        }

        grid.UpdateConfig(config);

        if (dataObj == null)
            GenerateDataObj();
        dataObj.SaveGridData(grid);
    }

    public virtual void DrawGizmos(bool editMode = false)
    {
        if (grid == null) return;
        grid.DrawGizmos(_editMode);
    }
}


#region -- << GENERIC CLASS >> : MONOBEHAVIOURGRID2D ------------------------------------ >>
public class MonoBehaviourGrid2D<TCell> : MonoBehaviourGrid2D, IGrid2D where TCell : Cell2D, new()
{
    public new Grid2D<TCell> grid;

    public override void Initialize(AbstractGrid2D.Config config)
    {
        grid = new Grid2D<TCell>(config);
    }

    public override void UpdateConfig(AbstractGrid2D.Config config)
    {
        base.UpdateConfig(config);

        grid.UpdateConfig(config);
        grid.cellMap.LoadData<Cell2D.Data>(dataObj.GetGridData<Cell2D.Data>());


        dataObj.SaveGridData(grid);
    }

    public override void DrawGizmos(bool editMode = false)
    {
        if (grid == null) return;
        grid.DrawGizmos(editMode);
    }
}
#endregion

#if UNITY_EDITOR
[CustomEditor(typeof(MonoBehaviourGrid2D), true)]
public class MonoBehaviourGrid2DCustomEditor : UnityEditor.Editor
{
    SerializedObject _serializedObject;
    MonoBehaviourGrid2D _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (MonoBehaviourGrid2D)target;
        _script.Awake();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        // If the grid is a typed grid, draw the typed grid property instead of the basic grid property
        SerializedProperty gridProp = _serializedObject.FindProperty("grid");
        if (gridProp != null)
        {
            EditorGUILayout.PropertyField(gridProp, true);

            SerializedProperty basicGridProp = _serializedObject.FindProperty("_grid");
            DrawPropertiesExcluding(_serializedObject, "_grid", "grid");
        }
        else
        {
            CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);
        }


        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            Repaint();
        }

        _script.UpdateConfig();

    }

    private void OnSceneGUI()
    {
        _script.DrawGizmos(true);
    }
}
#endif