using Darklight.UnityExt.Editor;
using Darklight.UnityExt.World;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.World
{

    [CreateAssetMenu(fileName = "NewWorldBounds", menuName = "Darklight/World/WorldBounds", order = 1)]
    public class WorldBounds : ScriptableObject
    {
        const int DEFAULT_RANGE_VALUE = 1000;


        public bool ShowGizmos = true;
        public Vector3 Center = Vector3.zero;
        public SingleAxisBounds XAxisBounds = new SingleAxisBounds(WorldAxis.X, new Vector2(-DEFAULT_RANGE_VALUE, DEFAULT_RANGE_VALUE));
        public SingleAxisBounds YAxisBounds = new SingleAxisBounds(WorldAxis.Y, new Vector2(-DEFAULT_RANGE_VALUE, DEFAULT_RANGE_VALUE));
        public SingleAxisBounds ZAxisBounds = new SingleAxisBounds(WorldAxis.Z, new Vector2(-DEFAULT_RANGE_VALUE, DEFAULT_RANGE_VALUE));

        public float Left { get => Center.x + XAxisBounds.Min; }
        public float Right { get => Center.x + XAxisBounds.Max; }
        public float Top { get => Center.y + YAxisBounds.Max; }
        public float Bottom { get => Center.y + YAxisBounds.Min; }
        public float Front { get => Center.z + ZAxisBounds.Min; }
        public float Back { get => Center.z + ZAxisBounds.Max; }

        public float Width { get => XAxisBounds.Max - XAxisBounds.Min; }
        public float Height { get => YAxisBounds.Max - YAxisBounds.Min; }


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
            if (!ShowGizmos) return;

            XAxisBounds.DrawGizmos(Center, YAxisBounds.Length);
            YAxisBounds.DrawGizmos(Center, XAxisBounds.Length);
            ZAxisBounds.DrawGizmos(Center, XAxisBounds.Length);
        }
    }
}