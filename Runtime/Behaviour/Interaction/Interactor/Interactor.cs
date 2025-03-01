using System;
using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Library;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Behaviour
{
    public enum InteractorShape
    {
        RECT,
        CIRCLE
    }

    public interface IInteractor
    {
        InteractorShape Shape { get; }
        LayerMask LayerMask { get; }
        Library<Interactable, string> NearbyInteractables { get; }
        Interactable TargetInteractable { get; }

        void TryAddInteractable(Interactable interactable);
        void RemoveInteractable(Interactable interactable);

        List<Interactable> FindOverlapInteractables();
        Interactable GetClosestReadyInteractable(Vector3 position);

        bool TryAssignTarget(Interactable interactable);
        void ClearTarget();

        bool InteractWith(Interactable interactable, bool force = false);
        bool InteractWithTarget();

        void RefreshNearbyInteractables();
    }

    [ExecuteAlways]
    public class Interactor : MonoBehaviour, IInteractor
    {
        [Header("Interactor Settings")]
        [SerializeField]
        InteractorShape _shape = InteractorShape.RECT;

        [SerializeField]
        LayerMask _layerMask;

        [SerializeField, HideIf("IsCircle")]
        Vector2 _overlapDimensions = new Vector2(1, 1);

        [SerializeField, HideIf("IsRect")]
        float _overlapRadius = 1;

        [SerializeField, ShowOnly]
        Vector2 _offsetPosition = new Vector2(0, 0);

        [Header("Interactables")]
        [SerializeField, ShowOnly]
        Interactable _lastTarget;

        [SerializeField, ShowOnly]
        Interactable _target;

        [Space(10)]
        [SerializeField, ShowOnly]
        Interactable _closestInteractable;

        [SerializeField]
        protected Library<Interactable, string> nearbyInteractables = new Library<
            Interactable,
            string
        >()
        {
            ReadOnlyKey = true,
            ReadOnlyValue = true
        };

        // ======== [[ PROPERTIES ]] ================================== >>>>
        public LayerMask LayerMask
        {
            get => _layerMask;
            set => _layerMask = value;
        }
        public Library<Interactable, string> NearbyInteractables => nearbyInteractables;
        public Interactable TargetInteractable => _target;
        public Vector2 OffsetPosition
        {
            get => _offsetPosition;
            set => _offsetPosition = value;
        }
        protected Vector2 OverlapCenter => (Vector2)transform.position + _offsetPosition;
        public Vector2 OverlapDimensions
        {
            get => _overlapDimensions;
            protected set => _overlapDimensions = value;
        }
        public float OverlapRadius
        {
            get => _overlapRadius;
            protected set => _overlapRadius = value;
        }
        public InteractorShape Shape
        {
            get => _shape;
            protected set => _shape = value;
        }
        public bool IsCircle => _shape == InteractorShape.CIRCLE;
        public bool IsRect => _shape == InteractorShape.RECT;

        // ======== [[ EVENTS ]] ================================== >>>>
        public event Action<Interactable> OnInteractableAdded;
        public event Action<Interactable> OnInteractableRemoved;

        #region ======== <METHODS> (( UNITY RUNTIME )) ================================== >>>>
        protected virtual void Start()
        {
            OnInteractableAdded += (interactable) => {
                //Debug.Log($"[{name}] OnInteractableAdded: {interactable.name}");
            };
            OnInteractableRemoved += (interactable) => {
                //Debug.Log($"[{name}] OnInteractableRemoved: {interactable.name}");
            };
        }

        public virtual void Update()
        {
            RefreshNearbyInteractables();

            // << UPDATE TARGET >> --------
            _closestInteractable = GetClosestReadyInteractable(OverlapCenter);
            TryAssignTarget(_closestInteractable);
        }

        protected virtual void OnDrawGizmos()
        {
            if (IsRect)
            {
#if UNITY_EDITOR
                CustomGizmos.DrawWireRect(
                    OverlapCenter,
                    _overlapDimensions,
                    Vector3.forward,
                    Color.red
                );
#endif
            }
            else if (IsCircle)
            {
#if UNITY_EDITOR
                CustomGizmos.DrawWireSphere(OverlapCenter, _overlapRadius, Color.red);
#endif
            }
            foreach (Interactable interactable in nearbyInteractables.Keys)
            {
                if (interactable == null)
                    continue;
                if (interactable == _target)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, interactable.Position);
                }
                else
                {
                    Gizmos.color = Color.yellow;
                }

                Gizmos.DrawSphere(interactable.Position, 0.05f);
            }
        }
        #endregion

        #region ======== <PUBLIC_METHODS> (( IInteractor )) ================================== >>>>
        public List<Interactable> FindOverlapInteractables()
        {
            List<Interactable> interactables = new List<Interactable>();
            Collider2D[] colliders = null;

            if (_shape == InteractorShape.RECT)
            {
                colliders = Physics2D.OverlapBoxAll(
                    OverlapCenter,
                    _overlapDimensions,
                    0,
                    _layerMask
                );
            }
            else if (_shape == InteractorShape.CIRCLE)
            {
                colliders = Physics2D.OverlapCircleAll(OverlapCenter, _overlapRadius, _layerMask);
            }
            foreach (Collider2D collider in colliders)
            {
                Interactable interactable = collider.GetComponent<Interactable>();
                if (interactable != null)
                {
                    interactables.Add(interactable);
                }
            }
            return interactables;
        }

        public virtual void TryAddInteractable(Interactable interactable)
        {
            if (interactable == null)
                return;
            if (nearbyInteractables.ContainsKey(interactable))
            {
                nearbyInteractables[interactable] = interactable.Name;
                return;
            }

            nearbyInteractables.Add(interactable, interactable.Name);
            OnInteractableAdded?.Invoke(interactable);
        }

        public virtual void RemoveInteractable(Interactable interactable)
        {
            if (interactable == null)
                return;
            if (nearbyInteractables.ContainsKey(interactable))
            {
                nearbyInteractables.Remove(interactable);
                OnInteractableRemoved?.Invoke(interactable);
            }
        }

        public Interactable GetClosestReadyInteractable(Vector3 position)
        {
            if (nearbyInteractables.Count == 0)
                return null;
            if (nearbyInteractables.Count == 1)
                return nearbyInteractables.Keys.First();

            Interactable closestInteractable = nearbyInteractables.Keys.First();
            float closestDistance = float.MaxValue;
            foreach (Interactable interactable in nearbyInteractables.Keys)
            {
                if (interactable == null)
                    continue;

                // Calculate the distance to the interactable.
                float distance = Vector3.Distance(interactable.Position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
            return closestInteractable;
        }

        public bool TryAssignTarget(Interactable interactable)
        {
            if (interactable == null)
                return false;
            if (_target == interactable)
                return false;
            if (_lastTarget == interactable)
                return false;

            bool result = interactable.AcceptTarget(this);
            if (result)
            {
                _lastTarget = _target;
                _target = interactable;

                if (_lastTarget != null)
                    _lastTarget.Reset();
            }
            //Debug.Log($"[{name}] TryAssignTarget: {interactable.name} => {result}");
            return result;
        }

        public void ClearTarget()
        {
            _lastTarget = _target;
            _target = null;

            _lastTarget.Reset();
        }

        public bool InteractWith(Interactable interactable, bool force = false)
        {
            if (interactable == null)
                return false;
            return interactable.AcceptInteraction(this, force);
        }

        public bool InteractWithTarget() => InteractWith(_target);

        public void RefreshNearbyInteractables()
        {
            // Update the interactables dictionary with the overlap interactables.
            List<Interactable> overlapInteractables = FindOverlapInteractables();
            foreach (Interactable interactable in overlapInteractables)
            {
                TryAddInteractable(interactable);
            }

            if (_target != null && !overlapInteractables.Contains(_target))
            {
                _target.Reset();
                _target = null;
            }

            if (_lastTarget != null && !overlapInteractables.Contains(_lastTarget))
            {
                _lastTarget.Reset();
                _lastTarget = null;
            }

            // Remove interactables from the dict that are no longer in the overlap interactables.
            List<Interactable> dictInteractables = new List<Interactable>(nearbyInteractables.Keys);
            List<Interactable> interactablesToRemove = new List<Interactable>();
            foreach (Interactable interactable in dictInteractables)
            {
                if (!overlapInteractables.Contains(interactable))
                {
                    interactablesToRemove.Add(interactable);
                }
            }
            foreach (Interactable interactable in interactablesToRemove)
            {
                RemoveInteractable(interactable);
            }
        }

        #endregion
    }
}
