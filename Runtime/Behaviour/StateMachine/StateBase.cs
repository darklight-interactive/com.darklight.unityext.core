using System;

using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
	[System.Serializable]
	public abstract class StateBase<TEnum> : IState<TEnum>
		where TEnum : Enum
	{
		protected readonly StateMachineBase<TEnum> StateMachineBase;
		public TEnum StateType { get; protected set; }

		public abstract void Enter();
		public abstract void Execute();
		public abstract void Exit();
		
		public StateBase(StateMachineBase<TEnum> stateMachineBase, TEnum stateType)
		{
			this.StateMachineBase = stateMachineBase;
			this.StateType = stateType;
		}
	}
}

