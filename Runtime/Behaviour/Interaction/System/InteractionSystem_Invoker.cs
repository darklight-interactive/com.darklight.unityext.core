namespace Darklight.Behaviour
{
    public abstract partial class InteractionSystem<TData, TType>
        where TData : InteractableData
        where TType : System.Enum
    {
        static class Invoker
        {
            static IInteractionCommand _command;

            public static void SetCommand(IInteractionCommand command)
            {
                _command = command;
            }

            public static void ExecuteCommand()
            {
                _command.Execute();
            }

            public static void ExecuteCommand(IInteractionCommand command)
            {
                SetCommand(command);
                ExecuteCommand();
            }
        }
    }
}
