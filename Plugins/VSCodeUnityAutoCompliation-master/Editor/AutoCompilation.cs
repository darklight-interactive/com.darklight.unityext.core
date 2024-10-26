using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;
using System.Net;
using UnityEditorInternal;
using System.Net.Sockets;

namespace PostcyberPunk.AutoCompilation
{
	[InitializeOnLoad]
	public static class AutoCompilation
	{
		private static HttpListener listener;
		private static bool needUpdate;
		private static string port = "10245";
		private static IAsyncResult _result;
		private static bool isEditorFocused = true;
		static AutoCompilation()
		{
			// isEditorFocused=InternalEditorUtility.isApplicationActive;
			if (!SessionState.GetBool("DisableAutoComplation", false))
			{
				needUpdate = false;
				// CompilationPipeline.compilationStarted += OnCompilationStarted;
				CompilationPipeline.compilationFinished += OnCompilationFinished;
				EditorApplication.quitting += _closeListener;
				EditorApplication.update += onUpdate;
				// _createListener();
			}
		}

		private static void _createListener()
		{
			// Debug.LogWarning("Creating");
			if (listener != null)
			{
				return;
			};
			try
			{
				listener = new HttpListener();
				listener.Prefixes.Add("http://127.0.0.1:" + port + "/refresh/");
				listener.Start();
				_result = listener.BeginGetContext(new AsyncCallback(OnRequest), listener);

				// Debug.Log("Auto Compilation HTTP server started");
			}
			catch (Exception e)
			{
				Debug.LogError("Auto Compilation starting failed:" + e);
			}

		}
		private static void OnRequest(IAsyncResult result)
		{
			if (listener.IsListening && !EditorApplication.isCompiling)
			{
				listener.EndGetContext(result);
				needUpdate = true;
				_result = listener.BeginGetContext(new AsyncCallback(OnRequest), listener);
			}
		}
		private static void _closeListener()
		{
			// Debug.Log("Closing Listener");
			if (listener == null)
			{
				// Debug.LogWarning("Listener is null");
				return;
			}

			listener.Stop();
			listener.Close();
			listener = null;
			// Debug.Log("Closed Listener");
		}
		private static void onUpdate()
		{
			//Check focus
			if (InternalEditorUtility.isApplicationActive != isEditorFocused)
			{
				isEditorFocused = !isEditorFocused;
				if (isEditorFocused)
				{
					_closeListener();
				}
				else if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
				{
					_createListener();
				}
			}
			if (!isEditorFocused && !EditorApplication.isCompiling && !EditorApplication.isUpdating && needUpdate)
			{
				_closeListener();
				// Debug.LogWarning("Compiled in background");
				needUpdate = false;
				AssetDatabase.Refresh();
			}
		}
		[MenuItem("Tools/AutoCompilation/Toggle Auto-Completion")]
		public static void ToggleAutoCompilation()
		{
			bool toggle = SessionState.GetBool("DisableAutoComplation", false);
			if (toggle)
			{
				_closeListener();
			}
			else
			{
				_createListener();
			}
			SessionState.SetBool("DisableAutoComplation", !toggle);
			Debug.Log("Auto Completion is " + (!toggle ? "Off" : "On"));
		}
		// private static void OnCompilationStarted(object _) => _closeListener();
		private static void OnCompilationFinished(object _) { if (!isEditorFocused) _createListener(); }
	}
}
