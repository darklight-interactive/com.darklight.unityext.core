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
            DetectionFilter _filter;

            [SerializeField, ReadOnly, AllowNesting]
            DetectionResult _result;

            public bool IsValid => _filter != null;
            public DetectionFilter Filter => _filter;
            public DetectionResult Result => _result;

            public Detector(DetectionFilter filter)
            {
                _filter = filter;
                _result = new DetectionResult();
            }

            public void Execute(Sensor sensor)
            {
                sensor.ExecuteScan(_filter, out _result);
            }
        }

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
        public class DetectionFilter : ScriptableObject
        {
            [SerializeField]
            [Tooltip("Layer mask for detection")]
            LayerMask _layerMask;

            [SerializeField]
            [Tooltip("Targeting type for the sensor")]
            TargetingType _targetingType = TargetingType.FIRST;

            [Header("Detection Sector")]
            [SerializeField]
            SectorType _sectorType = SectorType.FULL;

            [SerializeField, Range(0f, 360f)]
            [HideIf("IsFullSectorType")]
            float _initialAngle = 0f;

            [SerializeField, Range(0f, 360f)]
            [HideIf("IsFullSectorType")]
            float _terminalAngle = 360f;

            [SerializeField, Tag]
            string[] _whitelistTags;

            public LayerMask LayerMask => _layerMask;
            public TargetingType TargetingType => _targetingType;
            public SectorType SectorType => _sectorType;
            public bool IsFullSectorType => _sectorType == SectorType.FULL;
            public float InitialAngle => _initialAngle < 0f ? 360f + _initialAngle : _initialAngle;
            public float TerminalAngle =>
                _terminalAngle < 0f ? 360f + _terminalAngle : _terminalAngle;
            public string[] WhitelistTags => _whitelistTags;
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
