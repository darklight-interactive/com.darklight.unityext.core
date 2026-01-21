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

namespace Darklight.Behaviour
{
    [ExecuteAlways]
    public partial class Sensor : MonoBehaviour
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

        bool isValid => _config != null;

        public string Key => _config.name;

        public bool IsDisabled
        {
            get => _isDisabled;
            protected set => _isDisabled = value;
        }

        #region < PRIVATE_METHODS > [[ MONOBEHAVIOUR ]] ====================================================================
        void Update()
        {
            if (!isValid)
                return;

            if (_detectors != null)
            {
                foreach (var detector in _detectors)
                {
                    detector.Execute(this);
                }
            }
        }

        void OnDrawGizmos() => DrawGizmos();

        void OnDrawGizmosSelected() => DrawGizmosSelected();
        #endregion

        #region < PRIVATE_METHODS > [[ UTILITIES ]] ====================================================================
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

        #endregion

        #region < PRIVATE_METHODS > [[ COLLIDER DETECTION ]] ====================================================================
        bool DetectCollidersInConfigShape(
            SensorDetectionFilter filter,
            out Collider[] out_colliders
        )
        {
            out_colliders = new Collider[0];

            // << DETECT COLLIDERS IN LAYER MASK >> ------------------------------------------------------------
            if (_config.IsBoxShape)
            {
                out_colliders = Physics.OverlapBox(
                    transform.position,
                    _config.RectHalfExtents,
                    transform.rotation,
                    filter.LayerMask
                );
            }
            else if (_config.IsSphereShape)
            {
                out_colliders = Physics.OverlapSphere(
                    transform.position,
                    _config.SphereRadius,
                    filter.LayerMask
                );
            }

            return out_colliders.Length > 0;
        }

        bool DetectCollidersInSector(SensorDetectionFilter filter, ref Collider[] colliders)
        {
            List<Collider> initColliders = new List<Collider>(colliders);
            List<Collider> sectorColliders = new List<Collider>();

            _config.CalculateSectorAngleEdgePoints(
                transform,
                filter,
                out EdgePoint initialEdgePoint,
                out EdgePoint terminalEdgePoint
            );

            foreach (Collider collider in initColliders)
            {
                if (_config.IsPositionAngleInSector(transform, collider.transform.position, filter))
                    sectorColliders.Add(collider);
            }

            // (( RAYCAST TO THE INITIAL EDGE POINT )) ------------------------------------------------------------
            RaycastHit initialEdgeHit;
            Physics.Raycast(
                transform.position,
                initialEdgePoint.Position - transform.position,
                out initialEdgeHit,
                Vector3.Distance(transform.position, initialEdgePoint.Position),
                filter.LayerMask
            );

            // If the initial edge point is hit, add the collider to the sector colliders
            if (
                initialEdgeHit.collider != null
                && !sectorColliders.Contains(initialEdgeHit.collider)
            )
                sectorColliders.Add(initialEdgeHit.collider);

            // (( RAYCAST TO THE TERMINAL EDGE POINT )) ------------------------------------------------------------
            RaycastHit terminalEdgeHit;
            Physics.Raycast(
                transform.position,
                terminalEdgePoint.Position - transform.position,
                out terminalEdgeHit,
                Vector3.Distance(transform.position, terminalEdgePoint.Position),
                filter.LayerMask
            );

            // If the terminal edge point is hit, add the collider to the sector colliders
            if (
                terminalEdgeHit.collider != null
                && !sectorColliders.Contains(terminalEdgeHit.collider)
            )
                sectorColliders.Add(terminalEdgeHit.collider);

            colliders = sectorColliders.ToArray();
            return colliders.Length > 0;
        }
        #endregion

