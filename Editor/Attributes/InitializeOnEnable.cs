using System;

namespace Darklight.UnityExt.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class InitializeOnEnableAttribute : Attribute
    {
        public bool drawButton;
        public string methodName;

        public InitializeOnEnableAttribute(bool drawButton = false, string methodName = "Awake")
        {
            this.drawButton = drawButton;
            this.methodName = methodName;
        }
    }
}
