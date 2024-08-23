using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    #region -- << INTERNAL CLASS >> : CONFIG ------------------------------------ >>
    [System.Serializable]
    public class GridConfig
    {
        // -- Transform ---- >>
        [SerializeField, ShowOnly] bool _lockToTransform = true;
        Transform _transformParent;
        [SerializeField, ShowOnly] Vector3 _position = Vector3.zero;
        [SerializeField, ShowOnly] Vector3 _normal = Vector3.up;
        public Vector3 position { get => _position; set => _position = value; }
        public Vector3 normal { get => _normal; set => _normal = value; }

        // -- Origin ---- >>
        [SerializeField, ShowOnly] Vector2Int _originOffset = Vector2Int.zero;
        public Vector2Int originOffset { get => _originOffset; set => _originOffset = value; }

        // -- Grid Dimensions ---- >>
        [SerializeField, ShowOnly] Vector2Int _dimensions = new Vector2Int(3, 3);
        public Vector2Int dimensions { get => _dimensions; set => _dimensions = value; }
        public int numRows => _dimensions.y;
        public int numColumns => _dimensions.x;

        // -- Cell Dimensions ---- >>
        [SerializeField, ShowOnly]
        Vector2 _cellDimensions = new Vector2(1, 1);
        public Vector2 cellDimensions { get => _cellDimensions; set => _cellDimensions = value; }
        public float cellWidth => _cellDimensions.x;
        public float cellHeight => _cellDimensions.y;

        // -- Spacing ---- >>
        [SerializeField, ShowOnly]
        Vector2 _cellSpacing = new Vector2(0, 0);
        public Vector2 cellSpacing { get => _cellSpacing; set => _cellSpacing = value; }

        // -- Gizmos ---- >>
        [SerializeField, ShowOnly] bool _showGizmos = true;
        public bool showGizmos { get => _showGizmos; set => _showGizmos = value; }

        public void LockToTransform(Transform transform)
        {
            _lockToTransform = true;
            _transformParent = transform;
        }

        public void UnlockFromTransform()
        {
            _lockToTransform = false;
            _transformParent = null;
        }

        public void GetWorldSpaceData(out Vector3 position, out Vector2 dimensions, out Vector3 normal)
        {
            dimensions = new Vector2(numColumns, numRows);

            if (_lockToTransform)
            {
                position = _transformParent.position;
                normal = _transformParent.up;
            }
            else
            {
                position = _position;
                normal = _normal;
            }
        }

    }
    #endregion
}
