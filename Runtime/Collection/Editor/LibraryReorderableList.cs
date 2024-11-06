using UnityEngine;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

using NaughtyAttributes.Editor;

namespace Darklight.UnityExt.Collection.Editor
{
#if UNITY_EDITOR
    public class LibraryReorderableList : ReorderableList
    {
        // Layout constants
        private const float DEFAULT_INDENT = 15f;
        private const float MIN_COLUMN_WIDTH = 50f;
        private const bool DRAGGABLE = false;

        // Column configuration
        private struct ColumnInfo
        {
            public string Label;
            public float MinWidth;
            public float Weight;

            public ColumnInfo(string label, float minWidth, float weight)
            {
                Label = label;
                MinWidth = minWidth;
                Weight = weight;
            }
        }

        private bool _isKeyValueLibrary;
        private readonly ColumnInfo[] _columnsWithKey;
        private readonly ColumnInfo[] _columnsWithoutKey;

        // GUI Styles
        private readonly GUIStyle _centeredLabel;
        private readonly GUIStyle _expandedContentStyle;

        // Properties
        private FieldInfo _fieldInfo;
        private SerializedProperty _itemsProperty;
        private SerializedProperty _readOnlyKeyProperty;
        private SerializedProperty _readOnlyValueProperty;

        public LibraryReorderableList(
            SerializedObject serializedObject, 
            SerializedProperty itemsProperty,
            SerializedProperty readOnlyKeyProperty, 
            SerializedProperty readOnlyValueProperty, 
            FieldInfo fieldInfo) : base(serializedObject, itemsProperty, DRAGGABLE, true, true, true)
        {
            // Initialize fields
            _fieldInfo = fieldInfo;
            _itemsProperty = itemsProperty;
            _readOnlyKeyProperty = readOnlyKeyProperty;
            _readOnlyValueProperty = readOnlyValueProperty;

            _isKeyValueLibrary = fieldInfo.FieldType.GetGenericArguments().Length > 1;
            
            _columnsWithKey = new[]
            {
                new ColumnInfo("ID", 30f, 0.1f),
                new ColumnInfo("Key", 100f, 0.4f),
                new ColumnInfo("Value", 150f, 0.5f)
            };

            _columnsWithoutKey = new[]
            {
                new ColumnInfo("ID", 50f, 0.2f),
                new ColumnInfo("Value", 200f, 0.8f)
            };

            // Initialize styles
            _centeredLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            _expandedContentStyle = new GUIStyle(EditorStyles.label) { padding = new RectOffset(15, 0, 0, 0) };

            // Setup callbacks
            SetupCallbacks();
        }

        private void SetupCallbacks()
        {
            drawHeaderCallback = DrawHeaderInternal;
            drawElementCallback = DrawElementInternal;
            elementHeightCallback = GetElementHeightInternal;
            onRemoveCallback = OnRemoveElementInternal;
            drawElementBackgroundCallback = DrawElementBackgroundInternal;
        }

        private void DrawHeaderInternal(Rect rect)
        {
            var columnRects = CalculateColumnRects(rect);
            for (int i = 0; i < (_isKeyValueLibrary ? _columnsWithKey.Length : _columnsWithoutKey.Length); i++)
            {
                EditorGUI.LabelField(columnRects[i], (_isKeyValueLibrary ? _columnsWithKey[i].Label : _columnsWithoutKey[i].Label), _centeredLabel);
            }
        }

        private void DrawElementInternal(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = GetElementProperties(index);
            bool isExpandable = IsExpandableProperty(element.Value);

            EditorGUI.BeginChangeCheck();

            if (isExpandable && element.Value.isExpanded)
            {
                DrawExpandedElement(rect, element);
            }
            else
            {
                DrawCompactElement(rect, element);
            }

            if (EditorGUI.EndChangeCheck())
            {
                ApplyChanges();
            }
        }

