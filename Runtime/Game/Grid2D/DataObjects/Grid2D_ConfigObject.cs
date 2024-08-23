using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Game
{
    [CreateAssetMenu(menuName = "Darklight/Grid2D/DataObject")]
    public class Grid2D_ConfigObject : ScriptableObject
    {
        #region ---- ( EDITOR VALUES ) --------- >>
        DropdownList<Vector3> _editor_directions = new DropdownList<Vector3>()
    {
        { "Up", Vector3.up },
        { "Down", Vector3.down },
        { "Left", Vector3.left },
        { "Right", Vector3.right },
        { "Forward", Vector3.forward },
        { "Back", Vector3.back }
    };
        bool _editor_showTransform => !_lockToTransform;
        #endregion

        // -- (( SERIALIZED DATA )) --------------------------------- >>
        [SerializeField] bool _showGizmos = true;
        [SerializeField] bool _lockToTransform = true;
        [SerializeField, ShowOnly] Transform _transformParent;


        // (( Transform )) ------------------------------ >>
        [Header("-- Transform ---- >>")]
        [SerializeField, Foldout("Transform"), ShowIf("_editor_showTransform")]
        Vector3 _position = Vector3.zero;

        [SerializeField, Foldout("Transform"), ShowIf("_editor_showTransform")]
        [Dropdown("_directions")] Vector3 _normal = Vector3.up;

        // (( Origin )) ------------------------------ >>
        [Header("-- Origin ---- >>")]
        [SerializeField] Vector2Int _originOffset = new Vector2Int(0, 0);

        // (( Grid Dimensions )) ------------------------------ >>
        [Header("-- Dimensions ---- >>")]
        [SerializeField, Range(1, 10)] int _numColumns = 3;
        [SerializeField, Range(1, 10)] int _numRows = 3;

        // (( Cell Dimensions )) ------------------------------ >>
        [Header("-- Cell Dimensions ---- >>")]
        [SerializeField, Range(0.1f, 10f)] float _cellWidth = 1;
        [SerializeField, Range(0.1f, 10f)] float _cellHeight = 1;

        [Header("-- Cell Spacing ---- >>")]
        [SerializeField, Range(1, 10)] float _cellHorzSpacing = 1;
        [SerializeField, Range(1, 10)] float _cellVertSpacing = 1;



        public AbstractGrid2D.Config ToConfig()
        {
            AbstractGrid2D.Config config = new AbstractGrid2D.Config()
            {
                showGizmos = _showGizmos,
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
}