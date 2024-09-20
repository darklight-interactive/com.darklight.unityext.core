using System;
using System.Collections.Generic;

using UnityEngine;

using Darklight.UnityExt.Editor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Editor
{
	public enum LogSeverity { Info, Warning, Error }

	public class ConsoleGUI
	{
		private Vector2 scrollPosition;
		private float scrollHeightSetting = 20;
		private bool autoScroll = true; // Default to true to enable auto-scrolling.
		public class LogEntry
		{
			DateTime _timeStamp = DateTime.Now;
			LogSeverity _severity = LogSeverity.Info;
			string _message = string.Empty;
			int _indentLevel = 0;

			public string Timestamp => _timeStamp.ToString("mm:ss:ff");
			public string Message => new string(' ', _indentLevel * 4) + $"{_message}";
			public GUIStyle Style
			{
				get
				{
					Color textColor = Color.white; // Default to white
					switch (_severity)
					{
						case LogSeverity.Warning:
							textColor = Color.yellow;
							break;
						case LogSeverity.Error:
							textColor = Color.red;
							break;
					}

					return new GUIStyle(GUI.skin.label)
					{
						normal = { textColor = textColor },
						alignment = TextAnchor.MiddleLeft
					};
				}
			}
			public LogEntry(int indentLevel, string message, LogSeverity severity = LogSeverity.Info)
			{
				_indentLevel = indentLevel;
				_message = message.Trim();
				_severity = severity;
				_timeStamp = DateTime.Now;
			}
		}

		public List<LogEntry> AllLogEntries { get; private set; } = new List<LogEntry>();

		public void Log(string message, int indent = 0, LogSeverity severity = LogSeverity.Info)
		{
			LogEntry newLog = new LogEntry(indent, message, severity);
			AllLogEntries.Add(newLog);

			// If autoScroll is enabled, adjust the scroll position to the bottom.
			if (autoScroll)
			{
				// Assuming each log entry is roughly the same height, this will scroll to the bottom.
				scrollPosition.y = float.MaxValue;
			}
		}

		public void Clear()
		{
			AllLogEntries.Clear();
		}

#if UNITY_EDITOR
		bool _mainFoldoutOpen;
		bool _settingsFoldoutOpen;

		public void DrawInEditor()
		{
			// Draw the foldout
			_mainFoldoutOpen = EditorGUILayout.Foldout(_mainFoldoutOpen, $"CONSOLE_GUI", true, EditorStyles.foldoutHeader);

			// If the foldout is expanded, show the console
			if (_mainFoldoutOpen)
			{
				EditorGUI.indentLevel++; // Indent the contents of the foldout for better readability

				// < SCROLL VIEW > ---------------------------------------------------- >>
				// Dark gray background
				GUIStyle backgroundStyle = new GUIStyle
				{
					normal = { background = CustomInspectorGUI.MakeTex(600, 1, new Color(0.1f, 0.1f, 0.1f, 1.0f)) }
				};

				float scrollHeight = scrollHeightSetting * 10;
				scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, backgroundStyle, GUILayout.Height(scrollHeight));

				int logCount = 0;
				foreach (LogEntry log in AllLogEntries)
				{
					DrawLogEntry(log);
					logCount++;
				}

				EditorGUILayout.EndScrollView();

				// < BUTTONS > ---------------------------------------------------- >>
				// Button to clear the console
				if (GUILayout.Button("Clear Console"))
				{
					Clear();
				}

				// < SETTINGS > ---------------------------------------------------- >>
				_settingsFoldoutOpen = EditorGUILayout.Foldout(_settingsFoldoutOpen, "Settings", true, EditorStyles.foldoutHeader);
				if (_settingsFoldoutOpen)
				{
					EditorGUI.indentLevel++; // Indent the contents of the foldout for better readability
					DrawSettings();
				}
				EditorGUI.indentLevel--; // Reset indentation
			}
		}

		void DrawLogEntry(LogEntry log)
		{
			EditorGUILayout.BeginHorizontal(); // Start a horizontal group for inline elements

			string message = $"[{log.Timestamp}] || {log.Message}";
			EditorGUILayout.LabelField(message, log.Style, GUILayout.ExpandWidth(true));
			EditorGUILayout.EndHorizontal();
		}

		void DrawSettings()
		{
			// Toggle for enabling/disabling auto-scroll
			autoScroll = EditorGUILayout.Toggle("Auto-scroll", autoScroll);

			// Slider for adjusting the scroll height
			scrollHeightSetting = EditorGUILayout.Slider("Scroll Height", scrollHeightSetting, 1, 100);
		}
#endif

	}
}