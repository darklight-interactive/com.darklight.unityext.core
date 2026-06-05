using System;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.Behaviour
{
    [ExecuteAlways]
    [RequireComponent(typeof(Collider))]
    public class TriggerInteractor : Interactor
    {
        [SerializeField, Required]
        protected new Collider collider;

        void Awake()
        {
            collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (TryGetInteractable(other, out Interactable interactable))
                TrySetTarget(interactable);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (TryGetInteractable(other, out Interactable interactable) && interactable == CurrentTarget)
                ClearTarget();
        }
        
        /*
        public bool TryFindTarget(out Interactable target, out string errMsg)
        {
            target = null;
            errMsg = "";
            
            // << SCAN FOR TARGETS >>
            bool scanResult = _sensor.ExecuteScan(out Sensor.DetectionResult result, out errMsg);
            
            // << CHECK IF THERE IS A TARGET >>
            if (result.Target == null)
            {
                errMsg += "\nTarget is null";
                return false;
            }

            // << CHECK IF THE TARGET IS INTERACTABLE >>
            // If the target is an interactable, ask it to accept the interactor targeting it
            bool hasInteractable = TryGetInteractable(result.Target, out Interactable foundInteractable, out errMsg);
            if (hasInteractable && foundInteractable.AcceptTarget(this))
                CurrentTarget = foundInteractable;
            else
                CurrentTarget = null; // If the target is not interactable, set the current target to null
            
            target = CurrentTarget;
            return scanResult;
        }
        */
    }
}