using System;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
    public interface IInteractable
    {
        string Name { get; }
        string Key { get; }
        string Layer { get; }
        Vector3 Position { get; }

        InteractionRequestDataObject Request { get; }
        InteractionRecieverLibrary Recievers { get; }

        bool IsPreloaded { get; }
        bool IsRegistered { get; }
        bool IsInitialized { get; }

        // ===================== [[ METHODS ]] =====================
        void Preload();
        void Register();
        void Initialize();
        void Refresh();
        void Reset();

        bool AcceptTarget(IInteractor interactor, bool force = false);
        bool AcceptInteraction(IInteractor interactor, bool force = false);
    }

    /// <summary>
    /// This is the base interactable class uses the BaseInteractableData and BaseInteractableStateMachine
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TStateMachine"></typeparam>
    public abstract class Interactable : MonoBehaviour, IInteractable
    {
        protected const string PREFIX = "INTRCTBL";
        protected const string DEFAULT_NAME = "DefaultName";
        protected const string DEFAULT_KEY = "DefaultKey";
        protected const string DEFAULT_LAYER = "Default";

        public abstract string Name { get; }
        public abstract string Key { get; }
        public abstract string Layer { get; }
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

    public abstract class Interactable<TInfo, TStateMachine, TStateEnum, TTypeEnum>
        : Interactable,
            IInteractable
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
            Vector2 labelPos = (Vector2)transform.position + (Vector2.up * 0.25f);
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
