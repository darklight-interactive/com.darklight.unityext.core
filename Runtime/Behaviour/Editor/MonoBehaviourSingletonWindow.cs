using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Darklight.Editor;

namespace Darklight.Behaviour.Editor
{
    public class MonoBehaviourSingletonWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<SingletonInfo> singletons = new List<SingletonInfo>();
        private bool autoRefresh = true;
        private double lastRefreshTime;
        private const double REFRESH_INTERVAL = 1.0; // seconds

        private class SingletonInfo
        {
            public MonoBehaviour instance;
            public string typeName;
            public string sceneName;
            public bool isValid;
            public bool hasStaticInstance;
        }

        [MenuItem(EditorPath.MENU_ROOT + "Singleton Manager", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<MonoBehaviourSingletonWindow>("MonoBehaviour Singleton Window");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshSingletons();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            RefreshSingletons();
        }

        private void OnHierarchyChanged()
        {
            if (autoRefresh)
            {
                RefreshSingletons();
            }
        }

        private void Update()
        {
            if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > REFRESH_INTERVAL)
            {
                RefreshSingletons();
            }
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawSingletonsList();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                RefreshSingletons();
            }

            GUILayout.Space(10);

            autoRefresh = GUILayout.Toggle(autoRefresh, "Auto Refresh", EditorStyles.toolbarButton);

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField($"Singletons: {singletons.Count}", EditorStyles.miniLabel, GUILayout.Width(100));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSingletonsList()
        {
            if (singletons.Count == 0)
            {
                EditorGUILayout.HelpBox("No singletons found in the current scene.", MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Group by validity
            var validSingletons = singletons.Where(s => s.isValid).ToList();
            var invalidSingletons = singletons.Where(s => !s.isValid).ToList();

            if (validSingletons.Any())
            {
                DrawSingletonGroup("Active Singletons", validSingletons, Color.green);
            }

            if (invalidSingletons.Any())
            {
                GUILayout.Space(10);
                DrawSingletonGroup("Invalid/Duplicate Singletons", invalidSingletons, Color.red);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSingletonGroup(string title, List<SingletonInfo> group, Color color)
        {
            // Header
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = oldColor;

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.Space(5);

            foreach (var singleton in group)
            {
                DrawSingletonItem(singleton);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSingletonItem(SingletonInfo info)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Icon
            var icon = info.isValid ? EditorGUIUtility.IconContent("TestPassed") : EditorGUIUtility.IconContent("TestFailed");
            GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));

            GUILayout.Space(5);

            // Type name and info section
            GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
            GUILayout.Label(info.typeName, EditorStyles.boldLabel, GUILayout.Height(16));

            // Scene info
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label("Scene:", EditorStyles.miniLabel, GUILayout.Width(40));
            GUILayout.Label(info.sceneName, EditorStyles.miniLabel);
            GUILayout.EndHorizontal();

            // Static instance status
            var statusColor = info.hasStaticInstance ? "green" : "red";
            var statusText = info.hasStaticInstance ? "Registered" : "Not Registered";
            GUILayout.Label($"Status: <color={statusColor}>{statusText}</color>",
                new GUIStyle(EditorStyles.miniLabel) { richText = true }, GUILayout.Height(14));

            GUILayout.EndVertical();

            GUILayout.Space(5);

            // Object field
            GUI.enabled = info.instance != null;
            EditorGUILayout.ObjectField(info.instance, typeof(MonoBehaviour), true, GUILayout.Width(150));
            GUI.enabled = true;

            // Ping button
            if (info.instance != null && GUILayout.Button("Ping", GUILayout.Width(50)))
            {
                EditorGUIUtility.PingObject(info.instance);
                Selection.activeObject = info.instance;
            }

            GUILayout.EndHorizontal();
        }

        private void RefreshSingletons()
        {
            lastRefreshTime = EditorApplication.timeSinceStartup;
            singletons.Clear();

            // Find all MonoBehaviour instances that inherit from MonoBehaviourSingleton<T>
            var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (var mb in allMonoBehaviours)
            {
                if (mb == null) continue;

                var type = mb.GetType();

                // Check if this type inherits from MonoBehaviourSingleton<T>
                if (IsMonoBehaviourSingleton(type))
                {
                    var info = new SingletonInfo
                    {
                        instance = mb,
                        typeName = type.Name,
                        sceneName = mb.gameObject.scene.name,
                        hasStaticInstance = CheckStaticInstance(type, mb),
                        isValid = true
                    };

                    // Check for duplicates
                    var duplicates = singletons.Where(s => s.typeName == info.typeName).ToList();
                    if (duplicates.Any())
                    {
                        // Mark all as invalid
                        foreach (var dup in duplicates)
                        {
                            dup.isValid = false;
                        }
                        info.isValid = false;
                    }

                    singletons.Add(info);
                }
            }

            LogSingletonSummary();
            Repaint();
        }

        private void LogSingletonSummary()
        {
            List<string> singletonNames = singletons.Select(s => s.typeName).ToList();
            var validCount = singletons.Count(s => s.isValid);
            var invalidCount = singletons.Count - validCount;

            var logMessage = $"<color=cyan><b>[Singleton Manager]</b></color> Found {singletons.Count} singleton(s) " +
                           $"(Valid: {validCount}, Invalid: {invalidCount})\n" +
                           $"Singletons:";
            foreach (string name in singletonNames)
            {
                logMessage += $"\n - {name}";
            }

            Debug.Log(logMessage, this);
        }

        private bool IsMonoBehaviourSingleton(System.Type type)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType &&
                    baseType.GetGenericTypeDefinition() == typeof(MonoBehaviourSingleton<>))
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }

        private bool CheckStaticInstance(System.Type type, MonoBehaviour instance)
        {
            try
            {
                // Find the singleton base type
                var baseType = type.BaseType;
                while (baseType != null)
                {
                    if (baseType.IsGenericType &&
                        baseType.GetGenericTypeDefinition() == typeof(MonoBehaviourSingleton<>))
                    {
                        // Get the static Instance property
                        var instanceProperty = baseType.GetProperty("Instance",
                            BindingFlags.Public | BindingFlags.Static);

                        if (instanceProperty != null)
                        {
                            var staticInstance = instanceProperty.GetValue(null);
                            return staticInstance == instance as object;
                        }
                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }
            catch
            {
                // If we can't access the static instance, assume it's not registered
            }

            return false;
        }
    }
}
