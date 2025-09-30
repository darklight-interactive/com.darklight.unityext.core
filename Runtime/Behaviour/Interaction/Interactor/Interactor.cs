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
    [ExecuteAlways]
    public class Interactor : Sensor
    {
        protected Detector detector;
        protected Interactable lastInteractable;

        [SerializeField]
        protected SensorDetectionFilter interactableFilter;

        public Interactable TargetInteractable
        {
            get
            {
                if (detector.Result.Target == null)
                    return null;
                return detector.Result.Target.GetComponent<Interactable>();
            }
        }

        void Start()
        {
            GetOrAddDetector(interactableFilter, out detector);
        }

        public override bool ExecuteScan(
            SensorDetectionFilter filter,
            out DetectionResult result,
            out string debugInfo
        )
        {
            base.ExecuteScan(filter, out result, out debugInfo);

            if (result.Target == null)
            {
                debugInfo += "\nTarget is null";
                return false;
            }

            // If the target is an interactable, ask it to accept the interactor targeting it
            if (result.Target.GetComponent<Interactable>() != null)
            {
                if (result.Target.GetComponent<Interactable>().AcceptTarget(this))
                {
                    lastInteractable = result.Target.GetComponent<Interactable>();
                }
            }
            else if (lastInteractable != null)
            {
                lastInteractable.Reset();
                lastInteractable = null;
            }

            return true;
        }

        protected bool InteractWith(Interactable interactable, bool force = false)
        {
            if (interactable == null)
                return false;

            return interactable.AcceptInteraction(this, force);
        }

        public virtual bool InteractWithTarget(out string debugInfo)
        {
            debugInfo = "";
            if (detector.Result.Target == null)
            {
                debugInfo = $"[{name}] InteractWithTarget: Target is null";
                return false;
            }

            Interactable interactable = detector.Result.Target.GetComponent<Interactable>();
            if (interactable == null)
            {
                debugInfo = $"[{name}] InteractWithTarget: Interactable is null";
                return false;
            }

            bool result = InteractWith(interactable);
            debugInfo = $"[{name}] InteractWithTarget: {result}";
            return result;
        }

        public void InteractWithTarget()
        {
            InteractWithTarget(out _);
        }
    }
}
