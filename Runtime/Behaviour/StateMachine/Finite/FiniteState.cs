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
    /// <typeparam name="TEnum"></typeparam>
    [System.Serializable]
    public class FiniteState<TEnum> : StateBase<TEnum>
        where TEnum : Enum
    {
        public FiniteState(FiniteStateMachine<TEnum> finiteStateMachine, TEnum stateType) : base(finiteStateMachine, stateType) { }
        public override void Enter() => throw new NotImplementedException();
        public override void Execute() => throw new NotImplementedException();
        public override void Exit() => throw new NotImplementedException();
    }
}