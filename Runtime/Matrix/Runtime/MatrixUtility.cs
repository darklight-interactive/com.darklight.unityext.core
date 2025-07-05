using UnityEngine;

namespace Darklight.Matrix
{
    public partial class Matrix
    {
        public static class Utility
        {
            public static Vector2 ClampVector2(Vector2 value, float min, float max)
            {
                return new Vector2(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max));
            }

            /// <summary>
            /// Calculates the alignment offset based on the alignment and size.
            /// </summary>
            /// <param name="alignment">The alignment to calculate the offset for.</param>
            /// <param name="size">The size of the matrix.</param>
            /// <returns>The alignment offset in local space Vector2.</returns>
            public static Vector2 CalculateAlignmentOffset(Alignment alignment, Vector2 size)
            {
                Vector2 alignmentOffset = AlignmentOffsets.TryGetValue(
                    alignment,
                    out Vector2 offset
                )
                    ? offset
                    : Vector2.zero;

                alignmentOffset *= size;
                return alignmentOffset;
            }

            /// <summary>
            /// Converts a 2D alignment value to 3D based on the cell swizzle.
            /// </summary>
            public static Vector3 SwizzleVec2(Vector2 value, GridLayout.CellSwizzle swizzle)
            {
                switch (swizzle)
                {
                    case GridLayout.CellSwizzle.XYZ:
                        return new Vector3(value.x, value.y, 0);

                    case GridLayout.CellSwizzle.XZY:
                        return new Vector3(value.x, 0, value.y);
                    case GridLayout.CellSwizzle.YXZ:
                        return new Vector3(value.y, value.x, 0);
                    case GridLayout.CellSwizzle.YZX:
                        return new Vector3(0, value.x, value.y);
                    case GridLayout.CellSwizzle.ZXY:
                        return new Vector3(value.y, 0, value.x);
                    case GridLayout.CellSwizzle.ZYX:
                        return new Vector3(0, value.y, value.x);
                    default:
                        return new Vector3(value.x, 0, value.y); // Default to XZY
                }
            }

            public static Vector3Int SwizzleVec2Int(
                Vector2Int value,
                GridLayout.CellSwizzle swizzle
            )
            {
                Vector3 vec3 = SwizzleVec2(value, swizzle);
                return new Vector3Int(
                    Mathf.RoundToInt(vec3.x),
                    Mathf.RoundToInt(vec3.y),
                    Mathf.RoundToInt(vec3.z)
                );
            }
        }
    }
}
