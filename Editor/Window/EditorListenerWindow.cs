#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    /// <summary>
    /// Editor window that displays the last invocation times for Unity Editor events.
    /// </summary>
    public class EditorListenerWindow : EditorWindow
    {
        private static Dictionary<string, DateTime> _lastEventTimes =
            new Dictionary<string, DateTime>();
        private Vector2 _scrollPosition;
        private static EditorListenerWindow _window;

        [MenuItem("Tools/Darklight/Editor Listener")]
        public static void ShowWindow()
        {
            _window = GetWindow<EditorListenerWindow>("Editor Events");
            _window.titleContent = new GUIContent("Editor Events");
            _window.minSize = new Vector2(300, 200);
            _window.Show();
        }

        private void OnEnable()
        {
            // Initialize event times if not already set
            if (_lastEventTimes.Count == 0)
            {
                _lastEventTimes["Editor Reloaded"] = DateTime.MinValue;
                _lastEventTimes["Scene View"] = DateTime.MinValue;
                _lastEventTimes["Scene View Changed"] = DateTime.MinValue;
            }

            // Subscribe to the events
            EditorHandler.OnEditorReloaded += OnEditorReloaded;
            EditorHandler.OnSceneView += OnSceneView;
            EditorHandler.OnSceneViewChanged += OnSceneViewChanged;
        }

        private void OnDisable()
        {
            // Unsubscribe from the events
            EditorHandler.OnEditorReloaded -= OnEditorReloaded;
            EditorHandler.OnSceneView -= OnSceneView;
            EditorHandler.OnSceneViewChanged -= OnSceneViewChanged;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Editor Event Times", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var eventTime in _lastEventTimes)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(eventTime.Key, GUILayout.Width(150));

                string timeDisplay =
                    eventTime.Value == DateTime.MinValue
                        ? "Never"
                        : $"{eventTime.Value:HH:mm:ss.fff}";

                EditorGUILayout.LabelField(timeDisplay);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Auto-repaint the window every second to keep times up to date
            if (DateTime.Now.Millisecond < 100)
            {
                Repaint();
            }
        }

        private void OnEditorReloaded()
        {
            _lastEventTimes["Editor Reloaded"] = DateTime.Now;
            if (_window != null)
                _window.Repaint();
        }

        private void OnSceneView(SceneView sceneView)
        {
            _lastEventTimes["Scene View"] = DateTime.Now;
            if (_window != null)
                _window.Repaint();
        }

        private void OnSceneViewChanged(SceneView sceneView)
        {
            _lastEventTimes["Scene View Changed"] = DateTime.Now;
            if (_window != null)
                _window.Repaint();
        }
    }
}
#endif
