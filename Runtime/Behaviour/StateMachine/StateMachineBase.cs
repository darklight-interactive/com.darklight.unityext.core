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

using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
    /// <summary>
    /// An abstract base for state machines to build upon
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    [Serializable]
    public abstract class StateMachineBase<TEnum>
        where TEnum : Enum
    {
        [SerializeField] protected TEnum initialState;
        [SerializeField] protected TEnum currentState;

        public delegate void StateChangeEvent(TEnum state);
        public event StateChangeEvent OnStateChanged;

        public StateMachineBase()
        {
            initialState = GetAllStateEnums()[0];
        }

        public StateMachineBase(TEnum initialState)
        {
            this.initialState = initialState;
        }

        protected TEnum[] GetAllStateEnums() => (TEnum[])Enum.GetValues(typeof(TEnum));
        protected void RaiseStateChangedEvent(TEnum state) => OnStateChanged?.Invoke(state);
    }
}