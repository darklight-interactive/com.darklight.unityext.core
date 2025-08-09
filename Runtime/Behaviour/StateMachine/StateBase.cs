/* ======================================================================= ]]
 * Copyright (c) 2025 Darklight Interactive. All rights reserved.
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
 * This script defines a base class for states in a state machine.
 * It provides a common interface for states and implements the logic for
 * entering, executing, and exiting states.
 * ------------------------------------------------------------------ >>
 * MAJOR AUTHORS:
 * Sky Casey
 * ======================================================================= ]]
 */


using System;
using Darklight.Editor;
using UnityEngine;

namespace Darklight.Behaviour
{
    [System.Serializable]
    public abstract class StateBase<TEnum> : IState<TEnum>
        where TEnum : Enum
    {
        [SerializeField, ShowOnly]
        TEnum _stateType;

        public TEnum StateType => _stateType;

        /// <summary>
        /// Internal Enter method that calls OnEnter
        /// </summary>
        public virtual void Enter()
        {
            OnEnter();
        }

        /// <summary>
        /// Internal Execute method that calls OnExecute
        /// </summary>
        public virtual void Execute()
        {
            OnExecute();
        }

        /// <summary>
        /// Internal Exit method that calls OnExit
        /// </summary>
        public virtual void Exit()
        {
            OnExit();
        }

        /// <summary>
        /// Override to implement state entry logic
        /// </summary>
        protected abstract void OnEnter();

        /// <summary>
        /// Override to implement first frame logic
        /// </summary>
        protected abstract void OnFirstFrame();

        /// <summary>
        /// Override to implement state update logic
        /// </summary>
        protected abstract void OnExecute();

        /// <summary>
        /// Override to implement state exit logic
        /// </summary>
        protected abstract void OnExit();

        public StateBase(TEnum stateType)
        {
            _stateType = stateType;
        }
    }
}
