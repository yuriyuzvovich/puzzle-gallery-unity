using PuzzleGallery.Services.Logging;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class RateUsBannerPresenter : BannerPresenter
    {
        private readonly RateUsBannerView _view;

        public RateUsBannerPresenter(RateUsBannerView view)
            : base(view)
        {
            _view = view;
        }

        public override void Initialize()
        {
            if (_view != null)
            {
                _view.OnAction += HandleAction;
            }
        }

        public override void Dispose()
        {
            if (_view != null)
            {
                _view.OnAction -= HandleAction;
            }
        }

        private void HandleAction()
        {
            Logs.Debug("RateUsBanner: primary button clicked. This is a verbose debug message for testing.");
        }
    }
}
