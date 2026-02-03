using PuzzleGallery.Services.Logging;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class GoToCozyCategoryBannerPresenter : BannerPresenter
    {
        private readonly GoToCozyCategoryBannerView _view;

        public GoToCozyCategoryBannerPresenter(GoToCozyCategoryBannerView view)
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
            Logs.Debug("GoToCozyCategoryBanner: button clicked.");
        }
    }
}
