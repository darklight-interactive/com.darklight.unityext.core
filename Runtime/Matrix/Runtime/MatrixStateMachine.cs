using Darklight.Behaviour;

namespace Darklight.Matrix
{
    public partial class Matrix
    {
        public enum State
        {
            INVALID,
            PRELOADED,
            INITIALIZED
        }

        protected class StateMachine : SimpleStateMachine<State>
        {
            public StateMachine()
                : base(State.INVALID) { }
        }
    }
}
