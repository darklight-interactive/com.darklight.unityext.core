using System;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
	[System.Serializable]
	public abstract class StateBase<TEnum> : IState<TEnum>
		where TEnum : Enum
	{
		protected readonly StateMachineBase<TEnum> StateMachineBase;

		[SerializeField, ShowOnly] TEnum _stateType;
		
		public TEnum StateType => _stateType;

		public abstract void Enter();
		public abstract void Execute();
		public abstract void Exit();
		
		public StateBase(StateMachineBase<TEnum> stateMachineBase, TEnum stateType)
		{
			this.StateMachineBase = stateMachineBase;
			this._stateType = stateType;
		}
	}
}

