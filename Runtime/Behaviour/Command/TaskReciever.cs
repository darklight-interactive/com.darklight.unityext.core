using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using System.Linq;

using Darklight.UnityExt.Editor;

using Editor = UnityEditor.Editor;

#endif

namespace Darklight.UnityExt.Behaviour
{
	[Serializable]
	public class TaskReciever
	{
		ConsoleGUI _console = new ConsoleGUI();
		Queue<TaskCommand> _executionQueue = new Queue<TaskCommand>();
		StateMachine _stateMachine = new StateMachine();

		[SerializeField] int _tasksInQueue = 0;

		public bool Initialized { get; private set; } = false;
		public ConsoleGUI Console => _console;
		public State CurrentState
		{
			get
			{
				if (_stateMachine != null)
					return _stateMachine.CurrentState;
				else
					return State.NULL;
			}
		}
		public string LogPrefix => $"<{CurrentState}>";
		public int ExecutionQueueCount => _executionQueue.Count;

		public virtual void Awake()
		{
			_stateMachine.GoToState(State.AWAKE);
			Console.Log($"{LogPrefix}");
		}

		public virtual async Task Initialize()
		{
			_stateMachine.GoToState(State.INITIALIZE);
			Console.Log($"{LogPrefix}"); // show state change
			Initialized = true;
			await Task.CompletedTask;
		}

		public virtual void Update()
		{
			_tasksInQueue = _executionQueue.Count;
		}

		/// <summary>
		/// Enqueues a task bot to the execution queue.
		/// </summary>
		/// <param name="TaskCommand">The task bot to enqueue.</param>
		public Task Enqueue(TaskCommand TaskCommand, bool log = true)
		{
			_stateMachine.GoToState(State.ADD_TO_QUEUE);

			if (log)
				Console.Log($"{LogPrefix} EnqueueTaskCommand {TaskCommand.Name}", 1);

			_executionQueue.Enqueue(TaskCommand);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Enqueues a list of task commands to the execution queue.
		/// </summary>
		/// <param name="commands"></param>
		/// <param name="log"></param>
		/// <returns></returns>
		public async Task EnqueueList(IEnumerable<TaskCommand> commands, bool log = true)
		{
			_stateMachine.GoToState(State.ADD_TO_QUEUE);

			if (log)
				Console.Log($"{LogPrefix} EnqueueList<TaskCommand> Count:{commands.Count()}", 0);

			foreach (TaskCommand newBot in commands)
			{
				await Enqueue(newBot, log);
			}
		}

		public async Task EnqueueClones<T>(string cloneName, IEnumerable<T> items, Func<T, TaskCommand> CreateNewClone, bool log = true)
		{
			_stateMachine.GoToState(State.ADD_TO_QUEUE);

			if (log)
				Console.Log($"{LogPrefix} EnqueueClones {cloneName} x {items.ToList().Count} Clones", 0);

			foreach (T item in items)
			{
				TaskCommand newBot = CreateNewClone(item);
				await Enqueue(newBot, log);
			}
		}

		/// <summary>
		/// Executes an individual TaskCommand
		/// </summary>
		/// <param name="TaskCommand"></param>
		/// <returns></returns>
		public async Awaitable ExecuteNextCommand(TaskCommand TaskCommand)
		{
			_stateMachine.GoToState(State.EXECUTION);

			// Assign the TaskCommand to Execute on the background thread
			if (TaskCommand.ExecuteOnBackgroundThread)
			{
				await Awaitable.BackgroundThreadAsync();
			}
			// Default to Main Thread
			else { await Awaitable.MainThreadAsync(); }

			try
			{
				Console.Log($"Try to Execute {TaskCommand.Name}");
				await TaskCommand.ExecuteTask();
				await Awaitable.MainThreadAsync(); // default back to main thread
			}
			catch (OperationCanceledException e)
			{
				Console.Log($"TaskCommand {TaskCommand.Name} was cancelled: {e.Message}");
				Debug.Log($"TaskCommand {TaskCommand.Name} was cancelled: {e.StackTrace}");
			}
			catch (Exception e)
			{
				Console.Log($"ERROR: Executing {TaskCommand.Name}: {e.Message}");
				Debug.Log($"ERROR: Executing {TaskCommand.Name}");
			}
			finally
			{
				Console.Log($"\t COMPLETE: Finished Executing {TaskCommand.Name}");
			}
		}

		/// <summary>
		/// Iteratively executes all the task commands in the execution queue.
		/// </summary>
		public virtual async Awaitable ExecuteAllCommands()
		{
			_stateMachine.GoToState(State.EXECUTION);

			Console.Log($"{LogPrefix} START TaskCommands [{ExecutionQueueCount}]");

			while (_executionQueue.Count > 0)
			{
				// Dequeue the next TaskCommand
				TaskCommand TaskCommand = null;
				lock (_executionQueue)
				{
					if (_executionQueue.Count > 0)
					{
						TaskCommand = _executionQueue.Dequeue();
					}
				}

				// Try to Execute the TaskCommand
				await ExecuteNextCommand(TaskCommand);
			}

			Console.Log($"Finished Executing [{_executionQueue.Count}] TaskCommands");
			_stateMachine.GoToState(State.CLEAN);
		}

		/// <summary>
		/// Resets the task queen by clearing the execution queue.
		/// </summary>
		public virtual void Reset()
		{
			_executionQueue.Clear();
			Initialized = false;

			Console.Log("Reset");
		}

		public enum State { NULL, AWAKE, INITIALIZE, LOAD_DATA, WAIT, ADD_TO_QUEUE, EXECUTION, CLEAN, ERROR }
		class StateMachine : SimpleStateMachine<State>
		{
			public StateMachine() : base(State.NULL) { }
		}
	}
}
