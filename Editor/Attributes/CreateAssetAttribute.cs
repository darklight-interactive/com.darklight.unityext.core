using UnityEngine;

namespace Darklight.UnityExt.Editor
{
    using UnityEngine;

    public class CreateAssetAttribute : PropertyAttribute
    {
        public string defaultName;
        public string defaultPath;
        public string defaultShader; // Only used for Materials

        public CreateAssetAttribute(
            string defaultName = "NewAsset",
            string defaultPath = "Assets/Resources",
            string defaultShader = "Standard"
        )
        {
            this.defaultName = defaultName;
            this.defaultPath = defaultPath;
            this.defaultShader = defaultShader;
        }
    }
}
