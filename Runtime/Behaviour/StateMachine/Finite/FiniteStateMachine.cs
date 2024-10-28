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

using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
    /// <summary>
    /// An abstract base FiniteStateMachine that stores a Dictionary of key value pairs
    /// relating each defined state. The Key is an enum and the Value is an instance of the corresponding
    /// state class.
    /// </summary>
    /// <typeparam name="TEnum">The enum definition for the state keys</typeparam>
    [Serializable]
    public class FiniteStateMachine<TEnum> : StateMachineBase<TEnum>
        where TEnum : Enum
    {
        Dictionary<TEnum, FiniteState<TEnum>> _possibleFiniteStates = new Dictionary<TEnum, FiniteState<TEnum>>();
        [SerializeField] FiniteState<TEnum> _currentFiniteState;

        // Constructors
        public FiniteStateMachine() : base() { GenerateDefaultStates(); }
        public FiniteStateMachine(TEnum initialState) : base(initialState) { GenerateDefaultStates(); }
        public FiniteStateMachine(Dictionary<TEnum, FiniteState<TEnum>> possibleStates, TEnum initialState) : base(initialState)
        {
            this._possibleFiniteStates = possibleStates;
        }

        void GenerateDefaultStates()
        {
            _possibleFiniteStates = new Dictionary<TEnum, FiniteState<TEnum>>();
            foreach (TEnum state in GetAllStateEnums())
                AddState(new FiniteState<TEnum>(this, state));
        }

        /// <summary>
        /// Add a state to the possibleStates dictionary
        /// </summary>
        /// <param name="finiteState">The FiniteState to add to the dictionary.</param>
        public void AddState(FiniteState<TEnum> finiteState, bool overwrite = true)
        {
            if (_possibleFiniteStates == null) { _possibleFiniteStates = new Dictionary<TEnum, FiniteState<TEnum>>(); }

            if (_possibleFiniteStates.ContainsKey(finiteState.StateType))
            {
                if (overwrite)
                    _possibleFiniteStates[finiteState.StateType] = finiteState;
                else
                    Debug.LogWarning($"The state {finiteState.StateType} already exists in the state machine and will not be added.");
            }
            else
            {
                _possibleFiniteStates.Add(finiteState.StateType, finiteState);
            }
        }

        /// <summary>
        /// Step through the Execution call of the current state. 
        /// This is typically called from an Update loop.
        /// </summary>
        public virtual void Step()
        {
            if (_currentFiniteState != null) { _currentFiniteState.Execute(); }
            else { GoToState(initialState); }
        }

        /// <summary>
        /// Move the statemachine to a new current state.
        /// </summary>
        /// <param name="newState">The enum type key of the desired state.</param>
        /// <param name="force">If true, the state will be changed even if it is the same as the current state.</param>
        public virtual bool GoToState(TEnum newState, bool force = false)
        {
            // If the state is the same as the current state, return false
            if (!force && currentState.Equals(newState)) return false;

            // Exit the current state
            if (_currentFiniteState != null)
                _currentFiniteState.Exit();

            // Check if the state exists
            if (_possibleFiniteStates != null && _possibleFiniteStates.Count > 0 && _possibleFiniteStates.ContainsKey(newState))
            {
                // Enter the new state
                _currentFiniteState = _possibleFiniteStates[newState];
                _currentFiniteState.Enter();
            }
            else
            {
                // If the state does not exist, return false
                Debug.LogError($"The state {newState} does not exist in the state machine.");
                return false;
            }

            // Invoke the OnStateChanged event
            RaiseStateChangedEvent(currentState);
            return true;
        }

    }
}