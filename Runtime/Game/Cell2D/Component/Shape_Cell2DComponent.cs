

using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public class Shape_Cell2DComponent : Abstract_Cell2DComponent, ICell2DComponent
    {
        public Shape2D Shape { get => _shape; }

        [SerializeField] Shape2D _shape;
        [SerializeField, Range(3, 32)] int _segments = 16;

        public Shape_Cell2DComponent() { }
        public Shape_Cell2DComponent(Cell2D cell)
        {
            Initialize(cell);
        }
        public Shape_Cell2DComponent(Cell2D cell, Shape_Cell2DComponent template)
        {
            _segments = template._segments;
            Initialize(cell);
        }

        public override void Initialize(Cell2D cell)
        {
            base.Initialize(cell);

            Name = "Shape2DComponent";
            Type = ICell2DComponent.TypeKey.Shape;

            cell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            _shape = new Shape2D(position, radius, _segments, normal, Color.white);
        }

        public void Update()
        {
            Cell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            _shape.UpdateShape(position, radius, _segments, normal, Color.white);
        }

        public void DrawGizmos()
        {
            if (_shape == null) return;
            _shape.DrawGizmos(false);
        }

        public void DrawEditorGizmos() { }

        public void Copy(ICell2DComponent component)
        {
            if (!initialized) return;

            if (component is Shape_Cell2DComponent shapeComponent)
            {
                Cell = shapeComponent.Cell;
                _segments = shapeComponent._segments;
            }
        }
    }
}