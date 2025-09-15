using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Darklight.Editor;
using ImprovedTimers;
using NaughtyAttributes;
using UnityEngine;
using UnityUtils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Behaviour.Sensor
{
    [ExecuteAlways]
    public partial class SensorBase : MonoBehaviour
    {
        [SerializeField, ShowOnly]
        bool _isDisabled;

        [HorizontalLine(color: EColor.Gray)]
        [SerializeField, Expandable]
        [CreateAsset("NewSensorSettings", AssetUtility.BEHAVIOUR_FILEPATH + "/SensorConfig")]
        SensorConfig _config;

        [SerializeField]
        List<Detector> _detectors = new List<Detector>();

        [SerializeField, Foldout("Debug")]
        [Tooltip("Enable gizmo visualization in editor")]
        bool _showGizmos = true;

        [SerializeField, Foldout("Debug")]
        [Tooltip("Color of the gizmo when the sensor is not colliding")]
        Color _defaultColor = Color.white;

        [SerializeField, Foldout("Debug")]
        [Tooltip("Color of the gizmo when the sensor is colliding")]
        Color _collidingColor = Color.green;

        [SerializeField, Foldout("Debug")]
        [Tooltip("Color of the gizmo when the sensor is not colliding")]
        Color _closestTargetColor = Color.red;

        public SensorConfig Config
        {
            get => _config;
            set => _config = value;
        }

        public string Key => _config.name;

        public bool IsDisabled
        {
            get => _isDisabled;
            protected set => _isDisabled = value;
        }

        bool IsValid => _config != null;

        void Update()
        {
            if (!IsValid)
                return;

            if (!Application.isPlaying && _detectors != null)
            {
                foreach (var detector in _detectors)
                {
                    detector.Execute(this);
                }
            }
        }

        void OnDrawGizmos() => DrawGizmos();

        void OnDrawGizmosSelected() => DrawGizmosSelected();

        IEnumerator DisableRoutine(float duration)
        {
            IsDisabled = true;
            yield return new WaitForSeconds(duration);
            IsDisabled = false;
        }

        void ApplyWhitelist(string[] whitelist, ref Collider[] colliders)
        {
            if (whitelist.Length == 0)
                return;

            colliders = colliders.Where(c => whitelist.Contains(c.tag)).ToArray();
        }

        public virtual bool ExecuteScan(
            SensorDetectionFilter filter,
            out SensorDetectionResult result
        )
        {
            result = new SensorDetectionResult();
            Collider target = null;
            Collider[] colliders = new Collider[0];

            // << NULL CHECKS >> ------------------------------------------------------------
            if (Config == null)
                return false;

            if (filter == null)
                return false;

            // << DETECT COLLIDERS IN LAYER MASK >> ------------------------------------------------------------
            if (Config.IsBoxShape)
            {
                colliders = Physics.OverlapBox(
                    transform.position,
                    Config.BoxHalfExtents,
                    Quaternion.identity,
                    filter.LayerMask
                );
            }
            else if (Config.IsSphereShape)
            {
                colliders = Physics.OverlapSphere(
                    transform.position,
                    Config.SphereRadius,
                    filter.LayerMask
                );
            }

            // << FILTER COLLIDERS BY PRIORITY TAGS >> ------------------------------------------------------------
            if (filter.WhitelistTags != null && filter.WhitelistTags.Length > 0)
                ApplyWhitelist(filter.WhitelistTags, ref colliders);

            // << NULL CHECKS >> ------------------------------------------------------------
            if (colliders == null || colliders.Length == 0)
                return false;

            // << SET TARGET BASED ON TARGETING TYPE >> ------------------------------------------------------------
            switch (filter.TargetingType)
            {
                case TargetingType.FIRST:
                    target = colliders.First();
                    break;
                case TargetingType.CLOSEST:
                    GetClosest(colliders, out target);
                    break;
            }

            // << SET RESULT >> ------------------------------------------------------------
            result = new SensorDetectionResult(target, colliders);
            return true;
        }

        public void StartTimedDisable(float duration)
        {
            if (IsDisabled)
                return;

            StartCoroutine(DisableRoutine(duration));
        }

        public void Enable() => IsDisabled = false;

        public void Disable() => IsDisabled = true;

        #region < PUBLIC_METHODS > [[ GETTERS ]] ====================================================================

        public void GetOrAddDetector(SensorDetectionFilter filter, out Detector detector)
        {
            detector = _detectors.FirstOrDefault(d => d.Filter == filter);
            if (detector == null)
            {
                detector = new Detector(filter);
                _detectors.Add(detector);
            }
        }

        public void GetClosest(Collider[] colliders, out Collider closest)
        {
            if (colliders.Length == 0)
            {
                closest = null;
                return;
            }

            closest = colliders
                .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude)
                .First();
        }

        #endregion

        #region [[ SETTERS ]] ====================================================================



        #endregion

#if UNITY_EDITOR
        public virtual void DrawGizmos()
        {
            if (Config == null || !_showGizmos)
                return;
        }

        public virtual void DrawGizmosSelected()
        {
            if (Config == null || !_showGizmos)
                return;

            // << DRAW DEFAULT >> ------------------------------------------------------------
            if (_detectors.Count == 0 || _detectors[0].IsValid == false)
            {
                Config.DrawGizmos(_defaultColor, transform.position);
                return;
            }

            // << DRAW OUTLINE >> ------------------------------------------------------------
            Color outlineColor = _defaultColor;
            if (_detectors[0].Result.HasColliders)
                outlineColor = _collidingColor;

            Config.DrawGizmos(outlineColor, transform.position);

            // << DRAW LINE TO TARGET >> ------------------------------------------------------------
            if (_detectors[0].Result.HasTarget)
                DrawLineToTarget(_closestTargetColor, _detectors[0].Result.Target.transform);
        }

        void DrawLineToTarget(Color gizmoColor, Transform target)
        {
            CustomGizmos.DrawLine(transform.position, target.position, gizmoColor);
            CustomGizmos.DrawSolidCircle(target.position, 0.1f, Vector3.up, gizmoColor);
        }
#endif

        [System.Serializable]
        public class Detector
        {
            [SerializeField, Expandable, AllowNesting]
            SensorDetectionFilter _filter;

            [SerializeField, ReadOnly, AllowNesting]
            SensorDetectionResult _result;

            public bool IsValid => _filter != null;
            public SensorDetectionFilter Filter => _filter;
            public SensorDetectionResult Result => _result;

            public Detector(SensorDetectionFilter filter)
            {
                _filter = filter;
                _result = new SensorDetectionResult();
            }

            public void Execute(SensorBase sensor)
            {
                sensor.ExecuteScan(_filter, out _result);
            }
        }
    }
}
