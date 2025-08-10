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

        CountdownTimer _timer;

        public SensorSettings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        public string Key => _settings.name;
        public Vector3 Position => transform.position + Settings.OffsetPosition;
        public Transform Target => _target;
        public IEnumerable<Collider> DetectedColliders => _colliders;
        public IEnumerable<GameObject> DetectedObjects => _colliders.Select(c => c.gameObject);

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

        void UpdateColliders()
        {
            _colliders.Clear();
            if (Settings == null)
                return;

            // << DETECT COLLIDERS IN LAYER MASK >>
            if (Settings.IsBoxShape)
            {
                _colliders.AddRange(
                    Physics.OverlapBox(
                        Position,
                        Settings.BoxHalfExtents,
                        Quaternion.identity,
                        Settings.LayerMask
                    )
                );
            }
            else if (Settings.IsSphereShape)
            {
                _colliders.AddRange(
                    Physics.OverlapSphere(Position, Settings.SphereRadius, Settings.LayerMask)
                );
            }

            // << FILTER COLLIDERS BY TARGET TAGS >>
            if (Settings.TagFilter.Count > 0)
            {
                _colliders = _colliders.Where(c => Settings.TagFilter.Contains(c.tag)).ToList();
            }
        }

        void UpdateTarget()
        {
            if (Settings == null)
                return;

            if (Settings.TargetingType == TargetingType.FIRST)
            {
                _target = _colliders.First().transform;
            }
            else if (Settings.TargetingType == TargetingType.CLOSEST)
            {
                GetClosest(out Transform closest);
                _target = closest;
            }
        }

        IEnumerator DisableRoutine(float duration)
        {
            IsDisabled = true;
            yield return new WaitForSeconds(duration);
            IsDisabled = false;
        }

        #region < PROTECTED_METHODS > [[ HANDLERS ]] ================================================================
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

        #endregion

        #region < PUBLIC_METHODS > [[ GETTERS ]] ====================================================================

        public void GetClosest(out Transform closest)
        {
            if (_colliders.Count == 0)
            {
                closest = null;
                return;
            }

            closest = _colliders
                .OrderBy(c => (c.transform.position - Position).sqrMagnitude)
                .First()
                .transform;
        }

        public void GetClosestWithTag(string tag, out Transform closest)
        {
            if (_colliders.Count == 0)
            {
                closest = null;
                return;
            }

            closest = _colliders
                .Where(c => c.CompareTag(tag))
                .OrderBy(c => (c.transform.position - Position).sqrMagnitude)
                .First()
                .transform;
        }

        #endregion


        public virtual void StartTimedDisable(float duration)
        {
            if (IsDisabled)
                return;

            StartCoroutine(DisableRoutine(duration));
        }

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
            Handles.DrawWireCube(Position, Settings.BoxDimensions);
        }

        void DrawOverlapSphere(Color gizmoColor)
        {
            CustomGizmos.DrawWireSphere(Position, Settings.SphereRadius, gizmoColor);
        }

        void DrawLineToTarget(Color gizmoColor, GameObject target)
        {
            Handles.color = gizmoColor;
            Handles.DrawLine(Position, target.transform.position);
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
