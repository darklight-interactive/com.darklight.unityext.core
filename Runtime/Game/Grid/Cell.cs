using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;


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
        /// <param name="originPos">The origin position of the grid.</param>
        /// <param name="key">The key of the cell in the grid.</param>
        protected virtual Vector3 CalculateCellPosition(GridMapConfig config, Vector2Int key)
        {
            Vector3 originPosition = config.originPosition;

            // << POSITION CALCULATIONS >>
            // Calculate the offset for the grid origin key
            Vector2 originOffsetPostition = (Vector2)config.originOffset * config.cellDimensions * -1;

            // Calculate the offset for the input key position
            Vector2 keyOffsetPosition = (Vector2)key * config.cellDimensions;

            // Calculate the spacing offset && clamp it to avoid overlapping cells
            Vector2 spacingOffset = config.cellSpacing;
            spacingOffset.x = Mathf.Clamp(spacingOffset.x, 1, float.MaxValue);
            spacingOffset.y = Mathf.Clamp(spacingOffset.y, 1, float.MaxValue);

            // Combine origin offset and key offset, then apply spacing
            Vector2 gridOffset = (originOffsetPostition + keyOffsetPosition) * spacingOffset;

            // << NORMAL CALCULATIONS >>
            // Create a rotation matrix based on the grid's direction
            Quaternion rotation = Quaternion.LookRotation(config.gridNormal, Vector3.up);

            // Apply the rotation to the grid offset to get the final world offset
            Vector3 worldOffset = rotation * new Vector3(gridOffset.x, gridOffset.y, 0);

            // Combine the base position with the calculated world offset
            return originPosition + worldOffset;
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
