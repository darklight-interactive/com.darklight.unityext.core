using System.Collections.Generic;
using Darklight.Utility;
using UnityEngine;

namespace Darklight.Behaviour.Sensor
{
    public enum TargetingType
    {
        /// <summary>
        /// Target the first collider found, until it is no longer detected
        /// </summary>
        FIRST,

        /// <summary>
        /// Target the closest collider found, until another collider is determined to be closer
        /// </summary>
        CLOSEST
    }

    [CreateAssetMenu(fileName = "NewScanFilter", menuName = "Darklight/Behaviour/ScanFilter")]
    public class ScanFilter : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Layer mask for detection")]
        LayerMask _layerMask;

        [SerializeField]
        [Tooltip("Targeting type for the sensor")]
        Sensor.TargetingType _targetingType = Sensor.TargetingType.FIRST;

        [SerializeField]
        [Tooltip("Targeting priorities for the sensor")]
        PriorityTag.Comparator _priorityTagComparator = new();

        public LayerMask LayerMask => _layerMask;
        public Sensor.TargetingType TargetingType => _targetingType;
        public PriorityTag.Comparator PriorityTagComparator => _priorityTagComparator;
    }
}
