using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/* ==== EXAMPLE USAGE ====
public class DynamicRangeSlider : MonoBehaviour
{
    public Vector2 range = new Vector2(0f, 100f);

    [DynamicRange("range")]
    public float dynamicSlider;
}
*/

namespace Darklight.UnityExt.Editor
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class DynamicRangeAttribute : PropertyAttribute
    {
        public string Vector2FieldName { get; private set; }
        public bool ShowRangeValues { get; private set; }

        public DynamicRangeAttribute(string vector2FieldName, bool showRangeValues = false)
        {
            Vector2FieldName = vector2FieldName;
            ShowRangeValues = showRangeValues;
        }
    }
}
