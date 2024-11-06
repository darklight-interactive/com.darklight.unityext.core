using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace Darklight.UnityExt.Collection.Editor
{
#if UNITY_EDITOR
    public class LibraryReorderableList : ReorderableList
    {
        private const float PADDING = 2f;
        private const float KEY_WIDTH_PERCENT = 0.3f;
        private const float VALUE_WIDTH_PERCENT = 0.7f;

        private readonly SerializedObject _serializedObject;
        private readonly bool _showKeys;
        private GUIStyle _headerStyle;
        private GUIStyle _cellStyle;

    
        private FieldInfo _fieldInfo;
        private SerializedProperty _itemsProperty;

        public LibraryReorderableList(
            SerializedObject serializedObject,
            FieldInfo fieldInfo,
            SerializedProperty itemsProperty,
            bool showKeys = true
        ) : base(serializedObject, itemsProperty, false, true, true, true)
        {
            _serializedObject = serializedObject;
            _fieldInfo = fieldInfo;
            _itemsProperty = itemsProperty;
            _showKeys = showKeys;
            
            SetupStyles();
            SetupCallbacks();
        }

        private void SetupStyles()
        {
            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(5, 0, 0, 0)
            };

            _cellStyle = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(5, 0, 0, 0)
            };
        }

        private void SetupCallbacks()
        {
            drawHeaderCallback = DrawHeaderHandler;
            drawElementCallback = DrawElementHandler;
            elementHeightCallback = ElementHeightHandler;
        }
        

        private void DrawHeaderHandler(Rect rect)
        {
            if (_showKeys)
            {
                var keyRect = new Rect(rect.x, rect.y, rect.width * KEY_WIDTH_PERCENT, rect.height);
                var valueRect = new Rect(keyRect.xMax + PADDING, rect.y, rect.width * VALUE_WIDTH_PERCENT - PADDING, rect.height);
                
                EditorGUI.LabelField(keyRect, "Key", _headerStyle);
                EditorGUI.LabelField(valueRect, "Value", _headerStyle);
            }
            else
            {
                EditorGUI.LabelField(rect, "Value", _headerStyle);
            }
        }

        private void DrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= serializedProperty.arraySize) return;

            var element = serializedProperty.GetArrayElementAtIndex(index);
            if (element == null) return;

            var keyProp = element.FindPropertyRelative("_key");
            var valueProp = element.FindPropertyRelative("_value");

            if (_showKeys)
            {
                var keyRect = new Rect(rect.x, rect.y, rect.width * KEY_WIDTH_PERCENT, rect.height);
                var valueRect = new Rect(keyRect.xMax + PADDING, rect.y, rect.width * VALUE_WIDTH_PERCENT - PADDING, rect.height);

                DrawKeyField(keyRect, keyProp);
                DrawValueField(valueRect, valueProp);
            }
            else
            {
                DrawValueField(rect, valueProp);
            }
        }

        private void DrawKeyField(Rect rect, SerializedProperty keyProp)
        {
            if (keyProp == null) return;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, keyProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawValueField(Rect rect, SerializedProperty valueProp)
        {
            if (valueProp == null) return;

            EditorGUI.BeginChangeCheck();
            
            if (valueProp.propertyType == SerializedPropertyType.ObjectReference)
            {
                DrawObjectReferenceField(rect, valueProp);
            }
            else
            {
                EditorGUI.PropertyField(rect, valueProp, GUIContent.none);
            }

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawObjectReferenceField(Rect rect, SerializedProperty valueProp)
        {
            var obj = valueProp.objectReferenceValue;
            if (obj is ScriptableObject)
            {
                var foldoutRect = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
                var objectRect = new Rect(rect.x + 15, rect.y, rect.width - 15, EditorGUIUtility.singleLineHeight);

                valueProp.isExpanded = EditorGUI.Foldout(foldoutRect, valueProp.isExpanded, GUIContent.none);
                EditorGUI.ObjectField(objectRect, valueProp, GUIContent.none);

                if (valueProp.isExpanded && obj != null)
                {
                    EditorGUI.indentLevel++;
                    var serializedObject = new SerializedObject(obj);
                    serializedObject.Update();
                    
                    var iterator = serializedObject.GetIterator();
                    var yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    
                    while (iterator.NextVisible(true))
                    {
                        if (iterator.name == "m_Script") continue;
                        
                        var propertyRect = new Rect(rect.x, rect.y + yOffset, rect.width, EditorGUI.GetPropertyHeight(iterator));
                        EditorGUI.PropertyField(propertyRect, iterator, true);
                        yOffset += EditorGUI.GetPropertyHeight(iterator) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    
                    serializedObject.ApplyModifiedProperties();
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                EditorGUI.ObjectField(rect, valueProp, GUIContent.none);
            }
        }

        private float ElementHeightHandler(int index)
        {
            if (index >= serializedProperty.arraySize) return EditorGUIUtility.singleLineHeight;

            var element = serializedProperty.GetArrayElementAtIndex(index);
            if (element == null) return EditorGUIUtility.singleLineHeight;

            var valueProp = element.FindPropertyRelative("_value");
            if (valueProp == null) return EditorGUIUtility.singleLineHeight;

            if (valueProp.propertyType == SerializedPropertyType.ObjectReference && 
                valueProp.objectReferenceValue is ScriptableObject && 
                valueProp.isExpanded)
            {
                var obj = valueProp.objectReferenceValue;
                var serializedObject = new SerializedObject(obj);
                var iterator = serializedObject.GetIterator();
                float height = EditorGUIUtility.singleLineHeight;

                while (iterator.NextVisible(true))
                {
                    if (iterator.name == "m_Script") continue;
                    height += EditorGUI.GetPropertyHeight(iterator) + EditorGUIUtility.standardVerticalSpacing;
                }

                return height;
            }

            return EditorGUIUtility.singleLineHeight;
        }

        public void DrawList(Rect rect)
        {
            base.DoList(rect);
        }
    }
#endif
}
