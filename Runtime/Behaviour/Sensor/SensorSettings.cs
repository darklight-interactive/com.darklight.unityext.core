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
        [SerializeField]
        [Tooltip("The offset position of the sensor")]
        Vector3 _offsetPosition = Vector3.zero;

        [Header("Dimensions")]
        [SerializeField]
        [Tooltip("The shape of the sensor")]
        SensorShape _shape = SensorShape.BOX;

        [SerializeField, HideIf("IsSphereShape")]
        [Tooltip("The dimensions of the sensor's box")]
        Vector3 _boxDimensions = new Vector3(1, 1, 1);

        [SerializeField, HideIf("IsBoxShape")]
        [Tooltip("Radius of the detection sphere")]
        [Range(0.01f, 25f)]
        float _sphereRadius = 0.2f;

        [SerializeField]
        [Tooltip("Layer mask for ground detection")]
        LayerMask _layerMask;

        [Header("Debug")]
        [SerializeField]
        [Tooltip("Enable gizmo visualization in editor")]
        bool _showDebugGizmos = true;

        [SerializeField, NaughtyAttributes.ShowIf("_showDebugGizmos")]
        [Tooltip("Color of the gizmo when the sensor is colliding")]
        Color _collidingColor = Color.green;

        public Vector3 OffsetPosition => _offsetPosition;
        public SensorShape Shape => _shape;
        public Vector3 BoxDimensions => _boxDimensions;
        public Vector3 BoxHalfExtents => _boxDimensions / 2;
        public float SphereRadius => _sphereRadius;
        public bool IsBoxShape => _shape == SensorShape.BOX;
        public bool IsSphereShape => _shape == SensorShape.SPHERE;
        public LayerMask LayerMask => _layerMask;
        public bool ShowDebugGizmos => _showDebugGizmos;
        public Color DebugCollidingColor => _collidingColor;

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
