using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    #region -- << INTERNAL CLASS >> : CONFIG ------------------------------------ >>
    [System.Serializable]
    public class GridMapConfig
    {
        bool _initalized = false;
        Transform _transform;

        // ======================= [[ SERIALIZED DATA PROPERTIES ]] ======================= //
        [SerializeField, ShowOnly] bool _showGizmos = true;
        [SerializeField, ShowOnly] bool _showEditorGizmos = true;
        [SerializeField, ShowOnly] bool _lockToTransform = true;
        [SerializeField, ShowOnly] Vector3 _originPosition = new Vector3(0, 0, 0);
        [SerializeField, ShowOnly] Vector2Int _originKeyOffset = new Vector2Int(0, 0);
        [SerializeField, ShowOnly] Vector3 _gridNormal = Vector3.up;
        [SerializeField, ShowOnly] Vector2Int _gridDimensions = new Vector2Int(3, 3);
        [SerializeField, ShowOnly] Vector2 _cellDimensions = new Vector2(1, 1);
        [SerializeField, ShowOnly] Vector2 _cellSpacing = new Vector2(1, 1);

        // ======================= [[ PUBLIC REFERENCE PROPERTIES ]] ======================= //
        public bool showGizmos => _showGizmos;
        public bool showEditorGizmos => _showEditorGizmos;
        public bool lockToTransform => _lockToTransform;
        public Vector3 originPosition => _originPosition;
        public Vector2Int originOffset => _originKeyOffset;
        public Vector3 gridNormal => _gridNormal;
        public Vector2Int gridDimensions => _gridDimensions;
        public Vector2 cellDimensions => _cellDimensions;
        public Vector2 cellSpacing => _cellSpacing;

        // ======================= [[ CONSTRUCTORS ]] ======================= //
        public GridMapConfig() { }

        // ======================== [[ SETTERS ]] ======================== //
        public bool SetGizmos(bool showGizmos, bool showEditorGizmos = false)
        {
            _showGizmos = showGizmos;
            _showEditorGizmos = showEditorGizmos;
            return _showGizmos;
        }

        public void SetOriginOffset(Vector2Int originOffset)
        {
            _originKeyOffset = originOffset;
        }

        public void SetGridDimensions(Vector2Int gridDimensions)
        {
            _gridDimensions = gridDimensions;
        }

        public void SetCellDimensions(Vector2 cellDimensions)
        {
            _cellDimensions = cellDimensions;
        }

        public void SetCellSpacing(Vector2 cellSpacing)
        {
            _cellSpacing = cellSpacing;
        }

        // ======================== [[ GETTERS ]] ======================== //
        public int GetCellCount()
        {
            return _gridDimensions.x * _gridDimensions.y;
        }
    }
    #endregion
}
