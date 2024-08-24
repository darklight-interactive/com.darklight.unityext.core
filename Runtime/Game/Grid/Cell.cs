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
    public abstract class AbstractCell
    {
        public abstract BaseCellData Data { get; }
        public abstract void Update();
        public abstract void SetData(BaseCellData data);
        public abstract void DrawGizmos(bool editMode);
    }

    [System.Serializable]
    public class GenericCell<TData> : AbstractCell where TData : BaseCellData, new()
    {
        // -- Protected Data ---- >>
        [SerializeField] protected TData data;
        public override BaseCellData Data => data;

        // ===================== [[ CONSTRUCTORS ]] ===================== //
        public GenericCell() { }
        public GenericCell(Vector2Int key)
        {
            TData customData = new TData();
            customData.Initialize(key);
            Initialize(customData);
        }

        // ===================== [[ RUNTIME METHODS ]] ===================== //
        protected virtual void Initialize(TData data)
        {
            this.data = data;
        }

        // Update the cell to reflect any changes to the data object
        public override void Update() { }

        #region (( Getter Methods )) -------- >>
        /// <summary>
        /// Get the gizmo color of the cell.
        /// </summary>
        /// <param name="color"></param>
        protected virtual void GetGizmoColor(out Color color)
        {
            color = data.IsDisabled ? Color.grey : Color.white;
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
            position = data.Position;
            dimensions = data.Dimensions;
            normal = data.Normal;
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
        public void ToggleDisabled()
        {
            data.SetDisabled(!data.IsDisabled);
        }

        #endregion

        #region (( Gizmo Methods )) -------- >>
#if UNITY_EDITOR
        public override void DrawGizmos(bool editMode)
        {
            if (data == null)
                return;

            DrawCell();
            DrawLabel($"Cell {data.Key}");
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
    }
    #endregion

    public class Cell : GenericCell<CellData>
    {
        public Cell() : base() { }
        public Cell(Vector2Int key) : base(key) { }
    }
}
