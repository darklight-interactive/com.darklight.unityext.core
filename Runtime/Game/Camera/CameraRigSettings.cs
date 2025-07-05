using System;
using Darklight.Editor;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.Game
{
    [Serializable]
    public class CameraRigSettings
    {
        readonly Vector2 POSITION_RANGE = new Vector2(-100, 100);
        readonly Vector2 ROTATION_RANGE = new Vector2(-180, 180);
        readonly Vector2 SPEED_RANGE = new Vector2(0, 100);

        public enum ProjectionType
        {
            PERPSECTIVE,
            ORTHOGRAPHIC
        }

        [Header("Projection")]
        [SerializeField]
        ProjectionType _projection = ProjectionType.PERPSECTIVE;

        [SerializeField, ShowIf("IsPerspective"), AllowNesting, Range(0.1f, 190)]
        float _fov = 90f;

        [SerializeField, HideIf("IsPerspective"), AllowNesting, Range(0.1f, 100)]
        float _orthographicSize = 10f;

        [SerializeField, DynamicRange("SPEED_RANGE")]
        float _fovSpeed = 10f;

        [Header("Position")]
        [SerializeField, DynamicRange("SPEED_RANGE")]
        float _posSpeed = 10f;

        [SerializeField, DynamicRange("POSITION_RANGE")]
        float _positionOffsetX = 0f;

        [SerializeField, DynamicRange("POSITION_RANGE")]
        float _positionOffsetY = 10f;

        [SerializeField, DynamicRange("POSITION_RANGE")]
        float _positionOffsetZ = -10f;

        [Header("Rotation")]
        [SerializeField]
        bool _lookAtTarget;

        [SerializeField, DynamicRange("SPEED_RANGE")]
        float _rotSpeed = 10f;

        [SerializeField, ShowIf("LookAtTarget"), AllowNesting, DynamicRange("ROTATION_RANGE")]
        float _orbitAngle = 0f;

        [SerializeField, DynamicRange("ROTATION_RANGE")]
        float _rotOffsetX = 0f;

        [SerializeField, HideIf("LookAtTarget"), AllowNesting, DynamicRange("ROTATION_RANGE")]
        float _rotOffsetY = 0f;

        [SerializeField, HideIf("LookAtTarget"), AllowNesting, DynamicRange("ROTATION_RANGE")]
        float _rotOffsetZ = 0f;

        // << PROPERTIES >> -------------------------------------------------

        public ProjectionType Projection => _projection;
        public bool IsPerspective => _projection == ProjectionType.PERPSECTIVE;

        public float PerspectiveFOV => _fov;
        public float OrthographicSize => _orthographicSize;
        public float FOVSpeed => _fovSpeed;
        public float PosSpeed => _posSpeed;
        public float PositionOffsetX => _positionOffsetX;
        public float PositionOffsetY => _positionOffsetY;
        public float PositionOffsetZ => _positionOffsetZ;
        public bool LookAtTarget => _lookAtTarget;
        public float RotSpeed => _rotSpeed;
        public float OrbitAngle => _orbitAngle;
        public float RotOffsetX => _rotOffsetX;
        public float RotOffsetY => _rotOffsetY;
        public float RotOffsetZ => _rotOffsetZ;
    }
}
