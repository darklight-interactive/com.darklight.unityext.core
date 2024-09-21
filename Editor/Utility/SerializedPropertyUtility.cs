using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Editor.Utility
{
    public static class SerializedPropertyUtility
    {
        #region ======== [[ ConvertPropertyToString ]] ========
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
        #endregion

        #region ======== [[ GetPropertyType ]] ========

        /// <summary>
        /// Determines if the given SerializedProperty is of a simple type (e.g., int, float, string).
        /// </summary>
        /// <param name="property">The SerializedProperty to check.</param>
        /// <returns>True if the property is a simple type; otherwise, false.</returns>
        public static bool IsSimpleType(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.String:
                case SerializedPropertyType.Color:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.BoundsInt:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.FixedBufferSize:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines if the given SerializedProperty is of a numeric type (e.g., int, float).
        /// </summary>
        /// <param name="property">The SerializedProperty to check.</param>
        /// <returns>True if the property is a numeric type; otherwise, false.</returns>
        public static bool IsNumericType(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Character:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines if the given SerializedProperty is a reference type (e.g., object reference, managed reference).
        /// </summary>
        /// <param name="property">The SerializedProperty to check.</param>
        /// <returns>True if the property is a reference type; otherwise, false.</returns>
        public static bool IsReferenceType(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.ManagedReference:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines if the given SerializedProperty is a collection type (e.g., array, list).
        /// </summary>
        /// <param name="property">The SerializedProperty to check.</param>
        /// <returns>True if the property is a collection type; otherwise, false.</returns>
        public static bool IsCollectionType(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.FixedBufferSize:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines if the given SerializedProperty is a custom class or struct.
        /// </summary>
        /// <param name="property">The SerializedProperty to check.</param>
        /// <returns>True if the property is a custom class or struct; otherwise, false.</returns>
        public static bool IsCustomType(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Generic;
        }
        #endregion

        #region ======== [[ CloneSerializedProperty ]] ========
        /// <summary>
        /// Clones the values from one SerializedProperty to another.
        /// </summary>
        /// <param name="source">The source SerializedProperty to clone from.</param>
        /// <param name="destination">The destination SerializedProperty to clone to.</param>
        /// <exception cref="ArgumentException">Thrown when the property types do not match.</exception>
        public static void CloneSerializedProperty(SerializedProperty source, SerializedProperty destination)
        {
            if (source.propertyType != destination.propertyType)
            {
                throw new ArgumentException("Source and destination SerializedProperties must be of the same type.");
            }

            switch (source.propertyType)
            {
                case SerializedPropertyType.Integer:
                    destination.intValue = source.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    destination.boolValue = source.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    destination.floatValue = source.floatValue;
                    break;
                case SerializedPropertyType.String:
                    destination.stringValue = source.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    destination.colorValue = source.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    destination.objectReferenceValue = source.objectReferenceValue;
                    break;
                case SerializedPropertyType.Enum:
                    destination.enumValueIndex = source.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    destination.vector2Value = source.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    destination.vector3Value = source.vector3Value;
                    break;
                case SerializedPropertyType.Rect:
                    destination.rectValue = source.rectValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    destination.arraySize = source.arraySize;
                    break;
                case SerializedPropertyType.Character:
                    destination.intValue = source.intValue;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    destination.animationCurveValue = source.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    destination.boundsValue = source.boundsValue;
                    break;
                case SerializedPropertyType.Gradient:
                    // Gradients require special handling
                    destination.gradientValue = source.gradientValue;
                    break;
                case SerializedPropertyType.Quaternion:
                    destination.quaternionValue = source.quaternionValue;
                    break;
                case SerializedPropertyType.ExposedReference:
                    destination.exposedReferenceValue = source.exposedReferenceValue;
                    break;
                case SerializedPropertyType.ManagedReference:
                    destination.managedReferenceValue = source.managedReferenceValue;
                    break;
                default:
                    Debug.LogWarning($"Unsupported property type: {source.propertyType}");
                    break;
            }
        }

        #endregion

        #region ======== [[ ResetSerializedProperty ]] ========
        /// <summary>
        /// Resets a SerializedProperty to its default value.
        /// </summary>
        /// <param name="property">The SerializedProperty to reset.</param>
        public static void ResetSerializedProperty(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = 0;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = false;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = 0f;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = string.Empty;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = Color.black;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = null;
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = 0;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = Vector2.zero;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = Vector3.zero;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = Rect.zero;
                    break;
                case SerializedPropertyType.ArraySize:
                    property.arraySize = 0;
                    break;
                case SerializedPropertyType.Character:
                    property.intValue = '\0';
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = new AnimationCurve();
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = new Bounds();
                    break;
                case SerializedPropertyType.Gradient:
                    // Gradients require special handling
                    property.gradientValue = new Gradient();
                    break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = Quaternion.identity;
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    // FixedBufferSize is read-only and can't be modified directly
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = null;
                    break;
                case SerializedPropertyType.ManagedReference:
                    property.managedReferenceValue = null;
                    break;
                default:
                    Debug.LogWarning($"Unsupported property type: {property.propertyType}");
                    break;
            }
        }

        #endregion

        #region ======== [[ FindPropertyByPath ]] ========
        /// <summary>
        /// Finds a SerializedProperty by its path within a SerializedObject.
        /// </summary>
        /// <param name="serializedObject">The SerializedObject to search within.</param>
        /// <param name="propertyPath">The path to the property (e.g., "fieldName.propertyName").</param>
        /// <returns>The SerializedProperty if found; otherwise, null.</returns>
        public static SerializedProperty FindPropertyByPath(SerializedObject serializedObject, string propertyPath)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property == null)
            {
                Debug.LogWarning($"Property '{propertyPath}' not found in object '{serializedObject.targetObject.name}'.");
            }
            return property;
        }
        #endregion

        #region ======== [[ BatchApplyModifications ]] ========
        /// <summary>
        /// Applies modifications to multiple SerializedProperties at once.
        /// </summary>
        /// <param name="properties">An array of SerializedProperties to apply changes to.</param>
        public static void BatchApplyModifications(params SerializedProperty[] properties)
        {
            foreach (var property in properties)
            {
                if (property != null)
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    Debug.LogWarning("One or more properties are null and cannot be applied.");
                }
            }
        }
        #endregion

        #region ======== [[ GeneratePropertySummary ]] ========
        /// <summary>
        /// Generates a human-readable summary of all SerializedProperties within a SerializedObject.
        /// </summary>
        /// <param name="serializedObject">The SerializedObject to summarize.</param>
        /// <returns>A string summary of the SerializedObject's properties.</returns>
        public static string GeneratePropertySummary(SerializedObject serializedObject)
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true); // Move to the first visible property

            System.Text.StringBuilder summary = new System.Text.StringBuilder();
            summary.AppendLine($"Summary of {serializedObject.targetObject.name}:");

            while (iterator.NextVisible(false))
            {
                string propertyValue = ConvertPropertyToString(iterator);
                summary.AppendLine($"{iterator.displayName}: {propertyValue}");
            }

            return summary.ToString();
        }
        #endregion

    }
}