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
    /// An abstract base for state machines to build upon.
    /// This script defines an abstract generic base class for implementing type-safe 
    /// state machines using enum-based state definitions. It provides core functionality 
    /// for state management including:
    /// - Generic enum-based state representation for compile-time type safety
    /// - State transition logic with optional forced transitions
    /// - Event-driven architecture through StateChangeEvent delegate
    /// - Initial state configuration through constructor parameters
    /// - Protected utility methods for state enumeration access
    /// - Prevention of redundant state transitions through equality checking
    /// 
    /// Derived classes should implement specific state behaviors and override GoToState
    /// to add custom transition logic, validation, or state-specific initialization.
    /// </summary>
    /// <typeparam name="TEnum">The enum type representing the state machine states</typeparam>
    [Serializable]
    public abstract class StateMachineBase<TEnum>
        where TEnum : Enum
    {
        protected readonly TEnum initialStateEnum;

        [ShowOnly]
        protected TEnum currentStateEnum;

        public TEnum CurrentState => currentStateEnum;

        public delegate void StateChangeEvent(TEnum state);
        public event StateChangeEvent OnStateChanged;

        public StateMachineBase()
        {
            initialStateEnum = GetAllStateEnums()[0];
            OnConstruct();
        }

        public StateMachineBase(TEnum initialState)
        {
            this.initialStateEnum = initialState;
            OnConstruct();
        }

        private void OnConstruct() => GoToState(initialStateEnum);
        
        protected TEnum[] GetAllStateEnums() => (TEnum[])Enum.GetValues(typeof(TEnum));

        protected void RaiseStateChangedEvent(TEnum state) => OnStateChanged?.Invoke(state);

        public virtual bool GoToState(TEnum newState, bool force = false)
        {
            if (!force && EqualityComparer<TEnum>.Default.Equals(currentStateEnum, newState))
                return false;
            currentStateEnum = newState;
            return true;
        }
    }
}
