using System;
using Darklight.Behaviour;
using Darklight.Editor;
using UnityEngine;

namespace Darklight.Behaviour
{
    /// <summary>
    /// This is the base interactable class. Having a non-generic abstract class
    /// allows for the creation of a generic interactable class that can be
    /// used for unique interactable types, but still have the same base and
    /// therefore can be referenced as an non-generic Interactable.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TStateMachine"></typeparam>
    public abstract class Interactable : MonoBehaviour
    {
        protected const string PREFIX = "INTRCTBL";
        protected const string DEFAULT_NAME = "DefaultName";
        protected const string DEFAULT_KEY = "DefaultKey";
        protected const string DEFAULT_LAYER = "Default";

        public abstract string Name { get; }
        public abstract string Key { get; }
        public abstract string Layer { get; }
        public abstract Collider Collider { get; }
        public abstract InteractionRequestDataObject Request { get; protected set; }
        public abstract InteractionRecieverLibrary Recievers { get; protected set; }
        public abstract bool IsRegistered { get; protected set; }
        public abstract bool IsPreloaded { get; protected set; }
        public abstract bool IsInitialized { get; protected set; }

        public Vector3 Position => transform.position;

        /// <summary>
        /// Preload the interactable with core data & subscriptions <br/>
        /// This is called when the interactable is first created or enabled
        /// </summary>
        public abstract void Preload();

        /// <summary>
        /// Register the interactable with the Interaction System <br/>
        /// This is called when the interactable is enabled
        /// </summary>
        public abstract void Register();

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
        public abstract bool AcceptTarget(IInteractor interactor, bool force = false);
        public abstract bool AcceptInteraction(IInteractor interactor, bool force = false);

        public virtual string Print()
        {
            return $"{Name} : {Key}";
        }
    }

    /// <summary>
    /// This is the base interactable class uses the BaseInteractableData and BaseInteractableStateMachine
    /// </summary>
    /// <typeparam name="TInfo">
    /// The data class for the interactable. This is used to store serialized data
    /// for the interactable.
    /// </typeparam>
    /// <typeparam name="TStateMachine">
    /// The state machine for the interactable.
    /// </typeparam>
    /// <typeparam name="TStateEnum">
    /// The state enum for the interactable.
    /// </typeparam>
    /// <typeparam name="TTypeEnum">
    /// The type enum for the interactable.
    /// </typeparam>
    public abstract class Interactable<TInfo, TStateMachine, TStateEnum, TTypeEnum> : Interactable
        where TInfo : class
        where TStateMachine : FiniteStateMachine<TStateEnum>
        where TStateEnum : Enum
        where TTypeEnum : Enum
    {
        public abstract TInfo Info { get; }
        public abstract TStateMachine StateMachine { get; }
        public abstract TStateEnum CurrentState { get; }
        public abstract TTypeEnum TypeKey { get; }

        #region [[ EVENTS ]] <PUBLIC> ================================== >>>>
        public delegate void InteractionEvent();
        #endregion

        #region [[ UNITY_METHODS ]] < PROTECTED > ================================== >>>>
        protected void Awake() => Preload();

        protected void Start() => Initialize();

        protected void Update() => Refresh();

        protected virtual void OnDrawGizmos()
        {
            Vector3 labelPos = transform.position + (Vector3.up * 0.25f);
#if UNITY_EDITOR
            CustomGizmos.DrawLabel(
                CurrentState.ToString(),
                labelPos,
                new GUIStyle()
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = new GUIStyleState() { textColor = Color.white }
                }
            );
#endif
        }
        #endregion


        public abstract class InternalStateMachine : FiniteStateMachine<TStateEnum>
        {
            Interactable _interactable;

            public InternalStateMachine(Interactable interactable)
                : base()
            {
                _interactable = interactable;
            }
        }

        [Serializable]
        public abstract class InternalData
        {
            public abstract string Name { get; }
            public abstract string Key { get; }
            public abstract string Layer { get; }
        }

        public abstract class InternalData<TInteractable> : InternalData
            where TInteractable : Interactable
        {
            protected TInteractable interactable;

            public InternalData(TInteractable interactable)
            {
                this.interactable = interactable;
                LoadData(interactable);
            }

            public abstract void LoadData(TInteractable interactable);
        }
    }
}
