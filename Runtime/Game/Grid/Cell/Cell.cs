

using Darklight.UnityExt.Editor;

using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{

    public abstract class AbstractCell
    {
        public abstract void Initialize(Vector2Int key);
        public abstract void Update();
        public abstract TData GetData<TData>() where TData : BaseCellData;
        public abstract void SetData<TData>(TData data) where TData : BaseCellData;
        public abstract void ApplyConfig<TConfig>(TConfig config) where TConfig : AbstractGrid.Config;
        public abstract void DrawGizmos(bool editMode);
        protected abstract void OnEditToggle();




    }

    [System.Serializable]
    public abstract class BaseCell<TData> : AbstractCell
        where TData : BaseCellData, new()
    {
        [SerializeField] TData _data;
        public TData Data => _data;

        // ===================== [[ CONSTRUCTORS ]] ===================== //
        public BaseCell() => Initialize(Vector2Int.zero);
        public BaseCell(Vector2Int key) => Initialize(key);
        public override void Initialize(Vector2Int key)
        {
            _data = new TData();
            _data.SetKey(key);
        }

        public override abstract void Update();
        public override T GetData<T>() => Data as T;
        public override void SetData<T>(T data) => _data = data as TData;
        public override void ApplyConfig<TConfig>(TConfig config)
        {
            if (Data == null)
                return;

            Data.SetCoordinate(config.CalculateCoordinateFromKey(Data.key));
            Data.SetPosition(config.CalculatePositionFromKey(Data.key));
            Data.SetNormal(config.gridNormal);
            Data.SetDimensions(config.cellDimensions);
        }

        public override void DrawGizmos(bool editMode)
        {
            if (Data == null)
                return;

            DrawCell();
            DrawLabel($"{Data.coordinate}");
            if (editMode) DrawCellToggle();
        }

        protected override void OnEditToggle()
        {
            Data.SetDisabled(!Data.disabled);
        }
        #region (( Gizmo Methods )) -------- >>
#if UNITY_EDITOR
        /// <summary>
        /// Get the gizmo color of the cell.
        /// </summary>
        /// <param name="color"></param>
        protected virtual void GetGizmoColor(out Color color)
        {
            color = Data.disabled ? Color.grey : Color.white;
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
            position = Data.position;
            dimensions = Data.dimensions;
            normal = Data.normal;
            GetGizmoColor(out color);
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

#endif
        #endregion

    }

    public class BaseCell : BaseCell<BaseCellData>
    {
        public BaseCell() : base() { }
        public BaseCell(Vector2Int key) : base(key) { }

        public override void Update()
        {
            // Update the cell data
        }
    }
}
