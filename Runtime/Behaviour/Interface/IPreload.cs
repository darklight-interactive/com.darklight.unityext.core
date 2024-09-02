namespace Darklight.UnityExt.Behaviour.Interface
{
    public interface IPreload
    {
        /// <summary>
        /// Preload data <br/>
        /// - Typically called before the data is initialized <br/>
        /// - MonoBehaviour implementations should call this in the Awake method <br/>
        /// </summary>
        /// <param name="data">
        ///     The data to preload
        /// </param>
        void Preload();

        /// <summary>
        /// Initialize the data <br/>
        /// - Typically called after the data has been loaded <br/>
        /// - MonoBehaviour implementations should call this in the Start method <br/>
        /// </summary>
        void Initialize();

        /// <summary>
        /// Refresh the data <br/>
        /// - Typically called after the data has been modified <br/>
        /// - MonoBehaviour implementations should call this in the Update method <br/>
        /// </summary>
        void Refresh();

        /// <summary>
        /// Clear the data
        /// </summary>
        void Clear();
    }
}