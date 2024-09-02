using System;

namespace Darklight.UnityExt.Behaviour.Interface
{
    public interface IComponent<TBase, TTag>
        where TTag : Enum
    {
        /// <summary>
        /// Initialize the component.
        /// </summary>
        /// <remarks>
        /// This method should be called when the component is first attached to the base object
        /// or when the component is reset.
        /// </remarks>
        /// <param name="baseObj">
        ///     The base object that the component is attached to.
        /// </param>
        void Initialize(TBase baseObj);

        /// <summary>
        /// Update the component.
        /// </summary>
        void Updater();

        /// <summary>
        /// Draw gizmos for the component.
        /// </summary>
        void DrawGizmos();

        /// <summary>
        /// Draw editor gizmos for the component.
        /// </summary>
        void DrawEditorGizmos();

        /// <summary>
        /// Get the type key for the component.
        TTag GetTypeKey();
    }
}