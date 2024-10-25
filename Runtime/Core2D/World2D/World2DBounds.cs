using Darklight.UnityExt.Editor;
using Darklight.UnityExt.World;
using UnityEngine;

namespace Darklight.UnityExt.Core2D
{

    public class World2DBounds : ScriptableObject
    {
        const int DEFAULT_RANGE_VALUE = 25;

        public Color Color = Color.white;
        public Vector2 Center = new Vector2(0, 0);
        public SingleAxisBound XAxisBounds = new SingleAxisBound(WorldAxis.X, new Vector2(-DEFAULT_RANGE_VALUE, DEFAULT_RANGE_VALUE));
        public SingleAxisBound YAxisBounds = new SingleAxisBound(WorldAxis.Y, new Vector2(-DEFAULT_RANGE_VALUE, DEFAULT_RANGE_VALUE));

        public float Left { get => Center.x + XAxisBounds.Min; }
        public float Right { get => Center.x + XAxisBounds.Max; }
        public float Top { get => Center.y + YAxisBounds.Max; }
        public float Bottom { get => Center.y + YAxisBounds.Min; }

        public float Width { get => XAxisBounds.Max - XAxisBounds.Min; }
        public float Height { get => YAxisBounds.Max - YAxisBounds.Min; }


        public bool Contains(Vector2 point, float offset = 1)
        {
            return point.x >= Left + offset
                && point.x <= Right - offset
                && point.y >= Bottom + offset
                && point.y <= Top - offset;
        }

        public bool Contains(Vector2 point, Vector2 offset)
        {
            return point.x >= Left + offset.x
                && point.x <= Right - offset.x
                && point.y >= Bottom + offset.y
                && point.y <= Top - offset.y;
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

        public void DrawGizmos()
        {
            int lineThickness = 20;
            CustomGizmos.DrawLine(new Vector3(Left, Top, 0), new Vector3(Left, Bottom, 0), Color, lineThickness);
            CustomGizmos.DrawLine(new Vector3(Right, Top, 0), new Vector3(Right, Bottom, 0), Color, lineThickness);
            CustomGizmos.DrawLine(new Vector3(Left, Top, 0), new Vector3(Right, Top, 0), Color, lineThickness);
            CustomGizmos.DrawLine(new Vector3(Left, Bottom, 0), new Vector3(Right, Bottom, 0), Color, lineThickness);
        }
    }
}