using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.EventBus;
using PuzzleGallery.Core.MVP;
using PuzzleGallery.Scripts.UI;
using PuzzleGallery.Services.Screen.Runtime;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class GalleryPresenter : IPresenter, IVirtualizedDataSource<GalleryItemData>
    {
        private readonly IGalleryView _view;
        private readonly GalleryModel _model;
        private readonly GlobalEventBus _eventBus;
        private readonly IScreenService _screenService;
        private readonly GalleryConfig _config;

        private FilterType _currentFilter = FilterType.All;
        private List<GalleryItemData> _filteredItems;
        private bool _isInitialShow = true;

        public int ItemCount => _filteredItems?.Count ?? 0;

        public GalleryPresenter(
            IGalleryView view,
            GalleryModel model,
            GlobalEventBus eventBus,
            IScreenService screenService,
            GalleryConfig config)
        {
            _view = view;
            _model = model;
            _eventBus = eventBus;
            _screenService = screenService;
            _config = config;
        }

        public void Initialize()
        {
            _model.LoadImages(_config.BaseUrl, _config.TotalImages, _config.PremiumInterval);

            _filteredItems = _model.AllImages.ToList();

            _view.SetColumnCount(_screenService.GridColumnCount);
            _view.Initialize(ItemCount);

            _view.OnItemClicked += HandleItemClicked;
            _view.OnConfigureCell += HandleConfigureCell;
            _eventBus.Subscribe<FilterChangedEvent>(HandleFilterChanged);
            _screenService.OnOrientationChanged += HandleOrientationChanged;
        }

        public async UniTask ShowAsync()
        {
            _isInitialShow = true;
            ApplyFilter(_currentFilter);
            _isInitialShow = false;
            _view.ScrollToTop();
            await _view.ShowAsync();
        }

        public void Show()
        {
            _isInitialShow = true;
            ApplyFilter(_currentFilter);
            _isInitialShow = false;

            _view.ScrollToTop();
        }

        public void Dispose()
        {
            _view.OnItemClicked -= HandleItemClicked;
            _view.OnConfigureCell -= HandleConfigureCell;
            _eventBus.Unsubscribe<FilterChangedEvent>(HandleFilterChanged);
            _screenService.OnOrientationChanged -= HandleOrientationChanged;
        }

        public GalleryItemData GetItem(int index)
        {
            if (index < 0 || index >= _filteredItems.Count)
            {
                return null;
            }
            return _filteredItems[index];
        }

        private void ApplyFilter(FilterType filter)
        {
            _currentFilter = filter;

            int previousCount = _filteredItems?.Count ?? 0;

            _filteredItems = filter switch
            {
                FilterType.All => _model.AllImages.ToList(),
                FilterType.Odd => _model.AllImages.Where(i => i.Index % 2 == 1).ToList(),
                FilterType.Even => _model.AllImages.Where(i => i.Index % 2 == 0).ToList(),
                _ => _model.AllImages.ToList()
            };

            bool forceRecycle = (previousCount != ItemCount);
            bool scrollToTop = !_isInitialShow;
            _view.RefreshWithNewCount(ItemCount, forceRecycle: forceRecycle, scrollToTop: scrollToTop);
        }

        private void HandleItemClicked(int index)
        {
            var item = GetItem(index);
            if (item == null)
            {
                return;
            }

            _eventBus.Publish(new ImageClickedEvent(item));
        }

        private void HandleConfigureCell(int index, GalleryItemData data)
        {
            var item = GetItem(index);
            if (item != null)
            {
                data.Index = item.Index;
                data.Url = item.Url;
                data.IsPremium = item.IsPremium;
            }
        }

        private void HandleFilterChanged(FilterChangedEvent evt)
        {
            ApplyFilter(evt.FilterType);
        }

        private void HandleOrientationChanged(UnityEngine.ScreenOrientation orientation)
        {
            _view.SetColumnCount(_screenService.GridColumnCount);
            _view.RefreshData();
        }
    }
}
