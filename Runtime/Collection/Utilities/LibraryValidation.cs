using UnityEngine;
using System;

namespace Darklight.UnityExt.Collection.Utilities
{
    /// <summary>
    /// Provides validation methods for library items.
    /// </summary>
    public static class LibraryValidation
    {
        /// <summary>
        /// Validates a Unity object value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        public static void ValidateUnityObject<T>(T value)
        {
            if (typeof(T).IsSubclassOf(typeof(UnityEngine.Object)))
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Unity Object reference cannot be null");
                }

                if (value is UnityEngine.Object unityObj && unityObj == null)
                {
                    throw new ArgumentException("Unity Object reference is destroyed", nameof(value));
                }
            }
        }
    }
} 