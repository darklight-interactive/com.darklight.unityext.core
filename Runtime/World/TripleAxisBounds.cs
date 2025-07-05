using System;
using UnityEngine;

namespace Darklight.UnityExt.World
{
    /// <summary>
    /// A serializable class that represents a 3D bounding box.
    /// </summary>
    /// <remarks>
    /// This is used to check if a point is within the bounds of the world.
    /// </remarks>
    [Serializable]
    public class TripleAxisBounds
    {
        const int DEFAULT_RANGE_VALUE = 1000;
        public Vector3 Center = Vector3.zero;
        public SingleAxisBounds xAxisBounds = new SingleAxisBounds(
            WorldAxis.X,
            new Vector2(-DEFAULT_RANGE_VALUE, DEFAULT_RANGE_VALUE)
        );
        public SingleAxisBounds yAxisBounds = new SingleAxisBounds(
            WorldAxis.Y,
            new Vector2(-DEFAULT_RANGE_VALUE, DEFAULT_RANGE_VALUE)
        );
        public SingleAxisBounds zAxisBounds = new SingleAxisBounds(
            WorldAxis.Z,
            new Vector2(-DEFAULT_RANGE_VALUE, DEFAULT_RANGE_VALUE)
        );

        /// <summary>
        /// The left edge of the bounds. Min value of the X axis.
        /// </summary>
        public float Left
        {
            get => Center.x + xAxisBounds.Min;
        }

        /// <summary>
        /// The right edge of the bounds. Max value of the X axis.
        /// </summary>
        public float Right
        {
            get => Center.x + xAxisBounds.Max;
        }

        /// <summary>
        /// The top edge of the bounds. Max value of the Y axis.
        /// </summary>
        public float Top
        {
            get => Center.y + yAxisBounds.Max;
        }

        /// <summary>
        /// The bottom edge of the bounds. Min value of the Y axis.
        /// </summary>

        public float Bottom
        {
            get => Center.y + yAxisBounds.Min;
        }

        /// <summary>
        /// The front edge of the bounds. Min value of the Z axis.
        /// </summary>
        public float Front
        {
            get => Center.z + zAxisBounds.Min;
        }

        /// <summary>
        /// The back edge of the bounds. Max value of the Z axis.
        /// </summary>
        public float Back
        {
            get => Center.z + zAxisBounds.Max;
        }

        /// <summary>
        /// The width of the bounds. Max value of the X axis minus the min value of the X axis.
        /// </summary>
        public float Width
        {
            get => xAxisBounds.Max - xAxisBounds.Min;
        }

        /// <summary>
        /// The height of the bounds. Max value of the Y axis minus the min value of the Y axis.
        /// /// </summary>
        public float Height
        {
            get => yAxisBounds.Max - yAxisBounds.Min;
        }

        /// <summary>
        /// The depth of the bounds. Max value of the Z axis minus the min value of the Z axis.
        /// </summary>
        public float Depth
        {
            get => zAxisBounds.Max - zAxisBounds.Min;
        }

        /// <summary>
        /// The size of the bounds. A vector3 with the width, height, and depth of the bounds.
        /// </summary>

        public Vector3 Size
        {
            get => new Vector3(Width, Height, Depth);
        }

        /// <summary>
        /// Check if a point is within the bounds of the world.
        /// The sizeOffset parameter is used to check if a point is within the bounds of a certain size.
        /// </summary>
        public bool Contains(Vector3 point, Vector3 sizeOffset)
        {
            return point.x >= Left + sizeOffset.x
                && point.x <= Right - sizeOffset.x
                && point.y >= Bottom + sizeOffset.y
                && point.y <= Top - sizeOffset.y
                && point.z >= Front + sizeOffset.z
                && point.z <= Back - sizeOffset.z;
        }

        public bool Contains(Vector3 point, float sizeOffset = 1)
        {
            return point.x >= Left + sizeOffset
                && point.x <= Right - sizeOffset
                && point.y >= Bottom + sizeOffset
                && point.y <= Top - sizeOffset
                && point.z >= Front + sizeOffset
                && point.z <= Back - sizeOffset;
        }

        public Vector2 ClosestPointWithinBounds(Vector2 externalPoint, float offset = 1)
        {
            if (Contains(externalPoint))
            {
                return externalPoint;
            }

            float x = Mathf.Clamp(externalPoint.x, Left + offset, Right - offset);
            float y = Mathf.Clamp(externalPoint.y, Bottom + offset, Top - offset);
            return new Vector2(x, y);
        }

        public Vector3 ClosestPointWithinBounds(Vector3 externalPoint, Vector3 offset)
        {
            if (Contains(externalPoint))
            {
                return externalPoint;
            }

            float x = Mathf.Clamp(externalPoint.x, Left + offset.x, Right - offset.x);
            float y = Mathf.Clamp(externalPoint.y, Bottom + offset.y, Top - offset.y);
            float z = Mathf.Clamp(externalPoint.z, Front + offset.z, Back - offset.z);
            return new Vector2(x, y);
        }

        public void DrawGizmos()
        {
            xAxisBounds.DrawGizmos(Center, xAxisBounds.Length);
            yAxisBounds.DrawGizmos(Center, yAxisBounds.Length);
            zAxisBounds.DrawGizmos(Center, zAxisBounds.Length);
        }
    }
}
