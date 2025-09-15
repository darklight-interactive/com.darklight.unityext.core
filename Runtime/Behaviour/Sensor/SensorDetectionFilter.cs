using System.Collections.Generic;
using Darklight.Utility;
using NaughtyAttributes;
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
    public class SensorDetectionFilter : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Layer mask for detection")]
        LayerMask _layerMask;

        [SerializeField]
        [Tooltip("Targeting type for the sensor")]
        Sensor.TargetingType _targetingType = Sensor.TargetingType.FIRST;

        [SerializeField, Tag]
        string[] _whitelistTags;

        public LayerMask LayerMask => _layerMask;
        public Sensor.TargetingType TargetingType => _targetingType;
        public string[] WhitelistTags => _whitelistTags;
    }
}
