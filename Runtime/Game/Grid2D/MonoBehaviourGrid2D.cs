using UnityEngine;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using Darklight.UnityExt.Game;
using System.Collections.Generic;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif


#region -- << ABSTRACT CLASS >> : MONOBEHAVIOURGRID2D ------------------------------------ >>
public abstract class AbstractMonoBehaviourGrid2D : MonoBehaviour
{
    protected const string ASSET_PATH = "Assets/Resources/Darklight/Grid2D";
    protected const string CONFIG_PATH = ASSET_PATH + "/Config";
    protected const string DATA_PATH = ASSET_PATH + "/Data";

    protected abstract void GenerateConfigObj();
    protected abstract void GenerateDataObj();

    public abstract void InitializeGrid();
    public abstract void UpdateGrid();

    public abstract void DrawGizmos(bool editMode = false);
}
#endregion

#region -- << GENERIC CLASS >> : MONOBEHAVIOURGRID2D ------------------------------------ >>
public class GenericMonoBehaviourGrid2D<TCell, TData> : AbstractMonoBehaviourGrid2D
    where TCell : Cell, new()
    where TData : Cell.Data, new()
{
    // (( Grid2D )) ------------------------------ >>
    [SerializeField] private GenericGrid2D<TCell, TData> _grid;
    public GenericGrid2D<TCell, TData> grid { get => _grid; protected set => _grid = value; }

    // (( Grid2D_ConfigObject )) ------------------------------ >>
    [HorizontalLine(4)]
    [SerializeField, Expandable] Grid2D_ConfigObject _configObj;
    protected Grid2D_ConfigObject configObj { get => _configObj; set => _configObj = value; }
    protected override void GenerateConfigObj()
    {
        configObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_ConfigObject>(CONFIG_PATH, name);
    }

    // (( Grid2D_AbstractDataObject )) ------------------------------ >>
    [SerializeField, Expandable] Grid2D_AbstractDataObject _dataObj;
    protected Grid2D_AbstractDataObject dataObj { get => _dataObj; set => _dataObj = value; }
    protected override void GenerateDataObj()
    {
        dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_DataObject>(DATA_PATH, name);
    }

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
        grid = new GenericGrid2D<TCell, TData>(configObj.ToConfig());
    }

    public virtual void Update() => UpdateGrid();
    public override void UpdateGrid()
    {
        if (grid == null) InitializeGrid();
        grid.SetConfig(configObj.ToConfig());
    }

    public override void DrawGizmos(bool editMode = false)
    {
        if (grid == null) return;
        grid.DrawGizmos(editMode);
    }
}
#endregion

public class MonoBehaviourGrid2D : GenericMonoBehaviourGrid2D<Cell, Cell.Data> { }

#if UNITY_EDITOR
[CustomEditor(typeof(AbstractMonoBehaviourGrid2D), true)]
public class MonoBehaviourGrid2DCustomEditor : UnityEditor.Editor
{
    SerializedObject _serializedObject;
    AbstractMonoBehaviourGrid2D _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (AbstractMonoBehaviourGrid2D)target;
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