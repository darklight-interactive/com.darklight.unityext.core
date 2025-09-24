using System.Collections.Generic;
using Darklight.Editor;
using Darklight.Utility;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.Behaviour
{
    public partial class Sensor
    {
        [System.Serializable]
        public class Detector
        {
            [SerializeField, Expandable, AllowNesting]
            [CreateAsset(
                "NewDetectionFilter",
                AssetUtility.BEHAVIOUR_FILEPATH + "/DetectionFilter"
            )]
            SensorDetectionFilter _filter;

            [SerializeField, ReadOnly, AllowNesting]
            DetectionResult _result;

            public bool IsValid => _filter != null;
            public SensorDetectionFilter Filter => _filter;
            public DetectionResult Result => _result;

            public Detector(SensorDetectionFilter filter)
            {
                _filter = filter;
                _result = new DetectionResult();
            }

            public void Execute(Sensor sensor)
            {
                sensor.ExecuteScan(_filter, out _result);
            }
        }



        [System.Serializable]
        public class DetectionResult
        {
            [SerializeField, ReadOnly]
            Collider _target;

            [SerializeField, ReadOnly]
            Collider[] _colliders;

            public Collider Target => _target;
            public Collider[] Colliders => _colliders;

            public bool HasTarget => _target != null;
            public bool HasColliders => _colliders != null && _colliders.Length > 0;

            public DetectionResult()
            {
                _target = null;
                _colliders = new Collider[0];
            }

            public DetectionResult(Collider target, Collider[] colliders)
            {
                _target = target;
                _colliders = colliders;
            }
        }
    }
}
