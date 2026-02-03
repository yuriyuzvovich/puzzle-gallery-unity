using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class MainMenuScreenView : MonoBehaviour, IMainMenuScreenView
    {
        [Header("Components")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;

        [Header("Sub-Views")]
        [SerializeField] private CarouselView _carouselView;
        [SerializeField] private TabBarView _tabBarView;
        [SerializeField] private GalleryView _galleryView;

        public ICarouselView CarouselView => _carouselView;
        public ITabBarView TabBarView => _tabBarView;
        public IGalleryView GalleryView => _galleryView;

        public bool IsVisible => _canvas != null && _canvas.enabled;

        public void Show()
        {
            if (_canvas != null)
                _canvas.enabled = true;
            if (_graphicRaycaster != null)
                _graphicRaycaster.enabled = true;
        }

        public void Hide()
        {
            if (_canvas != null)
                _canvas.enabled = false;
            if (_graphicRaycaster != null)
                _graphicRaycaster.enabled = false;
        }
    }
}
