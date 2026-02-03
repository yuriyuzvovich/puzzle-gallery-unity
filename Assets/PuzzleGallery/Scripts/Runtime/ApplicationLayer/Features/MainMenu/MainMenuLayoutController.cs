using PuzzleGallery.Core;
using PuzzleGallery.Core.Devices;
using PuzzleGallery.Core.ServiceLocator;
using PuzzleGallery.Services.Logging;
using PuzzleGallery.Services.Screen.Runtime;
using UnityEngine;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class MainMenuLayoutController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _carouselRoot;
        [SerializeField] private RectTransform _tabBarRoot;
        [SerializeField] private RectTransform _galleryRoot;
        [SerializeField] private CarouselConfig _carouselConfig;
        [SerializeField] private MenuLayoutConfig _layoutConfig;

        [Header("Settings")]
        [SerializeField] private bool _collapseCarouselWhenDisabled = true;

        private IScreenService _screenService;

        private void OnEnable()
        {
            ApplyLayout();
        }

        private void Start()
        {
            if (ServiceLocator.Instance.TryGet<IScreenService>(out var screenService))
            {
                _screenService = screenService;
                _screenService.OnOrientationChanged += HandleOrientationChanged;
                _screenService.OnAppDeviceTypeChanged += HandleDeviceTypeChanged;
            }

            ApplyLayout();
        }

        private void OnDestroy()
        {
            if (_screenService != null)
            {
                _screenService.OnOrientationChanged -= HandleOrientationChanged;
                _screenService.OnAppDeviceTypeChanged -= HandleDeviceTypeChanged;
            }
        }

        private void HandleOrientationChanged(ScreenOrientation orientation)
        {
            ApplyLayout();
        }

        private void HandleDeviceTypeChanged(AppDeviceType deviceType)
        {
            ApplyLayout();
        }

        private void ApplyLayout()
        {
            if (_layoutConfig == null)
            {
                Logs.Warning("MainMenuLayoutController is missing MenuLayoutConfig.");
                return;
            }

            bool isTablet = _screenService != null && _screenService.IsTablet;
            var profile = _layoutConfig.GetProfile(isTablet ? AppDeviceType.Tablet : AppDeviceType.Phone);
            float carouselHeight = profile.CarouselHeightRatio;
            float tabBarHeight = profile.TabBarHeightRatio;

            if (_collapseCarouselWhenDisabled && _carouselConfig != null)
            {
                if (!_carouselConfig.Enabled || !HasConfiguredBanners())
                {
                    carouselHeight = 0f;
                }
            }

            carouselHeight = Mathf.Clamp01(carouselHeight);
            tabBarHeight = Mathf.Clamp(tabBarHeight, 0f, 1f - carouselHeight);

            float carouselMinY = 1f - carouselHeight;
            float tabBarMaxY = carouselMinY;
            float tabBarMinY = tabBarMaxY - tabBarHeight;
            float galleryMaxY = tabBarMinY;

            SetAnchors(_carouselRoot, carouselMinY, 1f);
            SetAnchors(_tabBarRoot, tabBarMinY, tabBarMaxY);
            SetAnchors(_galleryRoot, 0f, galleryMaxY);
        }

        private static void SetAnchors(RectTransform target, float minY, float maxY)
        {
            if (target == null)
            {
                return;
            }

            target.anchorMin = new Vector2(0f, minY);
            target.anchorMax = new Vector2(1f, maxY);
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }

        private bool HasConfiguredBanners()
        {
            if (_carouselConfig == null || _carouselConfig.Banners == null)
            {
                return false;
            }

            for (int i = 0; i < _carouselConfig.Banners.Length; i++)
            {
                var banner = _carouselConfig.Banners[i];
                if (banner != null && !string.IsNullOrWhiteSpace(banner.AssetKey))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
