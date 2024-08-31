using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{

    [System.Serializable]
    public class Overlap_Cell2DComponent : Abstract_Cell2DComponent, ICell2DComponent
    {
        public LayerMask layerMask { get => _layerMask; set => _layerMask = value; }

        LayerMask _layerMask;
        Collider2D[] _colliders;

        public Overlap_Cell2DComponent() { }
        public Overlap_Cell2DComponent(Cell2D cell)
        {
            _layerMask = 0;
            Initialize(cell);
        }
        public Overlap_Cell2DComponent(Cell2D cell, LayerMask layerMask)
        {
            _layerMask = layerMask;
            Initialize(cell);
        }
        public Overlap_Cell2DComponent(Cell2D cell, Overlap_Cell2DComponent template)
        {
            _layerMask = template.layerMask;
            Initialize(cell);
        }

        public override void Initialize(Cell2D cell)
        {
            base.Initialize(cell);
            Name = "Overlap2DComponent";
            Type = ICell2DComponent.TypeKey.Overlap;
        }

        public void Update()
        {
            UpdateColliders(_layerMask);
        }

        void UpdateColliders(LayerMask layerMask)
        {
            Cell.GetTransformData(out Vector3 position, out Vector3 normal, out float radius);
            Vector3 halfExtents = Vector3.one * radius;

            // Use Physics.OverlapBox to detect colliders within the cell dimensions
            _colliders = Physics2D.OverlapBoxAll(position, halfExtents, 0, layerMask);
        }

        void GetColor(out Color color)
        {
            if (_colliders == null || _colliders.Length == 0)
            {
                color = Color.green;
                return;
            }

            color = Color.red;
        }

        public void DrawGizmos()
        {
            Cell.GetTransformData(out Vector3 position, out Vector3 normal, out Vector2 dimensions);
            GetColor(out Color color);
            CustomGizmos.DrawWireRect(position, dimensions, normal, color);
        }

        public void DrawEditorGizmos() { }
    }

}