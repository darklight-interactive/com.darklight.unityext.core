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
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
    /// <summary>
    /// An abstract Finite State class for the FiniteStateMachine
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    [System.Serializable]
    public class FiniteState<TState>
        where TState : Enum
    {
        [SerializeField] TState _stateType;

        public FiniteStateMachine<TState> StateMachine { get; private set; }
        public TState StateType
        {
            get => _stateType;
            private set => _stateType = value;
        }
        public FiniteState(FiniteStateMachine<TState> stateMachine, TState stateType)
        {
            this.StateMachine = stateMachine;
            this.StateType = stateType;
        }

        public virtual void Enter() { }
        public virtual void Execute() { }
        public virtual void Exit() { }
    }

    /// <summary>
    /// An abstract base FiniteStateMachine that stores a Dictionary of key value pairs
    /// relating each defined state. The Key is an enum and the Value is an instance of the corresponding
    /// state class.
    /// </summary>
    /// <typeparam name="TState">The enum definition for the state keys</typeparam>
    [Serializable]
    public abstract class FiniteStateMachine<TState>
        where TState : Enum
    {
        Dictionary<TState, FiniteState<TState>> _possibleFinitesStates = new Dictionary<TState, FiniteState<TState>>();

        [SerializeField, ShowOnly] TState _initialState;
        [SerializeField, ShowOnly] TState _currentState;
        [SerializeField] FiniteState<TState> _currentFiniteState;

        // Event called when the state is changed
        public delegate void OnStateChange(TState state);
        public event OnStateChange OnStateChanged;

        // Properties
        public Dictionary<TState, FiniteState<TState>> PossibleStates { get => _possibleFinitesStates; }
        public TState InitialState { get => _initialState; }
        public TState CurrentState { get => _currentState; }

        // Constructors
        public FiniteStateMachine() { }
        public FiniteStateMachine(TState initialState)
        {
            this._possibleFinitesStates = new Dictionary<TState, FiniteState<TState>>();

            this._initialState = initialState;
            this._currentState = initialState;
        }
        public FiniteStateMachine(Dictionary<TState, FiniteState<TState>> possibleStates, TState initialState)
        {
            this._possibleFinitesStates = possibleStates;

            this._initialState = initialState;
            this._currentState = initialState;
        }

        void GenerateDefaultStates()
        {
            _possibleFinitesStates = new Dictionary<TState, FiniteState<TState>>();
            TState[] stateEnums = (TState[])Enum.GetValues(typeof(TState));
            foreach (TState state in stateEnums)
            {
                AddState(new FiniteState<TState>(this, state));
            }

        }

        /// <summary>
        /// Add a state to the possibleStates dictionary
        /// </summary>
        /// <param name="finiteState">The FiniteState to add to the dictionary.</param>
        public void AddState(FiniteState<TState> finiteState, bool overwrite = true)
        {
            if (_possibleFinitesStates == null) { _possibleFinitesStates = new Dictionary<TState, FiniteState<TState>>(); }

            if (_possibleFinitesStates.ContainsKey(finiteState.StateType))
            {
                if (overwrite)
                    _possibleFinitesStates[finiteState.StateType] = finiteState;
                else
                    Debug.LogWarning($"The state {finiteState.StateType} already exists in the state machine and will not be added.");
            }
            else
            {
                _possibleFinitesStates.Add(finiteState.StateType, finiteState);
            }
        }

        /// <summary>
        /// Step through the Execution call of the current state. 
        /// This is typically called from an Update loop.
        /// </summary>
        public virtual void Step()
        {
            if (_currentFiniteState != null) { _currentFiniteState.Execute(); }
            else { GoToState(_initialState); }
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
                if (_currentFiniteState != null && _currentFiniteState.StateType.Equals(newState))
                    return false;
            }

            // Exit the current state
            if (_currentFiniteState != null) { _currentFiniteState.Exit(); }

            // Check if the state exists
            if (_possibleFinitesStates != null && _possibleFinitesStates.Count > 0 && _possibleFinitesStates.ContainsKey(newState))
            {
                // Enter the new state
                _currentFiniteState = _possibleFinitesStates[newState];
                _currentFiniteState.Enter();
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
            _currentFiniteState = null;
        }
    }
}