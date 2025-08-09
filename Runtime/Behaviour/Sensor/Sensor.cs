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

        [SerializeField, ReadOnly]
        GameObject _targetObject;

        [SerializeField, ReadOnly]
        Vector3 _targetLastKnownPosition;

        CountdownTimer _timer;

        public SensorSettings Settings => _settings;
        public Vector3 Position => transform.position + Settings.OffsetPosition;
        public IEnumerable<Collider> Colliders => _colliders;
        public GameObject Target => _targetObject;
        public Vector3 TargetPosition =>
            _targetObject ? _targetObject.transform.position : Vector3.zero;
        public bool IsTargetInRange => TargetPosition != Vector3.zero;

        public bool IsDisabled
        {
            get => _isDisabled;
            protected set => _isDisabled = value;
        }
        public bool IsColliding => _colliders.Any();

        public event Action OnTargetChanged = delegate { };

        #region < PRIVATE_METHODS > [[ UNITY METHODS ]] ================================================================
        void Start() => Initialize();

        void Update() => Execute();

        void OnDrawGizmos() => DrawGizmos();

        void OnDrawGizmosSelected() => DrawGizmosSelected();
        #endregion

        GameObject GetTarget()
        {
            if (_colliders.Count == 0)
                return null;
            return _colliders.First().gameObject;
        }

        void UpdateTargetPosition(GameObject target = null)
        {
            this._targetObject = target;

            // If the target is in range and the target position has changed, or the target position is not zero,
            // then invoke the OnTargetChanged event
            if (
                IsTargetInRange
                && (
                    _targetLastKnownPosition != TargetPosition
                    || _targetLastKnownPosition != Vector3.zero
                )
            )
            {
                _targetLastKnownPosition = TargetPosition;
                OnTargetChanged?.Invoke();
            }
        }

        void UpdateColliders()
        {
            _colliders.Clear();
            if (Settings == null)
                return;

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
                var newTarget = GetTarget();
                if (newTarget != _targetObject)
                {
                    _targetObject = newTarget;
                }

                UpdateTargetPosition(newTarget);
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

        public virtual void TimedDisable(float duration)
        {
            if (IsDisabled)
                return;

            StartCoroutine(DisableRoutine(duration));
        }
        #endregion

#if UNITY_EDITOR
        public virtual void DrawGizmos()
        {
            if (Settings == null || !Settings.ShowDebugGizmos)
                return;

            Color color = Color.gray;
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

            if (_targetObject != null)
            {
                Handles.color = Settings.DebugCollidingColor;
                Handles.DrawLine(Position, _targetLastKnownPosition);
                Handles.DrawSolidDisc(_targetLastKnownPosition, Vector3.up, 0.1f);
            }
        }

        protected virtual void DrawOverlapBox(Color gizmoColor)
        {
            Handles.color = gizmoColor;
            Handles.DrawWireCube(Position, Settings.BoxDimensions);
        }

        protected virtual void DrawOverlapSphere(Color gizmoColor)
        {
            CustomGizmos.DrawWireSphere(Position, Settings.SphereRadius, gizmoColor);
        }
#endif

        public enum Shape
        {
            BOX,
            SPHERE
        }
    }
}
