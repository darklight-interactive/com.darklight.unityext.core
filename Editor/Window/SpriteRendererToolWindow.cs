using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public class SpriteRendererToolWindow : EditorWindow
    {
        const string TITLE = "Sprite Renderer Tool";

        static SpriteRendererToolWindow _window;

        Material _materialToApply;
        bool _includeChildren = true;
        int _spriteRendererCount = 0;

        [MenuItem(EditorPath.MENU_ROOT + TITLE)]
        public static void ShowWindow()
        {
            _window = GetWindow<SpriteRendererToolWindow>(TITLE);
            _window.titleContent = new GUIContent(TITLE);
            _window.minSize = new Vector2(300, 140);
            _window.Show();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            _spriteRendererCount = CountSpriteRenderersInSelection();
            Repaint();
        }

        private int CountSpriteRenderersInSelection()
        {
            int count = 0;
            foreach (GameObject go in Selection.gameObjects)
            {
                count += _includeChildren
                    ? go.GetComponentsInChildren<SpriteRenderer>(true).Length
                    : go.GetComponents<SpriteRenderer>().Length;
            }
            return count;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Apply Material To Sprite Renderers", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _materialToApply = (Material)EditorGUILayout.ObjectField(
                "Material",
                _materialToApply,
                typeof(Material),
                false
            );

            EditorGUI.BeginChangeCheck();
            _includeChildren = EditorGUILayout.Toggle("Include Children", _includeChildren);
            if (EditorGUI.EndChangeCheck())
            {
                _spriteRendererCount = CountSpriteRenderersInSelection();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sprite Renderers Found", _spriteRendererCount.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(_materialToApply == null || Selection.gameObjects.Length == 0))
            {
                if (GUILayout.Button("Apply To Selected"))
                {
                    ApplyMaterialToSelectedSpriteRenderers();
                }
            }

            EditorGUILayout.HelpBox(
                "Select one or more GameObjects, assign a material, then click Apply To Selected.",
                MessageType.Info
            );
        }

        private void ApplyMaterialToSelectedSpriteRenderers()
        {
            int updatedCount = 0;

            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                SpriteRenderer[] spriteRenderers = _includeChildren
                    ? selectedObject.GetComponentsInChildren<SpriteRenderer>(true)
                    : selectedObject.GetComponents<SpriteRenderer>();

                foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                {
                    Undo.RecordObject(spriteRenderer, "Apply Sprite Renderer Material");
                    spriteRenderer.sharedMaterial = _materialToApply;
                    EditorUtility.SetDirty(spriteRenderer);

                    updatedCount++;
                }
            }

            Debug.Log($"Applied material '{_materialToApply.name}' to {updatedCount} SpriteRenderer component(s).");
        }
    }
}
