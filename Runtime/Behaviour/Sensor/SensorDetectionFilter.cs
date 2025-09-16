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

    public enum SectorType
    {
        FULL,
        SMALL_ANGLE,
        LARGE_ANGLE
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

        [Header("Detection Sector")]
        [SerializeField]
        SectorType _sectorType = SectorType.FULL;

        [SerializeField, Range(-360f, 360f)]
        float _initialAngle = 0f;

        [SerializeField, Range(-360f, 360f)]
        float _terminalAngle = 360f;

        [SerializeField, Tag]
        string[] _whitelistTags;

        public LayerMask LayerMask => _layerMask;
        public Sensor.TargetingType TargetingType => _targetingType;
        public SectorType SectorType => _sectorType;
        public Vector2 DetectionSector
        {
            get
            {
                float out_initialAngle = _initialAngle;
                float out_terminalAngle = _terminalAngle;
                if (_initialAngle < 0f)
                {
                    out_initialAngle = 360f + _initialAngle;
                }
                if (_terminalAngle < 0f)
                {
                    out_terminalAngle = 360f + _terminalAngle;
                }
                return new Vector2(out_initialAngle, out_terminalAngle);
            }
        }
        public string[] WhitelistTags => _whitelistTags;
    }
}
