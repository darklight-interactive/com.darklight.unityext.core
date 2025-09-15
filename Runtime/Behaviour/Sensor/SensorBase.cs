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

        [SerializeField, Expandable]
        [CreateAsset("NewScanFilter", AssetUtility.BEHAVIOUR_FILEPATH + "/ScanFilter")]
        ScanFilter _defaultScanFilter;

        [SerializeField, ReadOnly]
        ScanResult _defaultScanResult;

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

        public ScanResult DefaultScanResult => _defaultScanResult;
        public ScanFilter DefaultScanFilter => _defaultScanFilter;

        bool IsValid => _config != null;

        void Update()
        {
            if (!IsValid)
                return;

            if (!Application.isPlaying && _defaultScanFilter != null)
            {
                ExecuteScan(_defaultScanFilter, out _defaultScanResult);
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

        protected virtual bool ExecuteScan(ScanFilter filter, out ScanResult result)
        {
            result = new ScanResult();
            Collider target = null;
            List<Collider> colliders = new List<Collider>();

            // << NULL CHECKS >> ------------------------------------------------------------
            if (Config == null)
                return false;

            if (filter == null)
                return false;

            // << DETECT COLLIDERS IN LAYER MASK >> ------------------------------------------------------------
            if (Config.IsBoxShape)
            {
                colliders.AddRange(
                    Physics.OverlapBox(
                        transform.position,
                        Config.BoxHalfExtents,
                        Quaternion.identity,
                        filter.LayerMask
                    )
                );
            }
            else if (Config.IsSphereShape)
            {
                colliders.AddRange(
                    Physics.OverlapSphere(transform.position, Config.SphereRadius, filter.LayerMask)
                );
            }

            // << FILTER COLLIDERS BY PRIORITY TAGS >> ------------------------------------------------------------
            if (filter.PriorityTagComparator.PriorityTags.Count > 0)
                filter.PriorityTagComparator.FilterCollidersWithHighestPriority(ref colliders);

            // << NULL CHECKS >> ------------------------------------------------------------
            if (colliders == null || colliders.Count == 0)
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
            result = new ScanResult(target, colliders);
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

        public void GetScanResult(ScanFilter filter, out ScanResult result)
        {
            ExecuteScan(filter, out result);
        }

        public void GetClosest(List<Collider> colliders, out Collider closest)
        {
            if (colliders.Count == 0)
            {
                closest = null;
                return;
            }

            closest = colliders
                .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude)
                .First();
        }

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

            // << DRAW OUTLINE >> ------------------------------------------------------------
            Color outlineColor = _defaultColor;
            if (DefaultScanResult.HasColliders)
                outlineColor = _collidingColor;

            Config.DrawGizmos(outlineColor, transform.position);

            // << DRAW LINE TO TARGET >> ------------------------------------------------------------
            DrawLineToTarget(_closestTargetColor, DefaultScanResult.Target.transform);
        }

        void DrawLineToTarget(Color gizmoColor, Transform target)
        {
            Handles.color = gizmoColor;
            CustomGizmos.DrawLine(transform.position, target.position, gizmoColor);
            CustomGizmos.DrawSolidCircle(target.position, 0.1f, Vector3.up, gizmoColor);
        }
#endif
    }
}
