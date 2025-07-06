using UnityEngine;

namespace Darklight.Editor
{
    using UnityEngine;

    public class CreateAssetAttribute : PropertyAttribute
    {
        public string defaultName;
        public string defaultPath;
        public string defaultShader; // Only used for Materials

        public CreateAssetAttribute(
            string fileName = "NewAsset",
            string defaultPath = "Assets/Resources"
        )
        {
            this.defaultName = fileName;
            this.defaultPath = defaultPath;
        }

        public CreateAssetAttribute(
            string fileName = "NewAsset",
            string defaultPath = "Assets/Resources",
            string defaultShader = "Standard"
        )
            : this(fileName, defaultPath)
        {
            this.defaultShader = defaultShader;
        }
    }
}
