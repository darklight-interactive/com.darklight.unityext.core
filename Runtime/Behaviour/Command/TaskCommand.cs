using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Darklight.UnityExt.Editor;
using Debug = UnityEngine.Debug;

namespace Darklight.UnityExt.Behaviour
{
	public class TaskCommand : IDisposable
	{
		Stopwatch _stopwatch;
		TaskReciever _reciever;
		Func<Task> _task;
		Guid _guid = Guid.NewGuid();
		string _name = "DefaultTaskCommand";
		bool _executeOnBackgroundThread = false;

		public Guid Guid => _guid;
		public string Name => _name;
		public bool ExecuteOnBackgroundThread => _executeOnBackgroundThread;
		public long ExecutionTime = 0;

		public TaskCommand(TaskReciever reciever, string name, Func<Task> task, bool executeOnBackgroundThread = false)
		{
			_stopwatch = Stopwatch.StartNew();
			this._reciever = reciever;
			this._task = task;
			this._name = name;
			this._executeOnBackgroundThread = executeOnBackgroundThread;
		}

		public async Task ExecuteTask()
		{
			_stopwatch.Restart();
			try
			{
				await _task();
			}
			catch (OperationCanceledException operation)
			{
				_reciever.Console.Log($"{Name}: Operation canceled.", 0, LogSeverity.Warning);
				Debug.LogWarning($"{Name} || {Guid} => Operation Canceled: {operation.Message}");
			}
			catch (Exception ex)
			{
				_reciever.Console.Log($"{Name}: Error encountered. See Unity Console for details.", 0, LogSeverity.Error);
				Debug.LogError($"{Name} || {Guid} => Exception: {ex.Message}\n" + ex.StackTrace);
			}
			finally
			{
				_stopwatch.Stop();
				ExecutionTime = _stopwatch.ElapsedMilliseconds;
				_reciever.Console.Log($"{Name}: Execution successful. Time: {_stopwatch.ElapsedMilliseconds}ms");
			}
		}

		public void Dispose()
		{
			_stopwatch?.Stop();
			_stopwatch = null;
		}
	}
}