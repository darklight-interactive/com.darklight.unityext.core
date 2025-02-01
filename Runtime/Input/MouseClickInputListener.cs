using System;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Darklight.UnityExt.Input
{
    /// <summary>
    /// Listens for mouse clicks and provides both screen and world positions of the click points.
    /// Integrates with the Universal Input system for consistent input handling.
    ///
    /// </summary>
    public class MouseClickInputListener : UniversalInputController
    {
        private Vector2 _currentMouseScreenPosition;
        private Vector3 _currentMouseWorldPosition;
        private Vector2 _lastClickScreenPosition;
        private Vector3 _lastClickWorldPosition;
        private bool _hasHit;
        private RaycastHit _lastHit;

        [Foldout("Debug")]
        [SerializeField]
        private bool _debug = true;

        [Foldout("Debug")]
        [SerializeField, ShowIf("_debug")]
        private bool _drawCurrentMouseWorldPosition = true;

        [Foldout("Debug")]
        [SerializeField, ShowIf("_debug")]
        private bool _drawLastClickWorldPosition = true;

        [Foldout("Camera")]
        [SerializeField]
        private Camera _camera;

        [Foldout("Camera")]
        [SerializeField]
        private LayerMask _clickableLayerMask = -1; // -1 means everything

        [Foldout("Camera")]
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

            UniversalInputManager.OnMousePosition += OnMousePosition;
            UniversalInputManager.OnMousePrimaryClick += OnMousePrimaryClick;
            UniversalInputManager.OnMouseSecondaryClick += OnMouseSecondaryClick;
        }

        protected virtual void OnMousePosition(Vector2 screenPosition)
        {
            _currentMouseScreenPosition = screenPosition;
            _currentMouseWorldPosition = CalculateScreenToWorld(_currentMouseScreenPosition);
        }

        protected virtual void OnMousePrimaryClick()
        {
            _lastClickScreenPosition = _currentMouseScreenPosition;
            _lastClickWorldPosition = CalculateScreenToWorld(_lastClickScreenPosition);
        }

        protected virtual void OnMouseSecondaryClick()
        {
            _lastClickScreenPosition = _currentMouseScreenPosition;
            _lastClickWorldPosition = CalculateScreenToWorld(_lastClickScreenPosition);
        }

        protected Vector3 CalculateScreenToWorld(Vector2 screenPosition)
        {
            // Create a ray from the camera through the click point
            Ray ray = _camera.ScreenPointToRay(screenPosition);

            // Perform the raycast
            _hasHit = Physics.Raycast(ray, out _lastHit, _maxRayDistance, _clickableLayerMask);

            if (_hasHit)
                return _lastHit.point;
            else
            {
                Debug.LogError("Could not find hitpoint");
                return ray.GetPoint(_maxRayDistance);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!_debug || _camera == null)
                return;

            // Draw the ray from camera through current mouse position
            Ray currentRay = _camera.ScreenPointToRay(_currentMouseScreenPosition);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(currentRay.origin, currentRay.direction * _maxRayDistance);

            if (_drawCurrentMouseWorldPosition)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(CurrentMouseWorldPosition, 0.5f);
            }

            if (_drawLastClickWorldPosition)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(LastClickWorldPosition, 0.5f);

                // Draw the ray that created the last hit
                Ray lastRay = _camera.ScreenPointToRay(_lastClickScreenPosition);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(lastRay.origin, lastRay.direction * _maxRayDistance);
            }

            // If we hit something, draw a line from the camera to the hit point
            if (_hasHit)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_lastHit.point, 0.2f);
                Gizmos.DrawLine(_camera.transform.position, _lastHit.point);
            }
        }
    }
}
