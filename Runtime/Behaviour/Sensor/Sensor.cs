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
    public class Sensor : MonoBehaviour
    {
        [SerializeField, Expandable, Required]
        [CreateAsset("NewSensorSettings", "Assets/Resources/Darklight/Behaviour/Sensor")]
        SensorSettings _settings;

        [HorizontalLine(color: EColor.Gray)]
        [SerializeField, ShowOnly]
        bool _isDisabled;

        [SerializeField, ReadOnly]
        List<Collider> _colliders = new List<Collider>();

        CountdownTimer _timer;

        public SensorSettings Settings
        {
            get => _settings;
            set => _settings = value;
        }
        public Vector3 Position => transform.position + Settings.OffsetPosition;
        public IEnumerable<Collider> Colliders => _colliders;

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
                _timer.Start();
            };
            _timer.Start();
        }

        protected virtual void Execute()
        {
            if (IsDisabled)
                return;

            UpdateColliders();
            _timer?.Tick();
        }

        #endregion

        #region < PUBLIC_METHODS > [[ GETTERS ]] ====================================================================

        public Collider GetClosest()
        {
            if (_colliders.Count == 0)
                return null;

            return _colliders.OrderBy(c => (c.transform.position - Position).sqrMagnitude).First();
        }

        public Collider GetClosestWithTag(string tag)
        {
            if (_colliders.Count == 0)
                return null;

            return _colliders
                .Where(c => c.CompareTag(tag))
                .OrderBy(c => (c.transform.position - Position).sqrMagnitude)
                .First();
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

            Color color = Settings.DebugDefaultColor;
            if (IsColliding && !IsDisabled)
                color = Settings.DebugCollidingColor;

            if (Settings.IsBoxShape)
                DrawOverlapBox(color);
            else if (Settings.IsSphereShape)
                DrawOverlapSphere(color);
        }

        public virtual void DrawGizmosSelected()
        {
            if (Settings == null || !Settings.ShowDebugGizmos)
                return;

            var closest = GetClosest();
            if (closest != null)
                DrawLineToTarget(Settings.DebugClosestTargetColor, closest.gameObject);

            foreach (var collider in _colliders)
            {
                if (collider == closest)
                    continue;

                DrawLineToTarget(Settings.DebugDefaultColor, collider.gameObject);
            }
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
