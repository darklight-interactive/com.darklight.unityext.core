using System.Collections.Generic;
using UnityEngine;

namespace Darklight.Behaviour
{
    public interface IInteractionCommand
    {
        void Execute();
    }

    public abstract class InteractionCommand<TData, TType, IReciever> : IInteractionCommand
        where IReciever : InteractionReciever<TData, TType>
        where TData : InteractableData
        where TType : System.Enum
    {
        protected IReciever _reciever;

        public InteractionCommand(IReciever reciever)
        {
            _reciever = reciever;
        }

        public abstract void Execute();
    }
}
