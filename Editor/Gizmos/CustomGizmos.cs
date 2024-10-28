using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace Darklight.UnityExt.Editor
{
        public static class CustomGizmos
        {

#if UNITY_EDITOR
                /// <summary>
                /// Calculates the vertices of a rectangle given its center, size, and normal.
                /// </summary>
                /// <param name="center">The center position of the rectangle.</param>
                /// <param name="area">The width and height of the rectangle.</param>
                /// <param name="normal">The normal normal the rectangle is facing.</param>
                /// <returns>An array of Vector3 representing the vertices of the rectangle.</returns>
                static Vector3[] GetRectangleVertices(Vector3 center, Vector2 area, Vector3 normal)
                {
                        Vector2 halfArea = area * 0.5f;
                        Vector3[] vertices = new Vector3[4]
                        {
                                new Vector3(-halfArea.x, 0, -halfArea.y),
                                new Vector3(halfArea.x, 0, -halfArea.y),
                                new Vector3(halfArea.x, 0, halfArea.y),
                                new Vector3(-halfArea.x, 0, halfArea.y)
                        };

                        // Calculate the rotation from the up normal to the normal normal
                        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

                        // Apply rotation to each vertex
                        for (int i = 0; i < vertices.Length; i++)
                        {
                                vertices[i] = rotation * vertices[i] + center;
                        }

                        return vertices;
                }

                /// <summary>
                /// Calculates the vertices of a rectangle given its center, size, and rotation.
                /// </summary>
                /// <param name="center">The center position of the rectangle.</param>
                /// <param name="area">The width and height of the rectangle.</param>
                /// <param name="rotation">The rotation applied to the rectangle.</param>
                /// <returns>An array of Vector3 representing the vertices of the rectangle.</returns>
                static Vector3[] GetRectangleVertices(Vector3 center, Vector2 area, Quaternion rotation)
                {
                        Vector2 halfArea = area * 0.5f;
                        Vector3[] vertices = new Vector3[4]
                        {
                                new Vector3(-halfArea.x, 0, -halfArea.y),
                                new Vector3(halfArea.x, 0, -halfArea.y),
                                new Vector3(halfArea.x, 0, halfArea.y),
                                new Vector3(-halfArea.x, 0, halfArea.y)
                        };

                        // Apply the specified rotation to each vertex
                        for (int i = 0; i < vertices.Length; i++)
                        {
                                vertices[i] = rotation * vertices[i] + center;
                        }

                        return vertices;
                }

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

                #region -- << RECTS >> ------------------------------------ >>
                public static void DrawWireSquare(Vector3 position, float size, Vector3 normal, Color color)
                {
                        Handles.color = color == null ? Color.black : color;
                        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, size * Vector2.one, normal), Color.clear, color);
                }

                public static void DrawWireSquare(Vector3 position, float size, Quaternion rotation, Color color)
                {
                        Handles.color = color == null ? Color.black : color;
                        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, size * Vector2.one, rotation), Color.clear, color);
                }

                public static void DrawSolidSquare(Vector3 position, float size, Vector3 normal, Color color)
                {
                        Handles.color = color;
                        Handles.DrawSolidRectangleWithOutline(
                            GetRectangleVertices(position, size * Vector2.one, normal),
                            color, Color.clear);
                }

                public static void DrawSolidSquare(Vector3 position, float size, Quaternion rotation, Color color)
                {
                        Handles.color = color;
                        Handles.DrawSolidRectangleWithOutline(
                            GetRectangleVertices(position, size * Vector2.one, rotation),
                            color, Color.clear);
                }

                public static void DrawWireSquare_withLabel(string label, Vector3 position, float size, Vector3 normal, Color color, GUIStyle labelStyle)
                {
                        DrawWireSquare(position, size, normal, color);

                        labelStyle.normal = new GUIStyleState { textColor = color }; // Set the text color

                        Vector3 labelOffset = new Vector3(size / 2, size / 2, 0);
                        Vector3 labelPosition = position + (size * labelOffset);
                        Handles.Label(labelPosition, label, labelStyle);
                }

                public static void DrawWireSquare_withLabel(string label, Vector3 position, float size, Quaternion rotation, Color color, GUIStyle labelStyle)
                {
                        DrawWireSquare(position, size, rotation, color);
                        labelStyle.normal = new GUIStyleState { textColor = color }; // Set the text color
                        Vector3 labelOffset = new Vector3(size / 2, size / 2, 0);
                        Vector3 labelPosition = position + (size * labelOffset);
                        Handles.Label(labelPosition, label, labelStyle);
                }

                public static void DrawWireRect(Vector3 position, Vector2 area, Vector3 normal, Color color)
                {
                        Handles.color = color;
                        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, area, normal), Color.clear, color);
                }

                public static void DrawWireRect(Vector3 position, Vector2 area, Quaternion rotation, Color color)
                {
                        Handles.color = color;
                        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, area, rotation), Color.clear, color);
                }

                public static void DrawSolidRect(Vector3 position, Vector2 area, Vector3 normal, Color color)
                {

                        Handles.color = color;
                        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, area, normal), color, Color.clear);
                }

                public static void DrawSolidRect(Vector3 position, Vector2 area, Quaternion rotation, Color color)
                {
                        Handles.color = color;
                        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, area, rotation), color, Color.clear);
                }
                #endregion


                /// <summary>
                /// Draws a line between two points with a specified color and thickness.
                /// </summary>
                /// <param name="start">The starting point of the line.</param>
                /// <param name="end">The ending point of the line.</param>
                /// <param name="color">The color of the line.</param>
                /// <param name="thickness">The thickness of the line.</param>
                public static void DrawLine(Vector3 start, Vector3 end, Color color, float thickness = 1f)
                {
                        Handles.color = color;
                        Handles.DrawAAPolyLine(thickness, new Vector3[] { start, end });
                }



                /// <summary>
                /// Draws a Handles.Button and executes the given action when clicked.
                /// </summary>
                /// <param name="position">The position of the button in world space.</param>
                /// <param name="size">The size of the button.</param>
                /// <param name="normal">The normal the button is facing.</param>
                /// <param name="color">The color of the button.</param>
                /// <param name="onClick">The action to be invoked when the button is clicked.</param>
                /// <param name="capFunction">The function used to draw the button cap.</param>
                public static void DrawButtonHandle(Vector3 position, float size, Vector3 normal, Color color, System.Action onClick, Handles.CapFunction capFunction)
                {
                        Handles.color = color;
                        if (Handles.Button(position, Quaternion.LookRotation(normal), size / 2, size, capFunction))
                        {
                                onClick?.Invoke(); // Invoke the action if the button is clicked
                        }
                }
#endif
        }
}