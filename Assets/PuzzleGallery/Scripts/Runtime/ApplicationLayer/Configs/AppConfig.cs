using PuzzleGallery.Core.Bootstrap;
using PuzzleGallery.Features.MainMenu;
using PuzzleGallery.Features.Premium;
using PuzzleGallery.Services.Logging.Data;
using UnityEngine;

namespace PuzzleGallery.Configs
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "PuzzleGallery/App Config")]
    public sealed class AppConfig : ScriptableObject, IBootstrapConfig
    {
        [Header("Scenes")]
        [SerializeField] private string _bootstrapperScene = "Bootstrapper";
        [SerializeField] private string _menuScene = "Menu";

        [Header("Sub Configurations")]
        [SerializeField] private GalleryConfig _galleryConfig;
        [SerializeField] private CarouselConfig _carouselConfig;
        [SerializeField] private ScreenConfig _screenConfig;
        [SerializeField] private RemoteImageConfig _remoteImageConfig;
        [SerializeField] private PremiumPopupConfig _premiumPopupConfig;
        [SerializeField] private LogsConfig _logsConfig;

        [Header("View Addresses")]
        [SerializeField] private string _puzzleViewAddress = "PuzzleView";

        [Header("Splash Settings")]
        [SerializeField] private float _splashFadeDuration = 0.5f;
        [SerializeField] private string[] _assetsToPreload;

        public string BootstrapperScene => _bootstrapperScene;
        public string MenuScene => _menuScene;
        public GalleryConfig GalleryConfig => _galleryConfig;
        public CarouselConfig CarouselConfig => _carouselConfig;
        public ScreenConfig ScreenConfig => _screenConfig;
        public RemoteImageConfig RemoteImageConfig => _remoteImageConfig;
        public PremiumPopupConfig PremiumPopupConfig => _premiumPopupConfig;
        public LogsConfig LogsConfig => _logsConfig;
        public string PuzzleViewAddress => _puzzleViewAddress;
        public float SplashFadeDuration => _splashFadeDuration;
        public string[] AssetsToPreload => _assetsToPreload;
    }
}
