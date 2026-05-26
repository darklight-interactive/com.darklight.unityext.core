using UnityEngine;

namespace Darklight.Project
{
    public abstract class ProjectSettings : ScriptableObject
    {
        public abstract Vector3 CameraDefaultRotation { get; }
    }
}