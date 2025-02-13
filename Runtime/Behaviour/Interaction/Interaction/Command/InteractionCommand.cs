using System.Collections.Generic;
using UnityEngine;

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
