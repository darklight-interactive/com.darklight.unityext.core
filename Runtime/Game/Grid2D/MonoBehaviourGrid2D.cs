using UnityEngine;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using Darklight.UnityExt.Game;
using System.Collections.Generic;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Base class for a 2D grid. Creates a Grid2D with Cell2D objects.
/// </summary>
[ExecuteAlways]
public class MonoBehaviourGrid2D<TCell> : MonoBehaviour, IGrid2D where TCell : Grid2D.Cell, new()
{
    protected const string CONFIG_PATH = "Assets/Resources/Grid2D_Config";
    protected const string DATA_PATH = "Assets/Resources/Grid2D_Data";

    public Type cellType => typeof(TCell);
    [SerializeField] Grid2D<TCell> _grid = new Grid2D<TCell>();
    protected Grid2D<TCell> grid => _grid;

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


    public void Awake() => InitializeGrid();
    public virtual void InitializeGrid()
    {
        // Create the grid based on the config object
        if (_configObj == null)
            GenerateConfigObj();
        _grid = new Grid2D<TCell>(_configObj.ToConfig());

        // Initialize the data object
        if (_dataObj == null)
            GenerateDataObj();
    }

    public void Update()
    {
        if (_grid.initialized)
        {
            ApplyTransform();
        }
    }

    public virtual void RefreshData()
    {
        _grid.RefreshData();
    }

    protected virtual void ApplyTransform()
    {
        if (_configObj)
            _configObj.showTransformValues = !_lockToTransform;

        if (_lockToTransform)
            _grid.SetTransform(transform);
    }

    public virtual void DrawGizmos(bool editMode = false)
    {
        _grid.DrawGizmos(_editMode);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MonoBehaviourGrid2D<>), true)]
public class MonoBehaviourGrid2DCustomEditor : UnityEditor.Editor
{
    SerializedObject _serializedObject;
    IGrid2D _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (IGrid2D)target;
        _script.InitializeGrid();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();


        CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();

            _script.RefreshData();

            EditorUtility.SetDirty(target);
            Repaint();

            //Debug.Log($"Inspector Updated: {_script}", target);
        }
    }

    void OnSceneGUI()
    {
        _script.DrawGizmos();
    }
}
#endif