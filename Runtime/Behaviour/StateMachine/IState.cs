using System;

namespace Darklight.UnityExt.Behaviour
{
    public interface IState
    {
        /// <summary>
        /// Called when entering the state
        /// </summary>
        void Enter();

        /// <summary>
        /// Called to update the state
        /// </summary>
        void Execute();

        /// <summary>
        /// Called when exiting the state
        /// </summary>
        void Exit();
    }

    public interface IState<TState> : IState
    {
        TState StateType { get; }
    }
}