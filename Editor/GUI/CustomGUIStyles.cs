using UnityEngine;

namespace Darklight.Editor
{
    public class CustomGUIStyles
    {
        public static GUIStyle TitleHeaderStyle =>
            new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                fixedHeight = 40
            };

        public static GUIStyle Header1Style =>
            new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                fixedHeight = 40
            };

        public static GUIStyle Header2Style =>
            new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                fixedHeight = 40
            };

        public static GUIStyle LeftAlignedStyle =>
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };

        public static GUIStyle CenteredStyle =>
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

        public static GUIStyle RightAlignedStyle =>
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };

        public static GUIStyle BoldStyle =>
            new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold };

        public static GUIStyle BoldCenteredStyle =>
            new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

        public static GUIStyle SmallTextStyle => new GUIStyle(GUI.skin.label) { fontSize = 10 };

        public static GUIStyle NormalTextStyle => new GUIStyle(GUI.skin.label) { fontSize = 12 };

        public static GUIStyle CreateStyle(
            int fontSize,
            Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter,
            FontStyle fontStyle = FontStyle.Normal
        )
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize;
            style.alignment = alignment;
            style.fontStyle = fontStyle;
            style.normal.textColor = color;
            return style;
        }
    }
}
