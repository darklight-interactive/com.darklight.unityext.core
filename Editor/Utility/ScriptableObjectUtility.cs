/* ======================================================================= ]]
 * Copyright (c) 2024 Darklight Interactive. All rights reserved.
 * Licensed under the Darklight Interactive Software License Agreement.
 * See LICENSE.md file in the project root for full license information.
 * ------------------------------------------------------------------ >>
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * ------------------------------------------------------------------ >>
 * For questions regarding this software or licensing, please contact:
 * Email: skysfalling22@gmail.com
 * Discord: skysfalling
 * ======================================================================= ]]
 * DESCRIPTION:
 * 
 * ------------------------------------------------------------------ >>
 * MAJOR AUTHORS: 
 * Sky Casey
 * ======================================================================= ]]
 */


using UnityEngine;
using System.IO;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Darklight.UnityExt.Utility
{
    #if UNITY_EDITOR
    /// <summary>
    /// Utility class for creating, loading, and deleting ScriptableObjects within the Unity Editor.
    /// </summary>
    public static class ScriptableObjectUtility
    {
        /// <summary>
        /// Creates a new ScriptableObject asset of type <typeparamref name="T"/> or loads an existing one if it already exists.
        /// </summary>
        /// <typeparam name="T">The type of ScriptableObject to create or load.</typeparam>
        /// <param name="pathToDirectory">The directory path where the ScriptableObject is or will be stored.</param>
        /// <param name="assetName">The name of the ScriptableObject asset.</param>
        /// <returns>The created or loaded ScriptableObject of type <typeparamref name="T"/>.</returns>
        public static T CreateOrLoadScriptableObject<T>(string pathToDirectory, string assetName) where T : ScriptableObject
        {
            // Ensure the path is formatted correctly
            if (!pathToDirectory.EndsWith("/"))
            {
                pathToDirectory += "/";
            }

            // Create the directory if it doesn't exist
            if (!Directory.Exists(pathToDirectory))
            {
                Directory.CreateDirectory(pathToDirectory);
                AssetDatabase.Refresh();
            }

            // Combine the path and asset name to get the full path
            string assetPath = pathToDirectory + assetName + ".asset";

            // Load the asset if it exists
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                // Create and save the asset if it doesn't exist
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return asset;
        }

        /// <summary>
        /// Creates a new ScriptableObject asset of type <typeparamref name="T"/> or loads an existing one if it already exists.
        /// The asset will be named after the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of ScriptableObject to create or load.</typeparam>
        /// <param name="pathToDirectory">The directory path where the ScriptableObject is or will be stored.</param>
        /// <returns>The created or loaded ScriptableObject of type <typeparamref name="T"/>.</returns>
        public static T CreateOrLoadScriptableObject<T>(string pathToDirectory) where T : ScriptableObject
        {
            return CreateOrLoadScriptableObject<T>(pathToDirectory, typeof(T).Name);
        }

        /// <summary>
        /// Deletes a ScriptableObject asset of the specified name from the given directory.
        /// </summary>
        /// <param name="pathToDirectory">The directory path where the ScriptableObject is stored.</param>
        /// <param name="assetName">The name of the ScriptableObject asset to delete.</param>
        public static void DeleteScriptableObject(string pathToDirectory, string assetName)
        {
            // Ensure the path is formatted correctly
            if (!pathToDirectory.EndsWith("/"))
            {
                pathToDirectory += "/";
            }

            // Combine the path and asset name to get the full path
            string assetPath = pathToDirectory + assetName + ".asset";

            // Load the asset if it exists
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset != null)
            {
                // Delete the asset if it exists
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }
        /// <summary>
        /// Deletes a ScriptableObject asset named after the type <typeparamref name="T"/> from the given directory.
        /// </summary>
        /// <typeparam name="T">The type of ScriptableObject to delete.</typeparam>
        /// <param name="pathToDirectory">The directory path where the ScriptableObject is stored.</param>
        public static void DeleteScriptableObject<T>(string pathToDirectory) where T : ScriptableObject
        {
            DeleteScriptableObject(pathToDirectory, typeof(T).Name);
        }
    }
    #endif

}