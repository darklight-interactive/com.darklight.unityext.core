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
public class SimpleGrid2D : MonoBehaviour
{
    [SerializeField] Grid2D _grid = new Grid2D();

    [HorizontalLine(4)]
    [SerializeField, Expandable] Grid2DConfigObject _configObj;

    public void Awake() => Initialize();
    public virtual void Initialize()
    {
        if (_configObj != null)
        {
            _grid = new Grid2D(_configObj.ToConfig());
        }
    }

    public void OnDrawGizmos()
    {
        _grid.DrawGizmos();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SimpleGrid2D))]
public class Grid2DCustomEditor : UnityEditor.Editor
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
        }
        else
        {
            _script.OnDrawGizmos();
        }

    }
}
#endif