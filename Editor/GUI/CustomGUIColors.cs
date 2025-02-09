using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Editor
{
    /// <summary>
    /// Provides extended functionality for working with Unity's Color struct.
    /// </summary>
    public static class CustomGUIColors
    {
        public enum CommonType
        {
            SUCCESS,
            WARNING,
            ERROR,
            INFO,
            DEBUG,
            DISABLED,
            SELECTED,
            HIGHLIGHT,
            X_AXIS,
            Y_AXIS,
            Z_AXIS
        }

        #region < FIELDS > [[ Common Colors ]] =====================================================================

        public static Color transparent = new Color(0, 0, 0, 0);
        public static Color white = new Color(1, 1, 1, 1);
        public static Color black = new Color(0, 0, 0, 1);
        public static Color red = new Color(1, 0, 0, 1);
        public static Color green = new Color(0, 1, 0, 1);
        public static Color blue = new Color(0, 0, 1, 1);
        public static Color yellow = new Color(1, 1, 0, 1);
        public static Color cyan = new Color(0, 1, 1, 1);
        public static Color magenta = new Color(1, 0, 1, 1);
        public static Color orange = new Color(1, 0.5f, 0, 1);
        public static Color purple = new Color(0.5f, 0, 1, 1);
        public static Color pink = new Color(1, 0.5f, 0.5f, 1);
        public static Color brown = new Color(0.5f, 0.25f, 0, 1);
        public static Color gray = new Color(0.5f, 0.5f, 0.5f, 1);
        public static Color lightGray = new Color(0.75f, 0.75f, 0.75f, 1);
        public static Color darkGray = new Color(0.25f, 0.25f, 0.25f, 1);

        public static Color success => Settings.Instance.successColor;
        public static Color warning => Settings.Instance.warningColor;
        public static Color error => Settings.Instance.errorColor;

        public static Color info => Settings.Instance.infoColor;
        public static Color debug => Settings.Instance.debugColor;
        public static Color disabled => Settings.Instance.disabledColor;

        public static Color selected => Settings.Instance.selectedColor;
        public static Color highlight => Settings.Instance.highlightColor;
        public static Color xAxis => Settings.Instance.xAxisColor;

        public static Color yAxis => Settings.Instance.yAxisColor;
        public static Color zAxis => Settings.Instance.zAxisColor;

        #endregion

        /// <summary>
        /// Attempts to parse a hex color string into a Color.
        /// </summary>
        /// <param name="hex_code">Hex color string (e.g., "#FF0000FF" or "FF0000FF")</param>
        /// <returns>Color if valid hex string, null if invalid</returns>

        /// <remarks>
        /// Hex string must be 8 characters (RRGGBBAA) or 6 characters (RRGGBB).
        /// The '#' prefix is optional.
        /// </remarks>
        public static void GetColor(string hex, out Color color)
        {
            color = transparent;
            if (string.IsNullOrEmpty(hex))
                return;

            // Remove # prefix if present
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            // Validate length (6 for RGB or 8 for RGBA)
            if (hex.Length != 6 && hex.Length != 8)
                return;

            // Validate hex characters
            foreach (char c in hex)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    return;
            }

            // Add alpha channel if not present
            if (hex.Length == 6)
                hex += "FF";

            UnityEngine.ColorUtility.TryParseHtmlString("#" + hex, out color);
        }

        #region < PUBLIC_METHODS > [[ Color Access ]] ============================================================

        /// <summary>
        /// Creates a color with modified alpha.
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Creates a darker version of the color.
        /// </summary>
        public static Color Darken(this Color color, float amount = 0.2f)
        {
            return new Color(
                Mathf.Max(0, color.r - amount),
                Mathf.Max(0, color.g - amount),
                Mathf.Max(0, color.b - amount),
                color.a
            );
        }

        /// <summary>
        /// Creates a lighter version of the color.
        /// </summary>
        public static Color Lighten(this Color color, float amount = 0.2f)
        {
            return new Color(
                Mathf.Min(1, color.r + amount),
                Mathf.Min(1, color.g + amount),
                Mathf.Min(1, color.b + amount),
                color.a
            );
        }

        #endregion

        #region < PUBLIC_METHODS > [[ Editor Colors ]] ==========================================================

        /// <summary>
        /// Gets a semi-transparent version of the color suitable for UI backgrounds.
        /// </summary>
        public static Color GetEditorBackground(this Color color)
        {
            return color.WithAlpha(0.3f);
        }

        /// <summary>
        /// Gets a color suitable for UI headers.
        /// </summary>
        public static Color GetEditorHeader(this Color color)
        {
            return color.Darken(0.1f).WithAlpha(0.5f);
        }

        #endregion

        #region < PUBLIC_METHODS > [[ Color Blending ]] ========================================================

        /// <summary>
        /// Blends two colors using normal alpha blending.
        /// </summary>
        public static Color Blend(Color background, Color foreground)
        {
            float alpha = foreground.a;
            return new Color(
                background.r * (1 - alpha) + foreground.r * alpha,
                background.g * (1 - alpha) + foreground.g * alpha,
                background.b * (1 - alpha) + foreground.b * alpha,
                Mathf.Max(background.a, foreground.a)
            );
        }

        /// <summary>
        /// Creates a gradient between two colors with a specified number of steps.
        /// </summary>
        public static Color[] CreateGradient(Color start, Color end, int steps)
        {
            Color[] gradient = new Color[steps];
            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)(steps - 1);
                gradient[i] = Color.Lerp(start, end, t);
            }
            return gradient;
        }

        #endregion

        public static void CreateColorWithAlpha(Color color, float alpha, out Color newColor)
        {
            newColor = new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// ScriptableObject to store color palette settings
        /// </summary>
        public class Settings : ScriptableObject
        {
            #region < FIELDS > [[ Common Colors ]] =====================================================================

            public Color successColor = new Color(0.3f, 0.8f, 0.3f, 1f);
            public Color warningColor = new Color(0.8f, 0.8f, 0.3f, 1f);
            public Color errorColor = new Color(0.8f, 0.3f, 0.3f, 1f);
            public Color infoColor = new Color(0.3f, 0.3f, 0.8f, 1f);
            public Color debugColor = new Color(0.8f, 0.3f, 0.8f, 1f);
            public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            public Color selectedColor = new Color(1f, 0.92f, 0.016f, 1f);
            public Color highlightColor = new Color(0.3f, 0.8f, 0.8f, 1f);
            public Color xAxisColor = new Color(1f, 0f, 0f, 1f);
            public Color yAxisColor = new Color(0f, 1f, 0f, 1f);
            public Color zAxisColor = new Color(0f, 0f, 1f, 1f);

            #endregion

            private static Settings instance;
            public static Settings Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = Resources.Load<Settings>("DarklightColorSettings");
                        if (instance == null)
                        {
                            instance = CreateInstance<Settings>();
#if UNITY_EDITOR
                            // Ensure Resources folder exists
                            if (!System.IO.Directory.Exists("Assets/Resources"))
                                System.IO.Directory.CreateDirectory("Assets/Resources");

                            UnityEditor.AssetDatabase.CreateAsset(
                                instance,
                                "Assets/Resources/DarklightColorSettings.asset"
                            );
                            UnityEditor.AssetDatabase.SaveAssets();
#endif
                        }
                    }
                    return instance;
                }
            }
        }

