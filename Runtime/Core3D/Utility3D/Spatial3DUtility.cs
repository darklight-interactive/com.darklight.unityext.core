using UnityEngine;
using Unity.Mathematics;

namespace Darklight.UnityExt.Core3D
{
    public static class Spatial3D
    {
        /// <summary>
        /// Converts local float3 positions to Vector3 world positions.
        /// </summary>
        /// <param name="transform">Transform to use for conversion</param>
        /// <param name="localPoint">float3 local position</param>
        /// <returns>Vector3 world position</returns>
        public static Vector3 LocalToWorldConversion(Transform transform, float3 localPoint)
        {
            Vector3 worldPos = transform.TransformPoint(localPoint);
            return worldPos;
        }

        /// <summary>
        /// Converts Vector3 local positions to Vector3 world positions
        /// </summary>
        /// <param name="transform">Transform to use for conversion</param>
        /// <param name="localPoint">Vector3 local position</param>
        /// <param name="worldPoint">Vector3 world position</param>
        /// <returns>Vector3 world position</returns>
        public static Vector3 LocalToWorldConversion(Transform transform, Vector3 localPoint, out Vector3 worldPoint)
        {
            worldPoint = transform.TransformPoint(localPoint);
            return worldPoint;
        }

        /// <summary>
        /// Converts Vector3 world positions to local float3 positions
        /// </summary>
        /// <param name="transform">Transform to use for conversion</param>
        /// <param name="worldPoint">Vector3 world position</param>
        /// <returns>float3 local position</returns>
        public static float3 WorldToLocalConversion(Transform transform, Vector3 worldPoint)
        {
            float3 localPos = transform.InverseTransformPoint(worldPoint);
            return localPos;
        }

        /// <summary>
        /// Converts Vector3 world positions to Vector3 local positions
        /// </summary>
        /// <param name="transform">Transform to use for conversion</param>
        /// <param name="worldPoint">Vector3 world position</param>
        /// <param name="localPoint">Vector3 local position</param>
        /// <returns>Vector3 local position</returns>
        public static float3 WorldToLocalConversion(Transform transform, Vector3 worldPoint, out Vector3 localPoint)
        {
            localPoint = transform.InverseTransformPoint(worldPoint);
            return localPoint;
        }
    }
}
