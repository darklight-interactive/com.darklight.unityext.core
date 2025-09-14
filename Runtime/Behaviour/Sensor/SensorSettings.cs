using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.Behaviour
{
    public partial class Sensor
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

        /// <summary>
        /// A class that assigns a priority to a tag
        /// </summary>
        [System.Serializable]
        public class PriorityTag
        {
            [SerializeField, Tag]
            string _tag;

            [SerializeField, Range(0, 1)]
            float _priority;

            public string Tag => _tag;
            public float Priority => _priority;
        }

        /// <summary>
        /// A class that contains a list of priority tags and methods to compare them
        /// </summary>
        [System.Serializable]
        public class PriorityTagComparator
        {
            [SerializeField]
            List<PriorityTag> _priorityTags = new();

            public List<PriorityTag> PriorityTags => _priorityTags;

            /// <summary>
            /// Get the highest priority tag from the list of colliders
            /// </summary>
            /// <param name="colliders">The list of colliders to check</param>
            /// <param name="highestPriorityTag">The highest priority tag found</param>
            public void GetHighestPriorityTag(
                List<Collider> colliders,
                out string highestPriorityTag
            )
            {
                highestPriorityTag = string.Empty;
                float highestPriority = -1;

                // Iterate through each priority tag and check if any colliders have that tag
                foreach (var priorityTag in PriorityTags)
                {
                    bool hasTag = colliders.Any(c => c.CompareTag(priorityTag.Tag));
                    // If the collider has the tag and the priority is higher than the current highest priority, update the highest priority and tag
                    if (hasTag && priorityTag.Priority > highestPriority)
                    {
                        highestPriority = priorityTag.Priority;
                        highestPriorityTag = priorityTag.Tag;
                    }
                }
            }

            /// <summary>
            /// Get the colliders with the highest priority tag from the list of colliders
            /// </summary>
            /// <param name="colliders">The list of colliders to check</param>
            /// <param name="collidersWithHighestPriority">The list of colliders with the highest priority tag</param>
            public void GetCollidersWithHighestPriority(
                List<Collider> colliders,
                out List<Collider> collidersWithHighestPriority
            )
            {
                collidersWithHighestPriority = new();
                GetHighestPriorityTag(colliders, out string highestPriorityTag);

                // If the highest priority tag is not empty, filter the colliders to only include those with the highest priority tag
                if (!string.IsNullOrEmpty(highestPriorityTag))
                {
                    collidersWithHighestPriority = colliders
                        .Where(c => c.CompareTag(highestPriorityTag))
                        .ToList();
                }
            }
        }
    }

    /// <summary>
    /// ScriptableObject containing all sensor-related settings for the survivor.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewSensorSettings",
        menuName = "Darklight/Behaviour/SensorSettings"
    )]
    public class SensorSettings : ScriptableObject
    {
        [Header("Dimensions")]
        [SerializeField]
        [Tooltip("The shape of the sensor")]
        Sensor.Shape _shape = Sensor.Shape.BOX;

        [SerializeField, HideIf("IsSphereShape")]
        [Tooltip("The dimensions of the sensor's box")]
        Vector3 _boxDimensions = new Vector3(1, 1, 1);

        [SerializeField, HideIf("IsBoxShape")]
        [Tooltip("Radius of the detection sphere")]
        [Range(0.01f, 100f)]
        float _sphereRadius = 0.2f;

        [Header("Detection")]
        [SerializeField]
        [Tooltip("Layer mask for detection")]
        LayerMask _layerMask;

        [SerializeField]
        [Tooltip("Targeting type for the sensor")]
        Sensor.TargetingType _targetingType = Sensor.TargetingType.FIRST;

        [SerializeField]
        [Tooltip("Targeting priorities for the sensor")]
        Sensor.PriorityTagComparator _priorityTagComparator = new();

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

        public Sensor.Shape Shape => _shape;
        public Vector3 BoxDimensions => _boxDimensions;
        public Vector3 BoxHalfExtents => _boxDimensions / 2;
        public float SphereRadius => _sphereRadius;
        public bool IsBoxShape => _shape == Sensor.Shape.BOX;
        public bool IsSphereShape => _shape == Sensor.Shape.SPHERE;
        public LayerMask LayerMask => _layerMask;
        public Sensor.TargetingType TargetingType => _targetingType;
        public Sensor.PriorityTagComparator PriorityTagComparator => _priorityTagComparator;
        public float TimerInterval => _timerInterval;
        public bool ShowDebugGizmos => _showGizmos;
        public Color DebugDefaultColor => _defaultColor;
        public Color DebugCollidingColor => _collidingColor;
        public Color DebugTargetColor => _closestTargetColor;

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