        private void DrawCompactElement(Rect rect, (SerializedProperty Id, SerializedProperty Key, SerializedProperty Value) element)
        {
            var columnRects = CalculateColumnRects(rect);

            EditorGUI.LabelField(columnRects[0], element.Id.intValue.ToString(), _centeredLabel);
            
            if (_isKeyValueLibrary)
            {
                DrawPropertyField(columnRects[1], element.Key, _readOnlyKeyProperty.boolValue);
                DrawPropertyField(columnRects[2], element.Value, _readOnlyValueProperty.boolValue);
            }
            else
            {
                DrawPropertyField(columnRects[1], element.Value, _readOnlyValueProperty.boolValue);
            }
        }

        private void DrawExpandedElement(Rect rect, (SerializedProperty Id, SerializedProperty Key, SerializedProperty Value) element)
        {
            float yPos = rect.y;
            float contentIndent = DEFAULT_INDENT * 2;

            // Header row with ID and Key
            var headerRect = new Rect(rect.x, yPos, rect.width, EditorGUIUtility.singleLineHeight);
            var idRect = new Rect(headerRect.x, headerRect.y, 30f, headerRect.height);
            var keyRect = new Rect(idRect.xMax + 5f, headerRect.y, headerRect.width - idRect.width - 5f, headerRect.height);

            EditorGUI.LabelField(idRect, element.Id.intValue.ToString(), _centeredLabel);
            DrawPropertyField(keyRect, element.Key, _readOnlyKeyProperty.boolValue);

            // Value section
            yPos += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var valueRect = new Rect(rect.x + contentIndent, yPos, rect.width - contentIndent, EditorGUIUtility.singleLineHeight);
            DrawExpandablePropertyField(valueRect, element.Value);
        }

