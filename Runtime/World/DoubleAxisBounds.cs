using System;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.World;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.World
{
    [Serializable]
    public class DoubleAxisBounds
    {
        readonly int valueRange;

        public bool ShowGizmos = true;
        public Vector2 Center = Vector2.zero;
        public SingleAxisBounds XAxisBounds;
        public SingleAxisBounds YAxisBounds;

        /// <summary>
        /// The left edge of the bounds. Min value of the X axis.
        /// </summary>
        public float Left
        {
            get => Center.x + XAxisBounds.Min;
        }

        /// <summary>
        /// The right edge of the bounds. Max value of the X axis.
        /// </summary>
        public float Right
        {
            get => Center.x + XAxisBounds.Max;
        }

        /// <summary>
        /// The top edge of the bounds. Max value of the Y axis.
        /// </summary>
        public float Top
        {
            get => Center.y + YAxisBounds.Max;
        }

        /// <summary>
        /// The bottom edge of the bounds. Min value of the Y axis.
        /// </summary>
        public float Bottom
        {
            get => Center.y + YAxisBounds.Min;
        }

        /// <summary>
        /// The width of the bounds. Max value of the X axis minus the min value of the X axis.
        /// </summary>
        public float Width
        {
            get => XAxisBounds.Max - XAxisBounds.Min;
        }

        /// <summary>
        /// The height of the bounds. Max value of the Y axis minus the min value of the Y axis.
        /// </summary>
        public float Height
        {
            get => YAxisBounds.Max - YAxisBounds.Min;
        }

        /// <summary>
        /// The size of the bounds. A vector2 with the width and height of the bounds.
        /// </summary>
        public Vector2 Size
        {
            get => new Vector2(Width, Height);
        }

        public DoubleAxisBounds(Vector2 center, int valueRange)
        {
            Center = center;
            XAxisBounds = new SingleAxisBounds(WorldAxis.X, new Vector2(-valueRange, valueRange));
            YAxisBounds = new SingleAxisBounds(WorldAxis.Y, new Vector2(-valueRange, valueRange));
        }

        /// <summary>
        /// Check if a point is within the bounds.
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <param name="sizeOffset">Offset from the edges to consider for the check</param>

        /// <returns>True if the point is within bounds, false otherwise</returns>
        public bool Contains(Vector2 point, Vector2 sizeOffset)
        {
            return point.x >= Left + sizeOffset.x
                && point.x <= Right - sizeOffset.x
                && point.y >= Bottom + sizeOffset.y
                && point.y <= Top - sizeOffset.y;
        }

        /// <summary>
        /// Check if a point is within the bounds with uniform offset.
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <param name="sizeOffset">Uniform offset from the edges to consider for the check</param>
        /// <returns>True if the point is within bounds, false otherwise</returns>
        public bool Contains(Vector2 point, float sizeOffset = 1)
        {
            return point.x >= Left + sizeOffset
                && point.x <= Right - sizeOffset
                && point.y >= Bottom + sizeOffset
                && point.y <= Top - sizeOffset;
        }

        /// <summary>
        /// Returns the closest point within the bounds to the given external point.
        /// </summary>
        /// <param name="externalPoint">The point to clamp to the bounds</param>
        /// <param name="offset">Offset from the edges to consider for clamping</param>
        /// <returns>The closest point within bounds</returns>
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

        /// <summary>
        /// Returns the closest point within the bounds to the given external point with separate offsets per axis.
        /// </summary>
        /// <param name="externalPoint">The point to clamp to the bounds</param>
        /// <param name="offset">Vector2 offset from the edges to consider for clamping</param>
        /// <returns>The closest point within bounds</returns>
        public Vector2 ClosestPointWithinBounds(Vector2 externalPoint, Vector2 offset)
        {
            if (Contains(externalPoint))
            {
                return externalPoint;
            }

            float x = Mathf.Clamp(externalPoint.x, Left + offset.x, Right - offset.x);
            float y = Mathf.Clamp(externalPoint.y, Bottom + offset.y, Top - offset.y);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Draws the bounds gizmos in the scene view.
        /// </summary>
        public void DrawGizmos()
        {
            if (!ShowGizmos)
                return;

            XAxisBounds.DrawGizmos(Center, YAxisBounds.Length);
            YAxisBounds.DrawGizmos(Center, XAxisBounds.Length);
        }
    }
}
