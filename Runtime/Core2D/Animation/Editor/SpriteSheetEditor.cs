#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Core2D.Animation
{
    [CustomEditor(typeof(SpriteSheet))]
    public class SpriteSheetEditor : UnityEditor.Editor
    {
        private const int PREVIEW_SIZE = 64; // Size of the sprite previews
        private SerializedProperty framesProperty;
        private Vector2 scrollPosition; // Add this field at class level
        private int selectedFrameIndex = -1; // Add this field at class level

        private void OnEnable()
        {
            // Cache the SerializedProperty for the frames array
            framesProperty = serializedObject.FindProperty("frames");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default properties
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Frames", EditorStyles.boldLabel);

            if (framesProperty != null && framesProperty.isArray)
            {
                // Add scroll view for horizontal scrolling only
                scrollPosition = EditorGUILayout.BeginScrollView(
                    scrollPosition,
                    false, // horizontal scrollbar
                    false, // vertical scrollbar
                    GUI.skin.horizontalScrollbar,
                    GUIStyle.none,
                    GUI.skin.scrollView,
                    GUILayout.Height(PREVIEW_SIZE + 20)
                );

                EditorGUILayout.BeginHorizontal();
                {
                    // Display each sprite in the frames array
                    for (int i = 0; i < framesProperty.arraySize; i++)
                    {
                        SerializedProperty spriteProperty = framesProperty.GetArrayElementAtIndex(
                            i
                        );
                        DrawSprite(spriteProperty, i);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();

                // Add/Remove buttons below the scroll view
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Add Frame"))
                        AddFrame();

                    if (GUILayout.Button("Remove Frame"))
                        RemoveFrame();
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawSprite(SerializedProperty spriteProperty, int index)
        {
            bool isSelected = selectedFrameIndex == index;
            GUI.backgroundColor = isSelected ? Color.cyan : Color.white;

            using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(PREVIEW_SIZE)))
            {
                if (
                    GUILayout.Button(
                        "",
                        GUILayout.Width(PREVIEW_SIZE),
                        GUILayout.Height(PREVIEW_SIZE)
                    )
                )
                {
                    selectedFrameIndex = isSelected ? -1 : index;
                }

                EditorGUI.ObjectField(
                    GUILayoutUtility.GetLastRect(),
                    spriteProperty.objectReferenceValue as Sprite,
                    typeof(Sprite),
                    false
                );
            }

            GUI.backgroundColor = Color.white;
        }

        void AddFrame()
        {
            int insertIndex =
                selectedFrameIndex >= 0 ? selectedFrameIndex + 1 : framesProperty.arraySize;
            framesProperty.InsertArrayElementAtIndex(insertIndex);
            selectedFrameIndex = insertIndex;
        }

        void RemoveFrame()
        {
            if (framesProperty.arraySize == 0)
                return;

            int removeIndex =
                selectedFrameIndex >= 0 ? selectedFrameIndex : framesProperty.arraySize - 1;
            framesProperty.DeleteArrayElementAtIndex(removeIndex);
            selectedFrameIndex = -1;
        }
    }
}
#endif
