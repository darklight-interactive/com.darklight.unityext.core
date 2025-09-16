using System;
using System.Collections.Generic;
using System.Linq;
using Darklight.Editor;
using Darklight.Utility;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Behaviour.Sensor
{
    public enum Shape
    {
        RECT2D,
        BOX3D,
        CIRCLE2D,
        SPHERE3D
    }

    /// <summary>
    /// ScriptableObject containing all sensor-related settings for the survivor.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewSensorSettings",
        menuName = "Darklight/Behaviour/SensorSettings"
    )]
    public class SensorConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The shape of the sensor")]
        Shape _shape = Shape.CIRCLE2D;

        [SerializeField, ShowIf("IsRect"), AllowNesting]
        Vector3 _rectDimensions = new Vector3(1, 1, 1);

        [SerializeField, ShowIf("IsCircle"), AllowNesting]
        [Range(0.01f, 100f)]
        float _radius = 0.2f;

        bool IsRect => _shape == Shape.RECT2D || _shape == Shape.BOX3D;
        bool IsCircle => _shape == Shape.CIRCLE2D || _shape == Shape.SPHERE3D;

        public Shape Shape => _shape;
        public bool IsBoxShape => _shape == Shape.BOX3D || _shape == Shape.RECT2D;
        public bool IsSphereShape => _shape == Shape.SPHERE3D || _shape == Shape.CIRCLE2D;
        public Vector3 RectHalfExtents => _rectDimensions / 2;
        public float SphereRadius => _radius;

        public Action OnChanged;

        /// <summary>
        /// Validates the settings to ensure they are within acceptable ranges.
        /// </summary>
        void OnValidate()
        {
            _rectDimensions = new Vector3(
                Mathf.Max(0.01f, _rectDimensions.x),
                Mathf.Max(0.01f, _rectDimensions.y),
                Mathf.Max(0.01f, _rectDimensions.z)
            );
            _radius = Mathf.Max(0.01f, _radius);
            OnChanged?.Invoke();
        }

        #region [[ CALCULATIONS ]] ================================================================

        /// <summary>
        /// Calculates the edge point for a 3D box in the given direction.
        /// </summary>
        Vector3 GetBoxEdgePoint(
            Vector3 dimensions,
            float angle,
            Vector3 position,
            Quaternion rotation
        )
        {
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
            Vector3 localDirection = Quaternion.Inverse(rotation) * direction;
            Vector3 halfExtents = dimensions / 2f;

            // Calculate intersection with box edge
            float t = Mathf.Min(
                halfExtents.x / Mathf.Abs(localDirection.x),
                halfExtents.y / Mathf.Abs(localDirection.y),
                halfExtents.z / Mathf.Abs(localDirection.z)
            );

            Vector3 localEdgePoint = localDirection * t;
            return position + rotation * localEdgePoint;
        }

        /// <summary>
        /// Calculates the edge point for a 3D sphere in the given direction.
        /// </summary>
        Vector3 GetSphereEdgePoint(float angle, Vector3 position)
        {
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
            return position + direction.normalized * _radius;
        }

        Vector3 GetEdgePoint(float angle, Vector3 position, Quaternion rotation)
        {
            if (IsRect)
            {
                return GetBoxEdgePoint(_rectDimensions, angle, position, rotation);
            }
            else if (IsCircle)
            {
                return GetSphereEdgePoint(angle, position);
            }

            return position;
        }

        float GetEdgePointAngle(Transform transform, Vector3 edgePoint)
        {
            Vector3 center = transform.position;
            Quaternion rotation = transform.rotation;

            // Calculate direction from center to edge point
            Vector3 direction = (edgePoint - center).normalized;
            Vector3 forward = rotation * Vector3.forward;

            // Calculate angle and normalize to 0-360 range
            float angle = Vector3.SignedAngle(forward, direction, Vector3.up);
            return angle < 0 ? angle + 360f : angle;
        }

        void CalculateShapePoints(Transform transform, out Dictionary<float, Vector3> shapePoints)
        {
            shapePoints = new Dictionary<float, Vector3>();

            // << CALCULATE SHAPE POINTS >>
            if (IsRect)
            {
                CustomGizmos.GenerateRectangleVertices(
                    transform.position,
                    new Vector2(_rectDimensions.x, _rectDimensions.z),
                    transform.rotation,
                    out Vector3[] vertices
                );
                foreach (var vertex in vertices)
                {
                    shapePoints.Add(GetEdgePointAngle(transform, vertex), vertex);
                }
            }
            else if (IsCircle)
            {
                CustomGizmos.GenerateRadialVertices(
                    transform.position,
                    _radius,
                    Vector3.up,
                    16,
                    out Vector3[] vertices
                );
                foreach (var vertex in vertices)
                {
                    shapePoints.Add(GetEdgePointAngle(transform, vertex), vertex);
                }
            }

            // Sort the shape points by angle
            shapePoints = shapePoints
                .OrderBy(point => point.Key)
                .ToDictionary(point => point.Key, point => point.Value);
        }
        #endregion

        public void GetDetectionSectorPoints(
            Transform transform,
            SensorDetectionFilter filter,
            out Vector3 initEdgePoint,
            out Vector3 terminalEdgePoint,
            out Dictionary<float, Vector3> detectionSectorPoints
        )
        {
            CalculateShapePoints(transform, out Dictionary<float, Vector3> shapePoints);
            detectionSectorPoints = new Dictionary<float, Vector3>();

            Vector2 detectionSector = filter.DetectionSector;
            float initialAngle = detectionSector.x;
            float terminalAngle = detectionSector.y;

            initEdgePoint = GetEdgePoint(initialAngle, transform.position, transform.rotation);
            terminalEdgePoint = GetEdgePoint(terminalAngle, transform.position, transform.rotation);

            /// << SHAPE POINTS IN DETECTION SECTOR >>
            if (initialAngle == terminalAngle)
                return;

            // Determine which sector to use based on the sector type
            Dictionary<float, Vector3> sectorPoints = GetSectorPoints(
                shapePoints,
                initialAngle,
                terminalAngle,
                filter.SectorType
            );

            foreach (var point in sectorPoints)
            {
                if (detectionSectorPoints.ContainsKey(point.Key))
                    continue;
                detectionSectorPoints.Add(point.Key, point.Value);
            }

            detectionSectorPoints = detectionSectorPoints
                .OrderBy(point => point.Key)
                .ToDictionary(point => point.Key, point => point.Value);
        }

        /// <summary>
        /// Gets the appropriate sector points based on the sector type.
        /// </summary>
        /// <param name="shapePoints">All available shape points</param>
        /// <param name="initialAngle">The initial angle of the detection sector</param>
        /// <param name="terminalAngle">The terminal angle of the detection sector</param>
        /// <param name="sectorType">The type of sector to calculate</param>
        /// <returns>Dictionary of angle-vector pairs for the selected sector</returns>
        private Dictionary<float, Vector3> GetSectorPoints(
            Dictionary<float, Vector3> shapePoints,
            float initialAngle,
            float terminalAngle,
            SectorType sectorType
        )
        {
            Dictionary<float, Vector3> sectorPoints = new Dictionary<float, Vector3>();

            switch (sectorType)
            {
                case SectorType.FULL:
                    // Include all points
                    sectorPoints = shapePoints;
                    break;

                case SectorType.SMALL_ANGLE:
                    // Prefer the sector with the smaller angle difference
                    sectorPoints = GetSmallerSectorPoints(shapePoints, initialAngle, terminalAngle);
                    break;

                case SectorType.LARGE_ANGLE:
                    // Prefer the sector with the larger angle difference
                    sectorPoints = GetLargerSectorPoints(shapePoints, initialAngle, terminalAngle);
                    break;
            }

            return sectorPoints;
        }

        /// <summary>
        /// Checks if a point angle is within the specified sector.
        /// </summary>
        /// <param name="pointAngle">The angle of the point to check</param>
        /// <param name="initialAngle">The initial angle of the sector</param>
        /// <param name="terminalAngle">The terminal angle of the sector</param>
        /// <returns>True if the point is within the sector</returns>
        private bool IsPointInSector(float pointAngle, float initialAngle, float terminalAngle)
        {
            if (initialAngle < terminalAngle)
            {
                return pointAngle >= initialAngle && pointAngle <= terminalAngle;
            }
            else
            {
                return pointAngle <= initialAngle && pointAngle >= terminalAngle;
            }
        }

        /// <summary>
        /// Gets points from the smaller sector (the one with smaller angle difference).
        /// </summary>
        /// <param name="shapePoints">All available shape points</param>
        /// <param name="initialAngle">The initial angle of the detection sector</param>
        /// <param name="terminalAngle">The terminal angle of the detection sector</param>
        /// <returns>Dictionary of angle-vector pairs for the smaller sector</returns>
        private Dictionary<float, Vector3> GetSmallerSectorPoints(
            Dictionary<float, Vector3> shapePoints,
            float initialAngle,
            float terminalAngle
        )
        {
            Dictionary<float, Vector3> sectorPoints = new Dictionary<float, Vector3>();

            // Calculate the angle difference for the primary sector
            float minorSectorAngle = CalculateSectorAngle(initialAngle, terminalAngle);

            // Calculate the angle difference for the complementary sector
            float majorSectorAngle = 360f - minorSectorAngle;

            // Determine which sector is smaller
            bool usePrimarySector = minorSectorAngle <= majorSectorAngle;

            foreach (var point in shapePoints)
            {
                bool inPrimarySector = IsPointInSector(point.Key, initialAngle, terminalAngle);
                bool inComplementarySector = !inPrimarySector;

                if (
                    (usePrimarySector && inPrimarySector)
                    || (!usePrimarySector && inComplementarySector)
                )
                {
                    sectorPoints.Add(point.Key, point.Value);
                }
            }

            return sectorPoints;
        }

        /// <summary>
        /// Gets points from the larger sector (the one with larger angle difference).
        /// </summary>
        /// <param name="shapePoints">All available shape points</param>
        /// <param name="initialAngle">The initial angle of the detection sector</param>
        /// <param name="terminalAngle">The terminal angle of the detection sector</param>
        /// <returns>Dictionary of angle-vector pairs for the larger sector</returns>
        private Dictionary<float, Vector3> GetLargerSectorPoints(
            Dictionary<float, Vector3> shapePoints,
            float initialAngle,
            float terminalAngle
        )
        {
            Dictionary<float, Vector3> sectorPoints = new Dictionary<float, Vector3>();

            // Calculate the angle difference for the primary sector
            float primarySectorAngle = CalculateSectorAngle(initialAngle, terminalAngle);

            // Calculate the angle difference for the complementary sector
            float complementarySectorAngle = 360f - primarySectorAngle;

            // Determine which sector is larger
            bool usePrimarySector = primarySectorAngle >= complementarySectorAngle;

            foreach (var point in shapePoints)
            {
                bool inPrimarySector = IsPointInSector(point.Key, initialAngle, terminalAngle);
                bool inComplementarySector = !inPrimarySector;

                if (
                    (usePrimarySector && inPrimarySector)
                    || (!usePrimarySector && inComplementarySector)
                )
                {
                    sectorPoints.Add(point.Key, point.Value);
                }
            }

            return sectorPoints;
        }

        /// <summary>
        /// Calculates the angle difference between two angles, handling wraparound.
        /// </summary>
        /// <param name="initialAngle">The initial angle</param>
        /// <param name="terminalAngle">The terminal angle</param>
        /// <returns>The angle difference in degrees</returns>
        private float CalculateSectorAngle(float initialAngle, float terminalAngle)
        {
            float angleDifference = Mathf.Abs(terminalAngle - initialAngle);

            // Handle wraparound case where the sector crosses 0/360 degrees
            if (angleDifference > 180f)
            {
                angleDifference = 360f - angleDifference;
            }

            return angleDifference;
        }

        /// <summary>
        /// Draws gizmos for the sensor shape and a forward direction line.
        /// </summary>
        /// <param name="gizmoColor">The color for the gizmo</param>
        /// <param name="position">The center position of the sensor</param>
        /// <param name="rotation">The rotation of the sensor transform</param>
        public void DrawGizmos(
            Color gizmoColor,
            SensorBase sensor,
            SensorDetectionFilter filter = null,
            bool showDebug = false
        )
        {
            // Draw the shape outline
            if (_shape == Shape.RECT2D)
            {
                CustomGizmos.DrawWireRect(
                    sensor.transform.position,
                    new Vector2(_rectDimensions.x, _rectDimensions.z),
                    sensor.transform.rotation,
                    gizmoColor
                );
            }
            else if (_shape == Shape.BOX3D)
            {
                CustomGizmos.DrawWireCube(
                    sensor.transform.position,
                    _rectDimensions,
                    sensor.transform.rotation,
                    gizmoColor
                );
            }
            else if (_shape == Shape.CIRCLE2D)
            {
                CustomGizmos.DrawWireCircle(
                    sensor.transform.position,
                    _radius,
                    Vector3.up,
                    gizmoColor
                );
            }
            else if (_shape == Shape.SPHERE3D)
            {
                CustomGizmos.DrawWireSphere(sensor.transform.position, _radius, gizmoColor);
            }
            // Draw forward direction line
            DrawDetectionSector(gizmoColor, sensor, filter, showDebug);
        }

        /// <summary>
        /// Draws lines from the center to the edge of the sensor shape for the detection sector angles.
        /// </summary>
        /// <param name="gizmoColor">The color for the lines</param>
        /// <param name="position">The center position of the sensor</param>
        /// <param name="rotation">The rotation of the sensor transform</param>
        /// <param name="filter">The sensor detection filter containing angle information</param>
        void DrawDetectionSector(
            Color gizmoColor,
            SensorBase sensor,
            SensorDetectionFilter filter = null,
            bool showDebug = false
        )
        {
            Vector3 position = sensor.transform.position;
            Quaternion rotation = sensor.transform.rotation;

            // Calculate edge points
            GetDetectionSectorPoints(
                sensor.transform,
                filter,
                out Vector3 initEdgePoint,
                out Vector3 terminalEdgePoint,
                out Dictionary<float, Vector3> detectionSectorPoints
            );

            // Draw debug points
            if (showDebug)
            {
                // Draw detection sector points
                foreach (var point in detectionSectorPoints)
                {
                    CustomGizmos.DrawSolidCircle(point.Value, 0.025f, Vector3.up, gizmoColor);
                    CustomGizmos.DrawLabel(
                        point.Key.ToString() + "°",
                        point.Value,
                        CustomGUIStyles.CenteredStyle
                    );
                }

                // Draw line for initial angle
                CustomGizmos.DrawLineWithLabel(
                    position,
                    initEdgePoint,
                    gizmoColor,
                    filter.DetectionSector.x.ToString() + "°"
                );

                // Draw line for terminal angle
                CustomGizmos.DrawLineWithLabel(
                    position,
                    terminalEdgePoint,
                    gizmoColor,
                    filter.DetectionSector.y.ToString() + "°"
                );
            }

            // Draw the detection sector polygon

            if (filter.SectorType == SectorType.FULL)
            {
                CustomGizmos.DrawPolygon(detectionSectorPoints.Values.ToArray(), gizmoColor, 0.2f);
            }
            else if (filter.SectorType == SectorType.SMALL_ANGLE)
            {
                // Add the center point to the vertices
                List<Vector3> vertices = new List<Vector3>
                {
                    sensor.transform.position,
                    initEdgePoint
                };
                vertices.AddRange(detectionSectorPoints.Values);
                vertices.AddRange(new Vector3[] { terminalEdgePoint, sensor.transform.position }); // Add the center point to the end of the array to close the polygon
                CustomGizmos.DrawPolygon(vertices.ToArray(), gizmoColor, 0.2f);
            }
            else if (filter.SectorType == SectorType.LARGE_ANGLE)
            {
                // Add the edge points to the vertices
                detectionSectorPoints.Add(filter.DetectionSector.x, initEdgePoint);
                detectionSectorPoints.Add(filter.DetectionSector.y, terminalEdgePoint);
                detectionSectorPoints = detectionSectorPoints
                    .OrderBy(point => point.Key)
                    .ToDictionary(point => point.Key, point => point.Value);

                // Get the index of the initial and terminal points
                int initialIndex = detectionSectorPoints
                    .Keys.ToList()
                    .IndexOf(filter.DetectionSector.x);
                int terminalIndex = detectionSectorPoints
                    .Keys.ToList()
                    .IndexOf(filter.DetectionSector.y);

                // Convert the dictionary to a list
                List<Vector3> vertices = detectionSectorPoints.Values.ToList();

                // Add the center point inbetween the initial and terminal points to close the polygon
                if (initialIndex < terminalIndex)
                {
                    vertices.Insert(initialIndex + 1, sensor.transform.position);
                }
                else
                {
                    vertices.Insert(terminalIndex + 1, sensor.transform.position);
                }

                // Rotate vertices so that the vertex with the larger index becomes the first element
                int largerIndex = Mathf.Max(initialIndex, terminalIndex);
                if (largerIndex > 0)
                {
                    // Rotate the list: move elements from largerIndex to the end to the beginning
                    List<Vector3> rotatedVertices = new List<Vector3>();
                    rotatedVertices.AddRange(
                        vertices.GetRange(largerIndex, vertices.Count - largerIndex)
                    );
                    rotatedVertices.AddRange(vertices.GetRange(0, largerIndex));
                    vertices = rotatedVertices;
                }

                vertices.Add(vertices[0]);

                // Draw the polygon
                CustomGizmos.DrawPolygon(vertices.ToArray(), gizmoColor, 0.2f);

                // Debug the vertices
                for (int i = 0; i < vertices.Count; i++)
                {
                    CustomGizmos.DrawLabel(
                        i.ToString(),
                        vertices[i] + Vector3.up * 0.1f,
                        CustomGUIStyles.CenteredStyle
                    );
                }
            }
        }
    }
}
