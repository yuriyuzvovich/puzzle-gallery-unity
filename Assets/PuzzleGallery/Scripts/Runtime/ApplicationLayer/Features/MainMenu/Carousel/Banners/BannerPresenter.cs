using PuzzleGallery.Core.MVP;

namespace PuzzleGallery.Features.MainMenu
{
    public abstract class BannerPresenter : IPresenter
    {
        protected IBannerView View { get; }

        protected BannerPresenter(IBannerView view)
        {
            View = view;
        }

        public abstract void Initialize();

        public abstract void Dispose();
    }
}
