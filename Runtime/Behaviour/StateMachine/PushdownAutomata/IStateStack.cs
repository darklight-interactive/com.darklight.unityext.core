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
   /// <summary>
   /// Defines the basic operations needed for a Automaton to store and retrieve states
   /// </summary>
   public interface IStateStack<TEnum>
   {
      /// <summary>
      /// Number of states stored
      /// </summary>
      int Count { get; }

      /// <summary>
      /// Peek at the top of the stack
      /// </summary>
      /// <returns></returns>
      IState<TEnum> Peek();

      /// <summary>
      /// Pop the top of the stack
      /// </summary>
      /// <returns></returns>
      IState<TEnum> Pop();

      /// <summary>
      /// Push a new state
      /// </summary>
      /// <param name="state"></param>
      void Push(IState<TEnum> state);

      /// <summary>
      /// Clear the stack
      /// </summary>
      void Clear();
   }
}