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
        public class DetectionResult
        {
            [SerializeField, ReadOnly]
            SensorDetectionFilter _filter;
            
            [SerializeField, ReadOnly]
            Collider _target;

            [SerializeField, ReadOnly]
            Collider[] _colliders;

            public SensorDetectionFilter Filter => _filter;
            public Collider Target => _target;
            public Collider[] Colliders => _colliders;

            public bool HasTarget => _target != null;
            public bool HasColliders => _colliders != null && _colliders.Length > 0;

            public DetectionResult()
            {
                _filter = null;
                _target = null;
                _colliders = new Collider[0];
            }

            public DetectionResult(SensorDetectionFilter filter, Collider target, Collider[] colliders)
            {
                _filter = filter;
                _target = target;
                _colliders = colliders;
            }

            public string PrintInfo()
            {
                string outInfo = $"Target: {_target?.name}, Colliders: {_colliders?.Length}";
                if (HasColliders)
                {
                    foreach (Collider collider in _colliders)
                        outInfo += $"\n{collider.name}";
                }
                return outInfo;
            }
        }
    }
}
