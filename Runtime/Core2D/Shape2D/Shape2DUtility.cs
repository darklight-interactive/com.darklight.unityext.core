using System.Collections.Generic;
using Darklight.Editor;
using UnityEngine;

namespace Darklight.Core2D
{
    public static class Shape2DUtility
    {
        /// <summary>
        /// Generates an array of points around a circle.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="normal">The normal of the circle.</param>
        /// <param name="count">The number of points to generate.</param>
        /// <returns>An array of points around the circle.</returns>
        public static Vector3[] GenerateRadialPoints(
            Vector3 center,
            float radius,
            Vector3 normal,
            int count
        )
        {
            List<Vector3> positions = new List<Vector3>();

            // Foreach step in the circle, calculate the points
            float angleStep = 360.0f / count;
            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                Vector3 newPoint =
                    center + Quaternion.AngleAxis(angle, normal) * Vector3.right * radius;
                positions.Add(newPoint);
            }
            return positions.ToArray();
        }

        /// <summary>
        /// Calculates the antipodal point of a given point around a circle. This is the point directly opposite the given point on the circle.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="point">The point to calculate the antipodal point of.</param>
        /// <returns>The antipodal point of the given point.</returns>
        public static Vector3 CalculateAntipodalPoint(Vector3 center, Vector3 point)
        {
            Vector3 directionXZ = point - center; // Get the direction vector from the center to the point
            Vector3 antipodalPoint = center - directionXZ; // Get the antipodal point by reversing the direction vector
            return new Vector3(antipodalPoint.x, center.y, antipodalPoint.z);
        }
    }
}
