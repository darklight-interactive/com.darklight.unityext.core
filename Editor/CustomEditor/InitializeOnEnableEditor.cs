using System.Reflection;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class InitializeOnEnableEditor : NaughtyInspector
    {
        private InitializeOnEnableAttribute _initializeAttr;

        protected override void OnEnable()
        {
            var targetType = target.GetType();
            _initializeAttr = targetType.GetCustomAttribute<InitializeOnEnableAttribute>();

            if (_initializeAttr != null)
            {
                InvokeAttributeMethod();
            }
        }

        public override void OnInspectorGUI()
        {
            // Only if the target class has the attribute
            if (_initializeAttr != null)
            {
                if (GUILayout.Button("Initialize"))
                {
                    InvokeAttributeMethod();
                }
            }

            base.OnInspectorGUI();
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
