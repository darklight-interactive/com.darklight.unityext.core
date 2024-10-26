using Darklight.UnityExt.Editor.Utility;

using UnityEditor;

using UnityEngine;

namespace Darklight.UnityExt.Editor
{
    public class ShowOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
    public class ShowOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (prop.propertyType == SerializedPropertyType.Generic && IsSerializableClass(prop) && prop.isExpanded)
            {
                height += CalculateShowOnlyClassHeight(prop);
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, prop);

            if (prop.isArray && prop.propertyType == SerializedPropertyType.Generic)
            {
                CustomInspectorGUI.DrawShowOnlyListFoldout(prop, label.text);
            }
            else if (prop.propertyType == SerializedPropertyType.Generic && IsSerializableClass(prop))
            {
                CustomInspectorGUI.DrawShowOnlySerializableClass(position, prop, label);
            }
            else
            {
                string valueString = SerializedPropertyUtility.ConvertPropertyToString(prop);
                EditorGUI.LabelField(position, label.text, valueString);
            }

            EditorGUI.EndProperty();
        }

        private bool IsSerializableClass(SerializedProperty prop)
        {
            return prop.hasVisibleChildren && prop.depth == 0; // Checks for complex types
        }

        private float CalculateShowOnlyClassHeight(SerializedProperty prop)
        {
            float totalHeight = 0f;

            SerializedProperty childProp = prop.Copy();
            bool enterChildren = true;

            while (childProp.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (childProp.depth == 0)
                {
                    break; // Exit when reaching next sibling property
                }

                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return totalHeight;
        }

    }
#endif
}
