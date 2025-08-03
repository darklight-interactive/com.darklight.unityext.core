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
        int ID { get; }
        Collider Collider { get; }
        Interactor CurrentInteractor { get; }
        Action OnAcceptTarget { get; set; }
        Action OnAcceptInteraction { get; set; }
        Action OnCompleteInteraction { get; set; }
        void Initialize();
        void Reset();
        bool AcceptTarget(Interactor interactor, bool force = false);
        bool AcceptInteraction(Interactor interactor, bool force = false);
        void CompleteInteraction();
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
    [RequireComponent(typeof(Collider))]
    public abstract class Interactable : MonoBehaviour, IInteractable
    {
        protected const string PREFIX = "INTRCTBL";

        public virtual int ID => GetInstanceID();
        public abstract Collider Collider { get; }
        public abstract Interactor CurrentInteractor { get; protected set; }
        public abstract Action OnAcceptTarget { get; set; }
        public abstract Action OnAcceptInteraction { get; set; }
        public abstract Action OnCompleteInteraction { get; set; }

        protected virtual void Start() => Initialize();

        protected abstract void OnDrawGizmos();

        /// <summary>
        /// Initialize the interactable within the scene by storing scene specific references and data. <br/>
        /// This is called when the interactable is first created, typically in Start()
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Reset the interactable to its default state & values
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Validate the interactable to ensure it is properly configured
        /// </summary>
        /// <returns>True if the interactable is valid, false otherwise</returns>
        public abstract bool Validate(out string outLog);

        /// <summary>
        /// Validate and accept a target from an interactor
        /// </summary>
        /// <param name="interactor">The interactor that is targeting the interactable</param>
        /// <param name="force">Whether to force the target to be accepted</param>
        /// <returns>True if the target is accepted, false otherwise</returns>
        public abstract bool AcceptTarget(Interactor interactor, bool force = false);

        /// <summary>
        /// Handle what the interactable does when it is targeted
        /// </summary>
        /// <param name="interactor">The interactor that is targeting the interactable</param>
        protected abstract void HandleTarget(Interactor interactor);

        /// <summary>
        /// /// Accept an interaction from an interactor
        /// </summary>
        /// <param name="interactor">The interactor that is interacting with the interactable</param>
        /// <param name="force">Whether to force the interaction to be accepted</param>
        /// <returns>True if the interaction is accepted, false otherwise</returns>
        public abstract bool AcceptInteraction(Interactor interactor, bool force = false);

        /// <summary>
        /// Handle what the interactable does when it is interacted with
        /// </summary>
        /// <param name="interactor">The interactor that is interacting with the interactable</param>
        protected abstract void HandleInteraction();

        /// <summary>
        /// Complete the interaction with the interactor. <br/>
        /// This is called when the interaction is complete & tells the interactor to reset.
        /// </summary>
        /// <param name="interactor">The interactor that is interacting with the interactable</param>
        public abstract void CompleteInteraction();

        /// <summary>
        /// Print the interactable to a string
        /// </summary>
        /// <returns>A string representation of the interactable</returns>
        public abstract string Print();
    }

    [Serializable]
    public abstract class Interactable<TType> : Interactable, IInteractable
        where TType : System.Enum
    {
        Collider _collider;
        Interactor _currentInteractor;

        [SerializeField, ReadOnly]
        private CollectionDictionary<TType, InteractionReciever<TType>> _recievers = new();
        public CollectionDictionary<TType, InteractionReciever<TType>> Recievers
        {
            get => _recievers;
            protected set => _recievers = value;
        }

        public override Collider Collider => _collider;
        public override Interactor CurrentInteractor
        {
            get => _currentInteractor;
            protected set => _currentInteractor = value;
        }

        #region < PRIVATE_METHODS > ================================================================

        /// <summary>
        /// Initialize the collider of the interactable. <br/>
        /// This is called when the interactable is Initialized.
        /// If the collider is not found, a box collider will be added.
        /// </summary>
        void InitializeCollider()
        {
            // If the collider is already set, return
            if (_collider != null)
                return;

            // Get the collider component
            _collider = GetComponent<Collider>();
            if (_collider == null)
            {
                // If the collider is not found, add a box collider
                _collider = gameObject.AddComponent<BoxCollider>();
            }
        }

        /// <summary>
        /// Initialize the recievers of the interactable. <br/>
        /// This is called when the interactable is Initialized.
        /// All receivers should be located either on the interactable or in a child of the interactable.
        /// </summary>
        public virtual void InitializeRecievers()
        {
            _recievers.Clear();
            var recievers = GetComponentsInChildren<InteractionReciever<TType>>();
            foreach (var reciever in recievers)
            {
                _recievers.Add(reciever.InteractionType, reciever);
                reciever.Initialize(this);
            }
        }
        #endregion

        #region [[ PUBLIC_METHODS ]] < > ================================== >>>>
        public override void Initialize()
        {
            InitializeCollider();
            InitializeRecievers();

            // Register the interactable with the InteractionSystem
            InteractionSystem<TType>.Instance.Registry.TryRegisterInteractable(
                this,
                out bool result
            );
            if (!result)
            {
                Debug.LogError($"{PREFIX} Failed to register", this);
                return;
            }

            // Validate the interactable
            if (!Validate(out string outLog))
            {
                Debug.LogError(outLog, this);
                return;
            }
        }

        public override bool Validate(out string outLog)
        {
            if (!InteractionSystem<TType>.Instance.Registry.IsRegistered(this))
            {
                outLog = $"{PREFIX} Not Registered";
                return false;
            }

            outLog = $"{PREFIX} Validation Passed";
            return true;
        }

        public override string Print()
        {
            return $"{ID}";
        }
        #endregion
    }
}
