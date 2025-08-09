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
 * This script defines a finite state machine (FSM) framework.
 * It provides an abstract base class for creating FSMs and an interface for defining states.
 * The FSM stores a dictionary of possible states, where each state is represented by an enum key
 * and an instance of the corresponding state class as the value.
 * The FSM allows transitioning between states and executing the current state's logic.
 * ------------------------------------------------------------------ >>
 * MAJOR AUTHORS:
 * Sky Casey
 * Garrett Blake
 * ======================================================================= ]]
 */

using System;
using Darklight.Editor;
using UnityEngine;

namespace Darklight.Behaviour
{
    public partial class FiniteStateMachine<TEnum>
        where TEnum : Enum
    {
        /// <summary>
        /// Abstract base class for finite states within a state machine.
        /// </summary>
        /// <typeparam name="TEnum">The enum type defining possible states</typeparam>
        [Serializable]
        public abstract class FiniteState : StateBase<TEnum>
        {
            [Header("Timing")]
            [SerializeField, ShowOnly]
            float _stateStartTime;

            [SerializeField, ShowOnly]
            float _stateElapsedTime;

            /// <summary>
            /// Indicates if this is the first frame of state execution
            /// </summary>
            protected bool IsFirstFrame { get; private set; }

            /// <summary>
            /// Time in seconds since this state was entered
            /// </summary>
            public float ElapsedTime => _stateElapsedTime;

            /// <summary>
            /// Time when this state was entered
            /// </summary>
            public float StartTime => _stateStartTime;

            /// <summary>
            /// Creates a new instance of a finite state
            /// </summary>
            /// <param name="finiteStateMachine">The parent state machine</param>
            /// <param name="stateType">The enum value representing this state</param>
            public FiniteState(TEnum stateType)
                : base(stateType) { }

            /// <summary>
            /// Called when the state is entered
            /// </summary>
            public override sealed void Enter()
            {
                _stateStartTime = Time.time;
                _stateElapsedTime = 0f;
                IsFirstFrame = true;
                base.Enter();
            }

            /// <summary>
            /// Called every frame while the state is active
            /// </summary>
            public override sealed void Execute()
            {
                _stateElapsedTime = Time.time - _stateStartTime;

                if (IsFirstFrame)
                {
                    OnFirstFrame();
                    IsFirstFrame = false;
                }

                base.Execute();
            }

            /// <summary>
            /// Called when the state is exited
            /// </summary>
            public override sealed void Exit()
            {
                _stateElapsedTime = 0f;
                base.Exit();
            }

            #region Utility Methods

            /// <summary>
            /// Checks if the state has been active for the specified duration
            /// </summary>
            /// <param name="duration">Duration in seconds</param>
            protected bool HasElapsed(float duration) => _stateElapsedTime >= duration;

            /// <summary>
            /// Gets the normalized progress (0-1) of the state duration
            /// </summary>
            /// <param name="duration">Total expected duration</param>
            protected float GetProgress(float duration) =>
                Mathf.Clamp01(_stateElapsedTime / duration);

            #endregion
        }

        public class DefaultFiniteState : FiniteState
        {
            public DefaultFiniteState(TEnum stateType)
                : base(stateType) { }

            protected override void OnEnter() { }

            protected override void OnFirstFrame() { }

            protected override void OnExecute() { }

            protected override void OnExit() { }
        }
    }
}
