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

namespace Darklight.Utility
{
    /// <summary>
    /// A utility class for retrieving information about the screen.
    /// </summary>
    public static class ScreenInfoUtility
    {
        public static Vector2 ScreenSize => GetScreenSize();

        /// public static float ScreenAspectRatio => GetScreenAspectRatio();

        /// <summary>
        /// Retrieves the size of the main game view.
        /// </summary>
        /// <returns>A Vector2 representing the size of the main game view.</returns>
        public static Vector2 GetScreenSize()
        {
#if UNITY_EDITOR
            return GetGameViewSizeInEditor();
#else
            return new Vector2(Screen.width, Screen.height);
#endif
        }

        private static Vector2 GetGameViewSizeInEditor()
        {
            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod(
                "GetSizeOfMainGameView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
            return (Vector2)Res;
        }

        /// <summary>
        /// Retrieves the aspect ratio value of the screen.
        /// </summary>
        /// <returns>The aspect ratio value of the screen.</returns>
        public static float GetScreenAspectRatio()
        {
            Vector2 gameViewSize = GetScreenSize();
            return gameViewSize.x / gameViewSize.y;
        }
    }
}
