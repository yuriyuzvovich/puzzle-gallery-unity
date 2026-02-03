namespace PuzzleGallery.Features.MainMenu
{
    public sealed class DecorativeBannerView : BannerView
    {
        protected override BannerPresenter CreatePresenter()
        {
            return new DecorativeBannerPresenter(this);
        }
    }
}
