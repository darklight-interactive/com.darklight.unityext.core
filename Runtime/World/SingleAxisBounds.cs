using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Darklight.Editor;
using UnityEngine;

namespace Darklight.World
{
    public enum WorldAxis
    {
        X,
        Y,
        Z
    }

    [Serializable]
    public class SingleAxisBounds
    {
        readonly Vector2 valueRange;

        [SerializeField, ShowOnly]
        WorldAxis _axis;

        [SerializeField, DynamicRange("valueRange")]
        float _min;

        [SerializeField, DynamicRange("valueRange")]
        float _max;

        [SerializeField]
        bool _showPlaneGizmo;

        public WorldAxis Axis
        {
            get => _axis;
            set => _axis = value;
        }
        public float Min
        {
            get { return _min = Mathf.Clamp(_min, valueRange.x, valueRange.y); }
            set { _min = Mathf.Clamp(value, valueRange.x, valueRange.y); }
        }
        public float Max
        {
            get { return _max = Mathf.Clamp(_max, valueRange.x, valueRange.y); }
            set { _max = Mathf.Clamp(value, valueRange.x, valueRange.y); }
        }
        public float Length => Mathf.Abs(Max - Min);

        public SingleAxisBounds()
        {
            _axis = WorldAxis.X;
            valueRange = new Vector2(-1000, 1000);
            Min = valueRange.x / 2;
            Max = valueRange.y / 2;
        }

        public SingleAxisBounds(WorldAxis axis, Vector2 range)
        {
            _axis = axis;
            valueRange = range;
            Min = range.x / 2;
            Max = range.y / 2;
        }

        void GetAxisVector(out Vector3 direction)
        {
            direction = Vector3.zero;
            switch (_axis)
            {
                case WorldAxis.X:
                    direction = Vector3.right;
                    break;
                case WorldAxis.Y:
                    direction = Vector3.up;
                    break;
                case WorldAxis.Z:
                    direction = Vector3.forward;
                    break;
            }
        }

        void GetAxisNormal(out Vector3 normal)
        {
            normal = Vector3.zero;
            switch (_axis)
            {
                case WorldAxis.X:
                    normal = Vector3.forward;
                    break;
                case WorldAxis.Y:
                    normal = Vector3.right;
                    break;
                case WorldAxis.Z:
                    normal = Vector3.up;
                    break;
            }
        }

        void GetAxisColor(out Color color)
        {
            color = Color.white;
            switch (_axis)
            {
                case WorldAxis.X:
                    color = Color.red;
                    break;
                case WorldAxis.Y:
                    color = Color.green;
                    break;
                case WorldAxis.Z:
                    color = Color.blue;
                    break;
            }
        }

        public void GetWorldValues(Vector3 center, out float min, out float max)
        {
            min = center.x;
            max = center.x;

            switch (_axis)
            {
                case WorldAxis.X:
                    min = center.x + Min;
                    max = center.x + Max;
                    break;
                case WorldAxis.Y:
                    min = center.y + Min;
                    max = center.y + Max;
                    break;
                case WorldAxis.Z:
                    min = center.z + Min;
                    max = center.z + Max;
                    break;
            }
        }

        public void DrawGizmos(Vector3 origin, float length)
        {
            GetAxisVector(out Vector3 direction);
            GetAxisColor(out Color color);

            // Calculate min and max points along the current axis
            Vector3 minPoint = origin + direction * Min;
            Vector3 maxPoint = origin + direction * Max;

            // Draw the min and max points
            CustomGizmos.DrawLine(minPoint, maxPoint, color);

            if (_showPlaneGizmo)
            {
                GetAxisNormal(out Vector3 normal);
                // Draw the plane parallel to the current axis
                Color planeColor = new Color(color.r, color.g, color.b, 0.2f);
                CustomGizmos.DrawSolidRect(origin, new Vector2(length, length), normal, planeColor);
            }
        }
    }
}
