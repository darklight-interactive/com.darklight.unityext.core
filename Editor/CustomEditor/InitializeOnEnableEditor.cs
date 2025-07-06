using System;
using System.Reflection;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class InitializeOnEnableEditor : NaughtyInspector
    {
        private InitializeOnEnableAttribute _initializeAttr;
        private bool _shouldUseNaughtyInspector;

        protected override void OnEnable()
        {
            var targetType = target.GetType();
            _initializeAttr = targetType.GetCustomAttribute<InitializeOnEnableAttribute>();

            // ✅ Check if NaughtyInspector is safe to run:
            var nonSerializedFields = targetType.GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );
            _shouldUseNaughtyInspector = false;

            foreach (var field in nonSerializedFields)
            {
                if (
                    field.GetCustomAttribute<NaughtyAttributes.ShowNonSerializedFieldAttribute>()
                    != null
                )
                {
                    _shouldUseNaughtyInspector = true;
                    break;
                }
            }

            if (_initializeAttr != null)
            {
                InvokeAttributeMethod();
            }
        }

        public override void OnInspectorGUI()
        {
            if (_initializeAttr != null)
            {
                if (GUILayout.Button("Initialize"))
                {
                    InvokeAttributeMethod();
                }
            }

            try
            {
                base.OnInspectorGUI(); // This will draw all NaughtyAttributes, even if there are no non-serialized fields
            }
            catch (ArgumentNullException ex)
            {
                //Debug.LogWarning("NaughtyInspector failed to draw non-serialized fields");
                //DrawDefaultInspector(); // Fallback to default inspector
            }
        }

        private void InvokeAttributeMethod()
        {
            var targetType = target.GetType();
            MethodInfo method = targetType.GetMethod(
                _initializeAttr.methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (method != null)
            {
                method.Invoke(target, null);
            }
            else
            {
                Debug.LogWarning(
                    $"Method '{_initializeAttr.methodName}' not found on {targetType.Name}"
                );
            }
        }
    }
}
