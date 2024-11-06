using System;
using System.Reflection;
using UnityEngine;
using Darklight.UnityExt.Collection;
#if UNITY_EDITOR
using UnityEditor;
using NaughtyAttributes.Editor;
#endif

namespace Darklight.UnityExt.Collection.Editor
{
/*
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Library), true)]
    public class LibraryPropertyDrawer : PropertyDrawerBase
    {
        const string ITEMS_PROP = "_items";
        const string GUI_SETTINGS = "_guiSettings";
        const string READ_ONLY_KEY = "ReadOnlyKey";
        const string READ_ONLY_VALUE = "ReadOnlyValue";
        const string SHOW_CACHE_STATS = "ShowCacheStats";

        readonly float SINGLE_LINE_HEIGHT = EditorGUIUtility.singleLineHeight;
        readonly float VERTICAL_SPACING = EditorGUIUtility.singleLineHeight * 0.5f;

        readonly GUIStyle LABEL_STYLE = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter
        };

        SerializedObject _serializedObject;
        SerializedProperty _libraryProperty;
        SerializedProperty _itemsProperty;
        SerializedProperty _guiSettingsProperty;
        LibraryReorderableList _list;

        bool _foldout;
        float _fullPropertyHeight;

        private void InitializeProperties(SerializedProperty property)
        {
            if (_serializedObject == null)
                _serializedObject = property.serializedObject;

            if (_libraryProperty == null || _libraryProperty.propertyPath != property.propertyPath)
            {
                _libraryProperty = property;
                _itemsProperty = property.FindPropertyRelative(ITEMS_PROP);
                _guiSettingsProperty = property.FindPropertyRelative(GUI_SETTINGS);

                if (_guiSettingsProperty == null)
                {
                    Debug.LogError($"Could not find GUISettings property in {property.propertyPath}");
                    return;
                }

                InitializeReorderableList();
            }
        }

        private void InitializeReorderableList()
        {
            if (_list != null)
            {
                Debug.Log("ReorderableList already initialized. Skipping.");
                return;
            }

            try
            {
                _list = new LibraryReorderableList(
                    _serializedObject,
                    fieldInfo,
                    _itemsProperty
                );

                SetupListCallbacks();

                Debug.Log("ReorderableList initialized successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error initializing ReorderableList: {e.Message}\n{e.StackTrace}");
            }
        }

        private void SetupListCallbacks()
        {
            _list.onChangedCallback += (list) =>
            {
                _libraryProperty.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_libraryProperty.serializedObject.targetObject);
                Debug.Log($"Library items changed. There are {_itemsProperty.arraySize} items in the library.", _libraryProperty.serializedObject.targetObject);
            };

            _list.onAddDropdownCallback = (rect, list) =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add Item"), false, () => 
                    ExecuteWithErrorHandling("AddDefaultItem"));
                menu.AddItem(new GUIContent("Reset Library"), false, () => 
                    ExecuteWithErrorHandling("Reset"));
                menu.AddItem(new GUIContent("Clear Library"), false, () => 
                    ExecuteWithErrorHandling("Clear"));
                menu.ShowAsContext();
            };

            _list.onRemoveCallback = (list) =>
            {
                ExecuteWithErrorHandling("RemoveAt", new object[] { list.index });
            };

            _list.drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "No items in library.", LABEL_STYLE);
            };
        }

        private void ExecuteWithErrorHandling(string methodName, object[] parameters = null)
        {
            try
            {
                InvokeLibraryMethod(methodName, out object returnValue, parameters);
                if (_list != null)
                {
                    _list.index = -1;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error executing {methodName}: {e.Message}\n{e.StackTrace}");
            }
        }

        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            InitializeProperties(property);
            if (_list == null) return;

            float currentYPos = rect.y;
            EditorGUI.BeginProperty(rect, label, property);

            DrawHeader(rect, property, label, ref currentYPos);

            if (property.isExpanded)
            {
                DrawGUISettings(rect, ref currentYPos);
                DrawList(rect, ref currentYPos);
            }

            _fullPropertyHeight = currentYPos - rect.y;
            EditorGUI.EndProperty();
        }

        private void DrawHeader(Rect rect, SerializedProperty property, GUIContent label, ref float currentYPos)
        {
            Type libraryFieldType = fieldInfo.FieldType;
            string typeName = GetGenericTypeName(libraryFieldType);
            int itemCount = _itemsProperty.arraySize;

            Rect titleRect = new Rect(rect.x, currentYPos, rect.width, SINGLE_LINE_HEIGHT);
            property.isExpanded = EditorGUI.Foldout(
                titleRect,
                property.isExpanded,
                new GUIContent($"{label.text} : {typeName} : Count: {itemCount}"),
                true
            );
            currentYPos += SINGLE_LINE_HEIGHT + VERTICAL_SPACING / 2;
        }

        private void DrawGUISettings(Rect rect, ref float currentYPos)
        {
            EditorGUI.BeginChangeCheck();
            
            var readOnlyKeyProp = _guiSettingsProperty.FindPropertyRelative(READ_ONLY_KEY);
            var readOnlyValueProp = _guiSettingsProperty.FindPropertyRelative(READ_ONLY_VALUE);
            var showCacheStatsProp = _guiSettingsProperty.FindPropertyRelative(SHOW_CACHE_STATS);

            Rect settingsRect = new Rect(rect.x, currentYPos, rect.width, SINGLE_LINE_HEIGHT);

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.PropertyField(settingsRect, readOnlyKeyProp);
                currentYPos += SINGLE_LINE_HEIGHT;
                settingsRect.y = currentYPos;

                EditorGUI.PropertyField(settingsRect, readOnlyValueProp);
                currentYPos += SINGLE_LINE_HEIGHT;
                settingsRect.y = currentYPos;

                EditorGUI.PropertyField(settingsRect, showCacheStatsProp);
                currentYPos += SINGLE_LINE_HEIGHT + VERTICAL_SPACING;
            }

            if (EditorGUI.EndChangeCheck())
            {
                _libraryProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawList(Rect rect, ref float currentYPos)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                Rect listRect = new Rect(rect.x, currentYPos, rect.width, SINGLE_LINE_HEIGHT);
                _list.DrawList(listRect);
                currentYPos += _list.GetHeight() + VERTICAL_SPACING;
            }
        }

        protected override float GetPropertyHeight_Internal(
            SerializedProperty property,
            GUIContent label
        )
        {
            if (!property.isExpanded)
                return SINGLE_LINE_HEIGHT;

            return _fullPropertyHeight;
        }

        private void DrawPropertyField(Rect rect, SerializedProperty valueProperty)
        {
            if (valueProperty.propertyType == SerializedPropertyType.Integer)
            {
                // Example range for int
                int minValue = 0;
                int maxValue = 100;
                valueProperty.intValue = EditorGUI.IntSlider(
                    rect,
                    valueProperty.intValue,
                    minValue,
                    maxValue
                );
            }
            else if (valueProperty.propertyType == SerializedPropertyType.Float)
            {
                // Example range for float
                float minValue = 0f;
                float maxValue = 1f;
                valueProperty.floatValue = EditorGUI.Slider(
                    rect,
                    valueProperty.floatValue,
                    minValue,
                    maxValue
                );
            }
            else
            {
                // Fallback to default property field for other types
                EditorGUI.PropertyField(rect, valueProperty, GUIContent.none);
            }
        }

        // ======== [[ HELPER METHODS ]] ===================================== >>>>
        /// <summary>
        /// Gets the instance of the Library<,> for the current SerializedProperty.
        /// </summary>
        /// <param name="property">The SerializedProperty representing the Library<,> field.</param>
        /// <returns>The Library<,> instance, or null if not found.</returns>
        object GetLibraryInstance(SerializedProperty property)
        {
            // Get the target object (the script holding the Library field)
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            Type targetType = targetObject.GetType();

            // Find the Library field that matches the property
            foreach (FieldInfo field in targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.FieldType.IsSubclassOf(typeof(Library)))  // Changed from checking for generic type
                {
                    if (property.name == field.Name)
                    {
                        var instance = field.GetValue(targetObject);
                        Debug.Log($"Found Library instance of type {instance.GetType().Name}");
                        return instance;
                    }
                }
            }

            Debug.LogWarning($"No matching Library instance found for property '{property.name}' on type '{targetType}'.");
            return null;
        }

        private void InvokeLibraryMethod(string methodName, out object returnValue, object[] parameters = null)
        {
            returnValue = null;
            var library = GetLibraryInstance(_libraryProperty);
            
            if (library == null)
            {
                Debug.LogError("Failed to get Library instance");
                return;
            }

            try
            {
                // Get the method info
                var methodInfo = library.GetType().GetMethod(methodName, 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (methodInfo == null)
                {
                    Debug.LogError($"Method '{methodName}' not found on type {library.GetType().Name}");
                    return;
                }

                Debug.Log($"Invoking method '{methodName}' on {library.GetType().Name}");

                // Execute the method within a write lock if it modifies the collection
                if (IsWriteOperation(methodName))
                {
                    library.GetType().GetMethod("ExecuteWrite")?.MakeGenericMethod(typeof(object))
                        .Invoke(library, new object[] { 
                            new System.Func<object>(() => methodInfo.Invoke(library, parameters))
                        });
                }
                else
                {
                    returnValue = methodInfo.Invoke(library, parameters);
                }

                // Update the UI
                UpdateEditorUI(library);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error invoking method '{methodName}': {e.Message}\n{e.StackTrace}");
            }
        }

        private bool IsWriteOperation(string methodName)
        {
            return methodName switch
            {
                "AddDefaultItem" or "Reset" or "Clear" or "RemoveAt" => true,
                _ => false
            };
        }

        private void UpdateEditorUI(object library)
        {
            // Apply modified properties
            _libraryProperty.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_libraryProperty.serializedObject.targetObject);
            
            // Force UI updates
            _libraryProperty.serializedObject.UpdateIfRequiredOrScript();
            EditorWindow.focusedWindow?.Repaint();
            EditorApplication.QueuePlayerLoopUpdate();
        }

        string GetGenericTypeName(Type type)
        {
            if (type == null)
                return "Unknown Type";

            // Get the generic type arguments
            Type[] genericArgs = type.GetGenericArguments();

            // Remove the backtick and number from the type name if it exists
            string baseTypeName = type.Name;
            int backtickIndex = baseTypeName.IndexOf('`');
            if (backtickIndex > 0)
            {
                baseTypeName = baseTypeName.Substring(0, backtickIndex);
            }

            // If the type has generic arguments, format them
            if (genericArgs.Length > 0)
            {
                string genericArgsString = string.Join(
                    ", ",
                    Array.ConvertAll(genericArgs, t => t.Name)
                );
                return $"{baseTypeName}<{genericArgsString}>";
            }

            // Return the cleaned-up type name
            return baseTypeName;
        }
    }
    
#endif
*/
}