        #region < PUBLIC_METHODS > [[ SCAN ]] ====================================================================
        /// <summary>
        /// Executes the scan for the sensor based on the
        /// </summary>
        /// <param name="filter"> The filter to use for the scan </param>
        /// <param name="result"> The result of the scan </param>
        /// <returns> True if the scan was successful, false otherwise </returns>
        public virtual bool ExecuteScan(
            SensorDetectionFilter filter,
            out DetectionResult result,
            out string debugInfo
        )
        {
            debugInfo = "";
            result = new DetectionResult();
            Collider target = null;
            Collider[] colliders = new Collider[0];

            // << NULL CHECKS >> ------------------------------------------------------------
            if (_config == null)
            {
                debugInfo = "Config is null";
                return false;
            }

            if (filter == null)
            {
                debugInfo = "Filter is null";
                return false;
            }

            // << DETECT COLLIDERS IN CONFIG SHAPE >> ------------------------------------------------------------
            if (DetectCollidersInConfigShape(filter, out colliders))
                debugInfo += $"\nDetected Colliders in Config Shape: {colliders.Length}";
            else
            {
                debugInfo += $"\nNo Colliders detected in Config Shape";
                return false;
            }

            // << FILTER COLLIDERS BY PRIORITY TAGS >> ------------------------------------------------------------
            if (filter.WhitelistTags != null && filter.WhitelistTags.Length > 0)
                ApplyWhitelist(filter.WhitelistTags, ref colliders);

            debugInfo += $"\nFiltered Result by Whitelist Tags: {colliders.Length}";

            // << FILTER COLLIDERS BY DETECTION SECTOR >> ------------------------------------------------------------
            if (filter.SectorType != SectorType.FULL)
                DetectCollidersInSector(filter, ref colliders);

            debugInfo += $"\nFiltered Result by Detection Sector: {colliders.Length}";

            // << REMOVE COLLIDER ON OBJECT >> ------------------------------------------------------------
            colliders = colliders.Where(c => c.gameObject != gameObject).ToArray();

            debugInfo += $"\nFiltered Result by GameObject: {colliders.Length}";

            // << NULL CHECKS >> ------------------------------------------------------------
            if (colliders == null || colliders.Length == 0)
            {
                debugInfo += $"\nNo Colliders found";
                return false;
            }

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
            result = new DetectionResult(target, colliders);
            return true;
        }
        #endregion

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

        #region < PUBLIC_METHODS > [[ SETTERS ]] ====================================================================
        public void StartTimedDisable(float duration)
        {
            if (IsDisabled)
                return;

            StartCoroutine(DisableRoutine(duration));
        }

        public void Enable() => IsDisabled = false;

        public void Disable() => IsDisabled = true;

        #endregion

        #region < PRIVATE_METHODS > [[ GIZMOS ]] ====================================================================
#if UNITY_EDITOR
        protected virtual void DrawGizmos()
        {
            if (_config == null || !_showOutline)
                return;
        }

        protected virtual void DrawGizmosSelected()
        {
            if (_config == null || !_showOutline)
                return;

            // << DRAW DEFAULT >> ------------------------------------------------------------
            if (_detectors.Count == 0 || _detectors[0].IsValid == false)
            {
                _config.DrawOutlineGizmos(transform, _defaultColor);
                return;
            }

            // << DRAW OUTLINE >> ------------------------------------------------------------
            Color outlineColor = _defaultColor;
            if (_detectors[0].Result.HasColliders)
                outlineColor = _collidingColor;

            _config.DrawOutlineGizmos(transform, outlineColor);

            // << DRAW LINE TO TARGET >> ------------------------------------------------------------
            if (_detectors[0].Result.HasTarget)
                DrawLineToTarget(_closestTargetColor, _detectors[0].Result.Target.transform);

            // << DRAW DETECTION SECTOR >> ------------------------------------------------------------
            DrawDetectionSector(transform, _detectors[0], _showDebug);
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
        void DrawDetectionSector(Transform transform, Detector detector, bool showDebug = false)
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            SensorDetectionFilter filter = detector.Filter;

            Color gizmoColor = _defaultColor;
            if (detector.Result.HasColliders)
                gizmoColor = _collidingColor;

            // << DRAW SECTOR POLYGON >> ------------------------------------------------------------
            if (!filter.IsFullSectorType)
            {
                DrawSectorPolygon(filter, gizmoColor);
            }

            // << DRAW SECTOR RAYCASTS >> ------------------------------------------------------------
            if (showDebug)
            {
                _config.CalculateSectorAngleEdgePoints(
                    transform,
                    filter,
                    out EdgePoint initialEdgePoint,
                    out EdgePoint terminalEdgePoint
                );
                DrawSectorEdgePointRay(transform, initialEdgePoint, gizmoColor);
                DrawSectorEdgePointRay(transform, terminalEdgePoint, gizmoColor);
            }
        }

