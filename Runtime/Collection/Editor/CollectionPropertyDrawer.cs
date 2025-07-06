using System;
using UnityEngine;
#if UNITY_EDITOR
using NaughtyAttributes.Editor;
using UnityEditor;
#endif

namespace Darklight.Collection.Editor
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

        // Background styling
        private static readonly Color BackgroundColor = new Color(0.15f, 0.15f, 0.25f, 0.8f);
        private static readonly Color BorderColor = new Color(0.3f, 0.3f, 0.5f, 1f);
        private const float BorderWidth = 1f;
        private const float Padding = 4f;

        protected override void OnGUI_Internal(
            Rect rect,
            SerializedProperty property,
            GUIContent label
        )
        {
            // Draw border
            EditorGUI.DrawRect(rect, BorderColor);

            // Draw background (inside border)
            Rect backgroundRect = new Rect(
                rect.x + BorderWidth,
                rect.y + BorderWidth,
                rect.width - BorderWidth * 2,
                rect.height - BorderWidth * 2
            );
            EditorGUI.DrawRect(backgroundRect, BackgroundColor);

            // Adjust content area for padding
            Rect contentRect = new Rect(
                rect.x + BorderWidth + Padding,
                rect.y + BorderWidth + Padding,
                rect.width - (BorderWidth + Padding) * 2,
                rect.height - (BorderWidth + Padding) * 2
            );

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

            // << LABEL >>
            Rect labelRect = new Rect(x, y, contentRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

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

        protected override float GetPropertyHeight_Internal(
            SerializedProperty property,
            GUIContent label
        )
        {
            float height = 0f;

            // Label
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // List
            if (_list != null)
            {
                height += _list.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            }

            // Property Field
            height += EditorGUI.GetPropertyHeight(property, true);

            // Add padding for border and background
            height += (BorderWidth + Padding) * 2;

            return height;
        }
    }
#endif
}
