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
    const string ASSET_PATH = "Assets/Resources/Grid2D";
    const string CONFIG_PATH = ASSET_PATH + "Config";
    const string DATA_PATH = ASSET_PATH + "Data";

    public Grid2D<Cell2D> basicGrid;

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
        _dataObj.Initialize(basicGrid);
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
        basicGrid = new Grid2D<Cell2D>(config);
    }

    public void UpdateConfig() => UpdateConfig(_configObj.ToConfig());
    public void UpdateConfig(AbstractGrid2D.Config config)
    {
        if (basicGrid == null)
            Initialize(config);

        if (_lockToTransform)
            config.SetWorldSpaceData(transform.position, transform.forward);
        else
            config.SetWorldSpaceData(Vector3.zero, Vector3.up);

        if (_configObj)
            _configObj.showTransformValues = !_lockToTransform;

        if (basicGrid == null)
        {
            Debug.LogError("Grid2D: Cannot update config with null grid.");
            return;
        }
        basicGrid.UpdateConfig(config);
        _dataObj.SaveData();
    }

    public virtual void DrawGizmos(bool editMode = false)
    {
        if (basicGrid == null) return;
        basicGrid.DrawGizmos(_editMode);
    }
}


#region -- << GENERIC CLASS >> : MONOBEHAVIOURGRID2D ------------------------------------ >>
public class MonoBehaviourGrid2D<TCell> : MonoBehaviourGrid2D, IGrid2D where TCell : Cell2D
{
    public Grid2D<TCell> typedGrid;
    public override void Initialize(AbstractGrid2D.Config config)
    {
        typedGrid = new Grid2D<TCell>(config);
    }

    public override void DrawGizmos(bool editMode = false)
    {
        if (typedGrid == null) return;
        typedGrid.DrawGizmos(editMode);
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
        SerializedProperty gridProp = _serializedObject.FindProperty("typedGrid");
        if (gridProp != null)
        {
            EditorGUILayout.PropertyField(gridProp, true);

            SerializedProperty basicGridProp = _serializedObject.FindProperty("basicGrid");
            DrawPropertiesExcluding(_serializedObject, "typedGrid", "basicGrid");
        }
        else
        {
            CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);
        }


        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            _script.UpdateConfig();
            Repaint();
        }
    }

    private void OnSceneGUI()
    {
        _script.DrawGizmos(true);
    }
}
#endif