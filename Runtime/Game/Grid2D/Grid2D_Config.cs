

using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game
{
    [System.Serializable]
    public class Grid2D_Config
    {
        public enum Alignment
        {
            TopLeft, TopCenter, TopRight,
            MiddleLeft, Center, MiddleRight,
            BottomLeft, BottomCenter, BottomRight
        }

        // ======================= [[ SERIALIZED FIELDS ]] ======================= //
        [SerializeField, ShowOnly] bool _showGizmos = true;
        [SerializeField, ShowOnly] bool _showEditorGizmos = true;
        [SerializeField, ShowOnly] bool _lockToTransform = true;

        [SerializeField, ShowOnly] Alignment _gridAlignment = Alignment.Center;
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

        public Alignment gridAlignment => _gridAlignment;
        public void SetGridAlignment(Alignment gridAlignment) => _gridAlignment = gridAlignment;

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
        public Grid2D_Config() { }


        #region (( Calculation Methods )) --------- >>
        public Vector3 CalculatePositionFromKey(Vector2Int key)
        {
            Grid2D_Config config = this;

            // Get the origin key of the grid
            Vector2Int originKey = config.CalculateOriginKey();

            // Calculate the spacing offset && clamp it to avoid overlapping cells
            Vector2 spacingOffsetPos = config.cellSpacing + Vector2.one; // << Add 1 to allow for values of 0
            spacingOffsetPos.x = Mathf.Clamp(spacingOffsetPos.x, 0.5f, float.MaxValue);
            spacingOffsetPos.y = Mathf.Clamp(spacingOffsetPos.y, 0.5f, float.MaxValue);

            // Calculate bonding offsets
            Vector2 bondingOffset = Vector2.zero;
            if (key.y % 2 == 0)
                bondingOffset.x = config.cellBonding.x;
            if (key.x % 2 == 0)
                bondingOffset.y = config.cellBonding.y;

            // Calculate the offset of the cell from the grid origin
            Vector2 originOffsetPos = originKey * config.cellDimensions;
            Vector2 keyOffsetPos = key * config.cellDimensions;

            // Calculate the final position of the cell
            Vector2 cellPosition = (keyOffsetPos - originOffsetPos); // << Calculate the position offset
            cellPosition *= spacingOffsetPos; // << Multiply the spacing offset
            cellPosition += bondingOffset; // << Add the bonding offset

            // Create a rotation matrix based on the grid's normal
            Quaternion rotation = Quaternion.LookRotation(config.gridNormal, Vector3.forward);

            // Apply the rotation to the grid offset and return the final world position
            return gridPosition + (rotation * new Vector3(cellPosition.x, cellPosition.y, 0));
        }

        public Vector2Int CalculateCoordinateFromKey(Vector2Int key)
        {
            Grid2D_Config config = this;
            Vector2Int originKey = config.CalculateOriginKey();
            return key - originKey;
        }

        Vector2Int CalculateOriginKey()
        {
            Grid2D_Config config = this;
            Vector2Int gridDimensions = config.gridDimensions - Vector2Int.one;
            Vector2Int originKey = Vector2Int.zero;

            switch (config.gridAlignment)
            {
                case Alignment.BottomLeft:
                    originKey = new Vector2Int(0, 0);
                    break;
                case Alignment.BottomCenter:
                    originKey = new Vector2Int(Mathf.FloorToInt(gridDimensions.x / 2), 0);
                    break;
                case Alignment.BottomRight:
                    originKey = new Vector2Int(Mathf.FloorToInt(gridDimensions.x), 0);
                    break;
                case Alignment.MiddleLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(gridDimensions.y / 2));
                    break;
                case Alignment.Center:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(gridDimensions.x / 2),
                        Mathf.FloorToInt(gridDimensions.y / 2)
                        );
                    break;
                case Alignment.MiddleRight:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(gridDimensions.x),
                        Mathf.FloorToInt(gridDimensions.y / 2)
                        );
                    break;
                case Alignment.TopLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(gridDimensions.y));
                    break;
                case Alignment.TopCenter:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(gridDimensions.x / 2),
                        Mathf.FloorToInt(gridDimensions.y)
                        );
                    break;
                case Alignment.TopRight:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(gridDimensions.x),
                        Mathf.FloorToInt(gridDimensions.y)
                        );
                    break;
            }

            return originKey;
        }


        #endregion
    }
}