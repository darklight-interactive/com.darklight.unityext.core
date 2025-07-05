using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
    /// <summary>
    /// Defines a basic singleton pattern for a Unity MonoBehaviour type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour, IUnityEditorListener
        where T : MonoBehaviour
    {
        static T _instance;

        public static string Prefix => $"<color=yellow><b>[{typeof(T).Name}]</b></color>";
        public static ConsoleGUI Console = new ConsoleGUI();
        public static T Instance
        {
            get
            {
                // If the instance is already set, return it.
                if (_instance != null)
                    return _instance;

                // Check if an instance of T already exists in the scene.
                _instance = FindFirstObjectByType<T>();
                if (_instance != null)
                {
                    Debug.Log(
                        $"{Prefix} Found existing instance of {typeof(T)} in scene.",
                        _instance
                    );
                    return _instance;
                }

                Debug.LogError($"{Prefix} Could not find instance of {typeof(T)} in scene.");
                return _instance;
            }
        }

        protected virtual void Initialize()
        {
            // << CREATE INSTANCE >> -------------------------- //
            if (_instance == null)
            {
                _instance = this as T;
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(this.gameObject);
                }
                Debug.Log($"{Prefix} Singleton instance created.", this);
            }
            else if (_instance != this)
            {
                if (Application.isPlaying)
                {
                    Destroy(this.gameObject);
                    Debug.LogWarning(
                        $"{Prefix} Singleton instance already exists. Destroying this instance.",
                        this
                    );
                }
                else
                {
                    Debug.LogError(
                        $"{Prefix} Singleton instance already exists. Please check the scene for duplicates.",
                        this
                    );
                }
                return;
            }
        }

        public virtual void OnEditorReloaded()
        {
            if (Application.isPlaying)
                return;

            if (_instance == null)
                Initialize();
        }
    }
}
