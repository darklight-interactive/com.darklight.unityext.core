/* ======================================================================= ]]
 * Copyright (c) 2024 Darklight Interactive. All rights reserved.
 * Licensed under the Darklight Interactive Software License Agreement.
 * See LICENSE.md file in the project root for full license information.
 * ------------------------------------------------------------------ >>
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * ------------------------------------------------------------------ >>
 * For questions regarding this software or licensing, please contact:
 * Email: skysfalling22@gmail.com
 * Discord: skysfalling
 * ======================================================================= ]]
 * DESCRIPTION:
    This script defines a finite state machine (FSM) framework. 
    It provides an abstract base class for creating FSMs and an interface for defining states.
    The FSM stores a dictionary of possible states, where each state is represented by an enum key 
    and an instance of the corresponding state class as the value. 
    The FSM allows transitioning between states and executing the current state's logic.
 * ------------------------------------------------------------------ >>
 * MAJOR AUTHORS: 
 * Sky Casey
 * Garrett Blake
 * ======================================================================= ]]
 */



using System;
using System.Collections.Generic;

namespace Darklight.UnityExt.Behaviour
{
    /// <summary>
    /// An interface to define the structure of a State
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        void Exit();

        /// <summary>
        /// Called when the state is executed.
        /// </summary>
        /// <remarks>
        /// This method should be called in the game's update loop.
        /// </remarks>
        void Execute();
    }

    /// <summary>
    /// An abstract Finite State class for the FiniteStateMachine
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    [System.Serializable]
    public abstract class FiniteState<TState> : IState where TState : Enum
    {
        public FiniteStateMachine<TState> StateMachine { get; private set; }
        public TState StateType { get; private set; }
        public FiniteState(FiniteStateMachine<TState> stateMachine, TState stateType)
        {
            this.StateMachine = stateMachine;
            this.StateType = stateType;
        }

        public abstract void Enter();
        public abstract void Exit();
        public abstract void Execute();
    }

    /// <summary>
    /// An abstract base FiniteStateMachine that stores a Dictionary of key value pairs
    /// relating each defined state. The Key is an enum and the Value is an instance of the corresponding
    /// state class.
    /// </summary>
    /// <typeparam name="TState">The enum definition for the state keys</typeparam>
    public abstract class FiniteStateMachine<TState> where TState : Enum
    {
        protected TState initialState;
        protected Dictionary<TState, FiniteState<TState>> possibleStates;
        protected FiniteState<TState> currentFiniteState;
        protected object[] args;

        // Event called when the state is changed
        public delegate void OnStateChange(TState state);
        public event OnStateChange OnStateChanged;

        // Public access to the current state
        public TState CurrentState { get { return currentFiniteState.StateType; } }

        // Constructors
        public FiniteStateMachine() { }
        public FiniteStateMachine(Dictionary<TState, FiniteState<TState>> possibleStates, TState initialState, object[] args)
        {
            this.initialState = initialState;
            this.possibleStates = possibleStates;
            if (possibleStates.ContainsKey(initialState))
            {
                this.currentFiniteState = possibleStates[initialState];
            }
            this.args = args;
        }

        /// <summary>
        /// Add a state to the possibleStates dictionary
        /// </summary>
        /// <param name="finiteState">The FiniteState to add to the dictionary.</param>
        public void AddState(FiniteState<TState> finiteState)
        {
            if (possibleStates == null) { possibleStates = new Dictionary<TState, FiniteState<TState>>(); }
            possibleStates.Add(finiteState.StateType, finiteState);
        }

        /// <summary>
        /// Step through the Execution call of the current state. 
        /// This is typically called from an Update loop.
        /// </summary>
        public virtual void Step()
        {
            if (currentFiniteState != null) { currentFiniteState.Execute(); }
            else { GoToState(initialState); }
        }

        public virtual bool GoToState(TState newState)
        {
            return GoToState(newState);
        }

        /// <summary>
        /// Move the statemachine to a new current state.
        /// </summary>
        /// <param name="newState">The enum type key of the desired state.</param>
        /// <param name="force">If true, the state will be changed even if it is the same as the current state.</param>
        public virtual bool GoToState(TState newState, bool force = false)
        {
            if (!force)
            {
                // If the state is the same as the current state, return false
                if (currentFiniteState != null && currentFiniteState.StateType.Equals(newState))
                    return false;
            }

            // Exit the current state
            if (currentFiniteState != null) { currentFiniteState.Exit(); }

            // Check if the state exists
            if (possibleStates != null && possibleStates.Count > 0 && possibleStates.ContainsKey(newState))
            {
                // Enter the new state
                currentFiniteState = possibleStates[newState];
                currentFiniteState.Enter();
            }
            else
            {
                // If the state does not exist, return false
                return false;
            }

            // Invoke the OnStateChanged event
            OnStateChanged?.Invoke(newState);
            return true;
        }

        /// <summary>
        /// Clear the current state value.
        /// </summary>
        public virtual void ClearState()
        {
            currentFiniteState = null;
        }
    }
}