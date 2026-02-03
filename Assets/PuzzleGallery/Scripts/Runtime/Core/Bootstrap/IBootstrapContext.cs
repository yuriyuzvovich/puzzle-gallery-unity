using PuzzleGallery.Core.ServiceLocator;

namespace PuzzleGallery.Core.Bootstrap
{
    /// <summary>
    /// Abstraction of bootstrap context providing access to bootstrap configuration
    /// and service registry during bootstrap initialization.
    /// </summary>
    /// <remarks>
    /// This interface enables:
    /// - Testability: Bootstrap steps can be unit tested with mock implementations
    /// - Decoupling: Steps depend on minimal IBootstrapConfig, not project-specific IAppConfig
    /// - Flexibility: Alternative IBootstrapConfig sources can be provided (JSON, remote, etc.)
    ///
    /// Service and feature configs are accessed via ServiceRegistry.Get&lt;TConfig&gt;().
    /// </remarks>
    public interface IBootstrapContext
    {
        /// <summary>
        /// Bootstrap-specific configuration (scene names, splash settings, preload list).
        /// For service/feature configs, use ServiceRegistry.Get&lt;TConfig&gt;().
        /// </summary>
        IBootstrapConfig Config { get; }

        /// <summary>
        /// Service registry for registering and retrieving services/configs during bootstrap.
        /// Use Register&lt;T&gt; to add services/configs and Get&lt;T&gt; to retrieve them.
        /// </summary>
        IServiceRegistry ServiceRegistry { get; }
    }
}