#if UNITY_EDITOR
        public static class DarklightColorSettingsProvider
        {
            private const string PREFERENCES_PATH = "Preferences/Darklight/Colors";

            [SettingsProvider]
            public static SettingsProvider CreateColorSettingsProvider()
            {
                var provider = new SettingsProvider(PREFERENCES_PATH, SettingsScope.User)
                {
                    label = "Color Palette",
                    guiHandler = (searchContext) =>
                    {
                        var settings = Settings.Instance;
                        EditorGUI.indentLevel++;

                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.LabelField("Common Colors", EditorStyles.boldLabel);
                            settings.successColor = EditorGUILayout.ColorField(
                                "Success",
                                settings.successColor
                            );
                            settings.warningColor = EditorGUILayout.ColorField(
                                "Warning",
                                settings.warningColor
                            );
                            settings.errorColor = EditorGUILayout.ColorField(
                                "Error",
                                settings.errorColor
                            );
                            settings.infoColor = EditorGUILayout.ColorField(
                                "Info",
                                settings.infoColor
                            );
                            settings.debugColor = EditorGUILayout.ColorField(
                                "Debug",
                                settings.debugColor
                            );
                            settings.disabledColor = EditorGUILayout.ColorField(
                                "Disabled",
                                settings.disabledColor
                            );
                            settings.selectedColor = EditorGUILayout.ColorField(
                                "Selected",
                                settings.selectedColor
                            );
                            settings.highlightColor = EditorGUILayout.ColorField(
                                "Highlight",
                                settings.highlightColor
                            );
                        }

                        EditorGUI.indentLevel--;

                        if (GUI.changed)
                        {
                            EditorUtility.SetDirty(settings);
                        }
                    },
                    keywords = new System.Collections.Generic.HashSet<string>(
                        new[]
                        {
                            "Color",
                            "Palette",
                            "Theme",
                            "Success",
                            "Warning",
                            "Error",
                            "Info",
                            "Debug",
                            "Disabled",
                            "Selected",
                            "Highlight"
                        }
                    )
                };

                return provider;
            }
        }
#endif
    }
}
