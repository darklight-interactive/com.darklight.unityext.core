using System.Collections.Generic;
using Darklight.Editor;
using UnityEngine;

namespace Darklight.Core2D
{
    [System.Serializable]
    public class Shape2D
    {
        public const int MAX_SEGMENTS = 32;

        // --------- Private Fields --------- ))
        Vector3[] _vertices;

        [SerializeField, ShowOnly]
        Vector3 _center = Vector3.zero;

        [SerializeField, ShowOnly]
        float _radius = 64;

        [SerializeField, ShowOnly]
        int _segments = 16;

        [SerializeField, ShowOnly]
        Vector3 _normal = Vector3.up;

        [SerializeField, ShowOnly]
        Color _gizmoColor = Color.white;

        // --------- References --------- ))
        public Vector3[] vertices => _vertices;
        public Vector3 Center
        {
            get => _center;
            protected set => _center = value;
        }
        public float Radius
        {
            get => _radius;
            protected set => _radius = value;
        }
        public int Segments
        {
            get => _segments;
            protected set => _segments = value;
        }
        public Vector3 Normal
        {
            get => _normal;
            protected set => _normal = value;
        }
        public Color GizmoColor
        {
            get => _gizmoColor;
            protected set => _gizmoColor = value;
        }

        public Shape2D()
        {
            _vertices = Shape2DUtility.GenerateRadialPoints(_center, _radius, _normal, _segments);
        }

        public Shape2D(Vector3 center, float radius, int segments, Vector3 normal, Color gizmoColor)
        {
            UpdateShape(center, radius, segments, normal, gizmoColor);
        }

        public void UpdateShape(
            Vector3 center,
            float radius,
            int segments,
            Vector3 normal,
            Color gizmoColor
        )
        {
            _center = center;
            _radius = radius;

            if (segments > MAX_SEGMENTS)
                segments = MAX_SEGMENTS;
            _segments = segments;

            _normal = normal;
            _gizmoColor = gizmoColor;

            _vertices = Shape2DUtility.GenerateRadialPoints(_center, _radius, _normal, _segments);
        }

        public void SetSegments(int segments)
        {
            if (segments > MAX_SEGMENTS)
                segments = MAX_SEGMENTS;
            _segments = segments;
            _vertices = Shape2DUtility.GenerateRadialPoints(_center, _radius, _normal, _segments);
        }

        public void SetRadius(float radius)
        {
            _radius = radius;
            _vertices = Shape2DUtility.GenerateRadialPoints(_center, _radius, _normal, _segments);
        }

        public void SetGizmoColor(Color color)
        {
            _gizmoColor = color;
        }

#if UNITY_EDITOR
        public void DrawGizmos(bool filled)
        {
            if (filled)
                CustomGizmos.DrawSolidRadialShape(
                    _center,
                    _radius,
                    Quaternion.FromToRotation(Vector3.up, _normal),
                    _segments,
                    _gizmoColor
                );
            else
                CustomGizmos.DrawWireRadialShape(
                    _center,
                    _radius,
                    Quaternion.FromToRotation(Vector3.up, _normal),
                    _segments,
                    _gizmoColor
                );
        }
#endif

        public bool IsPositionWithinRadius(Vector3 position)
        {
            return Vector3.Distance(_center, position) <= _radius;
        }
    }
}
