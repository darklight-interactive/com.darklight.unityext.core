using System;
using System.Collections.Generic;
using System.Linq;
using Darklight.Behaviour;
using Darklight.Collections;
using Darklight.Editor;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Behaviour
{
    public enum InteractorShape
    {
        RECT,
        CIRCLE
    }

    [ExecuteAlways]
    public class Interactor : Sensor
    {
        [Header("Interactor")]
        [HorizontalLine(color: EColor.Gray)]
        [SerializeField, ShowOnly]
        Interactable _lastTarget;

        [SerializeField, ShowOnly]
        Interactable _target;

        [SerializeField]
        CollectionDictionary<Interactable, string> _overlapInteractables =
            new CollectionDictionary<Interactable, string>();

        // ======== [[ PROPERTIES ]] ================================== >>>>
        public IEnumerable<Interactable> OverlapInteractables => _overlapInteractables.Keys;
        public Interactable TargetInteractable => _target;

        protected override void Execute()
        {
            base.Execute();
            RefreshOverlapInteractables();
            RefreshTargetInteractable();
        }

        #region < PROTECTED_METHODS > [[ HANDLE_INTERACTABLES ]] ================================================================
        protected void TryAddInteractable(Interactable interactable)
        {
            if (interactable == null || _overlapInteractables.ContainsKey(interactable))
                return;
            _overlapInteractables.Add(interactable, interactable.name);
        }

        protected void RemoveInteractable(Interactable interactable)
        {
            if (interactable == null || !_overlapInteractables.ContainsKey(interactable))
                return;
            _overlapInteractables.Remove(interactable);
        }

        protected IEnumerable<Interactable> GetOverlapInteractables()
        {
            return Colliders
                .Select(collider => collider.GetComponent<Interactable>())
                .Where(interactable => interactable != null); // Filter out null values
        }

        protected void RefreshOverlapInteractables()
        {
            // Update the interactables dictionary with the overlap interactables.
            IEnumerable<Interactable> overlapInteractables = GetOverlapInteractables();
            _overlapInteractables.Clear();
            foreach (Interactable interactable in overlapInteractables)
            {
                TryAddInteractable(interactable);
            }

            // Reset the target if it is no longer in the overlap interactables.
            if (_target != null && !overlapInteractables.Any(i => i == _target))
            {
                _target.Reset();
                _target = null;
            }

            // Reset the last target if it is no longer in the overlap interactables.
            if (_lastTarget != null && !overlapInteractables.Any(i => i == _lastTarget))
            {
                _lastTarget.Reset();
                _lastTarget = null;
            }

            /*
            // Remove interactables from the dict that are no longer in the overlap interactables.
            List<Interactable> dictInteractables = new List<Interactable>(
                _overlapInteractables.Keys
            );
            List<Interactable> interactablesToRemove = new List<Interactable>();
            foreach (Interactable interactable in dictInteractables)
            {
                // Remove interactables from the dict that are no longer in the overlap interactables.
                if (!overlapInteractables.Contains(interactable))
                {
                    interactablesToRemove.Add(interactable);
                }
            }

            foreach (Interactable interactable in interactablesToRemove)
            {
                RemoveInteractable(interactable);
            }
            */

            //Debug.Log($"[{name}] RefreshOverlapInteractables: {_overlapInteractables.Count}");
        }

        protected Interactable GetClosestReadyInteractable(Vector3 position)
        {
            if (_overlapInteractables.Count == 0)
                return null;
            if (_overlapInteractables.Count == 1)
                return _overlapInteractables.Keys.First();

            Interactable closestInteractable = _overlapInteractables.Keys.First();
            float closestDistance = float.MaxValue;
            foreach (Interactable interactable in _overlapInteractables.Keys)
            {
                if (interactable == null)
                    continue;

                // Calculate the distance to the interactable.
                float distance = Vector3.Distance(interactable.transform.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
            return closestInteractable;
        }

        protected void RefreshTargetInteractable()
        {
            var closestInteractable = GetClosestReadyInteractable(transform.position);
            TryAssignTarget(closestInteractable);
        }

        protected bool TryAssignTarget(Interactable interactable)
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

        protected void ClearTarget()
        {
            _lastTarget = _target;
            _target = null;

            //_lastTarget.Reset();
        }

        protected bool InteractWith(Interactable interactable, bool force = false)
        {
            if (interactable == null && interactable != _lastTarget)
                return false;

            return interactable.AcceptInteraction(this, force);
        }

        #endregion

        public bool TryInteractWithTarget()
        {
            return InteractWith(_target);
        }

        public virtual void TimedDisable()
        {
            Debug.Log($"[{name}] Disable");
            IsDisabled = true;
        }

        public virtual void Enable()
        {
            Debug.Log($"[{name}] Reset");
            ClearTarget();
            IsDisabled = false;
        }
    }
}
