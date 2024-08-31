
using System;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public enum CellComponentType
    {
        Base = 0,
        Overlap = 1 << 0,
        Shape = 1 << 1,
        Weight = 1 << 2
    }

    public interface ICellComponent
    {
        BaseCell cell { get; }
        CellComponentType type { get; }

        void Initialize(BaseCell cell);
        void Update();
        void DrawGizmos();
        void DrawEditorGizmos();
    }

    public abstract class AbstractCellComponent
    {
        BaseCell _cell;
        bool _initialized = false;

        public BaseCell cell { get => _cell; protected set => _cell = value; }
        public bool initialized { get => _initialized; protected set => _initialized = value; }

        public virtual void Initialize(BaseCell cell)
        {
            if (cell == null)
            {
                Debug.LogError("Cannot initialize component with null cell.");
                return;
            }

            this.cell = cell;
            this.initialized = true;
        }
    }

    public class BaseCellComponent : AbstractCellComponent, ICellComponent
    {
        public CellComponentType type { get; protected set; } = CellComponentType.Base;

        public BaseCellComponent() { }
        public BaseCellComponent(BaseCell cell) => Initialize(cell);

        public virtual void Update() { }
        public virtual void DrawGizmos() { }
        public virtual void DrawEditorGizmos() { }
    }

    [System.Serializable]
    public class Overlap2DComponent : AbstractCellComponent, ICellComponent
    {
        public CellComponentType type { get; protected set; } = CellComponentType.Overlap;
        public LayerMask layerMask { get => _layerMask; set => _layerMask = value; }

        [SerializeField] LayerMask _layerMask;
        Collider2D[] _colliders;

        public Overlap2DComponent() { }
        public Overlap2DComponent(BaseCell cell)
        {
            _layerMask = 0;
            Initialize(cell);
        }
        public Overlap2DComponent(BaseCell cell, LayerMask layerMask)
        {
            _layerMask = layerMask;
            Initialize(cell);
        }
        public Overlap2DComponent(BaseCell cell, Overlap2DComponent template)
        {
            _layerMask = template.layerMask;
            Initialize(cell);
        }

        public void Update()
        {
            UpdateColliders(_layerMask);
        }

        void UpdateColliders(LayerMask layerMask)
        {
            cell.GetTransformData(out Vector3 position, out Vector3 normal, out float radius);
            Vector3 halfExtents = Vector3.one * radius;

            // Use Physics.OverlapBox to detect colliders within the cell dimensions
            _colliders = Physics2D.OverlapBoxAll(position, halfExtents, 0, layerMask);
        }

        public void DrawGizmos()
        {
            cell.GetTransformData(out Vector3 position, out Vector3 normal, out float radius);
            CustomGizmos.DrawWireRect(position, Vector2.one * radius * 2, normal, Color.red);
        }

        public void DrawEditorGizmos() { }
    }

    [System.Serializable]
    public class Shape2DComponent : AbstractCellComponent, ICellComponent
    {
        public CellComponentType type { get; protected set; } = CellComponentType.Shape;

        [SerializeField] Shape2D _shape;
        public Shape2D Shape { get => _shape; }

        public Shape2DComponent() { }
        public Shape2DComponent(BaseCell cell)
        {
            _shape = null;
            Initialize(cell);
        }
        public Shape2DComponent(BaseCell cell, Shape2DComponent template)
        {
            _shape = template.Shape;
            Initialize(cell);
        }

        public override void Initialize(BaseCell cell)
        {
            base.Initialize(cell);
            cell.GetTransformData(out Vector3 position, out Vector3 normal, out float radius);
            _shape = new Shape2D(position, radius, 8, normal, Color.white);

            //Debug.Log($"{ComponentType} initialized with position {position}, radius {radius}, and normal {normal}.");
        }

        public void Update()
        {
            cell.GetTransformData(out Vector3 position, out Vector3 normal, out float radius);
            _shape = new Shape2D(position, radius, 8, normal, Color.white);
        }

        public void DrawGizmos()
        {
            if (_shape == null) return;
            _shape.DrawGizmos(false);
        }

        public void DrawEditorGizmos() { }
    }

    [System.Serializable]
    public class WeightComponent : AbstractCellComponent, ICellComponent
    {
        public CellComponentType type { get; protected set; } = CellComponentType.Weight;

        [SerializeField, Range(0, 100)] int _weight;
        public int Weight { get => _weight; }

        public WeightComponent() { }
        public WeightComponent(BaseCell cell)
        {
            _weight = 0;
            Initialize(cell);
        }
        public WeightComponent(BaseCell cell, WeightComponent template)
        {
            _weight = template.Weight;
            Initialize(cell);
        }

        public void Update() { }

        public void SetWeight(int weight)
        {
            _weight = weight;
        }

        public void DrawGizmos()
        {
            cell.GetTransformData(out Vector3 position, out Vector3 normal, out float radius);
            CustomGizmos.DrawLabel($"Weight: {_weight}", position, CustomGUIStyles.BoldCenteredStyle);
        }
        public void DrawEditorGizmos() { }
    }
}
