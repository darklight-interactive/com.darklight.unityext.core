using System;

namespace Darklight.UnityExt.Behaviour.Interface
{
    public interface IComponent<TBase, TTag> where TTag : Enum
    {
        int Guid { get; }
        TBase Base { get; }
        TTag Tag { get; }

        void Initialize();
        void Update();

        void DrawGizmos();
        void DrawEditorGizmos();
    }
}