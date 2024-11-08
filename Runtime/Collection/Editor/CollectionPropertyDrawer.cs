using System;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Collection.Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Collection), true)]
    public class CollectionPropertyDrawer : PropertyDrawerBase
    {
        private const string LIBRARY_ITEMS_PROP = "_libraryItems";
        private const string DICTIONARY_ITEMS_PROP = "_dictionaryItems";
        private CollectionReorderableList _list;
        private SerializedProperty _itemsProperty;

        protected override void OnGUI_Internal(
            Rect rect,
            SerializedProperty property,
            GUIContent label
        )
        {
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

            if (_list != null)
            {
                _list.DoList(rect);
            }

            EditorGUILayout.PropertyField(property);
        }

        protected override float GetPropertyHeight_Internal(
            SerializedProperty property,
            GUIContent label
        )
        {
            if (_list == null)
                return EditorGUIUtility.singleLineHeight;

            return _list.GetHeight();
        }
    }
#endif
}
