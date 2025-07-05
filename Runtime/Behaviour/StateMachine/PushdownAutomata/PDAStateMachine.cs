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
 * ======================================================================= ]]
 */


using System;
using System.Collections.Generic;
using Darklight.Editor;
using UnityEngine;

namespace Darklight.Behaviour
{
    /// <summary>
    /// A Pushdown State Machine that allows managing stacked states using push and pop operations.
    /// </summary>
    /// <typeparam name="TEnum">The enum type representing possible states.</typeparam>
    public abstract partial class PDAStateMachine<TEnum> : StateMachineBase<TEnum>
        where TEnum : Enum
    {
        Dictionary<TEnum, State> _states = new Dictionary<TEnum, State>();
        IStateStack<TEnum> _stateStack;

        /// <summary>
        /// Adds a state to the state machine.
        /// </summary>
        /// <param name="state">The state to add.</param>
        public void AddState(State state)
        {
            if (!_states.ContainsKey(state.StateType))
            {
                _states[state.StateType] = state;
            }
        }

        /// <summary>
        /// Pushes a new state onto the stack and activates it.
        /// </summary>
        /// <param name="newState">The state to push.</param>
        public void PushState(TEnum newState)
        {
            if (_states.ContainsKey(newState))
            {
                if (_stateStack.Count > 0)
                    _stateStack.Peek().Exit();
                _stateStack.Push(_states[newState]);
                _states[newState].Enter();
                RaiseStateChangedEvent(newState);
            }
        }

        /// <summary>
        /// Pops the current state from the stack and reactivates the previous state, if any.
        /// </summary>
        public void PopState()
        {
            if (_stateStack.Count > 0)
            {
                _stateStack.Pop().Exit();
                if (_stateStack.Count > 0)
                {
                    _stateStack.Peek().Enter();
                    RaiseStateChangedEvent(_stateStack.Peek().StateType);
                }
            }
        }

        /// <summary>
        /// Executes the current state’s logic, if a state is active.
        /// </summary>
        public void Execute()
        {
            if (_stateStack.Count > 0)
            {
                _stateStack.Peek().Execute();
            }
        }
    }
}
