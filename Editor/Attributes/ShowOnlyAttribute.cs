using UnityEngine;
using UnityEditor;

namespace Darklight.UnityExt.Editor
{
    public class ShowOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
    public class ShowOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, prop);

            if (prop.isArray && prop.propertyType == SerializedPropertyType.Generic)
            {
                CustomInspectorGUI.DrawReadOnlyListFoldout(prop, label.text);
            }
            else if (prop.propertyType == SerializedPropertyType.Generic && IsSerializableClass(prop))
            {
                CustomInspectorGUI.DrawSerializableClass(position, prop, label);
            }
            else
            {
                string valueString = CustomInspectorGUI.ConvertElementToString(prop);
                EditorGUI.LabelField(position, label.text, valueString);
            }

            EditorGUI.EndProperty();
        }

        private bool IsSerializableClass(SerializedProperty prop)
        {
            return prop.hasVisibleChildren && prop.depth == 0; // Checks for complex types
        }
    }
#endif
}
