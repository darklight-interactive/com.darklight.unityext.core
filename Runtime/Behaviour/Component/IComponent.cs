using System;
using System.Collections.Generic;

namespace Darklight.Behaviour.Interface
{
    public interface IComponent<T>
    {
        /// <summary>
        /// Initialize the component.
        /// </summary>
        /// <remarks>
        /// This method should be called when the component is first attached to the base object
        /// or when the component is reset.
        /// </remarks>
        /// <param name="baseComponent">
        ///     The base object that the component is attached to.
        /// </param>
        void OnInitialize(T baseComponent);

        /// <summary>
        /// Update the component.
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// Draw gizmos for the component.
        /// </summary>
        void DrawGizmos();

        /// <summary>
        /// Draw editor gizmos for the component.
        /// </summary>
        void DrawEditorGizmos();
    }

    public interface IComponent<T, TKey> : IComponent<T>
    {
        /// <summary>
        /// Get the type key of the component.
        /// </summary>
        TKey GetTypeKey();
    }
}
