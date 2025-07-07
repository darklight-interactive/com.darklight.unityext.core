using System;
using System.Collections.Generic;
using Darklight.Behaviour;
using Darklight.Collections;
using Darklight.Editor;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using NaughtyAttributes.Editor;
#endif

namespace Darklight.Behaviour
{
    /// <summary>
    /// This is the interface for the interactable class. This is used to
    /// define the base properties and methods for all interactable types.
    /// </summary>
    public interface IInteractable
    {
        InteractableData Data { get; }
        Collider Collider { get; }
        Action OnAcceptTarget { get; set; }
        Action OnAcceptInteraction { get; set; }
        Action OnReset { get; set; }
        void Initialize();
        void Refresh();
        void Reset();
        bool AcceptTarget(IInteractor interactor, bool force = false);
        bool AcceptInteraction(IInteractor interactor, bool force = false);
        bool Validate(out string outLog);
        string Print();
    }

    /// <summary>
    /// This is the abstract interactable class. Having a non-generic abstract class
    /// allows for the creation of a generic interactable class that can be
    /// used for unique interactable types, but still have the same base and
    /// therefore can be referenced as an non-generic Interactable.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TStateMachine"></typeparam>
    public abstract class Interactable : MonoBehaviour, IInteractable
    {
        protected const string PREFIX = "INTRCTBL";
        protected const string DEFAULT_NAME = "DefaultName";
        protected const string DEFAULT_KEY = "DefaultKey";
        protected const string DEFAULT_LAYER = "Default";

        public abstract InteractableData Data { get; }
        public abstract Collider Collider { get; }
        public abstract Action OnAcceptTarget { get; set; }
        public abstract Action OnAcceptInteraction { get; set; }
        public abstract Action OnReset { get; set; }

        protected virtual void Start() => Initialize();

        protected virtual void Update() => Refresh();

        protected abstract void OnDrawGizmos();

        /// <summary>
        /// Initialize the interactable within the scene by
        /// storing scene specific references and data.
        /// This is called when the interactable is first created, typically in Start()
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Refresh the interactable to update its state. <br/>
        /// This is called every frame, typically in Update()
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Reset the interactable to its default state & values
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Validate the interactable to ensure it is properly configured
        /// </summary>
        /// <returns>True if the interactable is valid, false otherwise</returns>
        public abstract bool Validate(out string outLog);
        public abstract bool AcceptTarget(IInteractor interactor, bool force = false);
        public abstract bool AcceptInteraction(IInteractor interactor, bool force = false);
        public abstract string Print();
    }

    [Serializable]
    public abstract class Interactable<TData, TType> : Interactable, IInteractable
        where TData : InteractableData
        where TType : System.Enum
    {
        [SerializeField]
        [
            Expandable,
            CreateAsset(
                "NewInteractableData",
                "Assets/Resources/Darklight/Interaction/InteractableData"
            )
        ]
        private TData _data;

        [SerializeField]
        private List<TType> _recieverRequest;

        [SerializeField]
        private CollectionDictionary<TType, InteractionReciever<TData, TType>> _activeRecievers;

        public override InteractableData Data => _data;
        public List<TType> RecieverRequest => _recieverRequest;
        public CollectionDictionary<TType, InteractionReciever<TData, TType>> ActiveRecievers
        {
            get => _activeRecievers;
            set => _activeRecievers = value;
        }

        #region [[ PUBLIC_METHODS ]] < > ================================== >>>>
        public override void Initialize()
        {
            if (_data == null)
            {
                Debug.LogError($"{PREFIX} {Data.Key} :: Data is null", this);
                return;
            }

            if (_recieverRequest == null)
            {
                Debug.LogError($"{PREFIX} {Data.Key} :: Request is null", this);
                return;
            }

            InteractionSystem<TData, TType>.Instance.Registry.TryRegisterInteractable(
                this,
                out bool result
            );
            if (!result)
            {
                Debug.LogError($"{PREFIX} {Data.Key} :: Failed to register", this);
                return;
            }

            if (!Validate(out string outLog))
            {
                Debug.LogError(outLog, this);
                return;
            }
        }

        public override bool AcceptTarget(IInteractor interactor, bool force = false)
        {
            if (interactor == null)
            {
                Debug.LogError($"{PREFIX} {Data.Key} :: Interactor is null", this);
                return false;
            }

            OnAcceptTarget?.Invoke();
            return true;
        }

        public override bool AcceptInteraction(IInteractor interactor, bool force = false)
        {
            if (interactor == null)
            {
                Debug.LogError($"{PREFIX} {Data.Key} :: Interactor is null", this);
                return false;
            }

            Debug.Log($"{PREFIX} {Data.Key} :: AcceptInteraction from {interactor}", this);
            OnAcceptInteraction?.Invoke();
            return true;
        }

        public override void Reset()
        {
            OnReset?.Invoke();
        }

        public override bool Validate(out string outLog)
        {
            if (_data == null || _recieverRequest == null)
            {
                outLog = $"{PREFIX} {Data.Key} :: Validation Failed";
                if (_data == null)
                    outLog += " :: Data is null";
                if (_recieverRequest == null)
                    outLog += " :: Request is null";
                return false;
            }

            if (!InteractionSystem<TData, TType>.Instance.Registry.IsRegistered(this))
            {
                outLog = $"{PREFIX} {Data.Key} :: Not Registered";
                return false;
            }

            outLog = $"{PREFIX} {Data.Key} :: Validation Passed";
            return true;
        }

        public override string Print()
        {
            return $"{Data.ID} :: {Data.Key}";
        }
        #endregion
    }
}
