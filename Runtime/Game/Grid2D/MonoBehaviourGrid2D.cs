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
public class MonoBehaviourGrid2D<TCell> : MonoBehaviour, IGrid2D where TCell : Cell2D, new()
{
    protected const string CONFIG_PATH = "Assets/Resources/Grid2D_Config";
    protected const string DATA_PATH = "Assets/Resources/Grid2D_Data";
    public class DataObject : Grid2D_DataObject<TCell> { }


    [SerializeField] Grid2D<TCell> _grid;
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
        _dataObj.Initialize(_grid);
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
        Initialize(_configObj.ToConfig());
    }

    public void Initialize(AbstractGrid2D.Config config)
    {
        // Create the grid based on the config object
        _grid = new Grid2D<TCell>(config);

        // Initialize the data object
        if (_dataObj == null)
            GenerateDataObj();
    }

    public void UpdateConfig() => UpdateConfig(_configObj.ToConfig());
    public void UpdateConfig(AbstractGrid2D.Config config)
    {
        _grid.UpdateConfig(config);
        ApplyTransform();

        _dataObj.SaveData();
    }

    protected virtual void ApplyTransform()
    {
        if (_configObj)
            _configObj.showTransformValues = !_lockToTransform;

        if (_lockToTransform)
            _grid.SetTransformParent(transform);
        else
            _grid.ResetTransform();
    }

    public virtual void DrawGizmos(bool editMode = false)
    {
        _grid.DrawGizmos(_editMode);
    }

    void OnDrawGizmos()
    {
        DrawGizmos();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MonoBehaviourGrid2D<>), true)]
public class MonoBehaviourGrid2DCustomEditor : UnityEditor.Editor
{
    SerializedObject _serializedObject;
    MonoBehaviourGrid2D<Cell2D> _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (MonoBehaviourGrid2D<Cell2D>)target;
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

            _script.UpdateConfig();

            EditorUtility.SetDirty(target);
            Repaint();

            //Debug.Log($"Inspector Updated: {_script}", target);
        }
    }

    private void OnSceneGUI()
    {
        if (_script == null)
            return;
        _script.DrawGizmos(true);
    }
}
#endif