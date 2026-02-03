using PuzzleGallery.Core.EventBus;
using PuzzleGallery.Core.MVP;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class TabBarPresenter : IPresenter
    {
        private readonly ITabBarView _view;
        private readonly GlobalEventBus _eventBus;
        private readonly bool _isFirstInitialization;

        public TabBarPresenter(
            ITabBarView view,
            GlobalEventBus eventBus,
            bool isFirstInitialization)
        {
            _view = view;
            _eventBus = eventBus;
            _isFirstInitialization = isFirstInitialization;
        }

        public void Initialize()
        {
            _view.OnTabSelected += HandleTabSelected;

            if (_isFirstInitialization)
            {
                _view.SelectTab(FilterType.All);
            }
        }

        public void Dispose()
        {
            _view.OnTabSelected -= HandleTabSelected;
        }

        private void HandleTabSelected(FilterType filterType)
        {
            _eventBus.Publish(new FilterChangedEvent(filterType));
        }
    }
}
