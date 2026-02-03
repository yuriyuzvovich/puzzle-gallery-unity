namespace PuzzleGallery.Features.MainMenu
{
    public interface IMenuPresenterFactory
    {
        CarouselPresenter CreateCarouselPresenter(
            ICarouselView view,
            CarouselConfig config);

        TabBarPresenter CreateTabBarPresenter(
            ITabBarView view,
            bool isFirstInitialization);

        GalleryPresenter CreateGalleryPresenter(
            IGalleryView view,
            GalleryConfig config);
    }
}
