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

namespace Darklight.Behaviour
{
    public partial class Sensor
    {
        public enum Shape
        {
            RECT2D,
            BOX3D,
            CIRCLE2D,
            SPHERE3D
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

        /// <summary>
        /// ScriptableObject containing all sensor-related settings for the survivor.
        /// </summary>
        [CreateAssetMenu(
            fileName = "NewSensorSettings",
            menuName = "Darklight/Behaviour/SensorSettings"
        )]
        public class Config : ScriptableObject
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

            void CalculateShapePoints(Transform transform, out List<EdgePoint> shapePoints)
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
                            new EdgePoint(GetEdgePointAngle(transform, vertex), vertex)
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
                            new EdgePoint(GetEdgePointAngle(transform, vertex), vertex)
                        );
                    }
                }

                // Sort the shape points by angle
                shapePoints = shapePoints.OrderBy(point => point.Angle).ToList();
            }

            bool IsPointInMinorSector(EdgePoint point, float initialAngle, float terminalAngle)
            {
                float difference = Mathf.Abs(initialAngle - terminalAngle);
                // If the difference is greater than 180 degrees, then the sector is wrapped around
                if (difference > 180f)
                {
                    return point.Angle < initialAngle || point.Angle > terminalAngle;
                }

                if (initialAngle < terminalAngle)
                {
                    // << MINOR SECTOR POINTS >>
                    // These points are the ones inbetween the initial and terminal points
                    return point.Angle >= initialAngle && point.Angle <= terminalAngle;
                }
                else
                {
                    // << MAJOR SECTOR POINTS >>
                    // These points are the ones outside the initial and terminal points
                    return point.Angle <= initialAngle && point.Angle >= terminalAngle;
                }
            }
            #endregion

            public void CalculateSensorPoints(
                Transform transform,
                DetectionFilter filter,
                out EdgePoint initialEdgePoint,
                out EdgePoint terminalEdgePoint,
                out List<EdgePoint> shapePoints,
                out List<EdgePoint> sectorPoints
            )
            {
                CalculateShapePoints(transform, out shapePoints);

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

                // << CALCULATE THE MINOR AND MAJOR SECTOR POINTS >>
                List<EdgePoint> minorSectorPoints = new List<EdgePoint>();
                List<EdgePoint> majorSectorPoints = new List<EdgePoint>();

                // Add the shape points to the minor or major sector points
                for (int i = 0; i < shapePoints.Count; i++)
                {
                    EdgePoint currentPoint = shapePoints[i];
                    if (IsPointInMinorSector(currentPoint, initialAngle, terminalAngle))
                    {
                        minorSectorPoints.Add(currentPoint);
                    }
                    else
                    {
                        majorSectorPoints.Add(currentPoint);
                    }
                }

                // Filter the sector points based on the sector type
                sectorPoints = new List<EdgePoint>();
                if (filter.SectorType == SectorType.FULL)
                {
                    sectorPoints.AddRange(minorSectorPoints);
                    sectorPoints.AddRange(majorSectorPoints);
                }
                else if (filter.SectorType == SectorType.SMALL_ANGLE)
                {
                    sectorPoints.AddRange(minorSectorPoints);
                }
                else if (filter.SectorType == SectorType.LARGE_ANGLE)
                {
                    sectorPoints.AddRange(majorSectorPoints);
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
}
