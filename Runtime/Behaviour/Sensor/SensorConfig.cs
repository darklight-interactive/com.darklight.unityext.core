using System;
using System.Collections.Generic;
using System.Linq;
using Darklight.Editor;
using NaughtyAttributes;
using UnityEngine;
using static Darklight.Behaviour.Sensor;

namespace Darklight.Behaviour
{
    /// <summary>
    /// ScriptableObject containing all sensor-related settings for the survivor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSensorConfig", menuName = "Darklight/Behaviour/SensorConfig")]
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
        EdgePoint GetBoxEdgePoint(
            Vector3 dimensions,
            float angle,
            Vector3 position,
            Quaternion rotation
        )
        {
            // Calculate direction in world space, accounting for transform rotation
            Vector3 direction =
                rotation * Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
            Vector3 localDirection = Quaternion.Inverse(rotation) * direction;
            Vector3 halfExtents = dimensions / 2f;

            // Calculate intersection with box edge
            float t = Mathf.Min(
                halfExtents.x / Mathf.Abs(localDirection.x),
                halfExtents.y / Mathf.Abs(localDirection.y),
                halfExtents.z / Mathf.Abs(localDirection.z)
            );

            Vector3 localEdgePoint = localDirection * t;
            return new EdgePoint(angle, position + rotation * localEdgePoint);
        }

        /// <summary>
        /// Calculates the edge point for a 3D sphere in the given direction.
        /// </summary>
        EdgePoint GetSphereEdgePoint(float angle, Vector3 position, Quaternion rotation)
        {
            // Calculate direction in world space, accounting for transform rotation
            Vector3 direction =
                rotation * Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
            return new EdgePoint(angle, position + direction.normalized * _radius);
        }

        EdgePoint GetEdgePoint(float angle, Transform transform)
        {
            if (IsRect)
            {
                return GetBoxEdgePoint(
                    _rectDimensions,
                    angle,
                    transform.position,
                    transform.rotation
                );
            }
            else if (IsCircle)
            {
                return GetSphereEdgePoint(angle, transform.position, transform.rotation);
            }

            return new EdgePoint(angle, Vector3.zero);
        }

        float NormalizeAngle(float angle)
        {
            if (angle < 0)
                return angle + 360f;
            else if (angle > 360f)
                return angle - 360f;
            return angle;
        }

        /// <summary>
        /// Determines if a given angle falls within the minor sector defined by initial and terminal angles.
        /// The minor sector is the smaller arc between two angles on a circle (0-360 degrees).
        /// </summary>
        /// <param name="angle">The angle to check (0-360 degrees)</param>
        /// <param name="initialAngle">The starting angle of the sector (0-360 degrees)</param>
        /// <param name="terminalAngle">The ending angle of the sector (0-360 degrees)</param>
        /// <returns>True if the angle is within the minor sector, false otherwise</returns>
        bool IsAngleInMinorSector(float angle, float initialAngle, float terminalAngle)
        {
            // Normalize angles to ensure they're within 0-360 range
            angle = NormalizeAngle(angle);
            initialAngle = NormalizeAngle(initialAngle);
            terminalAngle = NormalizeAngle(terminalAngle);

            // Calculate the angular difference between initial and terminal angles
            float angularDifference = Mathf.Abs(initialAngle - terminalAngle);

            // If the difference is greater than 180 degrees, the sector wraps around the 0/360 boundary
            // In this case, the minor sector is the smaller arc that crosses the 0-degree mark
            if (angularDifference > 180f)
            {
                // Minor sector wraps around: angle is in sector if it's before initial OR after terminal
                // Example: initial=30°, terminal=300° → minor sector is 300°-360° + 0°-30°
                return angle >= terminalAngle || angle <= initialAngle;
            }

            // Standard case: no wrapping around the boundary
            // The minor sector is simply the range between initial and terminal angles
            if (initialAngle <= terminalAngle)
            {
                // Normal case: initial angle is less than terminal angle
                // Minor sector is the arc from initial to terminal (inclusive)
                return angle >= initialAngle && angle <= terminalAngle;
            }
            else
            {
                // Edge case: initial angle is greater than terminal angle but difference <= 180°
                // This shouldn't happen with normalized angles, but handle it defensively
                // Minor sector would be the arc from terminal to initial (inclusive)
                return angle >= terminalAngle && angle <= initialAngle;
            }
        }

        float CalculateAngleFromPosition(Transform transform, Vector3 position)
        {
            Vector3 center = transform.position;
            Quaternion rotation = transform.rotation;

            // Calculate direction from center to edge point
            Vector3 direction = (position - center).normalized;
            Vector3 forward = rotation * Vector3.forward;

            // Calculate angle and normalize to 0-360 range
            float angle = Vector3.SignedAngle(forward, direction, Vector3.up);
            return NormalizeAngle(angle);
        }
        #endregion

        public bool IsPositionAngleInSector(
            Transform transform,
            Vector3 position,
            SensorDetectionFilter filter
        )
        {
            float angle = CalculateAngleFromPosition(transform, position);
            bool isInMinorSector = IsAngleInMinorSector(
                angle,
                filter.InitialAngle,
                filter.TerminalAngle
            );

            // Return the result based on the sector type
            if (filter.SectorType == SectorType.SMALL_ANGLE)
                return isInMinorSector;
            else if (filter.SectorType == SectorType.LARGE_ANGLE)
                return !isInMinorSector;
            else
                return true;
        }

