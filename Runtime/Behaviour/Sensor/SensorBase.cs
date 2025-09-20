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
        Dictionary<EdgePoint, bool> _raycastHits = new Dictionary<EdgePoint, bool>();

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


        bool DetectCollidersInSector(SensorDetectionFilter filter, out Collider[] out_colliders)
        {
            List<Collider> colliders = new List<Collider>();
            Config.CalculateSensorPoints(
                transform,
                filter,
                out EdgePoint initialEdgePoint,
                out EdgePoint terminalEdgePoint,
                out List<EdgePoint> shapePoints,
                out List<EdgePoint> sectorPoints
            );

            // << ADD THE INITIAL AND TERMINAL EDGE POINTS >>
            sectorPoints.Add(initialEdgePoint);
            sectorPoints.Add(terminalEdgePoint);

            // (( RAYCAST TO EACH SECTOR POINT )) ------------------------------------------------------------
            _raycastHits.Clear();
            foreach (EdgePoint sectorPoint in sectorPoints)
            {
                RaycastHit sectorHit;
                Physics.Raycast(
                    transform.position,
                    sectorPoint.Position - transform.position,
                    out sectorHit,
                    Vector3.Distance(transform.position, sectorPoint.Position),
                    filter.LayerMask
                );

                // (( Add the collider to the list if it is not already in the list ))
                if (sectorHit.collider != null)
                {
                    if (!colliders.Contains(sectorHit.collider))
                        colliders.Add(sectorHit.collider);
                    _raycastHits[sectorPoint] = true;
                }
                else
                {
                    _raycastHits[sectorPoint] = false;
                }
            }

            out_colliders = colliders.ToArray();
            return out_colliders.Length > 0;
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
            DetectCollidersInSector(filter, out colliders);

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

            // << DRAW DETECTION SECTOR >> ------------------------------------------------------------
            DrawDetectionSector(this, _detectors[0].Filter, _showDebug);
        }

        void DrawLineToTarget(Color gizmoColor, Transform target)
        {
            CustomGizmos.DrawLine(transform.position, target.position, gizmoColor);
            CustomGizmos.DrawSolidCircle(target.position, 0.1f, transform.rotation, gizmoColor);
        }

        /// <summary>
        /// Draws lines from the center to the edge of the sensor shape for the detection sector angles.
        /// </summary>
        /// <param name="gizmoColor">The color for the lines</param>
        /// <param name="position">The center position of the sensor</param>
        /// <param name="rotation">The rotation of the sensor transform</param>
        /// <param name="filter">The sensor detection filter containing angle information</param>
        void DrawDetectionSector(
            SensorBase sensor,
            SensorDetectionFilter filter = null,
            bool showDebug = false
        )
        {
            Vector3 position = sensor.transform.position;
            Quaternion rotation = sensor.transform.rotation;

            // Draw debug points
            if (showDebug)
            {
                // Draw sector points
                foreach (var point in sensor._raycastHits.Keys)
                {
                    Color gizmoColor = sensor._raycastHits[point] ? _collidingColor : _defaultColor;

                    CustomGizmos.DrawSolidCircle(point.Position, 0.025f, rotation, gizmoColor);
                    CustomGizmos.DrawLabel(
                        point.Angle.ToString() + "°",
                        point.Position + Vector3.up * 0.1f,
                        CustomGUIStyles.CenteredStyle
                    );

                    CustomGizmos.DrawLine(position, point.Position, gizmoColor);
                }
            }
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
