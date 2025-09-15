using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.Behaviour.Sensor
{
    [System.Serializable]
    public class SensorDetectionResult
    {
        [SerializeField, ReadOnly]
        Collider _target;

        [SerializeField, ReadOnly]
        Collider[] _colliders;

        public Collider Target => _target;
        public Collider[] Colliders => _colliders;

        public bool HasTarget => _target != null;
        public bool HasColliders => _colliders != null && _colliders.Length > 0;

        public SensorDetectionResult()
        {
            _target = null;
            _colliders = new Collider[0];
        }

        public SensorDetectionResult(Collider target, Collider[] colliders)
        {
            _target = target;
            _colliders = colliders;
        }
    }
}
