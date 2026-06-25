using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Darklight.Editor;
using Darklight.Utility;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Behaviour
{
    /// <summary>
    /// Represents a component responsible for detecting and interacting with entities
    /// or objects within a defined area or volume in the game world.
    /// The Sensor class supports various detection shapes and targeting mechanisms
    /// and provides functionality for enabling, disabling, and performing detection logic.
    /// <br/>
    /// NOTE: This component does NOT perform any actions on its own. It requires
    /// additional scripts or components to use the ExecuteScan() method to perform
    /// the actual detection and interaction logic.
    /// </summary>
    [ExecuteAlways]
    public partial class Sensor : MonoBehaviour
    {
        private readonly Color _defaultColor = Color.white;
        private readonly Color _collidingColor = Color.green;
        private readonly Color _closestTargetColor = Color.red;

        private string _uuid;

        [SerializeField, ReadOnly]
        bool _isDisabled;
        
        [Tooltip("Enable debug visualization in editor")]
        [SerializeField]
        bool _debugMode = false;
        
        [Tooltip("The time since the sensor was last updated")]
        [SerializeField, ReadOnly]
        DetectionResult _lastResult;
        
        /// <summary>
        /// The configuration for the sensor.
        /// </summary>
        [Header("Configuration")]
        [SerializeField, Expandable]
        [CreateAsset("NewSensorSettings", AssetUtility.BEHAVIOUR_FILEPATH + "/SensorConfig")]
        SensorConfig _config;
        
        /// <summary>
        /// The base detection filter for the sensor.
        /// </summary>
        [SerializeField, Expandable]
        [CreateAsset("NewDetectionFilter", AssetUtility.BEHAVIOUR_FILEPATH + "/SensorDetectionFilter")]
        SensorFilter _filter;
        
        /// <summary>
        /// Confirm that the config is not null
        /// </summary>
        public bool isValid => _config != null;

        public SensorConfig Config
        {
            get => _config;
            set
            {
                if (value == null)
                    return;
                
                _config = value;
            }
        }

        public SensorFilter Filter
        {
            get => _filter;
            set
            {
                if (value == null)
                    return;
                
                _filter = value;
            }
        }
        
        /// <summary>
        /// The unique identifier for the sensor.
        /// </summary>
        public string UUID => _uuid;
        
        /// <summary>
        /// Check if the sensor is disabled.
        /// </summary>
        public bool IsDisabled
        {
            get => _isDisabled;
            protected set => _isDisabled = value;
        }
        
        /// <summary>
        /// Get the last detection result for the sensor.
        /// </summary>
        public DetectionResult LastResult => _lastResult;

        #region < PRIVATE_METHODS > [[ MONOBEHAVIOUR ]] ====================================================================
        void Awake() => EnsureUUID();

#if UNITY_EDITOR
        void OnValidate() => EnsureUUID();
#endif

        void EnsureUUID()
        {
            if (!string.IsNullOrWhiteSpace(_uuid))
                return;

            _uuid = Guid.NewGuid().ToString("N");
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        void OnDrawGizmos()
        {
            DrawGizmos();
        }
        
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
            SensorFilter filter,
            out Collider[] outColliders
        )
        {
            outColliders = new Collider[0];

            // << DETECT COLLIDERS IN LAYER MASK >> ------------------------------------------------------------
            if (_config.IsBoxShape)
            {
                outColliders = Physics.OverlapBox(
                    transform.position,
                    _config.RectHalfExtents,
                    transform.rotation,
                    filter.LayerMask
                );
            }
            else if (_config.IsSphereShape)
            {
                outColliders = Physics.OverlapSphere(
                    transform.position,
                    _config.SphereRadius,
                    filter.LayerMask
                );
            }

            return outColliders.Length > 0;
        }

        bool DetectCollidersInSector(SensorFilter filter, ref Collider[] colliders)
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
            SensorFilter filter,
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
                debugInfo += $"Detected Colliders in Config Shape: {colliders.Length}";
            else
            {
                debugInfo = $"No Colliders detected in Config Shape";
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
            result = new DetectionResult(filter, target, colliders);
            _lastResult = result;
            return true;
        }
        
        
        /// <summary>
        /// An expanded version of the ExecuteScan method that allows for the specification of a SensorConfig.
        /// </summary>
        /// <param name="config">
        /// The SensorConfig to use for the scan.
        /// </param>
        /// <returns></returns>
        public bool ExecuteScan(SensorConfig config, SensorFilter filter, out DetectionResult result,
            out string debugInfo)
        {
            // Base Checks
            if (config == null || filter == null)
            {
                result = default;
                debugInfo = "SensorConfig or DetectionFilter cannot be null";
                return false;
            }
            
            _config = config;
            _filter = filter;
            
            return ExecuteScan(filter, out result, out debugInfo);
        }

        /// <summary>
        /// Executes the default scan using the preconfigured filter.
        /// </summary>
        /// <param name="result">The result of the default scan operation.</param>
        /// <param name="debugInfo">Additional debug information generated during the scan.</param>
        /// <returns>True if the scan was executed successfully, false otherwise.</returns>
        public bool ExecuteScan(out DetectionResult result, out string debugInfo)
        {
            return ExecuteScan(_filter, out result, out debugInfo);
        }
        #endregion

        #region < PUBLIC_METHODS > [[ GETTERS ]] ====================================================================

        /// <summary>
        /// Finds and returns the closest collider to the sensor's position from the provided array of colliders.
        /// </summary>
        /// <param name="colliders">An array of colliders to search through.</param>
        /// <param name="closest">The collider closest to the sensor's position, or null if no colliders are provided.</param>
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

        public void SetConfig(SensorConfig config)
        {
            if (config == null)
                return;
            
            _config = config;
        }
        
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
        protected virtual void DrawGizmos()
        {
            if (!isValid || IsDisabled || !_debugMode)
                return;

            if (!Application.isPlaying)
            {
                ExecuteScan(_filter, out _lastResult, out string debugInfo);

            }
            
                        
            // << DRAW OUTLINE >> ------------------------------------------------------------
            Color outlineColor = _defaultColor;
            if (_lastResult.HasColliders)
                outlineColor = _collidingColor;
            _config.DrawOutlineGizmos(transform, outlineColor);

            // << DRAW LINE TO TARGET >> ------------------------------------------------------------
            if (_lastResult.HasTarget)
                DrawLineToTarget(_closestTargetColor, _lastResult.Target.transform);

            DrawDetectionSector(transform, _lastResult, true);
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
        /// <param name="result">The sensor detection filter containing angle information</param>
        void DrawDetectionSector(Transform transform, DetectionResult result, bool showDebug = false)
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            SensorFilter filter = result.Filter;
            
            Color gizmoColor = _defaultColor;
            if (result.HasColliders)
                gizmoColor = _collidingColor;

            // << DRAW SECTOR POLYGON >> ------------------------------------------------------------
            if (filter != null && !filter.IsFullSectorType)
            {
                DrawSectorPolygon(filter, gizmoColor);
                
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

        void DrawSectorPolygon(SensorFilter filter, Color color)
        {
            _config.CalculateSectorEdgePoints(
                transform,
                filter,
                out List<EdgePoint> sectorEdgePoints
            );

            // << DRAW SECTOR POLYGON >> ------------------------------------------------------------
            sectorEdgePoints = sectorEdgePoints.OrderBy(k => k.Angle).ToList();

            // Check if any of the edge points are more than the initial angle and less than the terminal angle
            // If not, then we need to separate the edge points into two lists, one for the smaller angle and one for the larger angle
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

        /// <summary>
        /// Represents a point on the edge of a detection sector in 3D space.
        /// This structure is used to define the angular and positional characteristics
        /// of edge points that denote the boundaries of a sector-shaped detection area.
        /// </summary>
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
