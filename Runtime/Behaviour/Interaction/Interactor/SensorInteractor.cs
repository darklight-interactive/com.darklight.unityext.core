using NaughtyAttributes;
using UnityEngine;

namespace Darklight.Behaviour
{
    [ExecuteAlways]
    [RequireComponent(typeof(Sensor))]
    public class SensorInteractor : Interactor
    {
        [SerializeField, Required]
        private Sensor _sensor;

        void Awake()
        {
            _sensor = GetComponent<Sensor>();
        }

        public override void Enable()
        {
            base.Enable();
            _sensor?.Enable();
        }
        
        public override void Disable()
        {
            base.Disable();
            _sensor?.Disable();
        }

        public bool ScanForTarget(out string errMsg)
        {
            errMsg = "";
            
            // << SCAN FOR TARGETS >>
            if (!_sensor.ExecuteScan(out Sensor.DetectionResult result, out errMsg))
            {
                Debug.LogError(errMsg, this);
                return false;
            }

            // << CHECK IF THE TARGET IS INTERACTABLE >>
            // If the target is an interactable, ask it to accept the interactor targeting it
            TryGetInteractable(result.Target, out Interactable foundInteractable);
            return TrySetTarget(foundInteractable, true); // Set the target of the interactor, even if it is null
        }
    }
}