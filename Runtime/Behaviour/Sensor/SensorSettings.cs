using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.Behaviour
{
    /// <summary>
    /// ScriptableObject containing all sensor-related settings for the survivor.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewSensorSettings",
        menuName = "Darklight/Behaviour/SensorSettings"
    )]
    public class SensorSettings : ScriptableObject
    {
        [Header("Transform")]
        [SerializeField]
        [Tooltip("The offset position of the sensor")]
        Vector3 _offsetPosition = Vector3.zero;

        [Header("Dimensions")]
        [SerializeField]
        [Tooltip("The shape of the sensor")]
        Sensor.Shape _shape = Sensor.Shape.BOX;

        [SerializeField, HideIf("IsSphereShape")]
        [Tooltip("The dimensions of the sensor's box")]
        Vector3 _boxDimensions = new Vector3(1, 1, 1);

        [SerializeField, HideIf("IsBoxShape")]
        [Tooltip("Radius of the detection sphere")]
        [Range(0.01f, 25f)]
        float _sphereRadius = 0.2f;

        [Header("Detection")]
        [SerializeField]
        [Tooltip("Layer mask for detection")]
        LayerMask _layerMask;

        [SerializeField, Tag]
        [Tooltip("Tags to filter colliders by")]
        List<string> tagFilter = new();

        [Header("Timer")]
        [SerializeField]
        float _timerInterval = 1f;

        [Header("Debug")]
        [SerializeField]
        [Tooltip("Enable gizmo visualization in editor")]
        bool _showGizmos = true;

        [SerializeField, NaughtyAttributes.ShowIf("_showGizmos")]
        [Tooltip("Color of the gizmo when the sensor is not colliding")]
        Color _defaultColor = Color.grey;

        [SerializeField, NaughtyAttributes.ShowIf("_showGizmos")]
        [Tooltip("Color of the gizmo when the sensor is colliding")]
        Color _collidingColor = Color.green;

        [SerializeField, NaughtyAttributes.ShowIf("_showGizmos")]
        [Tooltip("Color of the gizmo when the sensor is not colliding")]
        Color _closestTargetColor = Color.red;

        public Vector3 OffsetPosition => _offsetPosition;
        public Sensor.Shape Shape => _shape;
        public Vector3 BoxDimensions => _boxDimensions;
        public Vector3 BoxHalfExtents => _boxDimensions / 2;
        public float SphereRadius => _sphereRadius;
        public bool IsBoxShape => _shape == Sensor.Shape.BOX;
        public bool IsSphereShape => _shape == Sensor.Shape.SPHERE;
        public LayerMask LayerMask => _layerMask;
        public List<string> TagFilter => tagFilter;
        public float TimerInterval => _timerInterval;
        public bool ShowDebugGizmos => _showGizmos;
        public Color DebugDefaultColor => _defaultColor;
        public Color DebugCollidingColor => _collidingColor;
        public Color DebugClosestTargetColor => _closestTargetColor;

        /// <summary>
        /// Validates the settings to ensure they are within acceptable ranges.
        /// </summary>
        private void OnValidate()
        {
            _boxDimensions = new Vector3(
                Mathf.Max(0.01f, _boxDimensions.x),
                Mathf.Max(0.01f, _boxDimensions.y),
                Mathf.Max(0.01f, _boxDimensions.z)
            );
            _sphereRadius = Mathf.Max(0.01f, _sphereRadius);
        }
    }
}
