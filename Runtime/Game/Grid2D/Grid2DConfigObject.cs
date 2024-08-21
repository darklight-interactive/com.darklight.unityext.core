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
    [Header("Grid Dimensions")]
    [SerializeField, Range(1, 10)] int _gridWidth = 3;
    [SerializeField, Range(1, 10)] int _gridHeight = 3;
    [SerializeField, Range(1, 10)] float _gridSpacing = 1;

    [Header("Grid Direction")]
    [SerializeField, Dropdown("_directions")] Vector3 _gridDirection = Vector3.forward;

    [Header("Grid Origin")]
    [SerializeField, Range(0, 10)] int _gridOriginX = 0;
    [SerializeField, Range(0, 10)] int _gridOriginY = 0;

    [Header("Cell Dimensions")]
    [SerializeField, Range(0.1f, 10f)] float _cellWidth = 1;
    [SerializeField, Range(0.1f, 10f)] float _cellHeight = 1;

    public Grid2D.Config ToConfig()
    {
        Grid2D.Config config = new Grid2D.Config()
        {
            dimensions = new Vector2Int(_gridWidth, _gridHeight),
            worldDirection = _gridDirection,
            originOffset = new Vector2Int(_gridOriginX, _gridOriginY),
            cellDimensions = new Vector2(_cellWidth, _cellHeight),
            cellSpacing = new Vector2(_gridSpacing, _gridSpacing)
        };
        return config;
    }
}
