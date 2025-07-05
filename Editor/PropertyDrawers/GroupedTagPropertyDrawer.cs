#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace Darklight.Editor
{
    /// <summary>
    /// Property drawer for GroupedTagAttribute that allows selecting tags organized by groups.
    /// </summary>
    [CustomPropertyDrawer(typeof(GroupedTagAttribute))]
    public class GroupedTagPropertyDrawer : PropertyDrawer
    {
        private const string NONE_OPTION = "(None)";
        private const string UNTAGGED_OPTION = "Untagged";
        private Dictionary<string, List<string>> _tagGroups;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return base.GetPropertyHeight(property, label) + GetHelpBoxHeight();
            }

            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.String)
            {
                DrawTagMenu(position, property, label);
            }
            else
            {
                string message = $"{typeof(GroupedTagAttribute).Name} supports only string fields";
                DrawDefaultPropertyAndHelpBox(position, property, message, MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }

        private void DrawTagMenu(Rect position, SerializedProperty property, GUIContent label)
        {
            InitializeTagGroups();

            string displayName = string.IsNullOrEmpty(property.stringValue)
                ? NONE_OPTION
                : property.stringValue;

            var content = EditorGUI.DropdownButton(
                position,
                new GUIContent(label.text + ": " + displayName),
                FocusType.Keyboard
            );

            if (content)
            {
                var menu = new GenericMenu();

                // Add None option
                menu.AddItem(
                    new GUIContent(NONE_OPTION),
                    string.IsNullOrEmpty(property.stringValue),
                    () => SetPropertyValue(property, string.Empty)
                );
                menu.AddSeparator("");

                // Add Built-in tags group
                var builtInTags = TagGroupUtility.BUILT_IN_TAGS.Split(',');
                foreach (var tag in builtInTags)
                {
                    string menuPath = $"Built-in/{tag}";
                    menu.AddItem(
                        new GUIContent(menuPath),
                        property.stringValue == tag,
                        () => SetPropertyValue(property, tag)
                    );
                }
                menu.AddSeparator("");

                // Add custom grouped items
                foreach (var group in _tagGroups.OrderBy(g => g.Key))
                {
                    foreach (var tag in group.Value.OrderBy(t => t))
                    {
                        string menuPath =
                            group.Key == "Ungrouped"
                                ? tag
                                : $"{group.Key}/{tag.Substring(group.Key.Length).TrimStart()}";

                        menu.AddItem(
                            new GUIContent(menuPath),
                            property.stringValue == tag,
                            () => SetPropertyValue(property, tag)
                        );
                    }
                }

                menu.DropDown(position);
            }
        }

        private void InitializeTagGroups()
        {
            if (_tagGroups == null)
            {
                _tagGroups = new Dictionary<string, List<string>>();
                var builtInTags = new HashSet<string>(TagGroupUtility.BUILT_IN_TAGS.Split(','));

                // Group tags using TagGroupUtility
                var serializedTagManager = new SerializedObject(
                    AssetDatabase
                        .LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")
                        .FirstOrDefault()
                );
                var tagsProp = serializedTagManager.FindProperty("tags");

                var groupedTags = TagGroupUtility.GroupTags(tagsProp, builtInTags, "Ungrouped");

                foreach (var group in groupedTags)
                {
                    _tagGroups[group.Key] = group
                        .Value.Tags.Select(t => t.tag)
                        .OrderBy(t => t)
                        .ToList();
                }
            }
        }

        private void SetPropertyValue(SerializedProperty property, string value)
        {
            property.stringValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }

        private void DrawDefaultPropertyAndHelpBox(
            Rect position,
            SerializedProperty property,
            string message,
            MessageType messageType
        )
        {
            var propertyRect = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.PropertyField(propertyRect, property, true);

            var helpBoxRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + 2f,
                position.width,
                GetHelpBoxHeight()
            );
            EditorGUI.HelpBox(helpBoxRect, message, messageType);
        }

        private float GetHelpBoxHeight() => EditorGUIUtility.singleLineHeight * 2f;
    }
}
#endif
