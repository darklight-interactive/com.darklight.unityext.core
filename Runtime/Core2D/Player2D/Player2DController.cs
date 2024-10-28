using System;
using System.Collections;

using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Input;

using UnityEditorInternal;

using UnityEngine;

namespace Darklight.UnityExt.Core2D.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player2DController : UniversalInputController, IUnityEditorListener
    {
        const string PREFIX = "<color=green>[Player2DController]</color> ";

        Rigidbody2D _rb;

        float _moveSpeed;




        [Header("State Machine")]
        [SerializeField] StateMachine _stateMachine;


        [Header("Settings")]
        [SerializeField] float _moveSpeedValue = 1f;
        [SerializeField] Vector2 _size = new Vector2(1, 1);

        #region < PRIVATE_METHODS > [[ PRELOAD ]] ================================================================ 
        void Preload()
        {
            // RIGIDBODY -------- >>
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0;
            _rb.angularDamping = 0;

            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // SETTINGS -------- >>
            _moveSpeed = _moveSpeedValue;

            // STATE MACHINE -------- >>
            _stateMachine = new StateMachine(this);

        }
        #endregion


        #region < PRIVATE_METHODS > [[ CONTROL MOVEMENT ]] ================================================================
        void SetPosition(Vector2 position)
        {
            transform.position = position;
        }



        #endregion

        #region < PRIVATE_METHODS > [[ UNITY RUNTIME ]] ================================================================ 

        protected override void Awake()
        {
            Preload();
            EnableInputs();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            _stateMachine.Step();
            _rb.linearVelocity = MoveInput * _moveSpeed;

        }
        #endregion

        #region < PUBLIC_METHODS > [[ EDITOR HANDLING ]] ================================================================ 
        public void OnEditorReloaded()
        {
            Preload();
        }
        #endregion


        enum PlayerState
        {
            IDLE,
            MOVING,
            STUNNED,
            DAMAGED,
            DEAD
        }

        [Serializable]
        class StateMachine : FiniteStateMachine<PlayerState>
        {
            Player2DController _controller;
            public StateMachine(Player2DController controller) : base()
            {
                _controller = controller;
                AddState(new IdleState(this));
                AddState(new MovingState(this));
                AddState(new StunnedState(this));
                AddState(new DamagedState(this));
                AddState(new DeadState(this));

                GoToState(PlayerState.IDLE);
            }

            abstract class BaseState : FiniteState<PlayerState>
            {
                protected Player2DController.StateMachine stateMachine;
                protected Player2DController controller;
                public BaseState(Player2DController.StateMachine stateMachine, PlayerState stateType) : base(stateMachine, stateType)
                {
                    this.stateMachine = stateMachine;
                    this.controller = stateMachine._controller;
                }
            }

            class IdleState : BaseState
            {
                public IdleState(StateMachine stateMachine) : base(stateMachine, PlayerState.IDLE) { }

                public override void Enter()
                {
                    controller._moveSpeed = 0;
                }

                public override void Execute()
                {
                    if (controller.MoveInput != Vector2.zero)
                    {
                        stateMachine.GoToState(PlayerState.MOVING);
                    }
                }
            }

            class MovingState : BaseState
            {
                public MovingState(StateMachine stateMachine) : base(stateMachine, PlayerState.MOVING) { }

                public override void Enter()
                {
                    controller._moveSpeed = controller._moveSpeedValue;
                }

                public override void Execute()
                {
                    if (Game2DManager.WorldBounds.Contains(controller.transform.position, controller._size))
                    {
                        controller._moveSpeed = controller._moveSpeedValue;
                    }
                    else
                    {
                        controller.SetPosition(Game2DManager.WorldBounds.ClosestPointWithinBounds(controller.transform.position, controller._size));
                        //StateMachine.GoToState(PlayerState.STUNNED);
                    }
                }
            }

            class StunnedState : BaseState
            {
                public StunnedState(StateMachine stateMachine) : base(stateMachine, PlayerState.STUNNED) { }

                public override void Enter()
                {
                    controller._moveSpeed = 0;
                    controller.StartCoroutine(ReturnToIdle());
                }

                IEnumerator ReturnToIdle()
                {
                    yield return new WaitForSeconds(0.25f);
                    stateMachine.GoToState(PlayerState.IDLE);
                }
            }

            class DamagedState : BaseState
            {
                public DamagedState(StateMachine stateMachine) : base(stateMachine, PlayerState.DAMAGED) { }

                public override void Enter()
                {
                    controller._moveSpeed = 0;
                }
            }

            class DeadState : BaseState
            {
                public DeadState(StateMachine stateMachine) : base(stateMachine, PlayerState.DEAD) { }

                public override void Enter()
                {
                    controller._moveSpeed = 0;
                }
            }
        }
    }
}
