using System;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Input
{
    /// <summary>
    /// Listens for mouse clicks and provides both screen and world positions of the click points.
    /// Integrates with the Universal Input system for consistent input handling.
    /// </summary>
    public class MouseClickInputListener : UniversalInputController
    {
        private Vector2 _currentMouseScreenPosition;
        private Vector3 _currentMouseWorldPosition;
        private Vector2 _lastClickScreenPosition;
        private Vector3 _lastClickWorldPosition;
        private bool _hasHit;
        private RaycastHit _lastHit;

        [Header("Debug")]
        [SerializeField]
        private bool _debug;

        [SerializeField, ShowIf("_debug")]
        private bool _drawCurrentMouseWorldPosition;

        [SerializeField, ShowIf("_debug")]
        private bool _drawLastClickWorldPosition;

        [Header("Click Settings")]
        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private LayerMask _clickableLayerMask = -1; // -1 means everything

        [SerializeField]
        private float _maxRayDistance = 100f;

        protected Vector2 CurrentMouseScreenPosition => _currentMouseScreenPosition;
        protected Vector3 CurrentMouseWorldPosition => _currentMouseWorldPosition;
        protected Vector2 LastClickScreenPosition => _lastClickScreenPosition;
        protected Vector3 LastClickWorldPosition => _lastClickWorldPosition;

        protected override void Awake()
        {
            base.Awake();

            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    _camera = FindFirstObjectByType<Camera>();
                }

                if (_camera == null)
                {
                    Debug.LogError("No camera assigned and no Main Camera found in the scene!");
                }
            }

            UniversalInputManager.OnMousePosition += HandleMousePosition;
            UniversalInputManager.OnMousePrimaryClick += HandleMousePrimaryClick;
            UniversalInputManager.OnMouseSecondaryClick += HandleMouseSecondaryClick;
        }

        protected void HandleMousePosition(Vector2 screenPosition)
        {
            _currentMouseScreenPosition = screenPosition;
            _currentMouseWorldPosition = CalculateScreenToWorld(_currentMouseScreenPosition);
        }

        protected void HandleMousePrimaryClick()
        {
            Debug.Log("Mouse Primary Click");
            _lastClickScreenPosition = _currentMouseScreenPosition;
            _lastClickWorldPosition = CalculateScreenToWorld(_lastClickScreenPosition);
        }

        protected void HandleMouseSecondaryClick()
        {
            Debug.Log("Mouse Secondary Click");
        }

        protected Vector3 CalculateScreenToWorld(Vector2 screenPosition)
        {
            // Create a ray from the camera through the click point
            Ray ray = _camera.ScreenPointToRay(screenPosition);

            // Perform the raycast
            _hasHit = Physics.Raycast(ray, out _lastHit, _maxRayDistance, _clickableLayerMask);

            return _hasHit ? _lastHit.point : ray.GetPoint(_maxRayDistance);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_debug)
                return;

            if (_drawCurrentMouseWorldPosition)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(CurrentMouseWorldPosition, 0.5f);
            }

            if (_drawLastClickWorldPosition)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(LastClickWorldPosition, 0.5f);
            }

            // If we hit something, draw a line from the camera to the hit point
            if (_hasHit)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_camera.transform.position, LastClickWorldPosition);
            }
        }
    }
}
