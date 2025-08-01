using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Darklight.Behaviour
{
    [ExecuteAlways]
    public abstract class InteractionReciever<TType> : MonoBehaviour
        where TType : System.Enum
    {
        protected Interactable<TType> _interactable;
        public Interactable<TType> Interactable => _interactable;
        public abstract TType InteractionType { get; }

        public virtual void Initialize(Interactable<TType> interactable)
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
