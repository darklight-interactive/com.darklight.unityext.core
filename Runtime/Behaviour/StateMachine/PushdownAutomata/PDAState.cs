

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

using Darklight.UnityExt.Editor;

using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
    public abstract partial class PDAStateMachine<TEnum>
    {

        /// <summary>
        /// Represents a state within the Pushdown State Machine, including entry, execution, and exit methods.
        /// </summary>
        /// <typeparam name="TEnum">The enum type representing the state.</typeparam>
        public abstract class State : StateBase<TEnum>
        {
            IStateStack<TEnum> _stack;
            HashSet<State> _stackableStates = new HashSet<State>();
            HashSet<State> _unstackableStates = new HashSet<State>();
            public State(PDAStateMachine<TEnum> stateMachine, TEnum stateType) : base(stateType)
            {
                _unstackableStates.Add(this);
            }

            public State(PDAStateMachine<TEnum> stateMachine, TEnum stateType, IEnumerable<State> stackableStates) : this(stateMachine, stateType)
            {
                _stackableStates.UnionWith(stackableStates);
                if (_stackableStates.Contains(this)) _stackableStates.Remove(this);
            }

            /// <summary> 
            /// Try to get the state we were in before the last transition.
            /// </summary>
            /// <param name="result">The previous state.</param>
            /// <returns>True if the stack isn't empty.</returns>
            public bool TryGetPreviousState(out IState result)
            {
                result = null;

                if (_stack.Count > 0)
                {
                    result = _stack.Peek();
                    return true;
                }

                return false;
            }

        }
    }
}