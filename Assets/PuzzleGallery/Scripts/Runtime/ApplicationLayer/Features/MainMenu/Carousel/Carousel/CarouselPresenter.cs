using PuzzleGallery.Core.MVP;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class CarouselPresenter : IPresenter
    {
        private readonly ICarouselView _view;
        private readonly CarouselModel _model;

        public CarouselPresenter(
            ICarouselView view,
            CarouselModel model)
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            if (_view == null || _model == null)
            {
                return;
            }

            if (!_model.IsEnabled || !_model.HasBanners)
            {
                _view.Hide();
                return;
            }

            _view.Show();
            _view.SetBanners(_model.Banners);
            _view.OnBannerClicked += HandleBannerClicked;
            _view.OnBannerChanged += HandleBannerChanged;

            _view.AutoScrollInterval = _model.AutoScrollInterval;
            _view.AutoScrollEnabled = _model.CanAutoScroll;
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnBannerClicked -= HandleBannerClicked;
                _view.OnBannerChanged -= HandleBannerChanged;
            }
        }

        private void HandleBannerClicked(int index)
        {
            if (index >= 0 && index < _model.Banners.Count)
            {
                var banner = _model.Banners[index];
            }
        }

        private void HandleBannerChanged(int index)
        {
        }
    }
}