        public void CalculateShapePoints(Transform transform, out List<EdgePoint> shapePoints)
        {
            shapePoints = new List<EdgePoint>();

            // << CALCULATE SHAPE POINTS >>
            if (IsRect)
            {
                Vector3[] vertices = CustomGizmos.GenerateRectangleVertices(
                    transform.position,
                    new Vector2(_rectDimensions.x, _rectDimensions.z),
                    transform.rotation,
                    4
                );
                foreach (var vertex in vertices)
                {
                    shapePoints.Add(
                        new EdgePoint(CalculateAngleFromPosition(transform, vertex), vertex)
                    );
                }
            }
            else if (IsCircle)
            {
                CustomGizmos.GenerateRadialVertices(
                    transform.position,
                    _radius,
                    transform.rotation,
                    16,
                    out Vector3[] vertices
                );
                foreach (var vertex in vertices)
                {
                    shapePoints.Add(
                        new EdgePoint(CalculateAngleFromPosition(transform, vertex), vertex)
                    );
                }
            }

            // Sort the shape points by angle
            shapePoints = shapePoints.OrderBy(point => point.Angle).ToList();
        }

        public void CalculateSectorAngleEdgePoints(
            Transform transform,
            SensorDetectionFilter filter,
            out EdgePoint initialEdgePoint,
            out EdgePoint terminalEdgePoint
        )
        {
            // << GET THE INITIAL AND TERMINAL ANGLES >>
            float initialAngle = filter.InitialAngle;
            float terminalAngle = filter.TerminalAngle;

            // If the initial angle is greater than the terminal angle, swap them
            if (initialAngle > terminalAngle)
            {
                initialAngle = filter.TerminalAngle;
                terminalAngle = filter.InitialAngle;
            }

            // Get the initial and terminal edge points
            initialEdgePoint = GetEdgePoint(initialAngle, transform);
            terminalEdgePoint = GetEdgePoint(terminalAngle, transform);
        }

        public void CalculateSectorEdgePoints(
            Transform transform,
            SensorDetectionFilter filter,
            out List<EdgePoint> sectorEdgePoints
        )
        {
            // << CALCULATE THE SHAPE POINTS >>
            CalculateShapePoints(transform, out List<EdgePoint> shapePoints);

            // << CALCULATE THE INITIAL AND TERMINAL EDGE POINTS >>
            CalculateSectorAngleEdgePoints(
                transform,
                filter,
                out EdgePoint initialEdgePoint,
                out EdgePoint terminalEdgePoint
            );

            // Add the initial and terminal edge points to the sector edge points
            sectorEdgePoints = new List<EdgePoint> { initialEdgePoint, terminalEdgePoint };

            // << CALCULATE THE MINOR AND MAJOR SECTOR POINTS >>
            List<EdgePoint> minorSectorPoints = new List<EdgePoint>();
            List<EdgePoint> majorSectorPoints = new List<EdgePoint>();

            // Add the shape points to the minor or major sector points
            for (int i = 0; i < shapePoints.Count; i++)
            {
                EdgePoint currentPoint = shapePoints[i];
                if (
                    IsAngleInMinorSector(
                        currentPoint.Angle,
                        initialEdgePoint.Angle,
                        terminalEdgePoint.Angle
                    )
                )
                {
                    minorSectorPoints.Add(currentPoint);
                }
                else
                {
                    majorSectorPoints.Add(currentPoint);
                }
            }

            // Filter the sector points based on the sector type
            if (filter.SectorType == SectorType.FULL)
            {
                sectorEdgePoints.AddRange(minorSectorPoints);
                sectorEdgePoints.AddRange(majorSectorPoints);
            }
            else if (filter.SectorType == SectorType.SMALL_ANGLE)
            {
                sectorEdgePoints.AddRange(minorSectorPoints);
            }
            else if (filter.SectorType == SectorType.LARGE_ANGLE)
            {
                sectorEdgePoints.AddRange(majorSectorPoints);
            }
        }

        /// <summary>
        /// Draws gizmos for the sensor shape and a forward direction line.
        /// </summary>
        /// <param name="gizmoColor">The color for the gizmo</param>
        /// <param name="position">The center position of the sensor</param>
        /// <param name="rotation">The rotation of the sensor transform</param>
        public void DrawOutlineGizmos(Transform transform, Color gizmoColor)
        {
            // Draw the shape outline
            if (_shape == Shape.RECT2D)
            {
                CustomGizmos.DrawWireRect(
                    transform.position,
                    new Vector2(_rectDimensions.x, _rectDimensions.z),
                    transform.rotation,
                    gizmoColor
                );
            }
            else if (_shape == Shape.BOX3D)
            {
                CustomGizmos.DrawWireCube(
                    transform.position,
                    _rectDimensions,
                    transform.rotation,
                    gizmoColor
                );
            }
            else if (_shape == Shape.CIRCLE2D)
            {
                CustomGizmos.DrawWireCircle(
                    transform.position,
                    _radius,
                    transform.rotation,
                    gizmoColor
                );
            }
            else if (_shape == Shape.SPHERE3D)
            {
                CustomGizmos.DrawWireSphere(transform.position, _radius, gizmoColor);
            }
        }
    }
}
