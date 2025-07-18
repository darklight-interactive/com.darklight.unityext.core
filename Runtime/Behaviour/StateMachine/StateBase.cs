using System;
using Darklight.Editor;
using UnityEngine;

namespace Darklight.Behaviour
{
    [System.Serializable]
    public abstract class StateBase<TEnum> : IState<TEnum>
        where TEnum : Enum
    {
        [SerializeField, ShowOnly]
        TEnum _stateType;

        public TEnum StateType => _stateType;

        public abstract void Enter();
        public abstract void Execute();
        public abstract void Exit();

        public StateBase(TEnum stateType)
        {
            _stateType = stateType;
        }
    }
}
