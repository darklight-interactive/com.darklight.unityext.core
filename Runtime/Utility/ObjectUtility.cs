using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;

namespace Darklight.UnityExt.Utility
{
    public static class ObjectUtility
    {
        #region ======== [[ DESTROY OBJECT ]] ================================== >>>>
        /// <summary>
        /// Destroys a GameObject in Play Mode or Editor Mode.
        /// </summary>
        /// <param name="obj">The object to destroy.</param>
        public static void DestroyAlways(Object obj)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.DestroyObjectImmediate(obj);
            }
            else
#endif
            {
                Object.Destroy(obj);
            }
        }
        #endregion


        #region ======== [[ INSTANTIATE OBJECT ]] ================================== >>>>
        /// <summary>
        /// Instantiates a GameObject in Play Mode or Editor Mode with options for position, rotation, and parenting.
        /// </summary>
        /// <param name="prefab">The original object to clone.</param>
        /// <param name="position">The position for the new object. Defaults to Vector3.zero.</param>
        /// <param name="rotation">The rotation for the new object. Defaults to Quaternion.identity.</param>
        /// <param name="parent">The parent transform for the new object. Defaults to null.</param>
        /// <param name="worldPositionStays">If true, retains the world position of the instantiated object. Defaults to true.</param>
        /// <typeparam name="T">The type of the object to instantiate, must be a GameObject or Component.</typeparam>
        /// <returns>The instantiated object.</returns>
        public static GameObject InstantiatePrefab(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            if (position == default) position = Vector3.zero;
            if (rotation == default) rotation = Quaternion.identity;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object instance = PrefabUtility.InstantiatePrefab(prefab);
                if (instance is not GameObject)
                {
                    Debug.LogError("Prefab is not a GameObject: " + prefab.name);
                    return null;
                }

                go.transform.position = position;
                go.transform.rotation = rotation;

                if (parent != null)
                {
                    go.transform.SetParent(parent, worldPositionStays);
                }

                Undo.RegisterCreatedObjectUndo(go, "Instantiate Object");
                return instance as GameObject;
            }
#endif
            return Object.Instantiate(prefab, position, rotation, parent);
        }

        public static GameObject InstantiatePrefab
            (GameObject prefab, Transform parent = null, bool worldPositionStays = false)
        {
            return InstantiatePrefab(prefab, Vector3.zero, Quaternion.identity, parent);
        }

        public static TComponent InstantiatePrefabWithComponent<TComponent>(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
            where TComponent : Component
        {
            GameObject go = InstantiatePrefab(prefab, position, rotation, parent);
            return go.GetComponent<TComponent>();
        }

        /// <summary>
        /// Instantiates multiple instances of a GameObject in Play Mode or Editor Mode.
        /// </summary>
        /// <param name="prefab">The original object to clone.</param>
        /// <param name="count">The number of instances to create.</param>
        /// <param name="parent">Optional parent transform for the new objects.</param>
        /// <typeparam name="T">The type of the object to instantiate, must be a GameObject or Component.</typeparam>
        /// <returns>A list of instantiated objects.</returns>
        public static List<GameObject> InstantiateMultiple(GameObject prefab, int count, Transform parent = null)
        {
            List<GameObject> instances = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                instances.Add(InstantiatePrefab(prefab, parent));
            }
            return instances;
        }
        #endregion


        /// <summary>
        /// Creates a new GameObject with the specified name and optional parent.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static GameObject CreateGameObject
            (string name, Func<GameObject, GameObject> initializer = null, Transform parent = null)
        {
            GameObject gameObject = new GameObject(name);
            if (initializer != null)
            {
                initializer(gameObject);
            }
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            return gameObject;
        }

        /// <summary>
        /// Creates a new GameObject with the specified name and optional parent.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <param name="name"></param>
        /// <param name="initializer"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static TComponent CreateGameObjectWithComponent<TComponent>
            (string name, Func<TComponent, TComponent> initializer = null, Transform parent = null)
            where TComponent : Component
        {
            GameObject gameObject = new GameObject(name);
            TComponent component = gameObject.AddComponent<TComponent>();
            if (initializer != null)
            {
                initializer(component);
            }
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            return component;
        }



        /// <summary>
        /// Creates a temporary GameObject in the Editor that is hidden and not saved with the scene.
        /// </summary>
        /// <param name="name">The name of the GameObject.</param>
        /// <returns>The created GameObject.</returns>
        public static GameObject CreateTemporaryEditorObject(string name)
        {
            // Create the GameObject with HideFlags that make it temporary
            GameObject tempObject = EditorUtility.CreateGameObjectWithHideFlags(name,
                HideFlags.HideAndDontSave | HideFlags.NotEditable);

            // Register the creation to allow undo
            Undo.RegisterCreatedObjectUndo(tempObject, "Create Temporary Editor Object");

            return tempObject;
        }


    }
}
