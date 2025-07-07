using System;

namespace Darklight.Behaviour
{
    public abstract class InteractableStateMachine<TInteractable, TStateEnum>
        : FiniteStateMachine<TStateEnum>
        where TInteractable : Interactable
        where TStateEnum : Enum
    {
        protected TInteractable interactable;

        public InteractableStateMachine(TInteractable interactable)
            : base(GetDefaultState())
        {
            this.interactable = interactable;
        }

        /// <summary>
        /// Gets the default state for the state machine
        /// </summary>
        /// <returns>The default state enum value</returns>
        private static TStateEnum GetDefaultState()
        {
            // Get all enum values and return the first one (usually the default)
            TStateEnum[] values = (TStateEnum[])Enum.GetValues(typeof(TStateEnum));
            return values.Length > 0 ? values[0] : default(TStateEnum);
        }
    }
}
