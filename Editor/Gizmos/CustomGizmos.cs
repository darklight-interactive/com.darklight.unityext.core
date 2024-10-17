using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Darklight.UnityExt.Editor
{
#if UNITY_EDITOR
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
            Handles.Label(position, label, labelStyle);
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
            labelStyle.normal = new GUIStyleState { textColor = color }; // Set the text color
            Handles.Label(position, label, labelStyle);
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
            Handles.color = color == null ? Color.black : color;
            Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, size * Vector2.one, direction), Color.clear, color);
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
            DrawWireSquare(position, size, direction, color);

            labelStyle.normal = new GUIStyleState { textColor = color }; // Set the text color
            Vector3 labelOffset = new Vector3(size / 2, size / 2, 0);
            Vector3 labelPosition = position + (size * labelOffset);
            Handles.Label(labelPosition, label, labelStyle);
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
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(
                GetRectangleVertices(position, size * Vector2.one, direction),
                color, Color.clear);
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
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, area, direction), Color.clear, color);
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
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, area, direction), color, Color.clear);
        }
        #endregion

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

        /// <summary>
        /// Draws an arrow at the specified position in the specified direction with a given color, head length, and angle.
        /// </summary>
        /// <param name="position">The starting position of the arrow.</param>
        /// <param name="direction">The direction in which the arrow points.</param>
        /// <param name="color">The color of the arrow.</param>
        /// <param name="arrowHeadLength">The length of the arrowhead.</param>
        /// <param name="arrowHeadAngle">The angle of the arrowhead.</param>
        public static void DrawArrow(Vector3 position, Vector3 direction, Color color, float arrowHeadLength = 1f, float arrowHeadAngle = 45.0f)
        {
            Handles.color = color; // Set the color for the arrow

            // Draw the arrow shaft
            Handles.DrawLine(position, position + direction);

            // Calculate the right and left vectors for the arrowhead
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

            // Draw the arrowhead
            Vector3 arrowTip = position + direction;
            Handles.DrawLine(arrowTip, arrowTip + right * arrowHeadLength);
            Handles.DrawLine(arrowTip, arrowTip + left * arrowHeadLength);
        }
    }
#endif
}