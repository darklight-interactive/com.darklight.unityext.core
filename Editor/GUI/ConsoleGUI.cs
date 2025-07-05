using System;
using System.Collections.Generic;
using Darklight.Editor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Editor
{
    public enum LogSeverity
    {
        Info,
        Warning,
        Error
    }

    public class ConsoleGUI
    {
        List<LogEntry> _logEntries = new List<LogEntry>(); // List of all log entries.

        bool _settingsFoldoutOpen;

        Vector2 _scrollPosition;
        float _height = 20;
        bool _autoScroll = true; // Default to true to enable auto-scrolling.

        /// <summary>
        /// Log a message to the console.
        /// </summary>
        /// <param name="message">
        ///		The message to log.
        /// </param>
        /// <param name="indent">
        /// 	The number of spaces to indent the message.
        /// </param>
        /// <param name="severity">
        /// 	The severity of the message.
        /// </param>
        public void Log(string message, int indent = 0, LogSeverity severity = LogSeverity.Info)
        {
            LogEntry newLog = new LogEntry(indent, message, severity);
            _logEntries.Add(newLog);

            // If autoScroll is enabled, adjust the scroll position to the bottom.
            if (_autoScroll)
            {
                // Assuming each log entry is roughly the same height, this will scroll to the bottom.
                _scrollPosition.y = float.MaxValue;
            }
        }

        /// <summary>
        /// Clear the console.
        /// </summary>
        public void Clear()
        {
            _logEntries.Clear();
        }

#if UNITY_EDITOR
        public void DrawInEditor()
        {
            DrawConsole(); // Draw the console
        }

        void DrawConsole()
        {
            // < TITLE > ---------------------------------------------------- >>
            EditorGUILayout.LabelField("Console", EditorStyles.boldLabel);

            // < SCROLL VIEW > ---------------------------------------------------- >>
            // Dark gray background
            GUIStyle backgroundStyle = new GUIStyle
            {
                normal =
                {
                    background = CustomInspectorGUI.MakeTex(
                        600,
                        1,
                        new Color(0.1f, 0.1f, 0.1f, 1.0f)
                    )
                }
            };

            float scrollHeight = _height * 10;
            _scrollPosition = EditorGUILayout.BeginScrollView(
                _scrollPosition,
                backgroundStyle,
                GUILayout.Height(scrollHeight)
            );

            int logCount = 0;
            foreach (LogEntry log in _logEntries)
            {
                DrawLogEntry(log);
                logCount++;
            }

            EditorGUILayout.EndScrollView();

            // < BUTTONS > ---------------------------------------------------- >>
            // Button to clear the console
            /*
            if (GUILayout.Button("Clear Console"))
            {
                Clear();
            }
            */

            // < SETTINGS > ---------------------------------------------------- >>
            DrawSettings();
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
            EditorGUI.indentLevel += 2;
            _settingsFoldoutOpen = EditorGUILayout.Foldout(
                _settingsFoldoutOpen,
                "Settings",
                true,
                EditorStyles.foldoutHeader
            );
            if (_settingsFoldoutOpen)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();

                // Toggle for enabling/disabling auto-scroll
                _autoScroll = EditorGUILayout.Toggle("Auto-scroll", _autoScroll);

                // Slider for adjusting the scroll height
                _height = EditorGUILayout.IntField("Console Height", (int)_height);

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel -= 2;
        }
#endif

        #region < PRIVATE_CLASS > [[ LogEntry ]] ================================================================
        class LogEntry
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

            public LogEntry(
                int indentLevel,
                string message,
                LogSeverity severity = LogSeverity.Info
            )
            {
                _indentLevel = indentLevel;
                _message = message.Trim();
                _severity = severity;
                _timeStamp = DateTime.Now;
            }
        }
        #endregion
    }
}
