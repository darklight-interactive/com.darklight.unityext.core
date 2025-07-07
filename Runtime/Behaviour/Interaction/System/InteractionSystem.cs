using System.Collections.Generic;
using Darklight.Behaviour;
using Darklight.Collections;
using Darklight.Editor;
using Darklight.Utility;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Behaviour
{
    [ExecuteAlways]
    public abstract partial class InteractionSystem<TData, TType>
        : MonoBehaviourSingleton<InteractionSystem<TData, TType>>,
            IUnityEditorListener
        where TData : InteractableData
        where TType : System.Enum
    {
        [SerializeField, Expandable]
        [CreateAsset(
            "NewInteractionSystemSettings",
            "Assets/Resources/Darklight/Interaction/SystemSettings"
        )]
        InteractionSystemSettings _settings;

        [SerializeField]
        InteractionSystem_Registry _registry = new();

        [HorizontalLine(4, color: EColor.Gray)]
        [SerializeField]
        CollectionDictionary<TType, GameObject> _recieverLibrary = new();

        public InteractionSystemSettings Settings => _settings;
        public InteractionSystem_Registry Registry => _registry;
        public CollectionDictionary<TType, GameObject> RecieverLibrary => _recieverLibrary;

        public override void OnEditorReloaded()
        {
            base.OnEditorReloaded();
            _registry.Reset();
        }

        [Button]
        protected override void Initialize()
        {
            base.Initialize();

            // Confirm Settings are loaded
            if (_settings == null)
                _settings = Factory.CreateSettings();

            // Confirm Registry is loaded
            if (_registry == null)
                _registry = new InteractionSystem_Registry();
        }

        public static void Invoke(IInteractionCommand command)
        {
            Invoker.ExecuteCommand(command);
        }
    }
}
