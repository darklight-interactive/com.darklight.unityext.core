using System;
using Darklight.Editor;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Darklight.Input
{
    /// <summary>
    /// Listens for mouse clicks and provides both screen and world positions of the click points.
    /// Integrates with the Universal Input system for consistent input handling.
    ///
    /// </summary>
    public class MouseClickInputListener : UniversalInputController
    {
        [SerializeField, Foldout("Values"), ShowOnly]
        private Vector2 _currentMouseScreenPosition;

        [SerializeField, Foldout("Values"), ShowOnly]
        private Vector3 _currentMouseWorldPosition;

        [SerializeField, Foldout("Values"), ShowOnly]
        private Vector2 _lastClickScreenPosition;

        [SerializeField, Foldout("Values"), ShowOnly]
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

        [Foldout("Debug")]
        [SerializeField, ShowIf("_debug")]
        private float _gizmoSphereRadius = 0.5f;

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
                Gizmos.DrawSphere(CurrentMouseWorldPosition, _gizmoSphereRadius);
                Gizmos.DrawLine(_camera.transform.position, CurrentMouseWorldPosition);
            }

            if (_drawLastClickWorldPosition)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(LastClickWorldPosition, _gizmoSphereRadius);
            }
        }
    }
}
