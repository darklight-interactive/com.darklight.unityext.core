
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
public static class Shape2DGizmos
{
    public static void DrawShape2D(Shape2D shape, Color color, bool filled = false)
    {
        if (shape == null) return;
        if (filled)
        {
            DrawFilledShape2D(shape, color);
            return;
        }

        List<Vector3> vertices = shape.vertices.ToList();
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 start = vertices[i];
            Vector3 end = vertices[(i + 1) % vertices.Count];
            Handles.color = color;
            Handles.DrawLine(start, end);
        }
    }

    public static void DrawRadialShape(Vector3 center, float radius, int segments, Vector3 normal, Color color)
    {
        Shape2D shape = new Shape2D(center, radius, segments, normal, color);
        DrawShape2D(shape, color);
    }

    public static void DrawCircle(Vector3 center, float radius, Color color)
    {
        DrawRadialShape(center, radius, 32, Vector3.up, color);
    }

    public static void DrawFilledShape2D(Shape2D shape, Color color)
    {
        if (shape == null || shape.vertices.Length < 3) return; // At least 3 vertices are needed to form a polygon

        Handles.color = color;
        Vector3[] verticesArray = shape.vertices.ToArray();

        // Handles.DrawAAConvexPolygon can be used for convex shapes
        Handles.DrawAAConvexPolygon(verticesArray);
    }

    public static void DrawFilledCircle(Vector3 center, float radius, Color color)
    {
        // Generate vertices for a circle
        int segments = 32;
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            vertices.Add(center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
        }

        // Draw the filled circle using DrawAAConvexPolygon
        Handles.color = color;
        Handles.DrawAAConvexPolygon(vertices.ToArray());
    }
}
#endif