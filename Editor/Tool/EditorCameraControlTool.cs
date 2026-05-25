#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Darklight.Editor.Window
{
    using Darklight.Editor.Utility;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom Editor Window to snap the Scene view camera to predefined world axes.
    /// </summary>
    public class EditorCameraControlTool : EditorWindow
    {
        [MenuItem(EditorPath.MENU_WINDOW + "Editor Camera Control")]
        public static void ShowWindow()
        {
            GetWindow<EditorCameraControlTool>("Editor Camera Control");
        }

        private void OnGUI()
        {
            GUILayout.Label("Snap Camera to Axis", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Top"))
                SceneViewUtility.SnapViewToDirection(SceneViewUtility.ViewDirection.TOP);
            if (GUILayout.Button("Bottom"))
                SceneViewUtility.SnapViewToDirection(SceneViewUtility.ViewDirection.BOTTOM);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Left"))
                SceneViewUtility.SnapViewToDirection(SceneViewUtility.ViewDirection.LEFT);
            if (GUILayout.Button("Right"))
                SceneViewUtility.SnapViewToDirection(SceneViewUtility.ViewDirection.RIGHT);
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
