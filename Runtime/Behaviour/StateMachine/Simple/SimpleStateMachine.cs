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

using UnityEditorInternal;

namespace Darklight.UnityExt.Behaviour
{
    /// <summary>
    /// A simple, enum based Finite State Machine.
    /// </summary>
    /// <remarks>
    /// This is intended to be a quick and dirty state tracker.
    /// </remarks>
    /// <typeparam name="TEnum">The corresponding Enum definition</typeparam>
    [Serializable]
    public class SimpleStateMachine<TEnum> : StateMachineBase<TEnum>
        where TEnum : Enum
    {
        public TEnum InitialState => initialState;
        public TEnum CurrentState => currentState;

        public SimpleStateMachine() : base() { }
        public SimpleStateMachine(TEnum initialState) : base(initialState) { }

        public virtual void GoToState(TEnum newState)
        {
            if (newState.Equals(CurrentState)) return;
            currentState = newState;
        }
    }
}