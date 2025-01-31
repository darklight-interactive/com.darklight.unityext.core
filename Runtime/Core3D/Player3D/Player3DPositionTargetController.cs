using System;
using System.Collections;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Input;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Core3D.Player
{
    /// <summary>
    /// Controls a 3D player character with physics-based movement and state management.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Player3DPositionTargetController : MouseClickInputListener, IUnityEditorListener
    {
        private const string PREFIX = "<color=blue>[Player3DController]</color> ";

        Rigidbody _rb;
        Vector3 _targetPosition;
        Vector3 _targetDirection;
        Quaternion _targetRotation;
        Vector3 _currentPosition;
        Vector3 _currentDirection;
        Quaternion _currentRotation;

        [Header("Settings")]
        [SerializeField]
        private MovementSettings _settings;

        [Header("State Machine")]
        [SerializeField]
        private StateMachine _stateMachine;

        [SerializeField]
        private Vector3 _size = new Vector3(1, 1, 1);

        [SerializeField]
        private bool _useGravity = true;

        protected MovementSettings settings => _settings;

        #region < PRIVATE_METHODS > [[ PRELOAD ]] ================================================================
        private void Preload()
        {
            // RIGIDBODY -------- >>
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = _useGravity;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            // STATE MACHINE -------- >>
            _stateMachine = new StateMachine(this);
        }
        #endregion

        #region < PRIVATE_METHODS > [[ CONTROL MOVEMENT ]] ================================================================

        private void SetTargetPosition(Vector3 position)
        {
            _targetPosition = position;
            _targetPosition.y = 0;

            _targetDirection = (position - transform.position).normalized;
            _targetRotation = Quaternion.LookRotation(_targetDirection);
        }

        private void HandleMovement()
        {
            if (_targetPosition == null)
                return;

            _currentPosition = transform.position;
            _currentPosition = Vector3.Lerp(
                _currentPosition,
                _targetPosition,
                _settings.moveSpeed * Time.deltaTime
            );

            _rb.MovePosition(_currentPosition);
        }

        private void HandleRotation()
        {
            if (_targetRotation != null)
            {
                _currentRotation = transform.rotation;
                _currentRotation = Quaternion.Slerp(
                    _currentRotation,
                    _targetRotation,
                    _settings.rotateSpeed * Time.deltaTime
                );

                transform.rotation = _currentRotation;
            }
        }

        #endregion

        #region < PRIVATE_METHODS > [[ UNITY RUNTIME ]] ================================================================
        protected void OnEditorReloaded()
        {
            Preload();
        }

        protected override void Awake()
        {
            base.Awake();

            Preload();
            EnableInputs();

            UniversalInputManager.OnMousePrimaryClick += () =>
                SetTargetPosition(CurrentMouseWorldPosition);
        }

        private void Update()
        {
            HandleMovement();
            _stateMachine.Step();
        }

        private void FixedUpdate()
        {
            HandleRotation();
        }
        #endregion

        #region < PUBLIC_METHODS > [[ SETTERS ]] ================================================================


        #endregion

        private enum PlayerState
        {
            IDLE,
            MOVING,
            JUMPING,
            FALLING,
            STUNNED,
            DAMAGED,
            DEAD
        }

        [Serializable]
        private class StateMachine : FiniteStateMachine<PlayerState>
        {
            private Player3DPositionTargetController _controller;

            public StateMachine(Player3DPositionTargetController controller)
                : base()
            {
                _controller = controller;
                AddState(new IdleState(this));
                AddState(new MovingState(this));

                GoToState(PlayerState.IDLE);
            }

            private abstract class BaseState : FiniteState<PlayerState>
            {
                protected Player3DPositionTargetController.StateMachine stateMachine;
                protected Player3DPositionTargetController controller;

                protected BaseState(
                    Player3DPositionTargetController.StateMachine stateMachine,
                    PlayerState stateType
                )
                    : base(stateType)
                {
                    this.stateMachine = stateMachine;
                    this.controller = stateMachine._controller;
                }
            }

            private class IdleState : BaseState
            {
                public IdleState(StateMachine stateMachine)
                    : base(stateMachine, PlayerState.IDLE) { }

                public override void Enter() { }

                public override void Execute()
                {
                    if (controller.MoveInput != Vector2.zero)
                    {
                        stateMachine.GoToState(PlayerState.MOVING);
                    }
                }
            }

            private class MovingState : BaseState
            {
                public MovingState(StateMachine stateMachine)
                    : base(stateMachine, PlayerState.MOVING) { }

                public override void Enter() { }

                public override void Execute()
                {
                    if (controller.MoveInput == Vector2.zero)
                    {
                        stateMachine.GoToState(PlayerState.IDLE);
                    }
                }
            }
        }

        [Serializable]
        protected class MovementSettings
        {
            [SerializeField]
            float _moveSpeed;

            [SerializeField]
            float _moveAcceleration;

            [SerializeField]
            float _rotateSpeed;

            public float moveSpeed => _moveSpeed;
            public float rotateSpeed => _rotateSpeed;
            public float moveAcceleration => _moveAcceleration;
        }
    }
}
