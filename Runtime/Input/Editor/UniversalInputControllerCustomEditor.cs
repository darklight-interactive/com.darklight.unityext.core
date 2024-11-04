using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Input.Editor
{
    [CustomEditor(typeof(UniversalInputController), true)]
    public class UniversalInputControllerCustomEditor : UnityEditor.Editor
    {
        protected UniversalInputController controller;

        public override void OnInspectorGUI()
        {
            controller = (UniversalInputController)target;
            using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
            {
                DrawHeaderButtons();
            }

            EditorGUILayout.Space(10);
            DrawDefaultInspector();
        }

        protected virtual void DrawHeaderButtons()
        {
            if (GUILayout.Button("Open Input Debug Window"))
            {
                InputDebugWindow.ShowWindow(controller);
            }
        }

        #region < PRIVATE_NESTED_CLASS > [[ Debug Window ]] ================================================================
        private class InputDebugWindow : EditorWindow
        {
            private UniversalInputController _target;
            private Vector2 _scrollPosition;
            private GUIStyle _boxStyle;
            private GUIStyle _labelStyle;
            private GUIStyle _headerStyle;
            private GUIStyle _activeStyle;
            private GUIStyle _inactiveStyle;

            public static void ShowWindow(UniversalInputController target)
            {
                InputDebugWindow window = GetWindow<InputDebugWindow>("Input Debug");
                window._target = target;
                window.minSize = new Vector2(300, 400);
            }

            private void InitStyles()
            {
                if (_boxStyle == null)
                {
                    _boxStyle = new GUIStyle(GUI.skin.box)
                    {
                        normal = { background = MakeTexture(new Color(0f, 0f, 0f, 0.7f)) },
                        padding = new RectOffset(10, 10, 10, 10)
                    };
                }

                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle(EditorStyles.label)
                    {
                        normal = { textColor = Color.white },
                        fontSize = 12
                    };
                }

                if (_headerStyle == null)
                {
                    _headerStyle = new GUIStyle(_labelStyle)
                    {
                        fontSize = 14,
                        fontStyle = FontStyle.Bold
                    };
                }

                if (_activeStyle == null)
                {
                    _activeStyle = new GUIStyle(_labelStyle)
                    {
                        normal = { textColor = Color.green }
                    };
                }

                if (_inactiveStyle == null)
                {
                    _inactiveStyle = new GUIStyle(_labelStyle)
                    {
                        normal = { textColor = Color.red }
                    };
                }
            }

            private Texture2D MakeTexture(Color color)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, color);
                tex.Apply();
                return tex;
            }

            private void OnGUI()
            {
                if (_target == null || !Application.isPlaying)
                {
                    return;
                }

                InitStyles();

                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                EditorGUILayout.BeginVertical(_boxStyle);

                // Header
                EditorGUILayout.LabelField("Input Debug", _headerStyle);
                EditorGUILayout.Space(5);

                // Move Input
                EditorGUILayout.LabelField("Move Input", _headerStyle);
                EditorGUILayout.LabelField(
                    $"Value: ({_target.MoveInput.x:F2}, {_target.MoveInput.y:F2})",
                    _labelStyle
                );
                EditorGUILayout.LabelField(
                    "Active:",
                    _target.IsMoveInputActive ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Started:",
                    _target.IsMoveInputStarted ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Canceled:",
                    _target.IsMoveInputCanceled ? _activeStyle : _inactiveStyle
                );

                EditorGUILayout.Space(10);

                // Primary Interact
                EditorGUILayout.LabelField("Primary Interact", _headerStyle);
                EditorGUILayout.LabelField(
                    "Started:",
                    _target.IsPrimaryInteractStarted ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Canceled:",
                    _target.IsPrimaryInteractCanceled ? _activeStyle : _inactiveStyle
                );

                EditorGUILayout.Space(10);

                // Secondary Interact
                EditorGUILayout.LabelField("Secondary Interact", _headerStyle);
                EditorGUILayout.LabelField(
                    "Started:",
                    _target.IsSecondaryInteractStarted ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Canceled:",
                    _target.IsSecondaryInteractCanceled ? _activeStyle : _inactiveStyle
                );

                EditorGUILayout.Space(10);

                // Menu Button
                EditorGUILayout.LabelField("Menu Button", _headerStyle);
                EditorGUILayout.LabelField(
                    "Started:",
                    _target.IsMenuButtonStarted ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Canceled:",
                    _target.IsMenuButtonCanceled ? _activeStyle : _inactiveStyle
                );

                EditorGUILayout.Space(10);

                // Event Listeners
                EditorGUILayout.LabelField("Event Listeners", _headerStyle);
                EditorGUILayout.LabelField(
                    "Move Input Started:",
                    HasListeners(_target.OnMoveInputStarted) ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Move Input:",
                    HasListeners(_target.OnMoveInput) ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Move Input Canceled:",
                    HasListeners(_target.OnMoveInputCanceled) ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Primary Interact Started:",
                    HasListeners(_target.OnPrimaryInteractStarted) ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Primary Interact Canceled:",
                    HasListeners(_target.OnPrimaryInteractCanceled) ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Secondary Interact Started:",
                    HasListeners(_target.OnSecondaryInteractStarted) ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Secondary Interact Canceled:",
                    HasListeners(_target.OnSecondaryInteractCanceled)
                        ? _activeStyle
                        : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Menu Button Started:",
                    HasListeners(_target.OnMenuButtonStarted) ? _activeStyle : _inactiveStyle
                );
                EditorGUILayout.LabelField(
                    "Menu Button Canceled:",
                    HasListeners(_target.OnMenuButtonCanceled) ? _activeStyle : _inactiveStyle
                );

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();

                // Repaint every frame while in play mode
                if (Application.isPlaying)
                {
                    Repaint();
                }
            }

            private bool HasListeners(System.Action action)
            {
                return action != null && action.GetInvocationList().Length > 0;
            }

            private bool HasListeners<T>(System.Action<T> action)
            {
                return action != null && action.GetInvocationList().Length > 0;
            }
        }
        #endregion
    }
}
