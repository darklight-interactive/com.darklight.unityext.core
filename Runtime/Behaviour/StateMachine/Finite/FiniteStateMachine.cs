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
using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
	/// <summary>
	/// A generic finite state machine implementation that manages state transitions and execution.
	/// </summary>
	/// <typeparam name="TEnum">The enum type defining possible states</typeparam>
	[Serializable]
	public class FiniteStateMachine<TEnum> : StateMachineBase<TEnum>
		where TEnum : Enum
	{
		private readonly Dictionary<TEnum, FiniteState<TEnum>> _possibleFiniteStates;

		private FiniteState<TEnum> _previousState;
		[SerializeField] FiniteState<TEnum> _currentFiniteState;

		// Properties
		public FiniteState<TEnum> PreviousFiniteState => _previousState;
		public FiniteState<TEnum> CurrentFiniteState => _currentFiniteState;

		// Events
		public event Action OnStep;

		#region Constructors

		public FiniteStateMachine()
			: base()
		{
			_possibleFiniteStates = new Dictionary<TEnum, FiniteState<TEnum>>();
			GenerateDefaultStates();
		}

		public FiniteStateMachine(TEnum initialState)
			: base(initialState)
		{
			_possibleFiniteStates = new Dictionary<TEnum, FiniteState<TEnum>>();
			GenerateDefaultStates();
		}

		public FiniteStateMachine(
			IReadOnlyDictionary<TEnum, FiniteState<TEnum>> possibleStates,
			TEnum initialState
		)
			: base(initialState)
		{
			_possibleFiniteStates = new Dictionary<TEnum, FiniteState<TEnum>>(possibleStates);
		}

		#endregion

		/// <summary>
		/// Generates default state instances for each enum value
		/// </summary>
		private void GenerateDefaultStates()
		{
			_possibleFiniteStates.Clear();
			foreach (TEnum state in GetAllStateEnums())
			{
				AddState(new FiniteState<TEnum>(state));
			}
		}

		/// <summary>
		/// Overrides the finite states with a new set of states
		/// </summary>
		/// <param name="states">The new states to use</param>
		protected void OverrideFiniteStates(IReadOnlyDictionary<TEnum, FiniteState<TEnum>> states)
		{
			_possibleFiniteStates.Clear();
			foreach (var state in states)
			{
				AddState(state.Value, false);
			}
		}

		/// <summary>
		/// Adds or updates a state in the state machine
		/// </summary>
		/// <param name="finiteState">The state to add/update</param>
		/// <param name="overwrite">Whether to overwrite existing state if present</param>
		/// <returns>True if state was added/updated successfully</returns>
		public bool AddState(FiniteState<TEnum> finiteState, bool overwrite = true)
		{
			if (finiteState == null)
			{
				Debug.LogError("Attempted to add null state to FSM");
				return false;
			}

			if (_possibleFiniteStates.ContainsKey(finiteState.StateType))
			{
				if (!overwrite)
				{
					Debug.LogWarning(
						$"State {finiteState.StateType} already exists and overwrite is false"
					);
					return false;
				}
				_possibleFiniteStates[finiteState.StateType] = finiteState;
				return true;
			}

			_possibleFiniteStates.Add(finiteState.StateType, finiteState);
			return true;
		}

		/// <summary>
		/// Executes the current state's logic
		/// </summary>
		/// <remarks>
		/// Should be called from Update or FixedUpdate depending on requirements
		/// </remarks>
		public virtual void Step()
		{
			if (_currentFiniteState != null)
			{
				_currentFiniteState.Execute();
				OnStep?.Invoke();
				return;
			}
			GoToState(initialStateEnum);
		}

		/// <summary>
		/// Transitions to a new state
		/// </summary>
		/// <param name="newState">The state to transition to</param>
		/// <param name="force">Force transition even if already in target state</param>
		/// <returns>True if transition was successful</returns>
		public override bool GoToState(TEnum newState, bool force = false)
		{
			// If not forcing and already in target state, do nothing
			if (!force && _currentFiniteState != null && EqualityComparer<TEnum>.Default.Equals(_currentFiniteState.StateType, newState))
				return false;

			// Ensure target state exists
			if (!_possibleFiniteStates.TryGetValue(newState, out var targetState))
			{
				Debug.LogError($"State {newState} does not exist in the state machine");
				return false;
			}

			// Store previous state before transition
			_previousState = _currentFiniteState;

			// Handle state transition
			_currentFiniteState?.Exit();
			_currentFiniteState = targetState;
			_currentFiniteState.Enter();

			// Notify base class of state change
			base.GoToState(newState);

			// Notify listeners of state change
			RaiseStateChangedEvent(_currentFiniteState.StateType);

			return true;
		}

		/// <summary>
		/// Checks if a specific state exists in the state machine
		/// </summary>
		/// <param name="state">The state to check</param>
		/// <returns>True if the state exists</returns>
		public bool HasState(TEnum state) => _possibleFiniteStates.ContainsKey(state);

		/// <summary>
		/// Removes a state from the state machine
		/// </summary>
		/// <param name="state">The state to remove</param>
		/// <returns>True if the state was removed</returns>
		public bool RemoveState(TEnum state)
		{
			if (EqualityComparer<TEnum>.Default.Equals(_currentFiniteState.StateType, state))
			{
				Debug.LogError($"Cannot remove current state {state}");
				return false;
			}

			return _possibleFiniteStates.Remove(state);
		}
	}
}
