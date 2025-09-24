using UnityEngine;
using NaughtyAttributes;

using static Darklight.Behaviour.Sensor;

namespace Darklight.Behaviour
{

    [CreateAssetMenu(fileName = "NewScanFilter", menuName = "Darklight/Behaviour/ScanFilter")]
    public class SensorDetectionFilter : ScriptableObject
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
}