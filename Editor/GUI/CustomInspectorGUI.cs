using System;
using System.Collections.Generic;
using System.Reflection;
using Darklight.Editor.Utility;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Darklight.Editor
{
    public static class CustomInspectorGUI
    {
#if UNITY_EDITOR
        static Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

        /// <summary>
        /// Focuses the Scene view on a specific point in 3D space.
        /// </summary>
        /// <param name="focusPoint">
        /// 		The point in 3D space to focus the Scene view on.
        /// </param>
        public static void FocusSceneView(Vector3 focusPoint)
        {
            if (SceneView.lastActiveSceneView != null)
            {
                // Set the Scene view camera pivot (center point) and size (zoom level)
                SceneView.lastActiveSceneView.pivot = focusPoint;

                // Repaint the scene view to immediately reflect changes
                SceneView.lastActiveSceneView.Repaint();
            }
        }

        #region < PUBLIC_STATIC_METHODS > [[ Draw Serialized Fields ]] ================================================================

        public static void DrawDefaultInspectorWithoutScript(UnityEngine.Object target)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                // Skip the script reference property
                if (property.propertyPath == "m_Script")
                    continue;

                EditorGUILayout.PropertyField(property, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws a show-only array or list as a foldout in the Inspector with each element displayed as a label.
        /// </summary>
        /// <param name="property">The SerializedProperty representing the array or list.</param>
        /// <param name="label">The label to display for the foldout.</param>
        /// <param name="elementNameProvider">A function that takes an index and a SerializedProperty element and returns a custom string for the element name.</param>
        public static void DrawShowOnlyListFoldout(
            SerializedProperty property,
            string label,
            Func<int, SerializedProperty, string> elementNameProvider = null
        )
        {
            if (
                property == null
                || (!property.isArray && property.propertyType != SerializedPropertyType.Generic)
            )
            {
                EditorGUILayout.HelpBox("Property is not an array or list", MessageType.Warning);
                return;
            }

            // Generate a unique key for foldout state
            string key = property.propertyPath;
            if (!foldoutStates.ContainsKey(key))
            {
                foldoutStates[key] = false;
            }

            // Draw the foldout
            foldoutStates[key] = EditorGUILayout.Foldout(foldoutStates[key], $"{label}", true);

            // If foldout is expanded, draw each element as a label
            if (foldoutStates[key])
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < property.arraySize; i++)
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(i);
                    // Use the custom name provider if available, otherwise default to "Element {i}"
                    string elementName =
                        elementNameProvider != null
                            ? elementNameProvider(i, element)
                            : $"Element {i}";
                    EditorGUILayout.LabelField(
                        elementName,
                        SerializedPropertyUtility.ConvertPropertyToString(element)
                    );
                }
                EditorGUI.indentLevel--;
            }
        }

        public static void DrawClassAsShowOnly(object classInstance, ref bool isExpanded)
        {
            if (classInstance == null)
            {
                EditorGUILayout.LabelField("No data available");
                return;
            }

            // Draw foldout for the entire class
            isExpanded = EditorGUILayout.Foldout(true, classInstance.GetType().Name);

            if (isExpanded)
            {
                EditorGUI.indentLevel++;

                // Use reflection to retrieve fields from the class
                var fields = classInstance
                    .GetType()
                    .GetFields(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    );
                foreach (var field in fields)
                {
                    // Only display fields marked as [Serializable] or [SerializeField]
                    if (field.IsPublic || Attribute.IsDefined(field, typeof(SerializeField)))
                    {
                        // Display field label and value
                        object fieldValue = field.GetValue(classInstance);
                        string fieldStringValue =
                            fieldValue != null ? fieldValue.ToString() : "null";

                        EditorGUILayout.LabelField(
                            ObjectNames.NicifyVariableName(field.Name),
                            fieldStringValue
                        );
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Draws all fields in the given SerializedProperty.
        /// </summary>
        /// <param name="property">The SerializedProperty representing the object to draw.</param>
        /// <param name="name">The name of the property to display, or an empty string for no name.</param>
        public static void DrawAllFieldsInProperty(SerializedProperty property, string name = "")
        {
            DrawHeader(name ?? property.displayName);
            IterateSerializedProperties(
                property,
                (currentProperty) =>
                {
                    EditorGUILayout.PropertyField(currentProperty, true);
                }
            );
        }

        /// <summary>
        /// Draws all fields in the given SerializedProperty in a disabled (read-only) state.
        /// </summary>
        /// <param name="property">The SerializedProperty representing the object to draw.</param>
        /// <param name="name">The name of the property to display, or an empty string for no name.</param>
        public static void DrawAllFieldsInPropertyAsDisabled(
            SerializedProperty property,
            string name = ""
        )
        {
            DrawHeader(name ?? property.displayName);
            EditorGUI.BeginDisabledGroup(true);
            IterateSerializedProperties(
                property,
                (currentProperty) =>
                {
                    EditorGUILayout.PropertyField(currentProperty, true);
                }
            );
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Draws all fields in the given SerializedProperty as labels, displaying only the field names and their values.
        /// </summary>
        /// <param name="property">The SerializedProperty representing the object to draw.</param>
        /// <param name="name">The name of the property to display, or an empty string for no name.</param>
        public static void DrawAllFieldsInPropertyAsShowOnly(
            SerializedProperty property,
            string name = ""
        )
        {
            DrawHeader(name ?? property.displayName);
            IterateSerializedProperties(
                property,
                (currentProperty) =>
                {
                    EditorGUILayout.LabelField(
                        currentProperty.displayName,
                        SerializedPropertyUtility.ConvertPropertyToString(currentProperty)
                    );
                }
            );
        }

        /// <summary>
        /// Iterates through all visible fields in the given SerializedProperty, invoking a specified action on each field.
        /// </summary>
        /// <param name="property">The SerializedProperty containing the fields to iterate through.</param>
        /// <param name="action">The action to perform on each field, typically for drawing.</param>
        static void IterateSerializedProperties(
            SerializedProperty property,
            System.Action<SerializedProperty> action
        )
        {
            SerializedProperty currentProperty = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();

            // Iterate through all properties
            while (
                currentProperty.NextVisible(true)
                && !SerializedProperty.EqualContents(currentProperty, endProperty)
            )
            {
                action(currentProperty);
            }
        }

        #endregion

        #region < PUBLIC_STATIC_METHODS > [[ Draw Toggle Fields ]] ================================================================

        /// <summary>
        /// Draws a toggle with a tooltip and label on the left side.
        /// </summary>
        /// <param name="label">The label text to display</param>
        /// <param name="tooltip">The tooltip text to show on hover</param>
        /// <param name="value">The current toggle value</param>
        /// <returns>The new toggle value</returns>
        public static bool DrawToggleLeft(string label, bool value, string tooltip = "")
        {
            return EditorGUILayout.ToggleLeft(
                new GUIContent(label, tooltip),
                value,
                EditorStyles.wordWrappedLabel
            );
        }

        public static bool DrawToggleLeft(string label, string tooltip, bool value, GUIStyle style)
        {
            return EditorGUILayout.ToggleLeft(new GUIContent(label, tooltip), value, style);
        }

        /// <summary>
        /// Draws a toggle group with a header and multiple toggles.
        /// </summary>
        /// <param name="groupLabel">The header text for the toggle group</param>
        /// <param name="toggles">Dictionary of toggle labels and their current values</param>

        /// <param name="tooltips">Optional dictionary of tooltips for each toggle</param>
        /// <returns>Dictionary of updated toggle values</returns>
        public static Dictionary<string, bool> DrawToggleGroup(
            Dictionary<string, (bool, string)> toggles
        )
        {
            var results = new Dictionary<string, bool>();
            foreach (var toggle in toggles)
            {
                string tooltip = toggle.Value.Item2;
                bool value = toggle.Value.Item1;

                results[toggle.Key] = DrawToggleLeft(toggle.Key, value, tooltip);
            }
            return results;
        }

        /// <summary>
        /// Draws a toggle with an icon and tooltip.
        /// </summary>
        /// <param name="label">The label text to display</param>
        /// <param name="tooltip">The tooltip text to show on hover</param>
        /// <param name="icon">The icon to display next to the toggle</param>
        /// <param name="value">The current toggle value</param>
        /// <returns>The new toggle value</returns>
        public static bool DrawIconToggle(string label, string tooltip, Texture icon, bool value)
        {
            GUIContent content = new GUIContent(label, icon, tooltip);
            return EditorGUILayout.ToggleLeft(content, value, EditorStyles.wordWrappedLabel);
        }

        /// <summary>
        /// Draws a toggle with custom styling options.
        /// </summary>
        /// <param name="label">The label text to display</param>
        /// <param name="tooltip">The tooltip text to show on hover</param>
        /// <param name="value">The current toggle value</param>
        /// <param name="style">Optional custom GUIStyle for the toggle</param>
        /// <param name="options">Optional layout options</param>
        /// <returns>The new toggle value</returns>
        public static bool DrawStyledToggle(
            string label,
            string tooltip,
            bool value,
            GUIStyle style = null,
            params GUILayoutOption[] options
        )
        {
            GUIStyle toggleStyle = style ?? EditorStyles.wordWrappedLabel;
            return EditorGUILayout.ToggleLeft(
                new GUIContent(label, tooltip),
                value,
                toggleStyle,
                options
            );
        }

        #endregion

        #region -- << GUI ELEMENTS >> ------------------------------------ >>
        public static void DrawHorizontalLine(Color color, int thickness = 1, int padding = 10)
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(thickness + padding));
            rect.height = thickness;
            rect.y += padding / 2;
            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// Draws a header label if one is provided.
        /// </summary>
        /// <param name="header">The header label to draw, or null if no header should be displayed.</param>
        public static void DrawHeader(string header)
        {
            if (!string.IsNullOrEmpty(header))
            {
                EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
            }
        }

        /// <summary>
        /// Creates a +/- control for an integer value with a title, current value, minimum value, maximum value, and a set value action.
        /// /// </summary>
        /// <param name="title">The title of the control.</param>
        /// <param name="currentValue">The current value of the control.</param>
        /// <param name="minValue">The minimum value of the control.</param>
        /// <param name="maxValue">The maximum value of the control.</param>
        /// <param name="setValue">The action to set the value of the control.</param>

        public static void CreateIntegerControl(
            string title,
            int currentValue,
            int minValue,
            int maxValue,
            System.Action<int> setValue
        )
        {
            GUIStyle controlBackgroundStyle = new GUIStyle();
            controlBackgroundStyle.normal.background = MakeTex(
                1,
                1,
                new Color(1.0f, 1.0f, 1.0f, 0.1f)
            );
            controlBackgroundStyle.alignment = TextAnchor.MiddleCenter;
            controlBackgroundStyle.margin = new RectOffset(20, 20, 0, 0);

            EditorGUILayout.BeginVertical(controlBackgroundStyle);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(title);
            GUILayout.FlexibleSpace();

            // +/- Buttons
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
            {
                setValue(Mathf.Max(minValue, currentValue - 1));
            }
            EditorGUILayout.LabelField(
                $"{currentValue}",
                CustomGUIStyles.CenteredStyle,
                GUILayout.MaxWidth(50)
            );
            if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
            {
                setValue(Mathf.Min(maxValue, currentValue + 1));
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Creates a foldout in the Unity Editor and executes the given Action if the foldout is expanded.
        /// </summary>
        /// <param name="foldoutLabel">Label for the foldout.</param>
        /// <param name="foldoutToggle">Reference to the variable tracking the foldout's expanded state.</param>
        /// <param name="innerAction">Action to execute when the foldout is expanded. This action contains the UI elements to be drawn inside the foldout.</param>
        public static void CreateFoldout(
            ref bool foldoutToggle,
            string foldoutLabel,
            Action innerAction
        )
        {
            // Draw the foldout
            foldoutToggle = EditorGUILayout.Foldout(
                foldoutToggle,
                foldoutLabel,
                true,
                EditorStyles.foldoutHeader
            );

            // If the foldout is expanded, execute the action
            if (foldoutToggle && innerAction != null)
            {
                EditorGUI.indentLevel++; // Indent the contents of the foldout for better readability
                innerAction.Invoke(); // Execute the provided Action
                EditorGUI.indentLevel--; // Reset indentation
            }
        }

        private static GUIStyle GetColoredHelpBoxStyle(Color backgroundColor = default)
        {
            var style = new GUIStyle(EditorStyles.helpBox);
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, backgroundColor);
            texture.Apply();
            style.normal.background = texture;
            return style;
        }

        public static void DrawPropertyGroup(
            string title,
            Action drawProperties,
            Color backgroundColor = default
        )
        {
            var style =
                backgroundColor == default
                    ? EditorStyles.helpBox
                    : GetColoredHelpBoxStyle(backgroundColor);
            using (new EditorGUILayout.VerticalScope(style))
            {
                EditorGUI.indentLevel++;
                drawProperties?.Invoke();
                EditorGUI.indentLevel--;
            }
        }

        public static bool DrawFoldoutPropertyGroup(
            string title,
            bool foldoutState,
            Action drawProperties,
            Color backgroundColor = default
        )
        {
            var style =
                backgroundColor == default
                    ? EditorStyles.helpBox
                    : GetColoredHelpBoxStyle(backgroundColor);
            bool newState = false;
            using (new EditorGUILayout.VerticalScope(style))
            {
                EditorGUI.indentLevel++;

                newState = EditorGUILayout.Foldout(foldoutState, title);
                if (newState)
                {
                    EditorGUI.indentLevel++;
                    drawProperties?.Invoke();
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            return newState;
        }

        public static void DrawFoldoutPropertyGroup(
            string title,
            bool foldoutState,
            Action drawProperties,
            out bool newState,
            Color backgroundColor = default
        )
        {
            newState = DrawFoldoutPropertyGroup(
                title,
                foldoutState,
                drawProperties,
                backgroundColor
            );
        }

        public static bool DrawTogglePropertyGroup(
            string title,
            bool toggleState,
            Action drawProperties,
            Color backgroundColor = default
        )
        {
            var style =
                backgroundColor == default
                    ? EditorStyles.helpBox
                    : GetColoredHelpBoxStyle(backgroundColor);

            if (!toggleState)
                style = GetColoredHelpBoxStyle(new Color(0.7f, 0.7f, 0.7f, 0.5f));

            using (new EditorGUILayout.VerticalScope(style))
            {
                toggleState = EditorGUILayout.ToggleLeft(title, toggleState);

                if (!toggleState)
                    return false;

                EditorGUI.indentLevel++;
                drawProperties?.Invoke();
                EditorGUI.indentLevel--;

                return true;
            }
        }

        public static void DrawTogglePropertyGroup(
            string title,
            bool toggleState,
            Action drawProperties,
            out bool newState,
            Color backgroundColor = default
        )
        {
            newState = DrawTogglePropertyGroup(title, toggleState, drawProperties, backgroundColor);
        }

        /// <summary>

        /// Creates a two-column label with a prefix label and a value label.
        /// </summary>
        /// <param name="label">The label for the prefix.</param>
        /// <param name="value">The value to display.</param>

        public static void CreateTwoColumnLabel(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            EditorGUILayout.LabelField(value);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a label for an enum property with a dropdown to select the enum value.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enumProperty"></param>
        /// <param name="label"></param>
        public static void DrawEnumProperty<TEnum>(ref TEnum enumProperty, string label)
            where TEnum : System.Enum
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label); // Adjust the width as needed

            GUILayout.FlexibleSpace();

            enumProperty = (TEnum)EditorGUILayout.EnumPopup(enumProperty);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Creates a label for an enum property with a dropdown to select the enum value.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enumProperty"></param>
        /// <param name="label"></param>
        public static void DrawEnumValue<TEnum>(TEnum enumProperty, string label)
            where TEnum : System.Enum
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label); // Adjust the width as needed

            GUILayout.FlexibleSpace();

            enumProperty = (TEnum)EditorGUILayout.EnumPopup(enumProperty);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        #endregion

        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        public static bool IsObjectOrChildSelected(GameObject obj)
        {
            // Check if the direct object is selected
            if (Selection.activeGameObject == obj)
            {
                return true;
            }

            // Check if any of the selected objects is a child of the inspected object
            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                if (selectedObject.transform.IsChildOf(obj.transform))
                {
                    return true;
                }
            }

            return false;
        }

        public static void DrawButton(string label, Action onClick)
        {
            if (GUILayout.Button(label))
            {
                onClick?.Invoke();
            }
        }
#endif
    }
}
