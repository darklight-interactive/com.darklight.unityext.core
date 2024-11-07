using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Darklight.UnityExt.Collection.Editor
{
    public class LibraryReorderableList : ReorderableList
    {
        private const float ID_COLUMN_WIDTH = 50f;
        private const float PADDING = 5f;
        private readonly Type _collectionType;
        private readonly Type _itemType;
        private readonly CollectionLibrary _collection;

        public LibraryReorderableList(
            SerializedObject serializedObject,
            FieldInfo fieldInfo,
            SerializedProperty itemsProperty
        ) : base(serializedObject, itemsProperty, true, true, true, true)
        {
            // Get the actual collection instance
            _collection = fieldInfo.GetValue(serializedObject.targetObject) as CollectionLibrary;
            
            if (_collection != null)
            {
                Debug.Log($"Collection Instance Type: {_collection.GetType().FullName}");
                
                // Try to find the CollectionLibrary<T> type in the hierarchy
                var collectionType = _collection.GetType();
                while (collectionType != null && (!collectionType.IsGenericType || collectionType.GetGenericTypeDefinition() != typeof(CollectionLibrary<>)))
                {
                    Debug.Log($"Checking type: {collectionType.FullName}");
                    collectionType = collectionType.BaseType;
                }

                if (collectionType != null)
                {
                    Debug.Log($"Found generic collection type: {collectionType.FullName}");
                    var genericArgs = collectionType.GetGenericArguments();
                    Debug.Log($"Generic arguments count: {genericArgs.Length}");
                    
                    foreach (var arg in genericArgs)
                    {
                        Debug.Log($"Generic argument: {arg.FullName}");
                    }

                    if (genericArgs.Length > 0)
                    {
                        _collectionType = genericArgs[0];
                        _itemType = typeof(CollectionItem<>).MakeGenericType(_collectionType);
                        
                        Debug.Log($"Collection Value Type: {_collectionType.FullName}");
                        Debug.Log($"Collection Item Type: {_itemType.FullName}");

                        // Try to get the items list
                        var itemsList = _collection.Items.ToList();
                        Debug.Log($"Items count: {itemsList.Count}");
                        
                        if (itemsList.Count > 0)
                        {
                            var firstItem = itemsList[0];
                            Debug.Log($"First item actual type: {firstItem.GetType().FullName}");
                            Debug.Log($"First item value type: {firstItem.Value?.GetType().FullName}");
                            Debug.Log($"First item value: {firstItem.Value}");
                        }
                    }
                }
                else
                {
                    Debug.LogError("Could not find CollectionLibrary<T> in type hierarchy!");
                }
            }
            else
            {
                Debug.LogError("Could not get collection instance from field!");
                Debug.Log($"Field type: {fieldInfo.FieldType.FullName}");
                Debug.Log($"Field declaring type: {fieldInfo.DeclaringType?.FullName}");
            }

            drawHeaderCallback = (Rect rect) =>
            {
                var idRect = new Rect(rect.x, rect.y, ID_COLUMN_WIDTH, rect.height);
                var valueRect = new Rect(rect.x + ID_COLUMN_WIDTH + PADDING, rect.y, rect.width - ID_COLUMN_WIDTH - PADDING, rect.height);

                EditorGUI.LabelField(idRect, "ID");
                EditorGUI.LabelField(valueRect, $"Value <{_collectionType?.Name ?? "unknown"}>");
            };

            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = itemsProperty.GetArrayElementAtIndex(index);
                var idProp = element.FindPropertyRelative("_id");
                var typedValueProp = element.FindPropertyRelative("_typedValue");

                // Debug element information
                Debug.Log($"Drawing element {index}:");
                Debug.Log($"Element type: {element.type}");
                Debug.Log($"Element path: {element.propertyPath}");
                
                if (typedValueProp != null)
                {
                    Debug.Log($"TypedValue property type: {typedValueProp.propertyType}");
                    Debug.Log($"TypedValue property path: {typedValueProp.propertyPath}");
                }

                var idRect = new Rect(rect.x, rect.y + 2, ID_COLUMN_WIDTH, EditorGUIUtility.singleLineHeight);
                var valueRect = new Rect(rect.x + ID_COLUMN_WIDTH + PADDING, rect.y + 2, rect.width - ID_COLUMN_WIDTH - PADDING, EditorGUIUtility.singleLineHeight);

                EditorGUI.LabelField(idRect, idProp?.intValue.ToString() ?? "?");

                if (typedValueProp != null)
                {
                    var typeLabel = $"({_collectionType?.Name ?? "unknown"}) ";
                    var labelWidth = GUI.skin.label.CalcSize(new GUIContent(typeLabel)).x;
                    
                    var typeLabelRect = new Rect(valueRect.x, valueRect.y, labelWidth, valueRect.height);
                    var propertyRect = new Rect(valueRect.x + labelWidth, valueRect.y, valueRect.width - labelWidth, valueRect.height);
                    
                    EditorGUI.LabelField(typeLabelRect, typeLabel);
                    EditorGUI.PropertyField(propertyRect, typedValueProp, GUIContent.none);
                }
                else
                {
                    EditorGUI.LabelField(valueRect, $"(null) Expected: {_collectionType?.Name ?? "unknown"}");
                }
            };

            elementHeightCallback = (int index) =>
            {
                return EditorGUIUtility.singleLineHeight + 4;
            };
        }
    }

    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetTypeHierarchy(this Type type)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                yield return current;
            }
        }
    }
}
