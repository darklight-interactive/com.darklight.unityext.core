using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Darklight.Editor;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Behaviour
{
    [ExecuteAlways]
    public class Sensor : MonoBehaviour, ISensor
    {
        [SerializeField, Expandable, Required]
        [CreateAsset("NewSensorSettings", "Assets/Resources/Darklight/Behaviour/Sensor")]
        SensorSettings _settings;

        [HorizontalLine(color: EColor.Gray)]
        [SerializeField, ShowOnly]
        bool _isDisabled;

        [SerializeField, ReadOnly]
        List<Collider> _colliders = new List<Collider>();

        public SensorSettings Settings => _settings;
        public Vector3 Position => transform.position + Settings.OffsetPosition;
        public IEnumerable<Collider> Colliders => _colliders;
        public bool IsDisabled
        {
            get => _isDisabled;
            protected set => _isDisabled = value;
        }
        public bool IsColliding => _colliders.Any();

        #region < PRIVATE_METHODS > [[ UNITY METHODS ]] ================================================================
        void Update() => Execute();

        void OnDrawGizmos() => DrawGizmos();
        #endregion

        #region < PUBLIC_METHODS > [[ HANDLERS ]] ================================================================
        public virtual void Execute()
        {
            if (IsDisabled)
                return;

            _colliders = GetCurrentColliders().ToList();
        }

        public virtual void TimedDisable(float duration)
        {
            if (IsDisabled)
                return;

            StartCoroutine(DisableRoutine(duration));
        }
        #endregion


        #region < PUBLIC_METHODS > [[ GETTERS ]] ================================================================
        public void TryGetClosestCollider(out Collider closestCollider)
        {
            closestCollider = _colliders
                .OrderBy(c => Vector3.Distance(Position, c.transform.position))
                .FirstOrDefault();
        }

        public IEnumerable<Collider> GetCurrentColliders()
        {
            Collider[] colliders = new Collider[0];
            if (Settings == null)
                return colliders;

            if (Settings.IsBoxShape)
            {
                colliders = Physics.OverlapBox(
                    Position,
                    Settings.BoxHalfExtents,
                    Quaternion.identity,
                    Settings.LayerMask
                );
            }
            else if (Settings.IsSphereShape)
            {
                colliders = Physics.OverlapSphere(
                    Position,
                    Settings.SphereRadius,
                    Settings.LayerMask
                );
            }

            return colliders;
        }
        #endregion

        #region < PRIVATE_METHODS > [[ COROUTINES ]] ================================================================
        IEnumerator DisableRoutine(float duration)
        {
            IsDisabled = true;
            yield return new WaitForSeconds(duration);
            IsDisabled = false;
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
    }
}
