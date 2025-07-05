using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Core2D.Animation
{
    /// <summary>
    /// ScriptableObject that holds a collection of sprites for frame animation
    /// </summary>
    [CreateAssetMenu(fileName = "New SpriteSheet", menuName = "Darklight/Animation/SpriteSheet")]
    public class SpriteSheet : ScriptableObject
    {
        [Range(1, 24)]
        public int frameRate = 4;
        public bool loop = true;
        public Spatial2D.Direction startDirection = Spatial2D.Direction.NONE;

        [Tooltip("Collection of sprites to animate")]
        public Sprite[] frames = new Sprite[0];

        /// <summary>
        /// Number of frames in the animation
        /// </summary>
        public int Length => frames?.Length ?? 0;

        /// <summary>
        /// Get the sprite at a specific frame index
        /// </summary>
        /// <param name="index">The frame index to retrieve</param>
        /// <returns>The sprite at the specified index, or null if invalid</returns>
        public Sprite GetSpriteAtFrame(int index)
        {
            if (frames != null && index >= 0 && index < frames.Length)
            {
                return frames[index];
            }
            return null;
        }

        public void CalculateFlipToDirection(
            Spatial2D.Direction targetDirection,
            out bool flipX,
            out bool flipY
        )
        {
            // Default to no flipping
            flipX = false;
            flipY = false;

            // If no start direction is set, assume EAST as default
            if (startDirection == Spatial2D.Direction.NONE)
                startDirection = Spatial2D.Direction.EAST;

            // Determine if we need to flip based on start direction
            switch (startDirection)
            {
                case Spatial2D.Direction.EAST:
                    // Flip for any direction facing left
                    flipX =
                        targetDirection == Spatial2D.Direction.WEST
                        || targetDirection == Spatial2D.Direction.NORTHWEST
                        || targetDirection == Spatial2D.Direction.SOUTHWEST;
                    break;

                case Spatial2D.Direction.WEST:
                    // Flip for any direction facing right
                    flipX =
                        targetDirection == Spatial2D.Direction.EAST
                        || targetDirection == Spatial2D.Direction.NORTHEAST
                        || targetDirection == Spatial2D.Direction.SOUTHEAST;
                    break;

                case Spatial2D.Direction.NORTH:
                    // Flip for any direction facing left
                    flipX =
                        targetDirection == Spatial2D.Direction.WEST
                        || targetDirection == Spatial2D.Direction.NORTHWEST
                        || targetDirection == Spatial2D.Direction.SOUTHWEST;
                    break;

                case Spatial2D.Direction.SOUTH:
                    // Flip for any direction facing left
                    flipX =
                        targetDirection == Spatial2D.Direction.WEST
                        || targetDirection == Spatial2D.Direction.NORTHWEST
                        || targetDirection == Spatial2D.Direction.SOUTHWEST;
                    break;
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(SpriteSheet))]
        public class SpriteSheetPropertyDrawer : PropertyDrawer
        {
            private const string LAST_DIRECTORY_PREF = "SpriteSheet_LastSaveDirectory";

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                float buttonWidth = 60f;
                float spacing = 5f;

                // Calculate rects
                Rect propertyRect = new Rect(
                    position.x,
                    position.y,
                    position.width
                        - (property.objectReferenceValue == null ? buttonWidth + spacing : 0),
                    position.height
                );

                Rect buttonRect = new Rect(
                    position.x + position.width - buttonWidth,
                    position.y,
                    buttonWidth,
                    position.height
                );

                // Draw the property field
                EditorGUI.PropertyField(propertyRect, property, label);

                // If the property is null, show the create button
                if (property.objectReferenceValue == null)
                {
                    if (GUI.Button(buttonRect, "Create"))
                    {
                        // Get the last used directory, defaulting to the Assets folder
                        string initialPath = EditorPrefs.GetString(LAST_DIRECTORY_PREF, "Assets");

                        string path = EditorUtility.SaveFilePanelInProject(
                            "Create Sprite Sheet",
                            "NewSpriteSheet",
                            "asset",
                            "Create a new sprite sheet asset",
                            initialPath
                        );

                        if (!string.IsNullOrEmpty(path))
                        {
                            // Save the directory for next time
                            string directory = System.IO.Path.GetDirectoryName(path);
                            EditorPrefs.SetString(LAST_DIRECTORY_PREF, directory);

                            var newSheet = CreateInstance<SpriteSheet>();
                            AssetDatabase.CreateAsset(newSheet, path);
                            AssetDatabase.SaveAssets();
                            property.objectReferenceValue = newSheet;
                            EditorUtility.SetDirty(property.serializedObject.targetObject);
                        }
                    }
                }
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
        }

        /// <summary>
        /// Creates a new SpriteSheet instance from a collection of sprites
        /// </summary>
        /// <param name="sprites">The sprites to include in the animation</param>
        /// <param name="loop">Whether the animation should loop</param>
        /// <returns>The created SpriteSheet asset</returns>
        public static SpriteSheet CreateInstance(Sprite[] sprites, bool loop = true)
        {
            var spriteSheet = CreateInstance<SpriteSheet>();
            spriteSheet.frames = sprites;
            spriteSheet.loop = loop;
            return spriteSheet;
        }

        /// <summary>
        /// Creates and saves a new SpriteSheet asset to the specified path
        /// </summary>
        /// <param name="path">The asset path where to save the SpriteSheet</param>
        /// <param name="sprites">The sprites to include in the animation</param>
        /// <param name="loop">Whether the animation should loop</param>
        /// <returns>The created SpriteSheet asset</returns>
        public static SpriteSheet CreateAsset(string path, Sprite[] sprites, bool loop = true)
        {
            var spriteSheet = CreateInstance(sprites, loop);
            AssetDatabase.CreateAsset(spriteSheet, path);
            AssetDatabase.SaveAssets();
            return spriteSheet;
        }
#endif
    }
}
