using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static Darklight.Core2D.Spatial2D;

namespace Darklight.Core2D
{
    /// <summary>
    /// Interface for 2D Spatial Objects.
    /// </summary>
    public interface ISpatial2D
    {
        Vector3 Position { get; }
        Vector2 Dimensions { get; }
        Vector3 Normal { get; }
    }

    /// <summary>
    /// Custom Static Utility Class for 2D Spatial Calculations.
    /// </summary>
    public static class Spatial2D
    {
        public enum Direction
        {
            NONE,

            [InspectorName("North (Positive Y)")]
            NORTH,

            [InspectorName("East (Positive X)")]
            EAST,

            [InspectorName("South (Negative Y)")]
            SOUTH,

            [InspectorName("West (Negative X)")]
            WEST,

            [InspectorName("Up-Right")]
            NORTHEAST,

            [InspectorName("Down-Right")]
            SOUTHEAST,

            [InspectorName("Down-Left")]
            SOUTHWEST,

            [InspectorName("Up-Left")]
            NORTHWEST
        }

        static readonly Dictionary<Direction, Vector2> directionVectors = new Dictionary<
            Direction,
            Vector2
        >
        {
            { Direction.NORTH, Vector2.up },
            { Direction.EAST, Vector2.right },
            { Direction.SOUTH, Vector2.down },
            { Direction.WEST, Vector2.left },
            { Direction.NORTHEAST, Vector2.up + Vector2.right },
            { Direction.SOUTHEAST, Vector2.down + Vector2.right },
            { Direction.SOUTHWEST, Vector2.down + Vector2.left },
            { Direction.NORTHWEST, Vector2.up + Vector2.left }
        };

        public static void GetDirectionVector(Direction direction, out Vector2 vector)
        {
            vector = directionVectors[direction];
        }

        public static void ConvertVectorToDirection(
            Vector2 input,
            out Spatial2D.Direction direction
        )
        {
            if (input.sqrMagnitude < float.Epsilon)
            {
                direction = Spatial2D.Direction.NONE;
                return;
            }

            Vector2 directionVector = input.normalized;
            float angle = Vector2.SignedAngle(Vector2.up, directionVector);

            // Normalize angle to 0-360 range
            if (angle < 0)
                angle += 360f;

            direction = angle switch
            {
                >= 337.5f or < 22.5f => Spatial2D.Direction.NORTH,
                >= 22.5f and < 67.5f => Spatial2D.Direction.NORTHEAST,
                >= 67.5f and < 112.5f => Spatial2D.Direction.EAST,
                >= 112.5f and < 157.5f => Spatial2D.Direction.SOUTHEAST,
                >= 157.5f and < 202.5f => Spatial2D.Direction.SOUTH,
                >= 202.5f and < 247.5f => Spatial2D.Direction.SOUTHWEST,
                >= 247.5f and < 292.5f => Spatial2D.Direction.WEST,
                >= 292.5f and < 337.5f => Spatial2D.Direction.NORTHWEST,
                _ => Spatial2D.Direction.NONE
            };
        }

        public enum AnchorPoint
        {
            TOP_LEFT,
            TOP_CENTER,
            TOP_RIGHT,
            CENTER_LEFT,
            CENTER,
            CENTER_RIGHT,
            BOTTOM_LEFT,
            BOTTOM_CENTER,
            BOTTOM_RIGHT
        }

        public static readonly Dictionary<AnchorPoint, Vector2> anchorPointOffsets = new Dictionary<
            AnchorPoint,
            Vector2
        >
        {
            { AnchorPoint.TOP_LEFT, new Vector2(-0.5f, 0.5f) },
            { AnchorPoint.TOP_CENTER, new Vector2(0, 0.5f) },
            { AnchorPoint.TOP_RIGHT, new Vector2(0.5f, 0.5f) },
            { AnchorPoint.CENTER_LEFT, new Vector2(-0.5f, 0) },
            { AnchorPoint.CENTER, new Vector2(0, 0) },
            { AnchorPoint.CENTER_RIGHT, new Vector2(0.5f, 0) },
            { AnchorPoint.BOTTOM_LEFT, new Vector2(-0.5f, -0.5f) },
            { AnchorPoint.BOTTOM_CENTER, new Vector2(0, -0.5f) },
            { AnchorPoint.BOTTOM_RIGHT, new Vector2(0.5f, -0.5f) }
        };

        /// <summary>
        /// Calculates the position offset of the anchor point.
        /// </summary>
        /// <param name="dimensions"></param>
        /// <param name="anchorTag"></param>
        /// <returns></returns>
        public static Vector3 CalculateAnchorPointOffset(Vector2 dimensions, AnchorPoint anchorTag)
        {
            return dimensions * anchorPointOffsets[anchorTag];
        }

        /// <summary>
        /// Calculates the worlf position of the anchor point based on a given center.
        /// </summary>
        /// <param name="center">
        ///     The center position of the object.
        /// </param>
        /// <param name="dimensions">
        ///     The dimensions of the object.
        /// </param>
        /// <param name="anchorTag"></param>
        /// <returns></returns>
        static Vector3 CalculateAnchorPointPosition(
            Vector3 center,
            Vector2 dimensions,
            AnchorPoint anchorTag
        )
        {
            return center + CalculateAnchorPointOffset(dimensions, anchorTag);
        }

        // ======== [[ TRANSFORM UTILITIES ]] ================================== >>>>
        // ---- (( HANDLERS )) ---- >>


        // ---- (( GETTERS )) ---- >>
        public static Vector3 GetAnchorPointOffset(Vector2 dimensions, AnchorPoint anchorTag)
        {
            return CalculateAnchorPointOffset(dimensions, anchorTag);
        }

        public static Vector3 GetAnchorPointPosition(
            Vector3 center,
            Vector2 dimensions,
            AnchorPoint anchorTag
        )
        {
            return CalculateAnchorPointPosition(center, dimensions, anchorTag);
        }

        // ---- (( SETTERS )) ---- >>
        public static void SetTransformToDefaultValues(Transform transform)
        {
            transform.position = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
        }

        public static void SetTransformValues(
            Transform transform,
            Vector3 position,
            Vector2 dimensions,
            Vector3 normal
        )
        {
            transform.position = position;
            SetTransformScale_ToDimensions(transform, dimensions);
            SetTransformRotation_ToNormal(transform, normal);
        }

        public static void SetTransformPos_ToAnchor(
            Transform transform,
            Vector3 position,
            Vector2 dimensions,
            AnchorPoint anchorTag
        )
        {
            Vector3 positionOffset = CalculateAnchorPointOffset(dimensions, anchorTag);
            transform.position = position - positionOffset;
        }

        public static void SetTransformScale_ToDimensions(Transform transform, Vector2 dimensions)
        {
            transform.localScale = new Vector3(dimensions.x, dimensions.y, 1);
        }

        public static void SetTransformScale_ToSquareRatio(Transform transform, float size)
        {
            transform.localScale = new Vector3(size, size, 1);
        }

        public static void SetTransformRotation_ToNormal(Transform transform, Vector3 normal)
        {
            transform.localRotation = Quaternion.LookRotation(normal, Vector3.up);
        }
    }
}
