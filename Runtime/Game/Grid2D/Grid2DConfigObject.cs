using Darklight.UnityExt.Game;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Darklight/Grid2D/DataObject")]
public class Grid2DConfigObject : ScriptableObject
{
    DropdownList<Vector3> _directions = new DropdownList<Vector3>()
    {
        { "Up", Vector3.up },
        { "Down", Vector3.down },
        { "Left", Vector3.left },
        { "Right", Vector3.right },
        { "Forward", Vector3.forward },
        { "Back", Vector3.back }
    };

    // -- (( SERIALIZED DATA )) --------------------------------- >>
    [Header("-- Dimensions ---- >>")]
    [SerializeField, Range(1, 10)] int _numColumns = 3;
    [SerializeField, Range(1, 10)] int _numRows = 3;

    [Header("-- Transform ---- >>")]
    [SerializeField] Vector3 _position = Vector3.zero;
    [SerializeField, Dropdown("_directions")] Vector3 _normal = Vector3.up;

    [Header("-- Origin ---- >>")]
    [SerializeField] Vector2Int _originOffset = new Vector2Int(0, 0);

    [Header("-- Cell Dimensions ---- >>")]
    [SerializeField, Range(0.1f, 10f)] float _cellWidth = 1;
    [SerializeField, Range(0.1f, 10f)] float _cellHeight = 1;

    [Header("-- Cell Spacing ---- >>")]
    [SerializeField, Range(1, 10)] float _cellHorzSpacing = 1;
    [SerializeField, Range(1, 10)] float _cellVertSpacing = 1;

    public Grid2D.Config ToConfig()
    {
        Grid2D.Config config = new Grid2D.Config()
        {
            dimensions = new Vector2Int(_numColumns, _numRows),
            position = _position,
            normal = _normal,
            originOffset = _originOffset,
            cellDimensions = new Vector2(_cellWidth, _cellHeight),
            cellSpacing = new Vector2(_cellHorzSpacing, _cellVertSpacing)
        };
        return config;
    }
}
