using System;

namespace Darklight.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class InitializeOnEnableAttribute : Attribute
    {
        public string methodName = "Awake";
        public bool drawButton = false;

        public InitializeOnEnableAttribute() { }

        public InitializeOnEnableAttribute(string methodName)
        {
            this.methodName = methodName;
            this.drawButton = false;
        }

        public InitializeOnEnableAttribute(string methodName, bool drawButton)
        {
            this.methodName = methodName;
            this.drawButton = drawButton;
        }
    }
}
