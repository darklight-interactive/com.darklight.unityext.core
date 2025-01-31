using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Editor
{
    /// <summary>
    /// An interface that allows for listening to editor reloads and scene view changes. <br/>
    /// Contains three methods: <br/>
    /// <see cref="OnEditorReloaded"/>: Called when the editor is reloaded. <br/>
    /// <see cref="OnSceneView"/>: Called when the scene view is updated. <br/>
    /// <see cref="OnSceneViewChanged"/>: Called when the scene view is changed. <br/>
    /// </summary>
    public interface IUnityEditorListener
    {
        /// <summary>
        /// Called when the editor is reloaded.
        /// </summary>
        void OnEditorReloaded() { }

        /// <summary>
        /// Called when the scene view is updated.
        /// </summary>
        void OnSceneView(SceneView sceneView) { }

        /// <summary>
        /// Called when the scene view is changed.
        /// </summary>
        void OnSceneViewChanged(SceneView sceneView) { }
    }

#if UNITY_EDITOR

    /// <summary>
    /// A class that listens for editor reloads and notifies all MonoBehaviour instances that implement IUnityEditorListener.
    /// </summary>
    [InitializeOnLoad]
    public class EditorHandler
    {
        public static string Prefix = "( Darklight.UnityExt )";
        private static List<IUnityEditorListener> Listeners = new List<IUnityEditorListener>();

        public static event Action OnEditorReloaded;
        public static event Action<SceneView> OnSceneView;
        public static event Action<SceneView> OnSceneViewChanged;

        static EditorHandler()
        {
            // Subscribe to editor reload events
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += OnEditorQuitting;
            SceneView.duringSceneGui += OnSceneGUI;
            OnEditorReloaded += () => DebugEvent("EditorReloaded");
            OnSceneView += (sceneView) => DebugEvent("SceneView");
            OnSceneViewChanged += (sceneView) => DebugEvent("SceneViewChanged");

            // Find all MonoBehaviour instances that implement IUnityEditorListener
            Listeners = UnityEngine
                .Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<IUnityEditorListener>()
                .ToList();

            foreach (IUnityEditorListener listener in Listeners)
            {
                InitializeListener(listener);
            }

            OnEditorReloaded?.Invoke();
            DebugEvent("EditorReloaded");
        }

        static void InitializeListener(IUnityEditorListener listener)
        {
            OnEditorReloaded += listener.OnEditorReloaded;
            OnSceneView += listener.OnSceneView;
            OnSceneViewChanged += listener.OnSceneViewChanged;
        }

        static void RemoveListener(IUnityEditorListener listener)
        {
            OnEditorReloaded -= listener.OnEditorReloaded;
            OnSceneView -= listener.OnSceneView;
            OnSceneViewChanged -= listener.OnSceneViewChanged;
        }

        static void OnBeforeAssemblyReload()
        {
            CleanupListeners();
        }

        static void OnEditorQuitting()
        {
            CleanupListeners();
        }

        static void CleanupListeners()
        {
            // Unsubscribe from SceneView event
            SceneView.duringSceneGui -= OnSceneGUI;

            // Remove all registered listeners
            foreach (var listener in Listeners)
            {
                RemoveListener(listener);
            }

            Listeners.Clear();
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            OnSceneView?.Invoke(sceneView);

            // Check for changes in the Scene view
            if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.KeyUp)
            {
                OnSceneViewChanged?.Invoke(sceneView);
            }
        }

        static void DebugEvent(string eventName)
        {
            Debug.Log(
                $"{Prefix} {eventName} -> Notifying {Listeners.Count} Listeners. \n {Listeners}"
            );
        }
    }
#endif
}
