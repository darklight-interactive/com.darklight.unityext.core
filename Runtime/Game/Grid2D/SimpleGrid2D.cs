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
public class SimpleGrid2D : MonoBehaviour
{
    [System.Serializable]
    public class SimpleCell : Grid2D.Cell<Grid2D.Cell.Data>
    {
        public SimpleCell(Grid2D grid, Vector2Int key) : base(grid, key) { }
        public override void Initialize() { }
        public override void DrawGizmos(bool editMode = false) { }
    }

    [SerializeField] protected Grid2D<SimpleCell> _grid;

    // ======= SIMPLE GRID CONFIGURATION ======= //
    [HorizontalLine(4)]
    [SerializeField, Expandable] Grid2D_ConfigObject _configObj;

    [HorizontalLine(2)]
    [SerializeField] bool _editMode = false;
    [SerializeField] bool _lockToTransform = true;


    public void Awake() => Initialize();
    public virtual void Initialize()
    {
        if (_configObj != null)
        {
            _grid = new Grid2D<SimpleCell>(_configObj.ToConfig());
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

    public virtual void DrawGizmos()
    {
        _grid.DrawGizmos(_editMode);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SimpleGrid2D), true)]
public class SimpleGrid2DCustomEditor : UnityEditor.Editor
{
    SerializedObject _serializedObject;
    SimpleGrid2D _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (SimpleGrid2D)target;
        _script.Awake();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _script.Initialize();
            EditorUtility.SetDirty(_script);
            Repaint();
        }
    }

    public void OnSceneGUI()
    {
        _script.DrawGizmos();
    }
}
#endif