using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public enum GridAlignment
    {
        TopLeft, TopCenter, TopRight,
        MiddleLeft, Center, MiddleRight,
        BottomLeft, BottomCenter, BottomRight
    }

    [System.Serializable]
    public class GridMapConfig
    {
        // ======================= [[ SERIALIZED FIELDS ]] ======================= //
        [SerializeField, ShowOnly] bool _showGizmos = true;
        [SerializeField, ShowOnly] bool _showEditorGizmos = true;
        [SerializeField, ShowOnly] bool _lockToTransform = true;

        [SerializeField, ShowOnly] GridAlignment _gridAlignment = GridAlignment.Center;
        [SerializeField, ShowOnly] Vector3 _gridPosition = new Vector3(0, 0, 0);
        [SerializeField, ShowOnly] Vector3 _gridNormal = Vector3.up;
        [SerializeField, ShowOnly] Vector2Int _gridDimensions = new Vector2Int(3, 3);

        [SerializeField, ShowOnly] Vector2 _cellDimensions = new Vector2(1, 1);
        [SerializeField, ShowOnly] Vector2 _cellSpacing = new Vector2(1, 1);
        [SerializeField, ShowOnly] Vector2 _cellBonding = new Vector2(0, 0);

        // ======================= [[ PUBLIC REFERENCE PROPERTIES ]] ======================= //
        public bool showGizmos => _showGizmos;
        public bool showEditorGizmos => _showEditorGizmos;
        public bool SetGizmos(bool showGizmos, bool showEditorGizmos = false)
        {
            _showGizmos = showGizmos;
            _showEditorGizmos = showEditorGizmos;
            return _showGizmos;
        }

        public bool lockToTransform => _lockToTransform;
        public void SetLockToTransform(bool lockToTransform) => _lockToTransform = lockToTransform;

        public GridAlignment gridAlignment => _gridAlignment;
        public void SetGridAlignment(GridAlignment gridAlignment) => _gridAlignment = gridAlignment;

        public Vector3 gridPosition => _gridPosition;
        public Vector3 gridNormal => _gridNormal;
        public Vector2Int gridDimensions => _gridDimensions;
        public void SetGridPosition(Vector3 originPosition) => _gridPosition = originPosition;
        public void SetGridNormal(Vector3 gridNormal) => _gridNormal = gridNormal;
        public void SetGridDimensions(Vector2Int gridDimensions) => _gridDimensions = gridDimensions;

        public Vector2 cellDimensions => _cellDimensions;
        public Vector2 cellSpacing => _cellSpacing;
        public Vector2 cellBonding => _cellBonding;
        public void SetCellDimensions(Vector2 cellDimensions) => _cellDimensions = cellDimensions;
        public void SetCellSpacing(Vector2 cellSpacing) => _cellSpacing = cellSpacing;
        public void SetCellBonding(Vector2 cellBonding) => _cellBonding = cellBonding;


        // ======================= [[ CONSTRUCTORS ]] ======================= //
        public GridMapConfig() { }
    }
}
