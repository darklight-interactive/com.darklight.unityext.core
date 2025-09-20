using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public static class CustomGizmos
    {
        // Static buffers to avoid garbage collection during frequent drawing operations
        private static readonly Vector3[] s_CubeVertexBuffer = new Vector3[8];
        private static readonly Vector3[][] s_CubeFaceBuffer = new Vector3[][]
        {
            new Vector3[4], // Front face (facing negative Z)
            new Vector3[4], // Right face (facing positive X)
            new Vector3[4], // Back face (facing positive Z)
            new Vector3[4], // Left face (facing negative X)
            new Vector3[4], // Top face (facing positive Y)
            new Vector3[4] // Bottom face (facing negative Y)
        };

        // Buffer for sphere circle points (33 points for 32 segments + 1 closing point)
        private static readonly Vector3[] s_CirclePoints = new Vector3[33];
#if UNITY_EDITOR

        /// <summary>
        /// Generates the base vertices for a rectangle in local space.
        /// </summary>
        /// <param name="area">The width and height of the rectangle.</param>
        /// <param name="subdivisions">The number of subdivisions along each axis. 0 means no subdivisions (4 vertices).</param>
        /// <returns>An array of Vector3 representing the vertices of the rectangle in local space.</returns>
        static Vector3[] GenerateBaseRectangleVertices(Vector2 area, int subdivisions = 0)
        {
            Vector2 halfArea = area * 0.5f;

            if (subdivisions <= 0)
            {
                // No subdivisions - return the 4 corner vertices
                return new Vector3[4]
                {
                    new Vector3(-halfArea.x, 0, -halfArea.y),
                    new Vector3(halfArea.x, 0, -halfArea.y),
                    new Vector3(halfArea.x, 0, halfArea.y),
                    new Vector3(-halfArea.x, 0, halfArea.y)
                };
            }
            else
            {
                // Generate edge-subdivided vertices
                // Total vertices = 4 corners + subdivisions on each of 4 edges
                // Each edge has (subdivisions - 1) additional vertices between corners
                int totalVertices = 4 + (subdivisions - 1) * 4;
                Vector3[] vertices = new Vector3[totalVertices];

                // Calculate step sizes for each edge
                float xStep = area.x / subdivisions;
                float zStep = area.y / subdivisions;

                int vertexIndex = 0;

                // Bottom edge (left to right)
                for (int i = 0; i <= subdivisions; i++)
                {
                    float xPos = -halfArea.x + (i * xStep);
                    vertices[vertexIndex++] = new Vector3(xPos, 0, -halfArea.y);
                }

                // Right edge (bottom to top, skip bottom corner)
                for (int i = 1; i <= subdivisions; i++)
                {
                    float zPos = -halfArea.y + (i * zStep);
                    vertices[vertexIndex++] = new Vector3(halfArea.x, 0, zPos);
                }

                // Top edge (right to left, skip right corner)
                for (int i = subdivisions - 1; i >= 0; i--)
                {
                    float xPos = -halfArea.x + (i * xStep);
                    vertices[vertexIndex++] = new Vector3(xPos, 0, halfArea.y);
                }

                // Left edge (top to bottom, skip top and bottom corners)
                for (int i = subdivisions - 1; i > 0; i--)
                {
                    float zPos = -halfArea.y + (i * zStep);
                    vertices[vertexIndex++] = new Vector3(-halfArea.x, 0, zPos);
                }

                return vertices;
            }
        }

        /// <summary>
        /// Applies transformation to an array of vertices.
        /// </summary>
        /// <param name="vertices">The vertices to transform.</param>
        /// <param name="rotation">The rotation to apply.</param>
        /// <param name="center">The center position to translate to.</param>
        private static void TransformVertices(
            Vector3[] vertices,
            Quaternion rotation,
            Vector3 center
        )
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = rotation * vertices[i] + center;
            }
        }

        /// <summary>
        /// Calculates the vertices of a rectangle given its center, size, and normal.
        /// </summary>
        /// <param name="center">The center position of the rectangle.</param>
        /// <param name="area">The width and height of the rectangle.</param>
        /// <param name="normal">The normal normal the rectangle is facing.</param>
        /// <param name="subdivisions">The number of subdivisions along each axis. 0 means no subdivisions (4 vertices).</param>
        /// <returns>An array of Vector3 representing the vertices of the rectangle.</returns>
        public static Vector3[] GenerateRectangleVertices(
            Vector3 center,
            Vector2 area,
            Vector3 normal,
            int subdivisions = 0
        )
        {
            Vector3[] vertices = GenerateBaseRectangleVertices(area, subdivisions);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
            TransformVertices(vertices, rotation, center);
            return vertices;
        }

        /// <summary>
        /// Calculates the vertices of a rectangle given its center, size, and rotation.
        /// </summary>
        /// <param name="center">The center position of the rectangle.</param>
        /// <param name="area">The width and height of the rectangle.</param>
        /// <param name="rotation">The rotation applied to the rectangle.</param>
        /// <param name="subdivisions">The number of subdivisions along each axis. 0 means no subdivisions (4 vertices).</param>
        /// <returns>An array of Vector3 representing the vertices of the rectangle.</returns>
        public static Vector3[] GenerateRectangleVertices(
            Vector3 center,
            Vector2 area,
            Quaternion rotation,
            int subdivisions = 0
        )
        {
            Vector3[] vertices = GenerateBaseRectangleVertices(area, subdivisions);
            TransformVertices(vertices, rotation, center);
            return vertices;
        }

        /// <summary>
        /// Generates an array of points around a circle.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle. Must be greater than 0.001f.</param>
        /// <param name="rotation">The rotation applied to the circle.</param>
        /// <param name="segments">The number of points to generate. Must be greater than 2.</param>
        /// <returns>An array of points around the circle.</returns>
        public static Vector3[] GenerateRadialVertices(
            Vector3 center,
            float radius,
            Quaternion rotation,
            int segments
        )
        {
            // Validate the radius and segments
            radius = Mathf.Max(radius, 0.001f);
            segments = Mathf.Max(segments, 3);

            List<Vector3> vertices = new List<Vector3>();

            // Foreach step in the circle, calculate the points
            float angleStep = 360.0f / segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep;
                Vector3 newPoint =
                    center
                    + rotation * Quaternion.AngleAxis(angle, Vector3.up) * Vector3.right * radius;
                vertices.Add(newPoint);
            }
            return vertices.ToArray();
        }

        /// <summary>
        /// Generates an array of points around a circle and outputs them to the provided array.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle. Must be greater than 0.001f.</param>
        /// <param name="rotation">The rotation applied to the circle.</param>
        /// <param name="segments">The number of points to generate. Must be greater than 2.</param>
        /// <param name="vertices">The output array of points around the circle.</param>
        public static void GenerateRadialVertices(
            Vector3 center,
            float radius,
            Quaternion rotation,
            int segments,
            out Vector3[] vertices
        )
        {
            vertices = GenerateRadialVertices(center, radius, rotation, segments);
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
        public static void DrawLabel(
            string label,
            Vector3 position,
            Color color,
            GUIStyle labelStyle
        )
        {
            labelStyle.normal = new GUIStyleState { textColor = color }; // Set the text color
            Handles.Label(position, label, labelStyle);
        }
        #endregion

        #region -- << DRAW LINE >> ------------------------------------ >>
        /// <summary>
        /// Draws a line between two points with a specified color and thickness.
        /// </summary>
        /// <param name="start">The starting point of the line.</param>
        /// <param name="end">The ending point of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float thickness = 0f)
        {
            Handles.color = color;

            if (thickness <= 0)
            {
                Handles.DrawLine(start, end);
                return;
            }

            Handles.DrawAAPolyLine(thickness, new Vector3[] { start, end });
        }

        public static void DrawLineWithLabel(
            Vector3 start,
            Vector3 end,
            Color color,
            string label,
            float thickness = 0f
        )
        {
            DrawLine(start, end, color, thickness);

            // Draw label at the middle of the line
            Vector3 middle = (start + end) / 2;
            Handles.Label(middle, label);
        }
        #endregion << DRAW LINE >> ------------------------------------ >>

        #region -- << DRAW BUTTON >> ------------------------------------------------- >>
        /// <summary>
        /// Draws a Handles.Button and executes the given action when clicked.
        /// </summary>
        /// <param name="position">The position of the button in world space.</param>
        /// <param name="size">The size of the button.</param>
        /// <param name="normal">The normal the button is facing.</param>
        /// <param name="color">The color of the button.</param>
        /// <param name="onClick">The action to be invoked when the button is clicked.</param>
        /// <param name="capFunction">The function used to draw the button cap.</param>
        public static void DrawButtonHandle(
            Vector3 position,
            float size,
            Vector3 normal,
            Color color,
            System.Action onClick,
            Handles.CapFunction capFunction
        )
        {
            Handles.color = color;
            if (
                Handles.Button(
                    position,
                    Quaternion.LookRotation(normal),
                    size / 2,
                    size / 2,
                    capFunction
                )
            )
            {
                onClick?.Invoke(); // Invoke the action if the button is clicked
            }
        }
        #endregion

        #region -- << DRAW POLYGON >> ------------------------------------ >>
        public static void DrawPolygon(Vector3[] vertices, Color color, float opacity = 1f)
        {
            Handles.color = new Color(color.r, color.g, color.b, opacity);
            Handles.DrawAAConvexPolygon(vertices);
        }
        #endregion

        #region -- << DRAW 2D RADIAL >> ------------------------------------ >>

        /// <summary>
        /// Draws a wire radial shape.
        /// </summary>
        /// <param name="center">The center of the shape.</param>
        /// <param name="radius">The radius of the shape. Must be greater than 0.001f.</param>
        /// <param name="normal">The normal of the shape.</param>
        /// <param name="segments">The number of segments of the shape. Must be greater than 2.</param>
        /// <param name="color">The color of the shape.</param>
        public static void DrawWireRadialShape(
            Vector3 center,
            float radius,
            Quaternion rotation,
            int segments,
            Color color
        )
        {
            Vector3[] vertices = GenerateRadialVertices(center, radius, rotation, segments);

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 start = vertices[i];
                Vector3 end = vertices[(i + 1) % vertices.Length];
                Handles.color = color;
                Handles.DrawLine(start, end);
            }
        }

        /// <summary>
        /// Draws a solid radial shape.
        /// </summary>
        /// <param name="center">The center of the shape.</param>
        /// <param name="radius">The radius of the shape. Must be greater than 0.001f.</param>
        /// <param name="normal">The normal of the shape.</param>
        /// <param name="segments">The number of segments of the shape. Must be greater than 2.</param>
        /// <param name="color">The color of the shape.</param>
        public static void DrawSolidRadialShape(
            Vector3 center,
            float radius,
            Quaternion rotation,
            int segments,
            Color color
        )
        {
            Vector3[] vertices = GenerateRadialVertices(center, radius, rotation, segments);

            Handles.color = color;
            Handles.DrawAAConvexPolygon(vertices);
        }
        #endregion

        #region -- << DRAW 2D RECT >> ------------------------------------ >>


        /// <summary>
        /// Draws a wire square at the specified position with the specified size and normal, and a label at the top center of the square.
        /// </summary>
        /// <param name="label">The text of the label to be drawn.</param>
        /// <param name="position">The position of the square in world space.</param>
        /// <param name="size">The size of the square.</param>
        /// <param name="normal">The normal the square is facing.</param>
        /// <param name="color">The color of the square.</param>
        /// <param name="labelStyle">The GUIStyle to use for the label.</param>
        public static void DrawWireSquare_withLabel(
            string label,
            Vector3 position,
            float size,
            Vector3 normal,
            Color color,
            GUIStyle labelStyle
        )
        {
            DrawWireSquare(position, size, normal, color);

            labelStyle.normal = new GUIStyleState { textColor = color }; // Set the text color

            Vector3 labelOffset = new Vector3(size / 2, size / 2, 0);
            Vector3 labelPosition = position + (size * labelOffset);
            Handles.Label(labelPosition, label, labelStyle);
        }

        /// <summary>
        /// Draws a wire square at the specified position with the specified size and rotation, and a label at the top center of the square.
        /// </summary>
        /// <param name="label">The text of the label to be drawn.</param>
        /// <param name="position">The position of the square in world space.</param>
        /// <param name="size">The size of the square.</param>
        /// <param name="rotation">The rotation applied to the square.</param>
        /// <param name="color">The color of the square.</param>
        /// <param name="labelStyle">The GUIStyle to use for the label.</param>
        public static void DrawWireSquare_withLabel(
            string label,
            Vector3 position,
            float size,
            Quaternion rotation,
            Color color,
            GUIStyle labelStyle
        )
        {
            DrawWireSquare(position, size, rotation, color);
            labelStyle.normal = new GUIStyleState { textColor = color }; // Set the text color
            Vector3 labelOffset = new Vector3(size / 2, size / 2, 0);
            Vector3 labelPosition = position + (size * labelOffset);
            Handles.Label(labelPosition, label, labelStyle);
        }

        /// <summary>
        /// Draws a wire rectangle at the specified position with the specified size and normal.
        /// </summary>
        /// <param name="position">The position of the rectangle in world space.</param>
        /// <param name="area">The width and height of the rectangle.</param>
        /// <param name="normal">The normal the rectangle is facing.</param>
        /// <param name="color">The color of the rectangle.</param>
        public static void DrawWireRect(Vector3 position, Vector2 area, Vector3 normal, Color color)
        {
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(
                GenerateRectangleVertices(position, area, normal),
                Color.clear,
                color
            );
        }

        /// <summary>
        /// Draws a wire rectangle at the specified position with the specified size and rotation.
        /// </summary>
        /// <param name="position">The position of the rectangle in world space.</param>
        /// <param name="area">The width and height of the rectangle.</param>
        /// <param name="rotation">The rotation applied to the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        public static void DrawWireRect(
            Vector3 position,
            Vector2 area,
            Quaternion rotation,
            Color color
        )
        {
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(
                GenerateRectangleVertices(position, area, rotation),
                Color.clear,
                color
            );
        }

        /// <summary>
        /// Draws a solid rectangle at the specified position with the specified size and normal.
        /// </summary>
        /// <param name="position">The position of the rectangle in world space.</param>
        /// <param name="area">The width and height of the rectangle.</param>
        /// <param name="normal">The normal the rectangle is facing.</param>
        /// <param name="color">The color of the rectangle.</param>
        public static void DrawSolidRect(
            Vector3 position,
            Vector2 area,
            Vector3 normal,
            Color color
        )
        {
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(
                GenerateRectangleVertices(position, area, normal),
                color,
                Color.clear
            );
        }

        /// <summary>
        /// Draws a solid rectangle at the specified position with the specified size and rotation.
        /// </summary>
        /// <param name="position">The position of the rectangle in world space.</param>
        /// <param name="area">The width and height of the rectangle.</param>
        /// <param name="rotation">The rotation applied to the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        public static void DrawSolidRect(
            Vector3 position,
            Vector2 area,
            Quaternion rotation,
            Color color
        )
        {
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(
                GenerateRectangleVertices(position, area, rotation),
                color,
                Color.clear
            );
        }

        /// <summary>
        /// Draws a wire square at the specified position with the specified size and normal.
        /// </summary>
        /// <param name="position">The position of the square in world space.</param>
        /// <param name="size">The size of the square.</param>
        /// <param name="normal">The normal the square is facing.</param>
        /// <param name="color">The color of the square.</param>
        public static void DrawWireSquare(Vector3 position, float size, Vector3 normal, Color color)
        {
            Handles.color = color == null ? Color.black : color;
            Handles.DrawSolidRectangleWithOutline(
                GenerateRectangleVertices(position, size * Vector2.one, normal),
                Color.clear,
                color
            );
        }

        /// <summary>
        /// Draws a wire square at the specified position with the specified size and rotation.
        /// </summary>
        /// <param name="position">The position of the square in world space.</param>
        /// <param name="size">The size of the square.</param>
        /// <param name="rotation">The rotation applied to the square.</param>
        /// <param name="color">The color of the square.</param>
        public static void DrawWireSquare(
            Vector3 position,
            float size,
            Quaternion rotation,
            Color color
        )
        {
            Handles.color = color == null ? Color.black : color;
            Handles.DrawSolidRectangleWithOutline(
                GenerateRectangleVertices(position, size * Vector2.one, rotation),
                Color.clear,
                color
            );
        }

        /// <summary>
        /// Draws a solid square at the specified position with the specified size and normal.
        /// </summary>
        /// <param name="position">The position of the square in world space.</param>
        /// <param name="size">The size of the square.</param>
        /// <param name="normal">The normal the square is facing.</param>
        /// <param name="color">The color of the square.</param>
        public static void DrawSolidSquare(
            Vector3 position,
            float size,
            Vector3 normal,
            Color color
        )
        {
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(
                GenerateRectangleVertices(position, size * Vector2.one, normal),
                color,
                Color.clear
            );
        }

        /// <summary>
        /// Draws a solid square at the specified position with the specified size and rotation.
        /// </summary>
        /// <param name="position">The position of the square in world space.</param>
        /// <param name="size">The size of the square.</param>
        /// <param name="rotation">The rotation applied to the square.</param>
        /// <param name="color">The color of the square.</param>
        public static void DrawSolidSquare(
            Vector3 position,
            float size,
            Quaternion rotation,
            Color color
        )
        {
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(
                GenerateRectangleVertices(position, size * Vector2.one, rotation),
                color,
                Color.clear
            );
        }

        #endregion << DRAW 2D SQUARE & RECTANGLE >> ------------------------------------ >>

        #region -- << DRAW 3D RECT >> ------------------------------------ >>
        /// <summary>
        /// Calculate all 8 vertices of the cube using the static buffer. <br/>
        /// Vertex order: (bottom face: 0-3, top face: 4-7) <br/>
        /// <code>
        ///    4 -------- 5
        ///   /|         /|
        ///  7 -------- 6 |
        ///  | |        | |
        ///  | 0 -------|-1
        ///  |/         |/
        ///  3 -------- 2
        /// </code>
        /// The cube is drawn with:<br/>
        /// - Bottom face: vertices 0,1,2,3 (counter-clockwise)<br/>
        /// - Top face: vertices 4,5,6,7 (counter-clockwise)<br/>
        /// - Side edges connecting corresponding vertices between faces<br/>
        /// </summary>
        static void CalculateCubeVertices(
            Vector3 position,
            Vector3 size,
            Quaternion rotation,
            out Vector3[] vertices
        )
        {
            Vector3 halfSize = size * 0.5f;
            s_CubeVertexBuffer[0] =
                rotation * new Vector3(-halfSize.x, -halfSize.y, -halfSize.z) + position;
            s_CubeVertexBuffer[1] =
                rotation * new Vector3(halfSize.x, -halfSize.y, -halfSize.z) + position;
            s_CubeVertexBuffer[2] =
                rotation * new Vector3(halfSize.x, -halfSize.y, halfSize.z) + position;
            s_CubeVertexBuffer[3] =
                rotation * new Vector3(-halfSize.x, -halfSize.y, halfSize.z) + position;
            s_CubeVertexBuffer[4] =
                rotation * new Vector3(-halfSize.x, halfSize.y, -halfSize.z) + position;
            s_CubeVertexBuffer[5] =
                rotation * new Vector3(halfSize.x, halfSize.y, -halfSize.z) + position;
            s_CubeVertexBuffer[6] =
                rotation * new Vector3(halfSize.x, halfSize.y, halfSize.z) + position;
            s_CubeVertexBuffer[7] =
                rotation * new Vector3(-halfSize.x, halfSize.y, halfSize.z) + position;

            vertices = s_CubeVertexBuffer;
        }

        /// <summary>
        /// Draws a 3D wire cube at the specified position with given dimensions.
        /// </summary>
        public static void DrawWireCube(
            Vector3 position,
            Vector3 size,
            Quaternion rotation,
            Color color
        )
        {
            Handles.color = color;
            CalculateCubeVertices(position, size, rotation, out Vector3[] vertices);

            // Draw all edges in a single call to reduce draw overhead
            // Format: each pair of vertices defines one edge of the cube
            Handles.DrawLines(
                new Vector3[]
                {
                    // Bottom face edges
                    vertices[0],
                    vertices[1],
                    vertices[1],
                    vertices[2],
                    vertices[2],
                    vertices[3],
                    vertices[3],
                    vertices[0],
                    // Top face edges
                    vertices[4],
                    vertices[5],
                    vertices[5],
                    vertices[6],
                    vertices[6],
                    vertices[7],
                    vertices[7],
                    vertices[4],
                    // Vertical edges connecting top and bottom faces
                    vertices[0],
                    vertices[4],
                    vertices[1],
                    vertices[5],
                    vertices[2],
                    vertices[6],
                    vertices[3],
                    vertices[7]
                }
            );
        }

        /// <summary>
        /// Draws a solid 3D cube with wireframe outline at the specified position.
        /// </summary>
        public static void DrawSolidCube(
            Vector3 position,
            Vector3 size,
            Quaternion rotation,
            Color faceColor,
            Color wireColor
        )
        {
            // Create semi-transparent color for faces
            Color transparentColor = new Color(
                faceColor.r,
                faceColor.g,
                faceColor.b,
                faceColor.a * 0.5f
            );
            Handles.color = transparentColor;

            Vector3 halfSize = size * 0.5f;

            // Calculate vertices (same as DrawWireCube)
            // ... vertex calculation code ...

            // Assign vertices to each face in the cube face buffer
            // Front face (negative Z)
            s_CubeFaceBuffer[0][0] = s_CubeVertexBuffer[0];
            s_CubeFaceBuffer[0][1] = s_CubeVertexBuffer[1];
            s_CubeFaceBuffer[0][2] = s_CubeVertexBuffer[5];
            s_CubeFaceBuffer[0][3] = s_CubeVertexBuffer[4];

            // Right face (positive X)
            s_CubeFaceBuffer[1][0] = s_CubeVertexBuffer[1];
            s_CubeFaceBuffer[1][1] = s_CubeVertexBuffer[2];
            s_CubeFaceBuffer[1][2] = s_CubeVertexBuffer[6];
            s_CubeFaceBuffer[1][3] = s_CubeVertexBuffer[5];

            // Back face (positive Z)
            s_CubeFaceBuffer[2][0] = s_CubeVertexBuffer[2];
            s_CubeFaceBuffer[2][1] = s_CubeVertexBuffer[3];
            s_CubeFaceBuffer[2][2] = s_CubeVertexBuffer[7];
            s_CubeFaceBuffer[2][3] = s_CubeVertexBuffer[6];

            // Left face (negative X)
            s_CubeFaceBuffer[3][0] = s_CubeVertexBuffer[3];
            s_CubeFaceBuffer[3][1] = s_CubeVertexBuffer[0];
            s_CubeFaceBuffer[3][2] = s_CubeVertexBuffer[4];
            s_CubeFaceBuffer[3][3] = s_CubeVertexBuffer[7];

            // Top face (positive Y)
            s_CubeFaceBuffer[4][0] = s_CubeVertexBuffer[4];
            s_CubeFaceBuffer[4][1] = s_CubeVertexBuffer[5];
            s_CubeFaceBuffer[4][2] = s_CubeVertexBuffer[6];
            s_CubeFaceBuffer[4][3] = s_CubeVertexBuffer[7];

            // Bottom face (negative Y)
            s_CubeFaceBuffer[5][0] = s_CubeVertexBuffer[3];
            s_CubeFaceBuffer[5][1] = s_CubeVertexBuffer[2];
            s_CubeFaceBuffer[5][2] = s_CubeVertexBuffer[1];
            s_CubeFaceBuffer[5][3] = s_CubeVertexBuffer[0];

            // Draw all faces with transparency
            foreach (var face in s_CubeFaceBuffer)
            {
                Handles.DrawSolidRectangleWithOutline(face, transparentColor, Color.clear);
            }

            // Add wireframe outline for better visibility
            DrawWireCube(position, size, rotation, wireColor);
        }
        #endregion

        #region -- << DRAW 2D CIRCLE >> ------------------------------------ >>

        /// <summary>
        /// Draws a wire circle.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle. Must be greater than 0.001f.</param>
        /// <param name="normal">The normal of the circle.</param>
        /// <param name="color">The color of the circle.</param>
        /// <param name="segments">The number of segments of the circle.
        ///     Must be greater than 6. Default is 32.</param>
        public static void DrawWireCircle(
            Vector3 center,
            float radius,
            Quaternion rotation,
            Color color,
            int segments = 16
        )
        {
            segments = Mathf.Max(segments, 6);

            DrawWireRadialShape(center, radius, rotation, segments, color);
        }

        /// <summary>
        /// Draws a wire circle.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle. Must be greater than 0.001f.</param>
        /// <param name="normal">The normal of the circle.</param>
        /// <param name="color">The color of the circle.</param>
        /// <param name="segments">The number of segments of the circle.
        ///     Must be greater than 6. Default is 32.</param>
        public static void DrawSolidCircle(
            Vector3 center,
            float radius,
            Quaternion rotation,
            Color color,
            int segments = 16
        )
        {
            segments = Mathf.Max(segments, 6);

            DrawSolidRadialShape(center, radius, rotation, segments, color);
        }

        #endregion

        #region -- << DRAW 3D SPHERE >> ------------------------------------ >>

        /// <summary>
        /// Draws a wireframe sphere using three orthogonal circles.
        /// </summary>
        public static void DrawWireSphere(
            Vector3 position,
            float radius,
            Color color,
            int segments = 32
        )
        {
            Handles.color = color;

            // Pre-calculate circle points once and reuse for all three circles
            float angleStep = 360f / segments;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                // Calculate point on unit circle and scale by radius
                float sin = Mathf.Sin(angle);
                float cos = Mathf.Cos(angle);
                s_CirclePoints[i] = new Vector3(cos, sin, 0) * radius;
            }

            // Store original matrix to restore later
            Matrix4x4 originalMatrix = Handles.matrix;

            // Draw three orthogonal circles using matrix transformations
            // XY plane (front view)
            Handles.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
            Handles.DrawAAPolyLine(2f, segments + 1, s_CirclePoints);

            // XZ plane (top view)
            Handles.matrix = Matrix4x4.TRS(position, Quaternion.Euler(90, 0, 0), Vector3.one);
            Handles.DrawAAPolyLine(2f, segments + 1, s_CirclePoints);

            // YZ plane (side view)
            Handles.matrix = Matrix4x4.TRS(position, Quaternion.Euler(0, 90, 0), Vector3.one);
            Handles.DrawAAPolyLine(2f, segments + 1, s_CirclePoints);

            // Restore original matrix
            Handles.matrix = originalMatrix;
        }

        /// <summary>
        /// Draws a solid sphere with wireframe outline for better depth perception.
        /// </summary>
        public static void DrawSolidSphere(
            Vector3 position,
            float radius,
            Color sphereColor,
            Color wireColor,
            int segments = 32
        )
        {
            // Draw solid sphere using Unity's built-in sphere handle
            Handles.color = new Color(
                sphereColor.r,
                sphereColor.g,
                sphereColor.b,
                sphereColor.a * 0.5f
            );
            Handles.SphereHandleCap(
                0,
                position,
                Quaternion.identity,
                radius * 2f,
                EventType.Repaint
            );

            // Add wireframe for better visibility and depth perception
            DrawWireSphere(position, radius, wireColor, segments);
        }
        #endregion
#endif
        // == [ END IF UNITY EDITOR ] ================================================================================================================
    }
}
