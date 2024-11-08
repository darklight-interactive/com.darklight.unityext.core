using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Darklight.UnityExt.Collection.Editor
{
    public class CollectionReorderableList : ReorderableList
    {
        private const float HEADER_PADDING = 12f;
        private const float ID_COLUMN_WIDTH = 50f;
        private const float KEY_COLUMN_WIDTH = 150f;
        private const float PADDING = 5f;

        private const string ID_PROP = "_id";
        private const string KEY_PROP = "_key";
        private const string VALUE_PROP = "_value";
        private readonly SerializedProperty _itemsProperty;

        private readonly Type _collectionType;
        private readonly Type _keyType;
        private readonly Type _valueType;
        private readonly bool _collectionIsDictionary;

        private Collection _collection;
        private Dictionary<int, bool> _foldoutStates = new Dictionary<int, bool>();
        private int _selectedIndex;

        public CollectionReorderableList(
            SerializedObject serializedObject,
            FieldInfo fieldInfo,
            SerializedProperty itemsProperty
        )
            : base(serializedObject, itemsProperty, true, true, true, true)
        {
            _collection = fieldInfo.GetValue(serializedObject.targetObject) as Collection;
            _itemsProperty = itemsProperty;

            if (_collection == null)
            {
                Debug.LogError(
                    $"CollectionLibrary field not found on {serializedObject.targetObject.name}"
                );
                return;
            }

            GetCollectionTypes(_collection, out _collectionType, out _keyType, out _valueType);
            _collectionIsDictionary = _keyType != null;

            SetupDrawCallbacks();
        }

        private void SetupDrawCallbacks()
        {
            drawHeaderCallback = rect =>
            {
                if (_collectionIsDictionary)
                    DrawDictionaryHeader(rect);
                else
                    DrawLibraryHeader(rect);
            };

            drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (_collectionIsDictionary)
                    DrawDictionaryElement(rect, index, isActive, isFocused);
                else
                    DrawLibraryElement(rect, index, isActive, isFocused);
            };

            onAddDropdownCallback += OnDropdownCallback;
            

            elementHeightCallback = GetElementHeight;
        }

        void OnDropdownCallback(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();

            if (_collectionIsDictionary)
            {
                menu.AddItem(new GUIContent("Add Dictionary Entry"), false, () => 
                {
                    var index = _itemsProperty.arraySize;
                    _itemsProperty.InsertArrayElementAtIndex(index);
                    var element = _itemsProperty.GetArrayElementAtIndex(index);
                    
                    // Set default values
                    var idProp = element.FindPropertyRelative(ID_PROP);
                    if (idProp != null) idProp.intValue = GetNextId();
                    
                    _itemsProperty.serializedObject.ApplyModifiedProperties();
                });
            }
            else
            {
                menu.AddItem(new GUIContent("Add Empty Item"), false, () => 
                {
                    var index = _itemsProperty.arraySize;
                    _itemsProperty.InsertArrayElementAtIndex(index);
                    var element = _itemsProperty.GetArrayElementAtIndex(index);
                    
                    
                    
                    _itemsProperty.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.AddSeparator("");
            
            menu.AddItem(new GUIContent("Sort by ID"), false, () => 
            {
                SortByID();
            });

            menu.AddItem(new GUIContent("Clear All"), false, () => 
            {
                if (EditorUtility.DisplayDialog("Clear All Items", 
                    "Are you sure you want to remove all items?", "Yes", "No"))
                {
                    _itemsProperty.ClearArray();
                    _itemsProperty.serializedObject.ApplyModifiedProperties();
                }
            });

            menu.DropDown(buttonRect);
        }

        private int GetNextId()
        {
            int maxId = -1;
            for (int i = 0; i < _itemsProperty.arraySize; i++)
            {
                var element = _itemsProperty.GetArrayElementAtIndex(i);
                var idProp = element.FindPropertyRelative(ID_PROP);
                if (idProp != null && idProp.intValue > maxId)
                {
                    maxId = idProp.intValue;
                }
            }
            return maxId + 1;
        }

        private void SortByID()
        {
            bool changed;
            do
            {
                changed = false;
                for (int i = 0; i < _itemsProperty.arraySize - 1; i++)
                {
                    var element1 = _itemsProperty.GetArrayElementAtIndex(i);
                    var element2 = _itemsProperty.GetArrayElementAtIndex(i + 1);
                    
                    var id1 = element1.FindPropertyRelative(ID_PROP);
                    var id2 = element2.FindPropertyRelative(ID_PROP);
                    
                    if (id1 != null && id2 != null && id1.intValue > id2.intValue)
                    {
                        _itemsProperty.MoveArrayElement(i + 1, i);
                        changed = true;
                    }
                }
            } while (changed);

            _itemsProperty.serializedObject.ApplyModifiedProperties();
        }

        #region [[ Get Types ]] ================================================================

        private void GetCollectionTypes(
            Collection collection,
            out Type collectionType,
            out Type keyType,
            out Type valueType
        )
        {
            collectionType = collection.GetType();
            keyType = null;
            valueType = null;

            if (collectionType.IsGenericType)
            {
                var genericArgs = collectionType.GetGenericArguments();
                if (genericArgs.Length == 2)
                {
                    keyType = genericArgs[0];
                    valueType = genericArgs[1];
                }
                else if (genericArgs.Length == 1)
                {
                    valueType = genericArgs[0];
                }
            }
        }

        private bool IsListType(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Generic
                && property.type.StartsWith("List`1");
        }

        private bool IsDictionaryType(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Generic
                && property.type.StartsWith("Dictionary`2");
        }

        private string GetListElementType(SerializedProperty property)
        {
            // Try to extract the type from the property path or type name
            string type = property.type;
            int startIndex = type.IndexOf('<') + 1;
            int endIndex = type.IndexOf('>');
            if (startIndex > 0 && endIndex > startIndex)
            {
                return type.Substring(startIndex, endIndex - startIndex);
            }
            return "unknown";
        }

        private void DrawSerializableObjectValue(Rect rect, SerializedProperty property, int index)
        {
            var obj = property.objectReferenceValue;
            bool hasSerializedProperties = false;

            // Check if object has serialized properties
            if (obj != null)
            {
                var serializedObject = new SerializedObject(obj);
                var iterator = serializedObject.GetIterator();
                hasSerializedProperties = iterator.NextVisible(true);

                // Make sure first property is the script before skipping
                if (hasSerializedProperties && iterator.name != "m_Script")
                {
                    hasSerializedProperties = true;
                }
                else
                {
                    hasSerializedProperties = iterator.NextVisible(false);
                }
            }

            // Draw foldout only if there are properties to show
            if (hasSerializedProperties)
            {
                var foldoutRect = new Rect(
                    rect.x + 20,
                    rect.y,
                    20,
                    EditorGUIUtility.singleLineHeight
                );
                _foldoutStates[index] = EditorGUI.Foldout(
                    foldoutRect,
                    _foldoutStates.GetValueOrDefault(index),
                    "",
                    true
                );

                // Draw object field with proper Unity styling
                var objectFieldRect = new Rect(
                    rect.x + 25,
                    rect.y,
                    rect.width - 25,
                    EditorGUIUtility.singleLineHeight
                );
                EditorGUI.PropertyField(objectFieldRect, property, GUIContent.none);

                // Draw expanded properties
                if (_foldoutStates.GetValueOrDefault(index))
                {
                    var indentedRect = new Rect(
                        rect.x + 15,
                        rect.y + EditorGUIUtility.singleLineHeight + 2,
                        rect.width - 15,
                        EditorGUIUtility.singleLineHeight
                    );

                    SerializedObject serializedValue = new SerializedObject(obj);
                    serializedValue.Update();

                    var childProperty = serializedValue.GetIterator();
                    bool enterChildren = true;

                    // Get first property
                    if (childProperty.NextVisible(enterChildren))
                    {
                        // Skip if it's the script property
                        if (childProperty.name == "m_Script")
                        {
                            childProperty.NextVisible(false);
                        }

                        // Draw all remaining properties
                        do
                        {
                            float propertyHeight = EditorGUI.GetPropertyHeight(childProperty, true);
                            EditorGUI.PropertyField(indentedRect, childProperty, true);
                            indentedRect.y += propertyHeight + 2;
                            enterChildren = false;
                        } while (childProperty.NextVisible(false));
                    }

                    serializedValue.ApplyModifiedProperties();
                }
            }
            else
            {
                var objectFieldRect = new Rect(
                    rect.x + 20,
                    rect.y,
                    rect.width - 20,
                    EditorGUIUtility.singleLineHeight
                );
                EditorGUI.PropertyField(objectFieldRect, property, GUIContent.none);
            }
        }

        private void DrawCollectionValue(Rect rect, SerializedProperty property)
        {
            Rect collectionRect = rect;
            collectionRect.x += 20;
            EditorGUI.PropertyField(collectionRect, property, GUIContent.none, true);
        }

        private void DrawRawObjectValue(Rect rect, object value, int index)
        {
            if (value == null)
            {
                EditorGUI.LabelField(rect, "null");
                return;
            }

            var type = value.GetType();
            var foldoutRect = new Rect(
                rect.x,
                rect.y,
                rect.width,
                EditorGUIUtility.singleLineHeight
            );

            // Show type and value preview
            string preview = GetObjectPreview(value);
            _foldoutStates[index] = EditorGUI.Foldout(
                foldoutRect,
                _foldoutStates.GetValueOrDefault(index),
                $"{type.Name}: {preview}",
                true
            );

            if (_foldoutStates.GetValueOrDefault(index))
            {
                var indentedRect = new Rect(
                    rect.x + 20,
                    rect.y + EditorGUIUtility.singleLineHeight + 2,
                    rect.width - 20,
                    EditorGUIUtility.singleLineHeight
                );

                // Draw all public properties and fields
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    var fieldValue = field.GetValue(value);
                    EditorGUI.LabelField(
                        indentedRect,
                        $"{field.Name}: {GetObjectPreview(fieldValue)}"
                    );
                    indentedRect.y += EditorGUIUtility.singleLineHeight + 2;
                }

                foreach (
                    var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                )
                {
                    if (prop.CanRead && prop.GetIndexParameters().Length == 0)
                    {
                        try
                        {
                            var propValue = prop.GetValue(value);
                            EditorGUI.LabelField(
                                indentedRect,
                                $"{prop.Name}: {GetObjectPreview(propValue)}"
                            );
                            indentedRect.y += EditorGUIUtility.singleLineHeight + 2;
                        }
                        catch { } // Skip properties that throw exceptions
                    }
                }
            }
        }

        private void DrawDictionaryObjectValue(Rect rect, SerializedProperty property, int index)
        {
            var foldoutRect = new Rect(
                rect.x,
                rect.y,
                rect.width,
                EditorGUIUtility.singleLineHeight
            );
            _foldoutStates[index] = EditorGUI.Foldout(
                foldoutRect,
                _foldoutStates.GetValueOrDefault(index),
                "Dictionary",
                true
            );

            if (_foldoutStates.GetValueOrDefault(index))
            {
                var indentedRect = new Rect(
                    rect.x + 20,
                    rect.y + EditorGUIUtility.singleLineHeight + 2,
                    rect.width - 20,
                    EditorGUIUtility.singleLineHeight
                );

                // Use non-generic dictionary to iterate
                var dict = property.managedReferenceValue as System.Collections.IDictionary;
                if (dict != null)
                {
                    foreach (System.Collections.DictionaryEntry entry in dict)
                    {
                        EditorGUI.LabelField(
                            indentedRect,
                            $"{entry.Key}: {GetObjectPreview(entry.Value)}"
                        );
                        indentedRect.y += EditorGUIUtility.singleLineHeight + 2;
                    }
                }
            }
        }

        private string GetObjectPreview(object value)
        {
            if (value == null)
                return "null";

            // Handle common types
            if (value is string str)
                return $"\"{str}\"";
            if (value is bool b)
                return b.ToString().ToLower();
            if (value.GetType().IsPrimitive)
                return value.ToString();
            if (value is UnityEngine.Object unityObj)
                return unityObj.name;

            // Handle collections
            if (value is System.Collections.ICollection collection)
            {
                return $"Count = {collection.Count}";
            }

            // For other types, show type name and ToString()
            string toString = value.ToString();
            if (toString != value.GetType().ToString())
            {
                return toString;
            }
            return value.GetType().Name;
        }

        #endregion

        #region [[ Get Element Heights ]] ==========================================================

        private float GetElementHeight(int index)
        {
            var element = _itemsProperty.GetArrayElementAtIndex(index);
            var typedValueProp = element.FindPropertyRelative(VALUE_PROP);
            float baseHeight = EditorGUIUtility.singleLineHeight + 4;

            if (typedValueProp == null)
                return baseHeight;

            // Get property path hash for foldout state
            int stateKey = element.propertyPath.GetHashCode();
            bool isFoldedOut = _foldoutStates.GetValueOrDefault(stateKey);

            // Object Reference (ScriptableObject, MonoBehaviour, etc)
            if (
                typedValueProp.propertyType == SerializedPropertyType.ObjectReference
                && typedValueProp.objectReferenceValue != null
            )
            {
                var obj = typedValueProp.objectReferenceValue;
                var serializedObject = new SerializedObject(obj);
                var iterator = serializedObject.GetIterator();

                if (iterator.NextVisible(true))
                {
                    bool hasProperties = iterator.name != "m_Script" || iterator.NextVisible(false);

                    if (hasProperties && isFoldedOut)
                    {
                        float propertyHeight = 0;

                        // Reset iterator
                        iterator = serializedObject.GetIterator();
                        iterator.NextVisible(true);

                        // Skip script property if it's first
                        if (iterator.name == "m_Script")
                        {
                            iterator.NextVisible(false);
                        }

                        // Calculate height for all remaining properties
                        do
                        {
                            propertyHeight += EditorGUI.GetPropertyHeight(iterator, true) + 2;
                        } while (iterator.NextVisible(false));

                        if (propertyHeight > 0)
                        {
                            return baseHeight + propertyHeight;
                        }
                    }
                }
            }
            // Arrays and Lists
            else if (typedValueProp.isArray || IsListType(typedValueProp))
            {
                return EditorGUI.GetPropertyHeight(typedValueProp, true);
            }
            // Dictionary
            else if (IsDictionaryType(typedValueProp))
            {
                var dict = typedValueProp.managedReferenceValue as System.Collections.IDictionary;
                if (dict != null && isFoldedOut)
                {
                    return baseHeight + (dict.Count * (EditorGUIUtility.singleLineHeight + 2));
                }
            }
            // Raw Object
            else if (
                typedValueProp.propertyType == SerializedPropertyType.Generic
                && typedValueProp.type == "object"
            )
            {
                var value = typedValueProp.managedReferenceValue;
                if (value != null && isFoldedOut)
                {
                    var type = value.GetType();
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

                    return baseHeight
                        + (
                            (fields.Length + properties.Count())
                            * (EditorGUIUtility.singleLineHeight + 2)
                        );
                }
            }
            // Regular property
            else
            {
                return EditorGUI.GetPropertyHeight(typedValueProp, true);
            }

            return baseHeight;
        }
        #endregion

        #region [[ Draw Library Elements ]] ========================================================

        void DrawLibraryHeader(Rect rect)
        {
            float currentX = rect.x + HEADER_PADDING;
            float currentY = rect.y;

            // Draw ID column
            EditorGUI.LabelField(new Rect(currentX, currentY, ID_COLUMN_WIDTH, rect.height), "ID");
            currentX += ID_COLUMN_WIDTH + PADDING;

            // Draw Value column
            EditorGUI.LabelField(
                new Rect(currentX, currentY, rect.width - currentX, rect.height),
                $"Value <{_valueType?.Name ?? "unknown"}>"
            );
        }

        void DrawLibraryElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _itemsProperty.GetArrayElementAtIndex(index);

            float currentX = rect.x;
            float currentY = rect.y + 2;

            // Draw ID
            DrawID(
                new Rect(currentX, currentY, ID_COLUMN_WIDTH, EditorGUIUtility.singleLineHeight),
                element
            );
            currentX += ID_COLUMN_WIDTH + PADDING;

            // Draw Value
            DrawValue(
                new Rect(
                    currentX,
                    currentY,
                    rect.width - currentX,
                    EditorGUIUtility.singleLineHeight
                ),
                element
            );
        }

        #endregion

        #region [[ Draw Dictionary Elements ]] ======================================================

        void DrawDictionaryHeader(Rect rect)
        {
            float currentX = rect.x + HEADER_PADDING;
            float currentY = rect.y;

            // Draw ID column
            EditorGUI.LabelField(new Rect(currentX, currentY, ID_COLUMN_WIDTH, rect.height), "ID");
            currentX += ID_COLUMN_WIDTH + PADDING;

            // Draw Key column
            EditorGUI.LabelField(
                new Rect(currentX, currentY, KEY_COLUMN_WIDTH, rect.height),
                $"Key <{_keyType?.Name ?? "unknown"}>"
            );
            currentX += KEY_COLUMN_WIDTH + PADDING;

            // Draw Value column
            EditorGUI.LabelField(
                new Rect(currentX, currentY, rect.width - currentX, rect.height),
                $"Value <{_valueType?.Name ?? "unknown"}>"
            );
        }

        void DrawDictionaryElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _itemsProperty.GetArrayElementAtIndex(index);

            float currentX = rect.x;
            float currentY = rect.y + 2;

            // Draw ID
            DrawID(
                new Rect(currentX, currentY, ID_COLUMN_WIDTH, EditorGUIUtility.singleLineHeight),
                element
            );
            currentX += ID_COLUMN_WIDTH + PADDING;

            // Draw Key
            DrawKey(
                new Rect(currentX, currentY, KEY_COLUMN_WIDTH, EditorGUIUtility.singleLineHeight),
                element
            );
            currentX += KEY_COLUMN_WIDTH + PADDING;

            // Draw Value
            DrawValue(
                new Rect(
                    currentX,
                    currentY,
                    rect.width - currentX,
                    EditorGUIUtility.singleLineHeight
                ),
                element
            );
        }

        #endregion

        #region [[ Draw Properties ]] ================================================================

        void DrawID(Rect rect, SerializedProperty property)
        {
            var idProp = property.FindPropertyRelative(ID_PROP);
            EditorGUI.LabelField(rect, idProp?.intValue.ToString() ?? "?");
        }

        void DrawKey(Rect rect, SerializedProperty property)
        {
            var keyProp = property.FindPropertyRelative(KEY_PROP);
            if (keyProp != null)
            {
                EditorGUI.PropertyField(rect, keyProp, GUIContent.none);
            }
        }

        void DrawValue(Rect rect, SerializedProperty property)
        {
            var typedValueProp = property.FindPropertyRelative(VALUE_PROP);
            if (typedValueProp == null)
                return;

            // Handle object reference values (ScriptableObjects, MonoBehaviours, etc)
            if (
                typedValueProp.propertyType == SerializedPropertyType.ObjectReference
                && typedValueProp.objectReferenceValue != null
            )
            {
                DrawSerializableObjectValue(
                    rect,
                    typedValueProp,
                    property.propertyPath.GetHashCode()
                );
            }
            // Handle arrays and lists
            else if (typedValueProp.isArray || IsListType(typedValueProp))
            {
                DrawCollectionValue(rect, typedValueProp);
            }
            // Handle Dictionary<string, object>
            else if (IsDictionaryType(typedValueProp))
            {
                DrawDictionaryObjectValue(
                    rect,
                    typedValueProp,
                    property.propertyPath.GetHashCode()
                );
            }
            // Handle raw object values
            else if (
                typedValueProp.propertyType == SerializedPropertyType.Generic
                && typedValueProp.type == "object"
            )
            {
                DrawRawObjectValue(
                    rect,
                    typedValueProp.managedReferenceValue,
                    property.propertyPath.GetHashCode()
                );
            }
            // Regular properties
            else
            {
                EditorGUI.PropertyField(rect, typedValueProp, GUIContent.none);
            }
        }

        #endregion
    }
}
