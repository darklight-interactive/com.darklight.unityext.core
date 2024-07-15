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
 * 
 * ------------------------------------------------------------------ >>
 * MAJOR AUTHORS: 
 * Sky Casey
 * ======================================================================= ]]
 */


using System;
using System.Collections.Generic;

namespace Darklight.UnityExt.Behaviour
{
    /// <summary>
    /// An abstract version of a Simple State Machine.
    /// This is intended to be inherited and expanded upon by a child class.
    /// </summary>
    /// <typeparam name="TState">The corresponding Enum definition</typeparam>
    public abstract class SimpleStateMachine<TState> where TState : Enum
    {
        private TState _currentState;
        public TState CurrentState
        {
            get => _currentState;
            private set
            {
                if (!EqualityComparer<TState>.Default.Equals(_currentState, value))
                {
                    TState previousState = _currentState;
                    _currentState = value;
                    OnStateChanged(previousState, _currentState);
                }
            }
        }

        /// <summary>
        /// Assigns the initial state when the class is created
        /// </summary>
        public SimpleStateMachine(TState initialState)
        {
            GoToState(initialState);
        }
        /// <summary>
        /// Update the current state of the machine
        /// </summary>
        public virtual void GoToState(TState newState)
        {
            if (newState.Equals(CurrentState)) return;
            CurrentState = newState;
        }

        public virtual void OnStateChanged(TState previousState, TState newState) { }
    }
}