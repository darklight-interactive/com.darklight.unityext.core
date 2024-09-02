using Darklight.UnityExt.Editor;
using UnityEngine;
using System.Collections.Generic;

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public class Cell2D_OverlapComponent : Cell2D.Component
    {
        [SerializeField] LayerMask _layerMask;
        Collider2D[] _colliders;

        // HashSets for tracking colliders
        private HashSet<Collider2D> _currentColliders = new HashSet<Collider2D>();
        private HashSet<Collider2D> _previousColliders = new HashSet<Collider2D>();

        // ======== [[ PROPERTIES ]] ================================== >>>>
        public LayerMask LayerMask { get => _layerMask; set => _layerMask = value; }

        // ======== [[ EVENTS ]] ================================== >>>>
        public delegate void OverlapEvent(Cell2D cell, Collider2D collider);
        public OverlapEvent OnColliderEnter;
        public OverlapEvent OnColliderExit;

        // ======== [[ CONSTRUCTORS ]] =========================== >>>>
        public Cell2D_OverlapComponent(Cell2D cell) : base(cell) { }
        public Cell2D_OverlapComponent(Cell2D cell, LayerMask layerMask) : base(cell)
        {
            _layerMask = layerMask;
        }

        // ======== [[ METHODS ]] ================================== >>>>
        public override void Updater()
        {
            UpdateColliders();
        }

        public override void DrawGizmos()
        {
            Cell.GetTransformData(out Vector3 position, out Vector3 normal, out Vector2 dimensions);
            GetColor(out Color color);
            CustomGizmos.DrawWireRect(position, dimensions, normal, color);
        }

        public void SetLayerMask(LayerMask layerMask)
        {
            _layerMask = layerMask;
        }

        void UpdateColliders()
        {
            if (_previousColliders == null)
            {
                _previousColliders = new HashSet<Collider2D>();
            }

            // Clear the currentColliders set to prepare for new detections
            _currentColliders = new HashSet<Collider2D>();

            Cell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            Vector3 halfExtents = Vector3.one * radius;

            // Use Physics2D.OverlapBoxAll to detect colliders within the cell dimensions
            _colliders = Physics2D.OverlapBoxAll(position, halfExtents, 0, _layerMask);

            // Add detected colliders to the currentColliders set
            foreach (var collider in _colliders)
            {
                _currentColliders.Add(collider);
            }

            // Detect colliders that have entered
            HashSet<Collider2D> enteredColliders = new HashSet<Collider2D>(_currentColliders);
            enteredColliders.ExceptWith(_previousColliders); // Elements in current but not in previous
            foreach (var collider in enteredColliders)
            {
                OnColliderEnter?.Invoke(Cell, collider);
            }

            // Detect colliders that have exited
            HashSet<Collider2D> exitedColliders = new HashSet<Collider2D>(_previousColliders);
            exitedColliders.ExceptWith(_currentColliders); // Elements in previous but not in current
            foreach (var collider in exitedColliders)
            {
                OnColliderExit?.Invoke(Cell, collider);
            }

            // Swap sets: previous becomes current, reuse the set
            (_currentColliders, _previousColliders) = (_previousColliders, _currentColliders);
        }

        void GetColor(out Color color)
        {
            if (_currentColliders == null || _currentColliders.Count == 0)
            {
                color = Color.grey;
                return;
            }

            color = Color.red;
        }
    }
}
