using System.Collections.Generic;
using System.Linq;
using Darklight.Editor;
using Darklight.Utility;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Behaviour.Sensor
{
    public enum Swizzle_2D
    {
        XY,
        XZ,
        YZ
    }

    public enum Shape
    {
        RECT2D,
        CIRCLE2D,
        BOX3D,
        SPHERE3D
    }

    /// <summary>
    /// ScriptableObject containing all sensor-related settings for the survivor.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewSensorSettings",
        menuName = "Darklight/Behaviour/SensorSettings"
    )]
    public class SensorConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The shape of the sensor")]
        Shape _shape = Shape.CIRCLE2D;

        [SerializeField, ShowIf("Is2D")]
        Swizzle_2D _swizzle_2D = Swizzle_2D.XY;

        [SerializeField, ShowIf(EConditionOperator.And, "Is2D", "IsRect2D")]
        Vector2 _rectDimensions = new Vector2(1, 1);

        [SerializeField, ShowIf(EConditionOperator.And, "Is3D", "IsBox3D")]
        Vector3 _boxDimensions = new Vector3(1, 1, 1);

        [SerializeField, ShowIf(EConditionOperator.Or, "IsCircle2D", "IsSphere3D")]
        [Range(0.01f, 100f)]
        float _radius = 0.2f;

        bool Is2D => _shape == Shape.RECT2D || _shape == Shape.CIRCLE2D;
        bool Is3D => _shape == Shape.BOX3D || _shape == Shape.SPHERE3D;
        bool IsRect2D => _shape == Shape.RECT2D;
        bool IsCircle2D => _shape == Shape.CIRCLE2D;
        bool IsBox3D => _shape == Shape.BOX3D;
        bool IsSphere3D => _shape == Shape.SPHERE3D;

        public Shape Shape => _shape;
        public bool IsBoxShape => _shape == Shape.BOX3D || _shape == Shape.RECT2D;
        public bool IsSphereShape => _shape == Shape.SPHERE3D || _shape == Shape.CIRCLE2D;
        public Vector3 BoxDimensions => _boxDimensions;
        public Vector3 BoxHalfExtents => _boxDimensions / 2;
        public float SphereRadius => _radius;

        /// <summary>
        /// Validates the settings to ensure they are within acceptable ranges.
        /// </summary>
        void OnValidate()
        {
            _rectDimensions = new Vector2(
                Mathf.Max(0.01f, _rectDimensions.x),
                Mathf.Max(0.01f, _rectDimensions.y)
            );
            _boxDimensions = new Vector3(
                Mathf.Max(0.01f, _boxDimensions.x),
                Mathf.Max(0.01f, _boxDimensions.y),
                Mathf.Max(0.01f, _boxDimensions.z)
            );
            _radius = Mathf.Max(0.01f, _radius);
        }

        Vector3 GetNormal(Swizzle_2D swizzle)
        {
            switch (swizzle)
            {
                case Swizzle_2D.XY:
                    return new Vector3(0, 0, 1);
                case Swizzle_2D.XZ:
                    return new Vector3(0, 1, 0);
                case Swizzle_2D.YZ:
                    return new Vector3(1, 0, 0);
                default:
                    return Vector3.zero;
            }
        }

        public void DrawGizmos(Color gizmoColor, Vector3 position)
        {
            if (IsRect2D)
            {
                CustomGizmos.DrawWireRect(
                    position,
                    _rectDimensions,
                    GetNormal(_swizzle_2D),
                    gizmoColor
                );
            }
            else if (IsCircle2D)
            {
                CustomGizmos.DrawWireCircle(position, _radius, GetNormal(_swizzle_2D), gizmoColor);
            }
            else if (IsBox3D)
            {
                CustomGizmos.DrawWireCube(
                    position,
                    _boxDimensions,
                    Quaternion.identity,
                    gizmoColor
                );
            }
            else if (IsSphere3D)
            {
                CustomGizmos.DrawWireSphere(position, _radius, gizmoColor);
            }
        }

        #region [[ CUSTOM EDITOR ]] ================================================================

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(SensorConfig))]
        public class SensorPresetCustomEditor : UnityEditor.Editor
        {
            SerializedObject _serializedObject;
            SensorConfig _script;

            private void OnEnable()
            {
                _serializedObject = new SerializedObject(target);
                _script = (SensorConfig)target;
            }

            public override void OnInspectorGUI()
            {
                _serializedObject.Update();

                EditorGUI.BeginChangeCheck();

                base.OnInspectorGUI();

                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();
                }
            }
        }
#endif

        #endregion
    }
}
