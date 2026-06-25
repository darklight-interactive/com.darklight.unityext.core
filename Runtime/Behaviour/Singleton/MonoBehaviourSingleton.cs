using System;
using Darklight.Editor;
using UnityEngine;

namespace Darklight.Behaviour
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

        /// <summary>
        /// Provides a globally accessible singleton instance of the derived MonoBehaviour class.
        /// Ensures that only a single instance of the type exists in the scene.
        /// </summary>
        /// <remarks>
        /// If an instance of the type already exists in the scene, it is returned. Otherwise,
        /// a log message is generated to indicate that the instance could not be located.
        /// This property is useful for manager or orchestrator classes that need to be globally available
        /// within the project.
        /// </remarks>
        public static T Instance
        {
            get
            {
                // Check if an instance already exists
                if (_instance != null)
                    return _instance;

                return TryFindExistingInstance();
            }
        }

        static T TryFindExistingInstance()
        {
            _instance = FindAnyObjectByType<T>();
        
            if (_instance == null)
                Debug.LogError($"{Prefix} Could not find instance of {typeof(T)} in scene.");
            
            return _instance;
        }
        
        /// <summary>
        /// Initializes the singleton instance for the derived class.
        /// This method ensures that only one instance of the singleton exists.
        /// <br/><br/>
        /// If an instance already exists, duplicate instances are destroyed,
        /// and appropriate debug logs are generated.
        /// </summary>
        void Initialize()
        {
            // Check if an instance already exists
            if (_instance == null)
            {
                _instance = this as T;
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
                Debug.Log($"{Prefix} Singleton instance created.", this);
                return;
            }

            // Check if a duplicate instance exists
            if (_instance != this)
            {
                HandleDuplicateInstance();
            }
        }

        void HandleDuplicateInstance()
        {
            bool isPlayMode = Application.isPlaying;
        
            if (isPlayMode)
            {
                Destroy(gameObject);
                Debug.LogWarning($"{Prefix} Singleton instance already exists. Destroying this instance.", this);
            }
            else
            {
                Debug.LogError($"{Prefix} Singleton instance already exists. Please check the scene for duplicates.", this);
            }
        }
        
        protected void OnEnable() => Initialize();
        
        public void OnEditorReloaded()
        {
            if (Application.isPlaying)
                return;

            if (_instance == null)
                Initialize();
        }


        
    }
}