        private void DrawExpandablePropertyField(Rect rect, SerializedProperty property)
        {
            if (property.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(rect, property, GUIContent.none);
                return;
            }

            var serializedObject = new SerializedObject(property.objectReferenceValue);
            serializedObject.Update();

            // Object field with foldout
            var foldoutRect = new Rect(rect.x, rect.y, 15f, EditorGUIUtility.singleLineHeight);
            var objectFieldRect = new Rect(rect.x + 15f, rect.y, rect.width - 15f, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);
            EditorGUI.PropertyField(objectFieldRect, property, GUIContent.none);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawSerializedObjectProperties(rect, serializedObject);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSerializedObjectProperties(Rect rect, SerializedObject serializedObject)
        {
            var iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == "m_Script") continue;

                float propertyHeight = EditorGUI.GetPropertyHeight(iterator);
                var propertyRect = new Rect(rect.x, rect.y + yOffset, rect.width, propertyHeight);
                EditorGUI.PropertyField(propertyRect, iterator, true);
                yOffset += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private float GetElementHeightInternal(int index)
        {
            var element = GetElementProperties(index);
            if (!IsExpandableProperty(element.Value) || !element.Value.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float height = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
            
            if (element.Value.objectReferenceValue != null)
            {
                var serializedObject = new SerializedObject(element.Value.objectReferenceValue);
                var iterator = serializedObject.GetIterator();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (iterator.name == "m_Script") continue;
                    height += EditorGUI.GetPropertyHeight(iterator) + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }

        private (SerializedProperty Id, SerializedProperty Key, SerializedProperty Value) GetElementProperties(int index)
        {
            var element = _itemsProperty.GetArrayElementAtIndex(index);
            return (
                element.FindPropertyRelative("_id"),
                element.FindPropertyRelative("_key"),
                element.FindPropertyRelative("_value")
            );
        }

        private bool IsExpandableProperty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ObjectReference && 
                   property.objectReferenceValue is ScriptableObject;
        }

        private Rect[] CalculateColumnRects(Rect totalRect)
        {
            var columns = _isKeyValueLibrary ? _columnsWithKey : _columnsWithoutKey;
            float totalWeight = columns.Sum(c => c.Weight);
            float availableWidth = totalRect.width - (columns.Length + 1) * DEFAULT_INDENT;
            
            var rects = new Rect[columns.Length];
            float currentX = totalRect.x + DEFAULT_INDENT;

            for (int i = 0; i < columns.Length; i++)
            {
                float width = Mathf.Max(columns[i].MinWidth, (availableWidth * columns[i].Weight / totalWeight));
                rects[i] = new Rect(currentX, totalRect.y, width, totalRect.height);
                currentX += width + DEFAULT_INDENT;
            }

            return rects;
        }

        public void DrawList(Rect rect)
        {
            DoList(rect);
        }

        public void ApplyChanges()
        {
            // Ensure the modified properties are serialized
            _itemsProperty.serializedObject.ApplyModifiedProperties();

            // Mark the target object dirty to ensure the changes are saved
            EditorUtility.SetDirty(_itemsProperty.serializedObject.targetObject);

            // Force the editor to repaint both the inspector and the scene
            _itemsProperty.serializedObject.UpdateIfRequiredOrScript();
            _itemsProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorApplication.QueuePlayerLoopUpdate(); // Updates Scene view if necessary
            EditorWindow.focusedWindow?.Repaint();
        }

        private void DrawElementBackgroundInternal(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (isActive)
            {
                EditorGUI.DrawRect(rect, new Color(0.24f, 0.49f, 0.90f, 0.3f));
            }
            else if (isFocused)
            {
                EditorGUI.DrawRect(rect, new Color(0.24f, 0.49f, 0.90f, 0.1f));
            }
        }

        private void DrawPropertyField(Rect rect, SerializedProperty property, bool readOnly = false)
        {
            if (readOnly)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(rect, property, GUIContent.none);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                DrawValueField(rect, property);
            }
        }

        private void DrawValueField(Rect rect, SerializedProperty valueProperty)
        {
            // Check if the value is a Unity Object type that can be expanded
            if (valueProperty.propertyType == SerializedPropertyType.ObjectReference)
            {
                Object objectRef = valueProperty.objectReferenceValue;
                if (objectRef != null && objectRef is ScriptableObject)
                {
                    // Draw foldout and object field on the same line
                    Rect foldoutRect = new Rect(rect.x, rect.y, 15f, EditorGUIUtility.singleLineHeight);
                    Rect objectFieldRect = new Rect(rect.x + 15f, rect.y, rect.width - 15f, EditorGUIUtility.singleLineHeight);

                    valueProperty.isExpanded = EditorGUI.Foldout(foldoutRect, valueProperty.isExpanded, GUIContent.none, true);
                    EditorGUI.PropertyField(objectFieldRect, valueProperty, GUIContent.none, false);

                    // If expanded, draw the scriptable object's properties
                    if (valueProperty.isExpanded && objectRef != null)
                    {
                        SerializedObject serializedObject = new SerializedObject(objectRef);
                        serializedObject.Update();

                        EditorGUI.indentLevel += 2;
                        
                        SerializedProperty prop = serializedObject.GetIterator();
                        bool enterChildren = true;
                        float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        
                        while (prop.NextVisible(enterChildren))
                        {
                            enterChildren = false;
                            if (prop.name == "m_Script") continue;

                            float propertyHeight = EditorGUI.GetPropertyHeight(prop);
                            Rect propertyRect = new Rect(rect.x, rect.y + yOffset, rect.width, propertyHeight);
                            EditorGUI.PropertyField(propertyRect, prop, true);
                            
                            yOffset += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
                        }

                        EditorGUI.indentLevel -= 2;
                        
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    EditorGUI.PropertyField(rect, valueProperty, GUIContent.none);
                }
            }
            else if (valueProperty.propertyType == SerializedPropertyType.Integer)
            {
                valueProperty.intValue = EditorGUI.IntField(rect, valueProperty.intValue);
            }
            else if (valueProperty.propertyType == SerializedPropertyType.Float)
            {
                valueProperty.floatValue = EditorGUI.FloatField(rect, valueProperty.floatValue);
            }
            else if (valueProperty.propertyType == SerializedPropertyType.Vector2)
            {
                valueProperty.vector2Value = EditorGUI.Vector2Field(rect, GUIContent.none, valueProperty.vector2Value);
            }
            else if (valueProperty.propertyType == SerializedPropertyType.Vector3)
            {
                valueProperty.vector3Value = EditorGUI.Vector3Field(rect, GUIContent.none, valueProperty.vector3Value);
            }
            else
            {
                EditorGUI.PropertyField(rect, valueProperty, GUIContent.none);
            }
        }

        private void OnRemoveElementInternal(ReorderableList list)
        {
            _itemsProperty.DeleteArrayElementAtIndex(list.index);
            _itemsProperty.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
