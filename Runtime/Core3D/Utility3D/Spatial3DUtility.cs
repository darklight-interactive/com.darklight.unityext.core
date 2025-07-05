using Unity.Mathematics;
using UnityEngine;

namespace Darklight.UnityExt.Core3D
{
    public static class Spatial3D
    {
        public enum Axis
        {
            X,
            Y,
            Z
        }

        public enum Direction
        {
            NULL,
            FORWARD,
            BACKWARD,
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        public static Vector3 GetDirectionVector(Direction direction)
        {
            return direction switch
            {
                Direction.FORWARD => Vector3.forward,
                Direction.BACKWARD => Vector3.back,
                Direction.LEFT => Vector3.left,
                Direction.RIGHT => Vector3.right,
                Direction.UP => Vector3.up,
                Direction.DOWN => Vector3.down,
                _ => Vector3.zero
            };
        }

        public static Direction GetDirectionEnum(Vector3 direction)
        {
            Vector3 normalizedDirection = direction.normalized;
            if (normalizedDirection == Vector3.forward)
                return Direction.FORWARD;
            if (normalizedDirection == Vector3.back)
                return Direction.BACKWARD;
            if (normalizedDirection == Vector3.left)
                return Direction.LEFT;
            if (normalizedDirection == Vector3.right)
                return Direction.RIGHT;
            if (normalizedDirection == Vector3.up)
                return Direction.UP;
            if (normalizedDirection == Vector3.down)
                return Direction.DOWN;
            return Direction.NULL;
        }

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
        public static Vector3 LocalToWorldConversion(
            Transform transform,
            Vector3 localPoint,
            out Vector3 worldPoint
        )
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
        public static float3 WorldToLocalConversion(
            Transform transform,
            Vector3 worldPoint,
            out Vector3 localPoint
        )
        {
            localPoint = transform.InverseTransformPoint(worldPoint);
            return localPoint;
        }
    }
}
