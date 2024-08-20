using Darklight.UnityExt.Game;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Darklight/Grid2D/DataObject")]
public class Grid2DSettings : ScriptableObject
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

    // -- (( PUBLIC REFERENCES )) -------------------------------------- >>
    public int gridWidth => _gridWidth;
    public int gridHeight => _gridHeight;
    public Vector3 gridDirection => _gridDirection;
    public float gridSpacing => _gridSpacing;
    public Vector2Int gridOriginKey => new Vector2Int(_gridOriginX, _gridOriginY);

    public float cellWidth => _cellWidth;
    public float cellHeight => _cellHeight;
}