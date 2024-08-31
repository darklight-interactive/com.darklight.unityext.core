using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [CreateAssetMenu(menuName = "Darklight/Grid2D/DataObject")]
    public class Grid2D_ConfigDataObject : ScriptableObject
    {
        #region ---- ( CUSTOM EDITOR DATA ) --------- >>
        public DropdownList<Vector3> editor_directions = new DropdownList<Vector3>()
        {
            { "Up", Vector3.up },
            { "Down", Vector3.down },
            { "Left", Vector3.left },
            { "Right", Vector3.right },
            { "Forward", Vector3.forward },
            { "Back", Vector3.back }
        };
        public bool editor_showTransform => !_lockToTransform;
        #endregion

        [Header("-- Grid Transform ---- >>")]
        [SerializeField] bool _lockToTransform = true;
        [SerializeField] Grid2D_Config.Alignment _gridAlignment = Grid2D_Config.Alignment.Center;
        [SerializeField, Dropdown("editor_directions")] Vector3 _gridNormal = Vector3.up;

        [Header("-- Grid Dimensions ---- >>")]
        [SerializeField, Range(1, 10)] int _numColumns = 3;
        [SerializeField, Range(1, 10)] int _numRows = 3;

        [Space(10)]
        [Header("-- Cell Dimensions ---- >>")]
        [SerializeField, Range(0.1f, 10f)] float _cellWidth = 1;
        [SerializeField, Range(0.1f, 10f)] float _cellHeight = 1;

        [Header("-- Cell Spacing ---- >>")]
        [SerializeField, Range(-0.5f, 10)] float _cellHorzSpacing = 0;
        [SerializeField, Range(-0.5f, 10)] float _cellVertSpacing = 0;

        [Header("-- Cell Bonding ---- >>")]
        [SerializeField, Range(-1, 1)] float _cellBondingX = 0;
        [SerializeField, Range(-1, 1)] float _cellBondingY = 0;

        [Header("-- Cell Components ---- >>")]
        [SerializeField, EnumFlags] ICell2DComponent.TypeKey componentTypes;

        [Space(10)]
        [Header("-- Gizmos ---- >>")]
        [SerializeField] bool _showGizmos = true;
        [SerializeField] bool _showEditorGizmos = true;


        public bool showGizmos => _showGizmos;
        public bool showEditorGizmos => _showEditorGizmos;

        public virtual Grid2D_Config ToConfig()
        {
            Grid2D_Config config = new Grid2D_Config();

            config.SetGizmos(_showGizmos, _showEditorGizmos);

            config.SetLockToTransform(_lockToTransform);
            config.SetGridAlignment(_gridAlignment);
            config.SetGridNormal(_gridNormal);
            config.SetGridDimensions(new Vector2Int(_numColumns, _numRows));

            config.SetCellDimensions(new Vector2(_cellWidth, _cellHeight));
            config.SetCellSpacing(new Vector2(_cellHorzSpacing, _cellVertSpacing));
            config.SetCellBonding(new Vector2(_cellBondingX, _cellBondingY));

            return config;
        }

        public virtual void ResetToDefaults()
        {
            _lockToTransform = true;
            _gridAlignment = Grid2D_Config.Alignment.Center;
            _gridNormal = Vector3.up;

            _numColumns = 3;
            _numRows = 3;

            _cellBondingX = 0;
            _cellBondingY = 0;

            _cellWidth = 1;
            _cellHeight = 1;

            _cellHorzSpacing = 0;
            _cellVertSpacing = 0;

            _showGizmos = true;
            _showEditorGizmos = true;
        }
    }
}