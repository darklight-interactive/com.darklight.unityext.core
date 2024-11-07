using UnityEngine;
using UnityEditor;
using System;
using NaughtyAttributes.Editor;

namespace Darklight.UnityExt.Collection.Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(CollectionLibrary), true)]
    public class LibraryPropertyDrawer : PropertyDrawerBase
    {
        private const string ITEMS_PROP = "_items";
        private LibraryReorderableList _list;
        private SerializedProperty _itemsProperty;

        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (_list == null)
            {
                _itemsProperty = property.FindPropertyRelative(ITEMS_PROP);
                if (_itemsProperty != null)
                {
                    _list = new LibraryReorderableList(
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
        }

        protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
        {
            if (_list == null)
                return EditorGUIUtility.singleLineHeight;

            return _list.GetHeight();
        }
    }
#endif
}
