using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Darklight.Behaviour
{
    [ExecuteAlways]
    public abstract class InteractionReciever<TData, TType> : MonoBehaviour
        where TData : InteractableData
        where TType : System.Enum
    {
        [SerializeField, ReadOnly]
        Interactable<TData, TType> _interactable;

        [SerializeField]
        TType _interactionType;

        public Interactable<TData, TType> Interactable => _interactable;
        public TType InteractionType => _interactionType;

        public virtual void Initialize(Interactable<TData, TType> interactable)
        {
            _interactable = interactable;

            _interactable.OnAcceptTarget += OnAcceptTarget;
            _interactable.OnAcceptInteraction += OnAcceptInteraction;
            _interactable.OnReset += OnReset;
        }

        public virtual bool Validate()
        {
            return _interactable != null;
        }

        public virtual void OnDestroy()
        {
            if (_interactable != null)
            {
                _interactable.OnAcceptTarget -= OnAcceptTarget;
                _interactable.OnAcceptInteraction -= OnAcceptInteraction;
                _interactable.OnReset -= OnReset;
            }
        }

        public abstract void OnAcceptTarget();
        public abstract void OnAcceptInteraction();
        public abstract void OnReset();
    }
}
