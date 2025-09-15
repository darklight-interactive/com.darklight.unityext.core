using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.Behaviour.Sensor
{
    [System.Serializable]
    public class ScanResult
    {
        [SerializeField, ReadOnly]
        Collider _target;

        [SerializeField, ReadOnly]
        List<Collider> _colliders;

        public Collider Target => _target;
        public List<Collider> Colliders => _colliders;

        public bool HasTarget => _target != null;
        public bool HasColliders => _colliders != null && _colliders.Count > 0;

        public ScanResult()
        {
            _target = null;
            _colliders = new List<Collider>();
        }

        public ScanResult(Collider target, List<Collider> colliders)
        {
            _target = target;
            _colliders = colliders;
        }
    }
}
