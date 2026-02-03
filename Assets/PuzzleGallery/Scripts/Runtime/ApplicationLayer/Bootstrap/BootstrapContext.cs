using PuzzleGallery.Configs;
using PuzzleGallery.Core.Bootstrap;
using PuzzleGallery.Core.ServiceLocator;

namespace PuzzleGallery.Bootstrap
{
    public sealed class BootstrapContext : IBootstrapContext
    {
        private readonly IBootstrapConfig _config;
        private readonly IServiceRegistry _serviceRegistry;

        public IBootstrapConfig Config => _config;
        public IServiceRegistry ServiceRegistry => _serviceRegistry;

        public BootstrapContext(AppConfig appConfig, Core.ServiceLocator.ServiceLocator serviceLocator)
        {
            _config = appConfig;
            _serviceRegistry = serviceLocator;
        }

        public BootstrapContext(IBootstrapConfig config, IServiceRegistry serviceRegistry)
        {
            _config = config;
            _serviceRegistry = serviceRegistry;
        }
    }
}
