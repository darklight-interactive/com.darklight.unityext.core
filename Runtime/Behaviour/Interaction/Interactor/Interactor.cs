using System;
using System.Collections.Generic;
using System.Linq;
using Darklight.Behaviour;
using Darklight.Behaviour.Sensor;
using Darklight.Collections;
using Darklight.Editor;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Behaviour
{
    [ExecuteAlways]
    public class Interactor : SensorBase
    {
        Detector _interactableDetector;
        Interactable _lastInteractable;

        [SerializeField]
        SensorDetectionFilter _interactableFilter;

        public Interactable TargetInteractable
        {
            get
            {
                if (_interactableDetector.Result.Target == null)
                    return null;
                return _interactableDetector.Result.Target.GetComponent<Interactable>();
            }
        }

        void Start()
        {
            GetOrAddDetector(_interactableFilter, out _interactableDetector);
        }

        public override bool ExecuteScan(
            SensorDetectionFilter filter,
            out SensorDetectionResult result
        )
        {
            base.ExecuteScan(filter, out result);

            // If the target is an interactable, ask it to accept the interactor targeting it
            if (result.Target.GetComponent<Interactable>() != null)
            {
                if (result.Target.GetComponent<Interactable>().AcceptTarget(this))
                {
                    _lastInteractable = result.Target.GetComponent<Interactable>();
                }
            }
            else if (_lastInteractable != null)
            {
                _lastInteractable.Reset();
                _lastInteractable = null;
            }

            return true;
        }

        protected bool InteractWith(Interactable interactable, bool force = false)
        {
            if (interactable == null)
                return false;

            return interactable.AcceptInteraction(this, force);
        }

        public virtual void InteractWithTarget(out bool result)
        {
            result = false;
            if (_interactableDetector.Result.Target == null)
            {
                //Debug.LogError($"[{name}] InteractWithTarget: Target is null");
                return;
            }

            Interactable interactable =
                _interactableDetector.Result.Target.GetComponent<Interactable>();
            if (interactable == null)
            {
                //Debug.LogError($"[{name}] InteractWithTarget: Interactable is null");
                return;
            }

            result = InteractWith(interactable);
            //Debug.Log($"[{name}] InteractWithTarget: {result}");
        }

        public void InteractWithTarget()
        {
            InteractWithTarget(out bool result);
        }
    }
}
