#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Darklight.Editor
{
    /// <summary>
    /// Editor window for managing Unity tags with support for adding, removing, and viewing both built-in and custom tags.
    /// </summary>
    public class TagManagerWindow : EditorWindow
    {
        private const string WINDOW_TITLE = "Tag Manager";
        private const float BUTTON_WIDTH = 100f;
        private const float SPACE_HEIGHT = 10f;

        private SerializedObject _tagManager;
        private SerializedProperty _tagsProp;
        private Vector2 _scrollPosition;
        private string _newTagName = string.Empty;
        private bool _builtInTagsFoldout;
        private readonly HashSet<string> _builtInTagsSet;
        private Dictionary<string, bool> _tagGroupFoldouts = new Dictionary<string, bool>();
        private const string DEFAULT_GROUP = "Ungrouped";

        public TagManagerWindow()
        {
            _builtInTagsSet = new HashSet<string>(TagGroupUtility.BUILT_IN_TAGS.Split(','));
        }

        [MenuItem("Tools/Darklight/Tag Manager", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<TagManagerWindow>(WINDOW_TITLE);
            window.titleContent = new GUIContent(WINDOW_TITLE);
            window.minSize = new Vector2(300, 400);
            window.Show();
        }

        private void OnEnable()
        {
            InitializeTagManager();
        }

        private void OnFocus()
        {
            // Refresh the tag manager when window gains focus
            InitializeTagManager();
        }

        private void InitializeTagManager()
        {
            var tagManagerAsset = AssetDatabase
                .LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")
                .FirstOrDefault();
            if (tagManagerAsset != null)
            {
                _tagManager = new SerializedObject(tagManagerAsset);
                _tagsProp = _tagManager.FindProperty("tags");
            }
        }

        private void OnGUI()
        {
            if (_tagManager == null)
            {
                EditorGUILayout.HelpBox("Failed to load Tag Manager.", MessageType.Error);
                return;
            }

            _tagManager.Update();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            DrawBuiltInTags();
            EditorGUILayout.Space(SPACE_HEIGHT);
            DrawUserTags();
            EditorGUILayout.Space(SPACE_HEIGHT);
            DrawTagCreation();

            EditorGUILayout.EndScrollView();
            _tagManager.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            using var scope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            EditorGUILayout.LabelField(WINDOW_TITLE, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Manage your project's tags. Note that built-in tags cannot be modified.",
                MessageType.Info
            );
        }

        private void DrawTagCreation()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _newTagName = EditorGUILayout.TextField(
                    "New Tag",
                    _newTagName?.Trim() ?? string.Empty
                );

                using (
                    new EditorGUI.DisabledScope(
                        string.IsNullOrEmpty(_newTagName) || TagExists(_newTagName)
                    )
                )
                {
                    if (GUILayout.Button("Add Tag", GUILayout.Width(BUTTON_WIDTH)))
                    {
                        AddTag(_newTagName);
                        _newTagName = string.Empty;
                        GUI.FocusControl(null);
                    }
                }
            }

            EditorGUILayout.HelpBox(
                "Tags with the same first word will be grouped together. Example: 'Enemy Boss' will be grouped under 'Enemy'.",
                MessageType.Info
            );

            if (!string.IsNullOrEmpty(_newTagName) && TagExists(_newTagName))
            {
                EditorGUILayout.HelpBox("This tag already exists!", MessageType.Warning);
            }
        }

        private void DrawBuiltInTags()
        {
            _builtInTagsFoldout = EditorGUILayout.Foldout(
                _builtInTagsFoldout,
                "Built-in Tags",
                true
            );
            if (_builtInTagsFoldout)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    foreach (var tag in _builtInTagsSet)
                    {
                        EditorGUILayout.LabelField(tag);
                    }
                }
            }
        }

        private void DrawUserTags()
        {
            EditorGUILayout.LabelField("User Tags:", EditorStyles.boldLabel);

            // Group tags using the utility class
            var tagGroups = TagGroupUtility.GroupTags(_tagsProp, _builtInTagsSet, DEFAULT_GROUP);

            // Draw grouped tags
            foreach (var group in tagGroups.OrderBy(g => g.Key == DEFAULT_GROUP).ThenBy(g => g.Key))
            {
                if (!_tagGroupFoldouts.ContainsKey(group.Key))
                {
                    _tagGroupFoldouts[group.Key] = true;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    string headerText =
                        group.Key == DEFAULT_GROUP
                            ? DEFAULT_GROUP
                            : $"{group.Key} ({group.Value.Tags.Count})";

                    _tagGroupFoldouts[group.Key] = EditorGUILayout.Foldout(
                        _tagGroupFoldouts[group.Key],
                        headerText,
                        true
                    );

                    if (_tagGroupFoldouts[group.Key])
                    {
                        EditorGUI.indentLevel++;
                        foreach (var (tag, index) in group.Value.Tags.OrderBy(t => t.tag))
                        {
                            string displayName = tag;
                            if (group.Key != DEFAULT_GROUP)
                            {
                                displayName = tag.Substring(group.Key.Length).TrimStart();
                            }
                            DrawTagInGroup(displayName, tag, index);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUILayout.Space(2);
            }
        }

        private void DrawTagInGroup(string displayName, string fullTag, int index)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // Show the simplified name but tooltip the full tag name
                var content = new GUIContent(displayName, fullTag);
                EditorGUILayout.LabelField(content);

                if (GUILayout.Button("Remove", GUILayout.Width(BUTTON_WIDTH)))
                {
                    RemoveTag(index);
                }
            }
        }

        private bool TagExists(string tag)
        {
            return _builtInTagsSet.Contains(tag) || InternalEditorUtility.tags.Contains(tag);
        }

        private void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || TagExists(tag))
                return;

            Undo.RecordObject(_tagManager.targetObject, "Add Tag");
            _tagsProp.InsertArrayElementAtIndex(_tagsProp.arraySize);
            var newTag = _tagsProp.GetArrayElementAtIndex(_tagsProp.arraySize - 1);
            newTag.stringValue = tag;
            _tagManager.ApplyModifiedProperties();
        }

        private void RemoveTag(int index)
        {
            Undo.RecordObject(_tagManager.targetObject, "Remove Tag");
            _tagsProp.DeleteArrayElementAtIndex(index);
            _tagManager.ApplyModifiedProperties();
        }
    }

    public class TagGroup
    {
        public string Prefix { get; set; }
        public List<(string tag, int index)> Tags { get; } = new List<(string tag, int index)>();
    }
}
#endif
