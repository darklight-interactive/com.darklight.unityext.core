

namespace Darklight.UnityExt.Matrix.Editor
{
#if UNITY_EDITOR

    using UnityEditor;

    using UnityEngine;

    [CustomEditor(typeof(Matrix))]
    public class MatrixCustomEditor : UnityEditor.Editor
    {
        SerializedObject _serializedObject;
        Matrix _script;
        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (Matrix)target;
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                _script.Refresh();
            }
        }
    }
#endif
}