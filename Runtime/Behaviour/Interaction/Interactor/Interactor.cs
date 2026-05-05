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
        private Interactable _currentTarget;

        /// <summary>
        /// Main reference to the current target 
        /// </summary>
        public Interactable CurrentTarget => _currentTarget;

        public override bool ExecuteScan(
            SensorDetectionFilter filter,
            out DetectionResult result,
            out string debugInfo
        )
        {
            base.ExecuteScan(filter, out result, out debugInfo);

            // << CHECK IF THERE IS A TARGET >>
            if (result.Target == null)
            {
                debugInfo += "\nTarget is null";
                return false;
            }

            // << CHECK IF THE TARGET IS INTERACTABLE >>
            // If the target is an interactable, ask it to accept the interactor targeting it
            Interactable targetInteractable = result.Target.GetComponent<Interactable>();
            if (targetInteractable != null)
            {
                if (targetInteractable.AcceptTarget(this))
                {
                    _currentTarget = result.Target.GetComponent<Interactable>();
                }
            }
            // << RESET THE LAST TARGET IF THE TARGET IS NOT INTERACTABLE >>
            else if (_currentTarget != null)
            {
                _currentTarget.Reset();
                _currentTarget = null;
            }

            return true;
        }

        bool InteractWith(Interactable interactable, bool force = false)
        {
            if (interactable == null)
                return false;

            return interactable.AcceptInteraction(this, force);
        }

        protected virtual bool InteractWithTarget(out string debugInfo)
        {
            debugInfo = "";
            if (_currentTarget == null)
            {
                debugInfo = $"[{name}] InteractWithTarget: Target is null";
                return false;
            }

            bool result = InteractWith(_currentTarget);
            debugInfo = $"[{name}] InteractWithTarget: {result}";
            return result;
        }

        public void InteractWithTarget()
        {
            InteractWithTarget(out _);
        }
    }
}
