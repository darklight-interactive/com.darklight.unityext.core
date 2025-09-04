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
    Custom property drawer for MotionVector that displays it as a Vector3 field in the Inspector.
 * ------------------------------------------------------------------ >>
 * MAJOR AUTHORS:
 * Sky Casey
 * ======================================================================= ]]
 */

using Darklight.World;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
namespace Darklight.World.Editor
{
    /// <summary>
    /// Custom property drawer for MotionVector that displays it as a Vector3 field in the Inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(MotionVector))]
    public class MotionVectorPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Draws the MotionVector as a Vector3 field in the Inspector.
        /// </summary>
        /// <param name="position">The position and size of the property in the Inspector.</param>
        /// <param name="property">The SerializedProperty to draw.</param>
        /// <param name="label">The label for the property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the horizontal and vertical properties
            SerializedProperty horizontalProp = property.FindPropertyRelative("_vector");
            SerializedProperty xProp = horizontalProp.FindPropertyRelative("x");
            SerializedProperty yProp = horizontalProp.FindPropertyRelative("y");
            SerializedProperty zProp = horizontalProp.FindPropertyRelative("z");

            // Create a temporary Vector3 to display in the Inspector
            Vector3 currentValue = new Vector3(
                xProp.floatValue,
                yProp.floatValue,
                zProp.floatValue
            );

            // Draw the Vector3 field
            Vector3 newValue = EditorGUI.Vector3Field(position, label, currentValue);

            // Update the underlying properties if the value changed
            if (newValue != currentValue)
            {
                xProp.floatValue = newValue.x;
                yProp.floatValue = newValue.y;
                zProp.floatValue = newValue.z;

                // Mark the property as dirty so Unity knows to save the changes
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Gets the height needed to draw this property.
        /// </summary>
        /// <param name="property">The SerializedProperty to get the height for.</param>
        /// <param name="label">The label for the property.</param>
        /// <returns>The height needed to draw the property.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector3, label);
        }
    }
}
#endif
