using PuzzleGallery.Core;
using PuzzleGallery.Core.EventBus;
using PuzzleGallery.Core.ServiceLocator;
using PuzzleGallery.Services.Screen.Runtime;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class MenuPresenterFactory : IMenuPresenterFactory
    {
        private readonly ServiceLocator _serviceLocator;

        public MenuPresenterFactory(ServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public CarouselPresenter CreateCarouselPresenter(
            ICarouselView view,
            CarouselConfig config)
        {
            var model = new CarouselModel(config);
            return new CarouselPresenter(view, model);
        }

        public TabBarPresenter CreateTabBarPresenter(
            ITabBarView view,
            bool isFirstInitialization)
        {
            var eventBus = _serviceLocator.Get<GlobalEventBus>();
            return new TabBarPresenter(view, eventBus, isFirstInitialization);
        }

        public GalleryPresenter CreateGalleryPresenter(
            IGalleryView view,
            GalleryConfig config)
        {
            var eventBus = _serviceLocator.Get<GlobalEventBus>();
            var screenService = _serviceLocator.Get<IScreenService>();
            var model = new GalleryModel();
            return new GalleryPresenter(view, model, eventBus, screenService,  config);
        }
    }
}
