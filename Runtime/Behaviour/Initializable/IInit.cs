namespace Darklight.UnityExt.Behaviour
{
    public interface IInit
    {
        bool IsInitialized { get; }

        void Initialize();
        void Reset();
    }
}
