using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;

namespace Darklight.UnityExt.Utility
{
    public static class GameObjectUtility
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
        /// <param name="original">The original object to clone.</param>
        /// <param name="position">The position for the new object. Defaults to Vector3.zero.</param>
        /// <param name="rotation">The rotation for the new object. Defaults to Quaternion.identity.</param>
        /// <param name="parent">The parent transform for the new object. Defaults to null.</param>
        /// <param name="worldPositionStays">If true, retains the world position of the instantiated object. Defaults to true.</param>
        /// <typeparam name="T">The type of the object to instantiate, must be a GameObject or Component.</typeparam>
        /// <returns>The instantiated object.</returns>
        public static T InstantiateObject<T>(T original, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool worldPositionStays = true) where T : Object
        {
            if (position == default) position = Vector3.zero;
            if (rotation == default) rotation = Quaternion.identity;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                T instance = (T)PrefabUtility.InstantiatePrefab(original);
                if (instance is GameObject go)
                {
                    if (parent != null)
                    {
                        go.transform.SetParent(parent, worldPositionStays);
                    }
                    go.transform.position = position;
                    go.transform.rotation = rotation;
                    Undo.RegisterCreatedObjectUndo(go, "Instantiate Object");
                }
                return instance;
            }
#endif
            return Object.Instantiate(original, position, rotation, parent);
        }

        /// <summary>
        /// Instantiates multiple instances of a GameObject in Play Mode or Editor Mode.
        /// </summary>
        /// <param name="original">The original object to clone.</param>
        /// <param name="count">The number of instances to create.</param>
        /// <param name="parent">Optional parent transform for the new objects.</param>
        /// <typeparam name="T">The type of the object to instantiate, must be a GameObject or Component.</typeparam>
        /// <returns>A list of instantiated objects.</returns>
        public static List<T> InstantiateMultiple<T>(T original, int count, Transform parent = null) where T : Object
        {
            List<T> instances = new List<T>();
            for (int i = 0; i < count; i++)
            {
                instances.Add(InstantiateObject(original, parent: parent));
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
            GameObject go = new GameObject(name);
            if (initializer != null)
            {
                initializer(go);
            }
            if (parent != null)
            {
                go.transform.SetParent(parent);
            }
            return go;
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
