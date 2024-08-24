

using Darklight.UnityExt.Editor;

using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public abstract class BaseCell
    {
        protected BaseCellData data;
        public abstract BaseCellData GetData();
        public abstract void SetData(BaseCellData data);
        public abstract void SetConfig(GridMapConfig config);
        public abstract void DrawGizmos(bool editMode);

        #region (( Calculation Methods )) --------- >>
        /// <summary>
        /// Method to calculate the world position of the cell 
        /// based on its key and the grid configuration.
        /// </summary>
        /// <param name="config">
        ///     The grid configuration containing origin, cell dimensions, spacing, etc.
        /// </param>
        /// <param name="key">
        ///     The key of the cell in the grid.
        /// </param>
        protected virtual Vector3 CalculateCellPosition(GridMapConfig config, Vector2Int key)
        {
            // Get the origin key of the grid
            Vector2Int originKey = CalculateOriginKey(config);

            // Calculate the spacing offset && clamp it to avoid overlapping cells
            Vector2 spacingOffsetPos = config.cellSpacing + Vector2.one; // << Add 1 to allow for values of 0
            spacingOffsetPos.x = Mathf.Clamp(spacingOffsetPos.x, 1, float.MaxValue);
            spacingOffsetPos.y = Mathf.Clamp(spacingOffsetPos.y, 1, float.MaxValue);

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
            Vector2 cellPosition = config.gridPosition; // << Start with the grid's position
            cellPosition += (keyOffsetPos - originOffsetPos); // << Add the grid offset
            cellPosition *= spacingOffsetPos; // << Multiply the spacing offset
            cellPosition += bondingOffset; // << Add the bonding offset

            // Create a rotation matrix based on the grid's normal
            Quaternion rotation = Quaternion.LookRotation(config.gridNormal, Vector3.up);

            // Apply the rotation to the grid offset and return the final world position
            return rotation * new Vector3(cellPosition.x, cellPosition.y, 0);
        }

        Vector2Int CalculateOriginKey(GridMapConfig config)
        {
            Vector2Int gridDimensions = config.gridDimensions - Vector2Int.one;
            Vector2Int originKey = Vector2Int.zero;

            switch (config.gridAlignment)
            {
                case GridAlignment.TopLeft:
                    originKey = new Vector2Int(0, 0);
                    break;
                case GridAlignment.TopCenter:
                    originKey = new Vector2Int(Mathf.FloorToInt(gridDimensions.x / 2), 0);
                    break;
                case GridAlignment.TopRight:
                    originKey = new Vector2Int(Mathf.FloorToInt(gridDimensions.x), 0);
                    break;
                case GridAlignment.MiddleLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(gridDimensions.y / 2));
                    break;
                case GridAlignment.Center:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(gridDimensions.x / 2),
                        Mathf.FloorToInt(gridDimensions.y / 2)
                        );
                    break;
                case GridAlignment.MiddleRight:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(gridDimensions.x),
                        Mathf.FloorToInt(gridDimensions.y / 2)
                        );
                    break;
                case GridAlignment.BottomLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(gridDimensions.y));
                    break;
                case GridAlignment.BottomCenter:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(gridDimensions.x / 2),
                        Mathf.FloorToInt(gridDimensions.y)
                        );
                    break;
                case GridAlignment.BottomRight:
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

    [System.Serializable]
    public class GenericCell<TData> : BaseCell
        where TData : BaseCellData, new()
    {
        // -- Protected Data ---- >>
        [SerializeField] protected new TData data;

        // ===================== [[ CONSTRUCTORS ]] ===================== //
        public GenericCell() { }
        public GenericCell(Vector2Int key)
        {
            TData customData = new TData();
            customData.Initialize(key);
            SetData(customData);
        }

        #region (( Getter Methods )) -------- >>
        public override BaseCellData GetData()
        {
            return data;
        }

        /// <summary>
        /// Get the gizmo color of the cell.
        /// </summary>
        /// <param name="color"></param>
        protected virtual void GetGizmoColor(out Color color)
        {
            color = data.disabled ? Color.grey : Color.white;
        }

        /// <summary>
        /// Get all applicable gizmo data for the cell.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dimensions"></param>
        /// <param name="normal"></param>
        /// <param name="color"></param>
        protected virtual void GetGizmoData(
            out Vector3 position,
            out Vector2 dimensions,
            out Vector3 normal,
            out Color color)
        {
            position = data.position;
            dimensions = data.dimensions;
            normal = data.normal;
            GetGizmoColor(out color);
        }

        #endregion

        #region (( Setter Methods )) -------- >>
        public override void SetData(BaseCellData data)
        {
            if (data is TData)
            {
                this.data = data as TData;
            }
        }

        public override void SetConfig(GridMapConfig config)
        {
            if (data == null)
                return;

            data.SetPosition(CalculateCellPosition(config, data.key));
            data.SetNormal(config.gridNormal);
            data.SetDimensions(config.cellDimensions);
        }

        public void ToggleDisabled()
        {
            data.SetDisabled(!data.disabled);
        }

        #endregion

        #region (( Gizmo Methods )) -------- >>
#if UNITY_EDITOR
        public override void DrawGizmos(bool editMode)
        {
            if (data == null)
                return;

            DrawCell();
            DrawLabel($"Cell {data.key}");
            if (editMode) DrawCellToggle();
        }

        protected virtual void DrawCell()
        {
            GetGizmoData(out Vector3 position, out Vector2 dimensions, out Vector3 normal, out Color color);

            // Draw the cell square
            CustomGizmos.DrawWireRect(position, dimensions, normal, color);
        }

        protected virtual void DrawLabel(string label)
        {
            GetGizmoData(out Vector3 position, out Vector2 dimensions, out Vector3 normal, out Color color);

            CustomGizmos.DrawLabel(label, position, CustomGUIStyles.CenteredStyle);
        }

        protected virtual void DrawCellToggle()
        {
            GetGizmoData(out Vector3 position, out Vector2 dimensions, out Vector3 normal, out Color color);

            // The size of the button handle is the minimum of the dimensions
            float size = Mathf.Min(dimensions.x, dimensions.y);
            size *= 0.75f; // << scale down the size a bit

            // Draw the button handle only if the grid is in edit mode
            CustomGizmos.DrawButtonHandle(position, size, normal, color, () =>
            {
                OnEditToggle();
            }, Handles.RectangleHandleCap);
        }

        protected virtual void OnEditToggle()
        {
            ToggleDisabled();
        }
#endif
        #endregion
    }

    public class Cell : GenericCell<CellData>
    {
        public Cell() : base() { }
        public Cell(Vector2Int key) : base(key) { }
    }
}
