using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Editor
{
    /// <summary>
    /// A serializable wrapper class for scene assets. This enables the use of scene assets
    /// within the Unity inspector, allowing developers to associate scenes with properties
    /// and manage them easily.
    /// </summary>
    [System.Serializable]
    public class SceneObject : System.IEquatable<SceneObject>
    {
        [SerializeField]
        private string m_SceneName;

        public static implicit operator string(SceneObject sceneObject)
        {
            return sceneObject == null ? null : sceneObject.m_SceneName;
        }

        public static implicit operator SceneObject(string sceneName)
        {
            return new SceneObject() { m_SceneName = sceneName };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SceneObject);
        }

        public bool Equals(SceneObject other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return m_SceneName == other.m_SceneName;
        }

        public override int GetHashCode()
        {
            return (m_SceneName != null ? m_SceneName.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return m_SceneName;
        }

        public static bool operator ==(SceneObject left, SceneObject right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SceneObject left, SceneObject right)
        {
            return !Equals(left, right);
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneObject))]
    public class SceneObjectEditor : PropertyDrawer
    {
        protected SceneAsset GetSceneObject(string sceneObjectName)
        {
            if (string.IsNullOrEmpty(sceneObjectName))
                return null;

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                if (scene.path.IndexOf(sceneObjectName) != -1)
                {
                    return AssetDatabase.LoadAssetAtPath(scene.path, typeof(SceneAsset))
                        as SceneAsset;
                }
            }

            Debug.Log(
                "Scene ["
                    + sceneObjectName
                    + "] cannot be used. Add this scene to the 'Scenes in the Build' in the build settings."
            );
            return null;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneObj = GetSceneObject(property.FindPropertyRelative("m_SceneName").stringValue);
            var newScene = EditorGUI.ObjectField(
                position,
                label,
                sceneObj,
                typeof(SceneAsset),
                false
            );
            if (newScene == null)
            {
                var prop = property.FindPropertyRelative("m_SceneName");
                prop.stringValue = "";
            }
            else
            {
                if (newScene.name != property.FindPropertyRelative("m_SceneName").stringValue)
                {
                    var scnObj = GetSceneObject(newScene.name);
                    if (scnObj == null)
                    {
                        Debug.LogWarning(
                            "The scene "
                                + newScene.name
                                + " cannot be used. To use this scene add it to the build settings for the project."
                        );
                    }
                    else
                    {
                        var prop = property.FindPropertyRelative("m_SceneName");
                        prop.stringValue = newScene.name;
                    }
                }
            }
        }
    }
#endif
}
