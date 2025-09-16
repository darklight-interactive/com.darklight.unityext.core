using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Darklight.Editor;
using ImprovedTimers;
using NaughtyAttributes;
using UnityEngine;
using UnityUtils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Behaviour.Sensor
{
    [ExecuteAlways]
    public partial class SensorBase : MonoBehaviour
    {
        [SerializeField, ShowOnly]
        bool _isDisabled;

        [HorizontalLine(color: EColor.Gray)]
        [SerializeField, Expandable]
        [CreateAsset("NewSensorSettings", AssetUtility.BEHAVIOUR_FILEPATH + "/SensorConfig")]
        SensorConfig _config;

        [SerializeField]
        List<Detector> _detectors = new List<Detector>();

        [SerializeField, Foldout("Debug")]
        [Tooltip("Enable outline visualization in editor")]
        bool _showOutline = true;

        [SerializeField, Foldout("Debug")]
        [Tooltip("Enable debug visualization in editor")]
        bool _showDebug = false;

        [SerializeField, Foldout("Debug")]
        [Tooltip("Color of the gizmo when the sensor is not colliding")]
        Color _defaultColor = Color.white;

        [SerializeField, Foldout("Debug")]
        [Tooltip("Color of the gizmo when the sensor is colliding")]
        Color _collidingColor = Color.green;

        [SerializeField, Foldout("Debug")]
        [Tooltip("Color of the gizmo when the sensor is not colliding")]
        Color _closestTargetColor = Color.red;

        public SensorConfig Config
        {
            get => _config;
            set => _config = value;
        }

        public string Key => _config.name;

        public bool IsDisabled
        {
            get => _isDisabled;
            protected set => _isDisabled = value;
        }

        bool IsValid => _config != null;

        void Update()
        {
            if (!IsValid)
                return;

            if (!Application.isPlaying && _detectors != null)
            {
                foreach (var detector in _detectors)
                {
                    detector.Execute(this);
                }
            }
        }

        void OnDrawGizmos() => DrawGizmos();

        void OnDrawGizmosSelected() => DrawGizmosSelected();

        IEnumerator DisableRoutine(float duration)
        {
            IsDisabled = true;
            yield return new WaitForSeconds(duration);
            IsDisabled = false;
        }

        void ApplyWhitelist(string[] whitelist, ref Collider[] colliders)
        {
            if (whitelist.Length == 0)
                return;

            colliders = colliders.Where(c => whitelist.Contains(c.tag)).ToArray();
        }

        #region < PRIVATE_METHODS > [[ COLLIDER DETECTION ]] ====================================================================

        /// <summary>
        /// Determines if a point is inside a polygon using the ray casting algorithm.
        /// </summary>
        /// <param name="point">The point to test</param>
        /// <param name="polygonVertices">Array of polygon vertices in order</param>
        /// <returns>True if the point is inside the polygon, false otherwise</returns>
        bool IsPointInPolygon(Vector3 point, Vector3[] polygonVertices)
        {
            bool inside = false;
            int j = polygonVertices.Length - 1;

            for (int i = 0; i < polygonVertices.Length; i++)
            {
                Vector3 vertexI = polygonVertices[i];
                Vector3 vertexJ = polygonVertices[j];

                // Check if ray from point intersects with edge from vertexI to vertexJ
                if (
                    ((vertexI.z > point.z) != (vertexJ.z > point.z))
                    && (
                        point.x
                        < (vertexJ.x - vertexI.x) * (point.z - vertexI.z) / (vertexJ.z - vertexI.z)
                            + vertexI.x
                    )
                )
                {
                    inside = !inside;
                }

                j = i;
            }

            return inside;
        }

        /// <summary>
        /// Determines if a collider is inside the detection sector using the ray casting algorithm.
        /// </summary>
        /// <param name="collider">The collider to test</param>
        /// <param name="transform">The transform of the sensor</param>
        /// <param name="filter">The sensor detection filter containing sector information</param>
        /// <returns>True if the collider is inside the detection sector, false otherwise</returns>
        bool IsColliderInDetectionSector(
            Collider collider,
            Transform transform,
            SensorDetectionFilter filter
        )
        {
            Config.GetDetectionSectorPoints(
                transform,
                filter,
                out Vector3 initEdgePoint,
                out Vector3 terminalEdgePoint,
                out Dictionary<float, Vector3> detectionSectorPoints
            );

            // Convert dictionary to array for easier processing
            Vector3[] polygonVertices = detectionSectorPoints.Values.ToArray();

            // Need at least 3 points to form a polygon
            if (polygonVertices.Length < 3)
                return false;

            // (( INIT EDGE RAYCAST )) ------------------------------------------------------------
            RaycastHit initEdgeHit;
            Physics.Raycast(
                transform.position,
                initEdgePoint - transform.position,
                out initEdgeHit,
                Vector3.Distance(transform.position, initEdgePoint),
                filter.LayerMask
            );
            if (initEdgeHit.collider == collider)
                return true;

            // (( TERMINAL EDGE RAYCAST )) ------------------------------------------------------------
            RaycastHit terminalEdgeHit;
            Physics.Raycast(
                transform.position,
                terminalEdgePoint - transform.position,
                out terminalEdgeHit,
                Vector3.Distance(transform.position, terminalEdgePoint),
                filter.LayerMask
            );
            if (terminalEdgeHit.collider == collider)
                return true;

            // (( POSITION CHECK )) ------------------------------------------------------------
            if (IsPointInPolygon(collider.transform.position, polygonVertices))
                return true;

            // Collider is not in the detection sector
            return false;
        }

        #endregion



        public virtual bool ExecuteScan(
            SensorDetectionFilter filter,
            out SensorDetectionResult result
        )
        {
            result = new SensorDetectionResult();
            Collider target = null;
            Collider[] colliders = new Collider[0];

            // << NULL CHECKS >> ------------------------------------------------------------
            if (Config == null)
                return false;

            if (filter == null)
                return false;

            // << DETECT COLLIDERS IN LAYER MASK >> ------------------------------------------------------------
            if (Config.IsBoxShape)
            {
                colliders = Physics.OverlapBox(
                    transform.position,
                    Config.RectHalfExtents,
                    transform.rotation,
                    filter.LayerMask
                );
            }
            else if (Config.IsSphereShape)
            {
                colliders = Physics.OverlapSphere(
                    transform.position,
                    Config.SphereRadius,
                    filter.LayerMask
                );
            }

            // << FILTER COLLIDERS BY PRIORITY TAGS >> ------------------------------------------------------------
            if (filter.WhitelistTags != null && filter.WhitelistTags.Length > 0)
                ApplyWhitelist(filter.WhitelistTags, ref colliders);

            // << NULL CHECKS >> ------------------------------------------------------------
            if (colliders == null || colliders.Length == 0)
                return false;

            // << FILTER COLLIDERS BY DETECTION SECTOR >> ------------------------------------------------------------
            colliders = colliders
                .Where(c => IsColliderInDetectionSector(c, transform, filter))
                .ToArray();

            // << SET TARGET BASED ON TARGETING TYPE >> ------------------------------------------------------------
            switch (filter.TargetingType)
            {
                case TargetingType.FIRST:
                    target = colliders.First();
                    break;
                case TargetingType.CLOSEST:
                    GetClosest(colliders, out target);
                    break;
            }

            // << SET RESULT >> ------------------------------------------------------------
            result = new SensorDetectionResult(target, colliders);
            return true;
        }

        public void StartTimedDisable(float duration)
        {
            if (IsDisabled)
                return;

            StartCoroutine(DisableRoutine(duration));
        }

        public void Enable() => IsDisabled = false;

        public void Disable() => IsDisabled = true;

        #region < PUBLIC_METHODS > [[ GETTERS ]] ====================================================================

        public void GetOrAddDetector(SensorDetectionFilter filter, out Detector detector)
        {
            detector = _detectors.FirstOrDefault(d => d.Filter == filter);
            if (detector == null)
            {
                detector = new Detector(filter);
                _detectors.Add(detector);
            }
        }

        public void GetClosest(Collider[] colliders, out Collider closest)
        {
            if (colliders.Length == 0)
            {
                closest = null;
                return;
            }

            closest = colliders
                .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude)
                .First();
        }

        #endregion

#if UNITY_EDITOR
        public virtual void DrawGizmos()
        {
            if (Config == null || !_showOutline)
                return;
        }

        public virtual void DrawGizmosSelected()
        {
            if (Config == null || !_showOutline)
                return;

            // << DRAW DEFAULT >> ------------------------------------------------------------
            if (_detectors.Count == 0 || _detectors[0].IsValid == false)
            {
                Config.DrawGizmos(_defaultColor, this);
                return;
            }

            // << DRAW OUTLINE >> ------------------------------------------------------------
            Color outlineColor = _defaultColor;
            if (_detectors[0].Result.HasColliders)
                outlineColor = _collidingColor;

            Config.DrawGizmos(outlineColor, this, _detectors[0].Filter, _showDebug);

            // << DRAW LINE TO TARGET >> ------------------------------------------------------------
            if (_detectors[0].Result.HasTarget)
                DrawLineToTarget(_closestTargetColor, _detectors[0].Result.Target.transform);
        }

        void DrawLineToTarget(Color gizmoColor, Transform target)
        {
            CustomGizmos.DrawLine(transform.position, target.position, gizmoColor);
            CustomGizmos.DrawSolidCircle(target.position, 0.1f, Vector3.up, gizmoColor);
        }
#endif

        [System.Serializable]
        public class Detector
        {
            [SerializeField, Expandable, AllowNesting]
            SensorDetectionFilter _filter;

            [SerializeField, ReadOnly, AllowNesting]
            SensorDetectionResult _result;

            public bool IsValid => _filter != null;
            public SensorDetectionFilter Filter => _filter;
            public SensorDetectionResult Result => _result;

            public Detector(SensorDetectionFilter filter)
            {
                _filter = filter;
                _result = new SensorDetectionResult();
            }

            public void Execute(SensorBase sensor)
            {
                sensor.ExecuteScan(_filter, out _result);
            }
        }
    }
}
