

using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public class Cell2D_ShapeComponent : Cell2D.Component
    {
        public Shape2D Shape { get => _shape; }

        [SerializeField] Shape2D _shape;
        [SerializeField, Range(3, 32)] int _segments = 16;

        public Cell2D_ShapeComponent(Cell2D cell) : base(cell) { }

        public override void OnInitialize(Cell2D baseObj)
        {
            base.OnInitialize(baseObj);

            BaseCell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            _shape = new Shape2D(position, radius, _segments, normal, Color.white);
        }

        public override void OnUpdate()
        {
            BaseCell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            _shape.UpdateShape(position, radius, _segments, normal, Color.white);
        }

        public override Cell2D.ComponentTypeKey GetTypeKey() => Cell2D.ComponentTypeKey.SHAPE;

        public override void DrawGizmos()
        {
            if (_shape == null) return;
            _shape.DrawGizmos(false);
        }

        public override void DrawEditorGizmos() { }
    }
}