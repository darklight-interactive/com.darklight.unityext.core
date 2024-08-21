using UnityEngine;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using Darklight.UnityExt.Game;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Base class for a 2D grid. Creates a Grid2D with Cell2D objects.
/// </summary>
[ExecuteAlways]
public class MonoBehaviourGrid2D<TCell> : MonoBehaviour, IGrid2D where TCell : Grid2D.Cell, new()
{
    [SerializeField] Grid2D<TCell> _grid = new Grid2D<TCell>();
    protected Grid2D<TCell> grid => _grid;

    [HorizontalLine(4)]
    [SerializeField, Expandable] Grid2D_ConfigObject _configObj;

    [HorizontalLine(2)]
    [SerializeField] bool _editMode;
    [SerializeField] bool _lockToTransform;


    public void Awake() => Initialize();
    public virtual void Initialize()
    {
        if (_configObj != null)
        {
            _grid = new Grid2D<TCell>(_configObj.ToConfig());
            //Debug.Log($"Grid Initialized: {_grid}", this);
        }
    }

    public void Update()
    {
        if (_grid.initialized)
        {
            ApplyTransform();
        }
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
        _script.Initialize();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        /*
        // Display the grid property first
        SerializedProperty gridProperty = _serializedObject.FindProperty("_grid");
        EditorGUILayout.PropertyField(gridProperty);

        // Draw other properties
        DrawPropertiesExcluding(serializedObject, "m_Script", "_grid");
        */

        CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();

            _script.Initialize();

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