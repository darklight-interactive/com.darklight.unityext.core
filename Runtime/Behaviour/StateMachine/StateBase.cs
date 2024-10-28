using System;

using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
    [System.Serializable]
    public abstract class StateBase<TEnum> : IState<TEnum>
        where TEnum : Enum
    {
        [SerializeField] TEnum _stateType;

        public TEnum StateType { get => _stateType; set => _stateType = value; }
        public StateMachineBase<TEnum> StateMachine { get; set; }

        public StateBase(StateMachineBase<TEnum> stateMachine, TEnum stateType)
        {
            StateMachine = stateMachine;
            StateType = stateType;
        }

        public abstract void Enter();
        public abstract void Execute();
        public abstract void Exit();
    }
}

