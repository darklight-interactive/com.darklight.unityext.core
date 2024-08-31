using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{

    [System.Serializable]
    public class Overlap_Cell2DComponent : Abstract_Cell2DComponent, ICell2DComponent
    {
        [SerializeField] LayerMask _layerMask;
        Collider2D[] _colliders;

        public LayerMask layerMask { get => _layerMask; set => _layerMask = value; }


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

        public override void Initialize(Cell2D cell)
        {
            base.Initialize(cell);
            Name = "Overlap2DComponent";
            Type = ICell2DComponent.TypeKey.Overlap;

            Cell2D_Config config = Cell.Config;
            _layerMask = config.LayerMask;
        }

        public void Update()
        {
            UpdateColliders(_layerMask);

            Cell2D_Config config = Cell.Config;
            _layerMask = config.LayerMask;
        }

        void UpdateColliders(LayerMask layerMask)
        {
            Cell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
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

        public void Copy(ICell2DComponent component)
        {
            if (!(component is Overlap_Cell2DComponent)) return;
            Overlap_Cell2DComponent overlapComponent = component as Overlap_Cell2DComponent;

            Cell = overlapComponent.Cell;
            _layerMask = overlapComponent.layerMask;
        }
    }

}