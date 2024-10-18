using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Darklight.UnityExt.Editor
{
    public static class CustomGizmos
    {
        #region -- << LABELS >> ------------------------------------ >>

        /// <summary>
        /// Draws a label at the specified position using Handles.Label.
        /// </summary>
        /// <param name="label">The text of the label to be drawn.</param>
        /// <param name="position">The position in world space where the label will be drawn.</param>
        /// <param name="labelStyle">The GUIStyle to use for the label.</param>
        public static void DrawLabel(string label, Vector3 position, GUIStyle labelStyle)
        {
#if UNITY_EDITOR
            Handles.Label(position, label, labelStyle);
#endif
        }

        /// <summary>
        /// Draws a label at the specified position using Handles.Label with a specified color.
        /// </summary>
        /// <param name="label">The text of the label to be drawn.</param>
        /// <param name="position">The position in world space where the label will be drawn.</param>
        /// <param name="color">The color of the label text.</param>
        /// <param name="labelStyle">The GUIStyle to use for the label.</param>
        public static void DrawLabel(string label, Vector3 position, Color color, GUIStyle labelStyle)
        {
#if UNITY_EDITOR
            labelStyle.normal = new GUIStyleState { textColor = color }; // Set the text color
            Handles.Label(position, label, labelStyle);
#endif
        }
        #endregion

        #region -- << SHAPES >> ------------------------------------ >>

        /// <summary>
        /// Draws a wireframe square at the specified position, size, and direction with a specified color.
        /// </summary>
        /// <param name="position">The center position of the square in world space.</param>
        /// <param name="size">The size of the square.</param>
        /// <param name="direction">The normal direction of the square.</param>
        /// <param name="color">The color of the wireframe.</param>
        public static void DrawWireSquare(Vector3 position, float size, Vector3 direction, Color color)
        {
#if UNITY_EDITOR
            Handles.color = color == null ? Color.black : color;
            Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, size * Vector2.one, direction), Color.clear, color);
#endif
        }

        /// <summary>
        /// Draws a wireframe square with a label at the specified position, size, and direction with a specified color.
        /// </summary>
        /// <param name="label">The text of the label to be drawn.</param>
        /// <param name="position">The center position of the square in world space.</param>
        /// <param name="size">The size of the square.</param>
        /// <param name="direction">The normal direction of the square.</param>
        /// <param name="color">The color of the wireframe and label.</param>
        /// <param name="labelStyle">The GUIStyle to use for the label.</param>
        public static void DrawWireSquare_withLabel(string label, Vector3 position, float size, Vector3 direction, Color color, GUIStyle labelStyle)
        {
#if UNITY_EDITOR
            DrawWireSquare(position, size, direction, color);

            labelStyle.normal = new GUIStyleState { textColor = color }; // Set the text color
            Vector3 labelOffset = new Vector3(size / 2, size / 2, 0);
            Vector3 labelPosition = position + (size * labelOffset);
            Handles.Label(labelPosition, label, labelStyle);
#endif
        }

        /// <summary>
        /// Draws a solid square at the specified position, size, and direction with a specified color.
        /// </summary>
        /// <param name="position">The center position of the square in world space.</param>
        /// <param name="size">The size of the square.</param>
        /// <param name="direction">The normal direction of the square.</param>
        /// <param name="color">The color of the square.</param>
        public static void DrawSolidSquare(Vector3 position, float size, Vector3 direction, Color color)
        {
#if UNITY_EDITOR
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(
                GetRectangleVertices(position, size * Vector2.one, direction),
                color, Color.clear);
#endif
        }

        /// <summary>
        /// Draws a wireframe rectangle at the specified position, area, and direction with a specified color.
        /// </summary>
        /// <param name="position">The center position of the rectangle in world space.</param>
        /// <param name="area">The width and height of the rectangle.</param>
        /// <param name="direction">The normal direction of the rectangle.</param>
        /// <param name="color">The color of the wireframe.</param>
        public static void DrawWireRect(Vector3 position, Vector2 area, Vector3 direction, Color color)
        {
#if UNITY_EDITOR
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, area, direction), Color.clear, color);
#endif
        }

        /// <summary>
        /// Draws a solid rectangle at the specified position, area, and direction with a specified color.
        /// </summary>
        /// <param name="position">The center position of the rectangle in world space.</param>
        /// <param name="area">The width and height of the rectangle.</param>
        /// <param name="direction">The normal direction of the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        public static void DrawSolidRect(Vector3 position, Vector2 area, Vector3 direction, Color color)
        {
#if UNITY_EDITOR
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, area, direction), color, Color.clear);
#endif
        }
        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// Draws a Handles.Button and executes the given action when clicked.
        /// </summary>
        /// <param name="position">The position of the button in world space.</param>
        /// <param name="size">The size of the button.</param>
        /// <param name="direction">The direction the button is facing.</param>
        /// <param name="color">The color of the button.</param>
        /// <param name="onClick">The action to be invoked when the button is clicked.</param>
        /// <param name="capFunction">The function used to draw the button cap.</param>
        public static void DrawButtonHandle(Vector3 position, float size, Vector3 direction, Color color, System.Action onClick, Handles.CapFunction capFunction)
        {
            Handles.color = color;
            if (Handles.Button(position, Quaternion.LookRotation(direction), size / 2, size, capFunction))
            {
                onClick?.Invoke(); // Invoke the action if the button is clicked
            }
        }
#endif

        /// <summary>
        /// Calculates the vertices of a rectangle given its center, size, and direction.
        /// </summary>
        /// <param name="center">The center position of the rectangle.</param>
        /// <param name="area">The width and height of the rectangle.</param>
        /// <param name="normalDirection">The normal direction the rectangle is facing.</param>
        /// <returns>An array of Vector3 representing the vertices of the rectangle.</returns>
        static Vector3[] GetRectangleVertices(Vector3 center, Vector2 area, Vector3 normalDirection)
        {
            Vector2 halfArea = area * 0.5f;
            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(-halfArea.x, 0, -halfArea.y),
                new Vector3(halfArea.x, 0, -halfArea.y),
                new Vector3(halfArea.x, 0, halfArea.y),
                new Vector3(-halfArea.x, 0, halfArea.y)
            };

            // Calculate the rotation from the up direction to the normal direction
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normalDirection);

            // Apply rotation to each vertex
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = rotation * vertices[i] + center;
            }

            return vertices;
        }
    }
}