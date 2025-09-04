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
        Interactable _lastInteractable;

        public Interactable TargetInteractable
        {
            get
            {
                if (Target == null)
                    return null;
                return Target.GetComponent<Interactable>();
            }
        }

        protected override void UpdateTarget()
        {
            base.UpdateTarget();

            // If the target is an interactable, ask it to accept the interactor targeting it
            if (TargetInteractable != null)
            {
                bool result = TargetInteractable.AcceptTarget(this);
                if (result)
                {
                    _lastInteractable = TargetInteractable;
                }
            }
            else if (_lastInteractable != null)
            {
                _lastInteractable.Reset();
                _lastInteractable = null;
            }
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
            if (Target == null)
            {
                //Debug.LogError($"[{name}] InteractWithTarget: Target is null");
                return;
            }

            Interactable interactable = Target.GetComponent<Interactable>();
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
