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

    #region [[ CELL DATA ]] ===================================================================== ]]

    #region -- << INTERFACE >> : ICellData ------------------------------------ >>
    public interface ICellData
    {
        string Name { get; }
        Vector2Int Key { get; }
        Vector2 Dimensions { get; }
        Vector3 Position { get; }
        Vector3 Normal { get; }
        bool IsDisabled { get; }

        void Initialize(Vector2Int key);
    }
    #endregion

    #region -- << ABSTRACT CLASS >> : BaseCellData ------------------------------------ >>
    [System.Serializable]
    public abstract class BaseCellData : ICellData
    {
        [SerializeField, ShowOnly] private string _name = "BaseCell";
        [SerializeField, ShowOnly] private Vector2Int _key = Vector2Int.zero;
        [SerializeField, ShowOnly] private Vector2 _dimensions = Vector2.one;
        [SerializeField, ShowOnly] private Vector3 _position = Vector3.zero;
        [SerializeField, ShowOnly] private Vector3 _normal = Vector3.up;
        [SerializeField, ShowOnly] private bool _isDisabled = false;

        public string Name { get => _name; protected set => _name = value; }
        public Vector2Int Key { get => _key; protected set => _key = value; }
        public Vector2 Dimensions { get => _dimensions; protected set => _dimensions = value; }
        public Vector3 Position { get => _position; protected set => _position = value; }
        public Vector3 Normal { get => _normal; protected set => _normal = value; }
        public bool IsDisabled { get => _isDisabled; protected set => _isDisabled = value; }

        public BaseCellData() { }
        public BaseCellData(Vector2Int key) => Initialize(key);
        public virtual void Initialize(Vector2Int key)
        {
            _key = key;
            _name = $"Cell2D {key}";
        }

        public void SetPosition(Vector3 position) => _position = position;
        public void SetNormal(Vector3 normal) => _normal = normal;
        public void SetDimensions(Vector2 dimensions) => _dimensions = dimensions;
        public void SetDisabled(bool disabled) => _isDisabled = disabled;

        public virtual void CopyFrom(BaseCellData data)
        {
            if (data == null)
            {
                Debug.LogError("Cannot copy data from null object.");
                return;
            }

            _name = data.Name;
            _key = data.Key;
            _dimensions = data.Dimensions;
            _position = data.Position;
            _normal = data.Normal;
            _isDisabled = data.IsDisabled;
        }
    }
    #endregion

    #region -- << CLASS >> : CellData ------------------------------------ >>
    [System.Serializable]
    public class CellData : BaseCellData
    {
        public CellData() : base(Vector2Int.zero) { }
        public CellData(Vector2Int key, GridConfig config) : base(key) { }
    }
    #endregion
    #endregion

    #region [[ CELL ]] ===================================================================== ]]

    public interface ICell
    {
        BaseCellData Data { get; }
        void Update();
        void DrawGizmos(bool editMode);
    }

    [System.Serializable]
    public abstract class BaseCell
    {
        // -- Protected Data ---- >>
        [SerializeField] protected BaseCellData data;
        public BaseCellData Data { get => data; protected set => data = value; }

        // ===================== [[ CONSTRUCTORS ]] ===================== //
        public BaseCell(Vector2Int key, GridConfig config)
        {
            CellData customData = new CellData(key, config);
            Initialize(customData);
        }

        // ===================== [[ RUNTIME METHODS ]] ===================== //
        protected virtual void Initialize(BaseCellData data)
        {
            this.data = data;
        }

        // Update the cell to reflect any changes to the data object
        public virtual void Update() { }

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
        public void SetData(BaseCellData data) => this.data = data;
        public void ToggleDisabled()
        {
            data.SetDisabled(!data.IsDisabled);
        }

        #endregion

        #region (( Gizmo Methods )) -------- >>
#if UNITY_EDITOR
        public virtual void DrawGizmos(bool editMode)
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
        #endregion
    }

    #region -- << GENERIC CLASS >> : CELL<> ------------------------------------ >>
    [System.Serializable]
    public class Cell<TData> : BaseCell where TData : BaseCellData, new()
    {
        public Cell() : base(Vector2Int.zero, null) { }
        public Cell(Vector2Int key, GridConfig config) : base(key, config) { }
    }
    #endregion

    #region -- << CLASS >> : Cell ------------------------------------ >>

    [System.Serializable]
    public class Cell : Cell<CellData>
    {
        public Cell() : base(Vector2Int.zero, null) { }
        public Cell(Vector2Int key, GridConfig config) : base(key, config) { }
    }

    #endregion

    #endregion
}
