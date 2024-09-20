using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Editor.Utility
{
    public static class SerializedPropertyUtility
    {
        private static readonly Dictionary<SerializedPropertyType, Func<SerializedProperty, string>> PropertyTypeToStringMap = new Dictionary<SerializedPropertyType, Func<SerializedProperty, string>>
        {
        { SerializedPropertyType.Integer, prop => prop.intValue.ToString() },
        { SerializedPropertyType.Boolean, prop => prop.boolValue.ToString() },
        { SerializedPropertyType.Float, prop => prop.floatValue.ToString("0.00000") },
        { SerializedPropertyType.String, prop => prop.stringValue },
        { SerializedPropertyType.Enum, prop => prop.enumDisplayNames[prop.enumValueIndex] },
        { SerializedPropertyType.Vector2, prop => prop.vector2Value.ToString("F5") },
        { SerializedPropertyType.Vector3, prop => prop.vector3Value.ToString("F5") },
        { SerializedPropertyType.Vector2Int, prop => prop.vector2IntValue.ToString() },
        { SerializedPropertyType.Vector3Int, prop => prop.vector3IntValue.ToString() },
        { SerializedPropertyType.Quaternion, prop => prop.quaternionValue.eulerAngles.ToString("F5") },
        { SerializedPropertyType.Color, prop => prop.colorValue.ToString() },
        { SerializedPropertyType.Bounds, prop => prop.boundsValue.ToString() },
        { SerializedPropertyType.Rect, prop => prop.rectValue.ToString() },
        { SerializedPropertyType.ObjectReference, prop => prop.objectReferenceValue == null ? "None" : prop.objectReferenceValue.name },
        { SerializedPropertyType.AnimationCurve, prop => "AnimationCurve" },
        { SerializedPropertyType.LayerMask, prop => LayerMask.LayerToName(prop.intValue) },
        { SerializedPropertyType.Gradient, prop => "Gradient" }, // You can expand this to a proper string representation if needed
        { SerializedPropertyType.ExposedReference, prop => prop.exposedReferenceValue?.name ?? "None" },
        { SerializedPropertyType.ManagedReference, prop => prop.managedReferenceFullTypename }
        };

        /// <summary>
        /// Converts a SerializedProperty element to a string representation based on its type.
        /// </summary>
        /// <param name="prop">The SerializedProperty element.</param>
        /// <returns>A string representation of the element.</returns>
        public static string ConvertPropertyToString(SerializedProperty prop)
        {
            if (PropertyTypeToStringMap.TryGetValue(prop.propertyType, out var converter))
            {
                return converter(prop);
            }
            return "[Unsupported Type]";
        }
    }
}