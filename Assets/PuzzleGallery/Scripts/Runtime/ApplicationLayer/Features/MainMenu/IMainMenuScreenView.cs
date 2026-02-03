using PuzzleGallery.Core.MVP;

namespace PuzzleGallery.Features.MainMenu
{
    public interface IMainMenuScreenView : IView
    {
        ICarouselView CarouselView { get; }

        ITabBarView TabBarView { get; }

        IGalleryView GalleryView { get; }
    }
}
