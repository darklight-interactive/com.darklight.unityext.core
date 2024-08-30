

using Darklight.UnityExt.Editor;

using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public abstract class AbstractCell
    {
        [SerializeField] protected BaseCellData data;
        public abstract BaseCellData GetData();
        public abstract void SetData(BaseCellData data);
        public abstract void SetConfig(AbstractGrid.Config config);

        public abstract void Update();
        public abstract void DrawGizmos(bool editMode);
    }

    [System.Serializable]
    public abstract class BaseCell<TData> : AbstractCell
        where TData : BaseCellData, new()
    {
        // ===================== [[ CONSTRUCTORS ]] ===================== //
        public BaseCell() { }
        public BaseCell(Vector2Int key)
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

        public void SetKey(Vector2Int key)
        {
            if (data == null)
                return;

            data.SetKey(key);
        }

        public override void SetConfig(AbstractGrid.Config config)
        {
            if (data == null)
                return;

            data.SetCoordinate(config.CalculateCoordinateFromKey(data.key));
            data.SetPosition(config.CalculatePositionFromKey(data.key));
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
            DrawLabel($"{data.coordinate}");
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

    public class BaseCell : BaseCell<BaseCellData>
    {
        public BaseCell() : base() { }
        public BaseCell(Vector2Int key) : base(key) { }
        public override void Update() { }
    }
}
