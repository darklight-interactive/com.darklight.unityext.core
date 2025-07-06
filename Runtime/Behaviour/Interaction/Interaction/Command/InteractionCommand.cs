using System.Collections.Generic;
using UnityEngine;

namespace Darklight.Behaviour
{
    public interface IInteractionCommand
    {
        void Execute();
    }

    public abstract class InteractionCommand<IReciever> : IInteractionCommand
        where IReciever : InteractionReciever
    {
        protected IReciever _reciever;

        public InteractionCommand(IReciever reciever)
        {
            _reciever = reciever;
        }

        public abstract void Execute();
    }
}
