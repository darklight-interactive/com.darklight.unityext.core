using UnityEngine;

#if UNITY_EDITOR
#endif

interface IInteractionReciever
{
    public InteractionType InteractionType { get; }
}

public abstract class InteractionReciever : MonoBehaviour, IInteractionReciever
{
    public abstract InteractionType InteractionType { get; }
}
