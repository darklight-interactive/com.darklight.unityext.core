using System;
using UnityEngine;
#if UNITY_EDITOR
using NaughtyAttributes.Editor;
using UnityEditor;
#endif

namespace Darklight.Collections.Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Collection), true)]
    public class CollectionPropertyDrawer : PropertyDrawerBase
    {
        private const string LIBRARY_ITEMS_PROP = "_libraryItems";
        private const string DICTIONARY_ITEMS_PROP = "_dictionaryItems";
        private const float INDENTATION = 10f;
        private CollectionReorderableList _list;
        private SerializedProperty _itemsProperty;
        private bool _isExpanded;

        protected override void OnGUI_Internal(
            Rect rect,
            SerializedProperty property,
            GUIContent label
        )
        {
            Rect contentRect = rect;

            if (_list == null)
            {
                var libraryItemsProperty = property.FindPropertyRelative(LIBRARY_ITEMS_PROP);
                var dictionaryItemsProperty = property.FindPropertyRelative(DICTIONARY_ITEMS_PROP);

                if (libraryItemsProperty != null)
                {
                    _itemsProperty = libraryItemsProperty;
                }
                else if (dictionaryItemsProperty != null)
                {
                    _itemsProperty = dictionaryItemsProperty;
                }
                else
                {
                    Debug.LogError(
                        "No items property found",
                        property.serializedObject.targetObject
                    );
                    return;
                }

                if (_itemsProperty != null)
                {
                    _list = new CollectionReorderableList(
                        property.serializedObject,
                        fieldInfo,
                        _itemsProperty
                    );
                }
            }

            float x = contentRect.x;
            float y = contentRect.y;

            // << FOLDOUT LABEL >>
            Rect foldoutRect = new Rect(x, y, contentRect.width, EditorGUIUtility.singleLineHeight);
            _isExpanded = EditorGUI.Foldout(foldoutRect, _isExpanded, label, true);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Only show content if expanded
            if (_isExpanded)
            {
                // << LIST >>
                x += INDENTATION; // Indentation
                if (_list != null)
                {
                    float listHeight = _list.GetHeight();
                    Rect listRect = new Rect(x, y, contentRect.width - INDENTATION, listHeight);
                    _list.DoList(listRect);
                    y += listHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                // << PROPERTY FIELD >>
                x += INDENTATION; // Secondary Indentation
                float propertyFieldHeight = EditorGUI.GetPropertyHeight(property, true);
                Rect propertyRect = new Rect(
                    x,
                    y,
                    contentRect.width - (INDENTATION * 2),
                    propertyFieldHeight
                );
                EditorGUI.PropertyField(
                    propertyRect,
                    property,
                    new GUIContent(property.displayName),
                    true
                );
            }
        }

        protected override float GetPropertyHeight_Internal(
            SerializedProperty property,
            GUIContent label
        )
        {
            float height = 0f;

            // Foldout Label
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Only include content height if expanded
            if (_isExpanded)
            {
                // List
                if (_list != null)
                {
                    height += _list.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                }

                // Property Field
                height += EditorGUI.GetPropertyHeight(property, true);
            }

            return height;
        }
    }
#endif
}
