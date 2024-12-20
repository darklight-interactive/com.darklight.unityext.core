using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Darklight/Shape2D/Preset")]
public class Shape2DPreset : ScriptableObject
{
    [SerializeField, Range(1, 3000)] int _radius;
    [SerializeField, Range(2, 64)] int _segments;
    [SerializeField] Color _gizmoColor = Color.white;

    public Shape2D CreateShape2D()
    {
        Shape2D shape = new Shape2D(Vector3.zero, _radius, _segments, Vector3.up, _gizmoColor);
        return shape;
    }

    public Shape2D CreateShape2D(Vector3 position, Vector3 normal)
    {
        Shape2D shape = new Shape2D(position, _radius, _segments, normal, _gizmoColor);
        return shape;
    }
}