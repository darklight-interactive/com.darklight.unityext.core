using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{

    [System.Serializable]
    public class Cell2D_OverlapComponent : Cell2D.Component
    {
        [SerializeField] LayerMask _layerMask;
        Collider2D[] _colliders;

        public LayerMask layerMask { get => _layerMask; set => _layerMask = value; }

        public Cell2D_OverlapComponent(Cell2D cell) : base(cell) { }
        public Cell2D_OverlapComponent(Cell2D cell, LayerMask layerMask) : base(cell)
        {
            _layerMask = layerMask;
        }

        public override void Initialize()
        {
            Tag = Type.OVERLAP;
        }

        public override void Update()
        {
            UpdateColliders(_layerMask);
        }

        public override void DrawGizmos()
        {
            Base.GetTransformData(out Vector3 position, out Vector3 normal, out Vector2 dimensions);
            GetColor(out Color color);
            CustomGizmos.DrawWireRect(position, dimensions, normal, color);
        }

        void UpdateColliders(LayerMask layerMask)
        {
            Base.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
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

        public override void DrawEditorGizmos() { }
    }

}