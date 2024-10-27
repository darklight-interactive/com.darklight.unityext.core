using System;

using Darklight.UnityExt.Editor;

using NaughtyAttributes;

using UnityEngine;

namespace Darklight.UnityExt.Game
{

    [Serializable]
    public class CameraRigSettings
    {
        readonly Vector2 POSITION_RANGE = new Vector2(-100, 100);
        readonly Vector2 ROTATION_RANGE = new Vector2(-180, 180);
        readonly Vector2 SPEED_RANGE = new Vector2(0, 100);

        public ProjectionType Projection = ProjectionType.PERPSECTIVE;

        [Header("Position")]
        public float PosSpeed = 10f;
        [DynamicRange("POSITION_RANGE")] public float PositionOffsetX = 0f;
        [DynamicRange("POSITION_RANGE")] public float PositionOffsetY = 10f;
        [DynamicRange("POSITION_RANGE")] public float PositionOffsetZ = -10f;

        [Header("Rotation")]
        public bool LookAtTarget;
        [SerializeField] public float RotSpeed = 10f;
        [ShowIf("LookAtTarget"), AllowNesting, DynamicRange("ROTATION_RANGE")] public float OrbitAngle = 0f;
        [DynamicRange("ROTATION_RANGE")] public float RotOffsetX = 0f;
        [HideIf("LookAtTarget"), AllowNesting, DynamicRange("ROTATION_RANGE")] public float RotOffsetY = 0f;
        [HideIf("LookAtTarget"), AllowNesting, DynamicRange("ROTATION_RANGE")] public float RotOffsetZ = 0f;

        [Header("Field of View")]
        [SerializeField, Range(0.1f, 190)] public float FOV = 90f;
        [SerializeField] public float FOVSpeed = 10f;

        public enum ProjectionType { PERPSECTIVE, ORTHOGRAPHIC }
    }
}
