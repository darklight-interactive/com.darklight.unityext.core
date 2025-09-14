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

namespace Darklight.Behaviour
{
    [ExecuteAlways]
    public partial class Sensor : MonoBehaviour
    {
        [SerializeField, Expandable]
        [CreateAsset("NewSensorSettings", "Assets/Resources/Darklight/Behaviour/Sensor")]
        SensorSettings _settings;

        [HorizontalLine(color: EColor.Gray)]
        [SerializeField, ShowOnly]
        bool _isDisabled;

        [SerializeField, ReadOnly]
        Transform _target;

        [SerializeField, ReadOnly]
        List<Collider> _colliders = new List<Collider>();

        [SerializeField, ReadOnly]
        List<Collider> _priorityColliders = new List<Collider>();

        CountdownTimer _timer;

        public SensorSettings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        public string Key => _settings.name;
        public Transform Target => _target;

        public bool IsDisabled
        {
            get => _isDisabled;
            protected set => _isDisabled = value;
        }
        public bool IsColliding => _colliders.Any();

        #region < PRIVATE_METHODS > [[ UNITY METHODS ]] ================================================================
        void Start() => Initialize();

        void Update() => Execute();

        void OnDrawGizmos() => DrawGizmos();

        void OnDrawGizmosSelected() => DrawGizmosSelected();
        #endregion


        #region [[ HANDLER METHODS ]] ================================================================



        IEnumerator DisableRoutine(float duration)
        {
            IsDisabled = true;
            yield return new WaitForSeconds(duration);
            IsDisabled = false;
        }

        protected virtual void Initialize()
        {
            if (Settings == null)
                return;

            _timer = new CountdownTimer(Settings.TimerInterval);

            // When the timer stops, update the target position and start the timer again
            _timer.OnTimerStop += () =>
            {
                UpdateColliders();
                UpdateTarget();

                _timer.Start();
            };
            _timer.Start();
        }

        protected virtual void Execute()
        {
            if (IsDisabled)
                return;

            if (!Application.isPlaying)
            {
                UpdateColliders();
                UpdateTarget();
                return;
            }

            _timer?.Tick();
        }

        protected virtual void UpdateColliders()
        {
            _colliders.Clear();
            if (Settings == null)
                return;

            // << DETECT COLLIDERS IN LAYER MASK >>
            if (Settings.IsBoxShape)
            {
                _colliders.AddRange(
                    Physics.OverlapBox(
                        transform.position,
                        Settings.BoxHalfExtents,
                        Quaternion.identity,
                        Settings.LayerMask
                    )
                );
            }
            else if (Settings.IsSphereShape)
            {
                _colliders.AddRange(
                    Physics.OverlapSphere(
                        transform.position,
                        Settings.SphereRadius,
                        Settings.LayerMask
                    )
                );
            }

            // << FILTER COLLIDERS BY PRIORITY TAGS >>
            Settings.PriorityTagComparator.GetCollidersWithHighestPriority(
                _colliders,
                out _priorityColliders
            );

            // << OVERRIDE COLLIDERS WITH PRIORITY COLLIDERS >>
            if (Settings.PriorityTagComparator.PriorityTags.Count > 0)
                _colliders = _priorityColliders;
        }

        protected virtual void UpdateTarget()
        {
            // << NULL CHECKS >>
            if (_settings == null || _colliders == null || _colliders.Count == 0)
            {
                _target = null;
                return;
            }

            // << SET TARGET BASED ON TARGETING TYPE >>
            switch (Settings.TargetingType)
            {
                case TargetingType.FIRST:
                    _target = _colliders.First().transform;
                    break;
                case TargetingType.CLOSEST:
                    GetClosest(_colliders, out Transform closest);
                    _target = closest;
                    break;
            }
        }

        public void StartTimedDisable(float duration)
        {
            if (IsDisabled)
                return;

            StartCoroutine(DisableRoutine(duration));
        }

        public void Enable() => IsDisabled = false;

        public void Disable() => IsDisabled = true;

        #endregion

        #region < PUBLIC_METHODS > [[ GETTERS ]] ====================================================================

        public void GetClosest(List<Collider> colliders, out Transform closest)
        {
            if (colliders.Count == 0)
            {
                closest = null;
                return;
            }

            closest = colliders
                .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude)
                .First()
                .transform;
        }

        public void GetDetectedColliders(out List<Collider> colliders)
        {
            colliders = _colliders;
        }

        public void GetDetectedObjects(out List<GameObject> objects)
        {
            objects = _colliders.Select(c => c.gameObject).ToList();
        }

        #endregion

#if UNITY_EDITOR
        public virtual void DrawGizmos()
        {
            if (Settings == null || !Settings.ShowDebugGizmos)
                return;
        }

        public virtual void DrawGizmosSelected()
        {
            if (Settings == null || !Settings.ShowDebugGizmos)
                return;

            Color collisionColor = Settings.DebugDefaultColor;
            if (IsColliding && !IsDisabled)
                collisionColor = Settings.DebugCollidingColor;

            if (Settings.IsBoxShape)
                DrawOverlapBox(collisionColor);
            else if (Settings.IsSphereShape)
                DrawOverlapSphere(collisionColor);

            if (Target != null)
                DrawLineToTarget(Settings.DebugTargetColor, Target.gameObject);

            /*
            foreach (var collider in _colliders)
            {
                if (collider == closest)
                    continue;

                DrawLineToTarget(Settings.DebugDefaultColor, collider.gameObject);
            }
            */
        }

        void DrawOverlapBox(Color gizmoColor)
        {
            Handles.color = gizmoColor;
            Handles.DrawWireCube(transform.position, Settings.BoxDimensions);
        }

        void DrawOverlapSphere(Color gizmoColor)
        {
            CustomGizmos.DrawWireSphere(transform.position, Settings.SphereRadius, gizmoColor);
        }

        void DrawLineToTarget(Color gizmoColor, GameObject target)
        {
            Handles.color = gizmoColor;
            Handles.DrawLine(transform.position, target.transform.position);
            Handles.DrawSolidDisc(target.transform.position, Vector3.up, 0.1f);
        }
#endif

        public enum Shape
        {
            BOX,
            SPHERE
        }
    }
}