        void DrawSectorPolygon(SensorDetectionFilter filter, Color color)
        {
            _config.CalculateSectorEdgePoints(
                transform,
                filter,
                out List<EdgePoint> sectorEdgePoints
            );

            // << DRAW SECTOR POLYGON >> ------------------------------------------------------------
            sectorEdgePoints = sectorEdgePoints.OrderBy(k => k.Angle).ToList();

            // Check if any of the edge points are more than the initial angle and less than the terminal angle
            // If not, then we need to seperate the edge points into two lists, one for the smaller angle and one for the larger angle
            if (
                sectorEdgePoints.Any(k =>
                    IsAngleInBetween(k.Angle, filter.InitialAngle, filter.TerminalAngle)
                )
            )
            {
                //Debug.Log("All Edge points are in between the initial and terminal angles");
            }
            else
            {
                // Add 360 to all values less than the smaller angle
                float smallerAngle = Mathf.Min(filter.InitialAngle, filter.TerminalAngle);

                // Sort the edge points by angle
                List<EdgePoint> smallerAngleEdgePoints = new List<EdgePoint>();
                List<EdgePoint> largerAngleEdgePoints = new List<EdgePoint>();
                foreach (var point in sectorEdgePoints)
                {
                    if (point.Angle <= smallerAngle)
                        smallerAngleEdgePoints.Add(point);
                    else
                        largerAngleEdgePoints.Add(point);
                }

                // Resort the edge points by angle
                smallerAngleEdgePoints = smallerAngleEdgePoints.OrderBy(k => k.Angle).ToList();
                largerAngleEdgePoints = largerAngleEdgePoints.OrderBy(k => k.Angle).ToList();

                // Add them together, larger angle first
                sectorEdgePoints = new List<EdgePoint>();
                sectorEdgePoints.AddRange(largerAngleEdgePoints);
                sectorEdgePoints.AddRange(smallerAngleEdgePoints);
            }

            // Add the center point
            sectorEdgePoints.Add(new EdgePoint(-1, transform.position));

            // Draw the polygon
            CustomGizmos.DrawPolygon(
                sectorEdgePoints.Select(k => k.Position).ToArray(),
                color,
                0.2f
            );

            //DebugEdgePoints(edgePoints);
        }

        void DrawSectorEdgePointRay(Transform transform, EdgePoint point, Color color)
        {
            CustomGizmos.DrawSolidCircle(point.Position, 0.025f, transform.rotation, color);
            CustomGizmos.DrawLabel(
                point.Angle.ToString() + "°",
                point.Position + Vector3.up * 0.1f,
                CustomGUIStyles.CenteredStyle
            );

            CustomGizmos.DrawLine(transform.position, point.Position, color);
        }

        bool IsAngleInBetween(float angle, float initialAngle, float terminalAngle)
        {
            if (initialAngle < angle && angle < terminalAngle)
                return true;
            else if (initialAngle > angle && angle > terminalAngle)
                return true;
            else
                return false;
        }

        void DebugEdgePoints(List<EdgePoint> edgePoints)
        {
            string debugString = "";
            foreach (var point in edgePoints)
            {
                debugString += point.Angle.ToString() + "°, ";
            }
            Debug.Log(debugString);
        }
#endif
        #endregion

        #region < PUBLIC_ENUMS > ====================================================================

        public enum Shape
        {
            RECT2D,
            BOX3D,
            CIRCLE2D,
            SPHERE3D
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

        public struct EdgePoint
        {
            float _angle;
            Vector3 _position;
            public float Angle
            {
                get => _angle;
                set
                {
                    if (value < 0f)
                        _angle = 360f + value;
                    if (value > 360f)
                        _angle = value - 360f;
                    else
                        _angle = value;
                }
            }

            public Vector3 Position
            {
                get => _position;
                set => _position = value;
            }

            public EdgePoint(float angle, Vector3 position)
            {
                _angle = angle;
                _position = position;
            }
        }

        #endregion
    }
}
