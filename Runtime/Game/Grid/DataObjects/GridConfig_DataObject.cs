using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    [CreateAssetMenu(menuName = "Darklight/Grid2D/DataObject")]
    public class GridMapConfig_DataObject : ScriptableObject
    {
        #region ---- ( CUSTOM EDITOR DATA ) --------- >>
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

        [Header("-- Transform ---- >>")]
        [SerializeField] bool _lockToTransform = true;

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

        // (( Gizmos )) ------------------------------ >>
        [Header("-- Gizmos ---- >>")]
        [SerializeField] bool _showGizmos = true;
        [SerializeField] bool _showEditorGizmos = true;


        public GridMapConfig ToConfig()
        {
            GridMapConfig config = new GridMapConfig();
            config.SetGizmos(_showGizmos, _showEditorGizmos);
            config.SetOriginOffset(_originOffset);
            config.SetGridDimensions(new Vector2Int(_numColumns, _numRows));
            config.SetCellDimensions(new Vector2(_cellWidth, _cellHeight));
            config.SetCellSpacing(new Vector2(_cellHorzSpacing, _cellVertSpacing));

            return config;
        }
    }
}