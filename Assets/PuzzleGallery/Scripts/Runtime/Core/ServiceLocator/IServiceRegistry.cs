namespace PuzzleGallery.Core.ServiceLocator
{
    /// <summary>
    /// Service registry for managing services and configuration objects during bootstrap.
    /// Accepts both IService implementations and config types (ScriptableObjects).
    /// </summary>
    public interface IServiceRegistry
    {
        /// <summary>
        /// Registers a service or config. If item implements IService, Initialize() will be called.
        /// </summary>
        /// <typeparam name="T">The service or config type</typeparam>
        /// <param name="item">The service or config instance to register</param>
        void Register<T>(T item) where T : class;

        /// <summary>
        /// Retrieves a registered service or config. Throws if not found.
        /// </summary>
        /// <typeparam name="T">The service or config type</typeparam>
        /// <returns>The registered service or config instance</returns>
        T Get<T>() where T : class;

        /// <summary>
        /// Attempts to retrieve a registered service or config.
        /// </summary>
        /// <typeparam name="T">The service or config type</typeparam>
        /// <param name="item">Output parameter for the service or config instance</param>
        /// <returns>True if item was found, false otherwise</returns>
        bool TryGet<T>(out T item) where T : class;
    }
}
