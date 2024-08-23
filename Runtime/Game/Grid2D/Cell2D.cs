using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game
{
    public abstract class AbstractCellData
    {
        public abstract string name { get; protected set; }
        public abstract Vector2Int key { get; protected set; }
        public abstract Vector2 dimensions { get; protected set; }
        public abstract Vector3 position { get; protected set; }
        public abstract Vector3 normal { get; protected set; }
        public abstract bool disabled { get; protected set; }

        public abstract void GetWorldSpaceData(out Vector3 position, out Vector3 normal);
        public abstract void GetDimensions(out Vector2 dimensions);

        public abstract void SetWorldSpaceData(Vector3 position, Vector3 normal);
        public abstract void SetDimensions(Vector2 dimensions);
        public abstract void SetDisabled(bool disabled);

        public virtual void CopyFrom(AbstractCellData data)
        {
            if (data == null)
            {
                Debug.LogError("Cannot copy data from null object.");
                return;
            }

            name = data.name;
            key = data.key;
            dimensions = data.dimensions;
            position = data.position;
            normal = data.normal;
            disabled = data.disabled;
        }
    }


    [System.Serializable]
    public class Cell2D
    {
        [System.Serializable]
        public class Data : AbstractCellData
        {
            [SerializeField, ShowOnly] string _name = "DefaultCell";
            [SerializeField, ShowOnly] Vector2Int _key;
            [SerializeField, ShowOnly] Vector2 _dimensions = Vector2.one;
            [SerializeField, ShowOnly] Vector3 _position = Vector3.zero;
            [SerializeField, ShowOnly] Vector3 _normal = Vector3.up;
            [SerializeField, ShowOnly] bool _disabled = false;

            public override string name { get => _name; protected set => _name = value; }
            public override Vector2Int key { get => _key; protected set => _key = value; }
            public override Vector2 dimensions { get => _dimensions; protected set => _dimensions = value; }
            public override Vector3 position { get => _position; protected set => _position = value; }
            public override Vector3 normal { get => _normal; protected set => _normal = value; }
            public override bool disabled { get => _disabled; protected set => _disabled = value; }


            public Data() { }
            public Data(Vector2Int key) => Initialize(key);
            public Data(AbstractCellData data)
            {
                _name = data.name;
                _key = data.key;
                _dimensions = data.dimensions;
                _position = data.position;
                _normal = data.normal;
                _disabled = data.disabled;
            }

            public void Initialize(Vector2Int key)
            {
                _key = key;
                _name = $"Cell2D {key}";
            }



            #region (( Getter Methods )) -------- >>
            public override void GetWorldSpaceData(out Vector3 position, out Vector3 normal)
            {
                position = _position;
                normal = _normal;
            }
            public override void GetDimensions(out Vector2 dimensions)
            {
                dimensions = _dimensions;
            }
            #endregion

            #region (( Setter Methods )) -------- >>
            public override void SetWorldSpaceData(Vector3 position, Vector3 normal)
            {
                _position = position;
                _normal = normal;
            }
            public void SetPosition(Vector3 position)
            {
                _position = position;
            }
            public override void SetDimensions(Vector2 dimensions)
            {
                _dimensions = dimensions;
            }
            public override void SetDisabled(bool disabled)
            {
                _disabled = disabled;
            }
            #endregion
        }

        // -- Protected Data ---- >>
        protected AbstractCellData data; // Data object for the cell

        // ===================== [[ CONSTRUCTORS ]] ===================== //
        public Cell2D() => Initialize(new Data());
        public Cell2D(Cell2D.Data data) => Initialize(data);
        public Cell2D(Vector2Int key) => Initialize(new Cell2D.Data(key));
        public Cell2D(Vector2Int key, AbstractGrid2D.Config config)
        {
            Cell2D.Data customData = new Cell2D.Data(key);
            ApplyConfigToData(config);
            Initialize(customData);
        }

        // ===================== [[ RUNTIME METHODS ]] ===================== //
        protected virtual void Initialize(AbstractCellData data)
        {
            this.data = data;
            Debug.Log($"Initialized Cell2D {data.key}");
        }

        // Update the cell to reflect any changes to the data object
        public virtual void Update() { }

        #region (( Getter Methods )) -------- >>

        public AbstractCellData GetData() => data;

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
            data.GetWorldSpaceData(out Vector3 o_pos, out Vector3 o_normal);
            data.GetDimensions(out Vector2 o_dimensions);
            GetGizmoColor(out Color o_color);

            position = o_pos;
            dimensions = o_dimensions;
            normal = o_normal;
            color = o_color;
        }

        #endregion

        #region (( Setter Methods )) -------- >>
        public void SetData(AbstractCellData data) => this.data = data;
        public void ApplyConfigToData(AbstractGrid2D.Config config)
        {
            if (config == null)
            {
                //Debug.LogError("Cannot apply config to data. Config is null.");
                return;
            }

            if (data == null)
            {
                //Debug.LogError("Cannot apply config to data. Data is null.");
                return;
            }

            Vector3 pos = AbstractGrid2D.CalculateWorldPositionFromKey(data.key, config);
            data.SetWorldSpaceData(pos, config.normal);
            data.SetDimensions(config.cellDimensions);
        }

        public void SetWorldSpaceData(Vector3 position, Vector3 normal) => data.SetWorldSpaceData(position, normal);
        public void ToggleDisabled()
        {
            data.SetDisabled(!data.disabled);
        }

        #endregion

        #region (( Gizmo Methods )) -------- >>
#if UNITY_EDITOR
        public virtual void DrawGizmos(bool editMode)
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

    #region -- << GENERIC CLASS >> : CELL2D ------------------------------------ >>
    [System.Serializable]
    public class Cell<TData> : Cell2D where TData : Cell2D.Data, new()
    {
        public new TData data { get => base.data as TData; protected set => base.data = value; }

        public Cell() => Initialize(new TData());
        public Cell(TData data) => Initialize(data);
        public Cell(Vector2Int key)
        {
            TData customData = new TData();
            customData.Initialize(key);
            Initialize(customData);
        }

        public Cell(Vector2Int key, AbstractGrid2D.Config config)
        {
            TData customData = new TData();
            customData.Initialize(key);
            ApplyConfigToData(config);
            Initialize(customData);
        }

        protected override void Initialize(AbstractCellData data)
        {
            if (data != null)
                this.data = data as TData;
            else
                this.data = new TData();
        }
    }
    #endregion
}