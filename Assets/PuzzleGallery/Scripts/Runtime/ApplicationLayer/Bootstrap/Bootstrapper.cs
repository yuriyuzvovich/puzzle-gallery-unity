using Cysharp.Threading.Tasks;
using PuzzleGallery.Configs;
using PuzzleGallery.Core.Bootstrap;
using PuzzleGallery.Core.EventBus;
using PuzzleGallery.Core.ServiceLocator;
using PuzzleGallery.Core.StateMachine.Integration;
using PuzzleGallery.Core.StateMachine.Runtime;
using PuzzleGallery.Features.MainMenu;
using PuzzleGallery.Features.Premium;
using PuzzleGallery.Features.Puzzle;
using PuzzleGallery.Features.Splash;
using PuzzleGallery.Services.Asset;
using PuzzleGallery.Services.Logging;
using PuzzleGallery.Services.Popup;
using PuzzleGallery.Services.RemoteImage;
using PuzzleGallery.Services.ResourcesAccess;
using PuzzleGallery.Services.Save.Integration;
using PuzzleGallery.Services.Screen.Integration;
using UnityEngine;

namespace PuzzleGallery.Bootstrap
{
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private AppConfig _appConfig;

        [Header("Scene References")]
        [SerializeField] private SplashScreenView splashScreenView;
        [SerializeField] private Transform uiRoot;

        private StateMachine _stateMachine;
        private SplashPresenter _splashPresenter;
        private PuzzleScreenPresenter _puzzleScreenPresenter;
        private BootstrapContext _bootstrapContext;

        private async void Start()
        {
            DontDestroyOnLoad(gameObject);
            await RunBootstrapAsync();
        }

        private void OnDestroy()
        {
            var eventBus = ServiceLocator.Instance.TryGet<GlobalEventBus>(out var eb) ? eb : null;
            eventBus?.Unsubscribe<ImageClickedEvent>(HandleImageClicked);

            ServiceLocator.Instance.Clear();
        }

        private async UniTask RunBootstrapAsync()
        {
            var locator = ServiceLocator.Instance;

            RegisterConfigs(locator, _appConfig);

            _bootstrapContext = new BootstrapContext(_appConfig, locator);

            var bootstrapStepsRunner = new BootstrapRunner(_bootstrapContext)
                .AddStep(new InitLogsServiceStep())
                .AddStep(new InitEventBusStep())
                .AddStep(new InitAssetServiceStep())
                .AddStep(new InitResourcesServiceStep())
                .AddStep(new InitScreenServiceStep())
                .AddStep(new InitRemoteImageServiceStep())
                .AddStep(new InitPopupServiceStep())
                .AddStep(new InitSaveServiceStep())
                .AddStep(new PreloadAssetsStep());

            splashScreenView.Show();
            splashScreenView.SetProgress(0f);

            await bootstrapStepsRunner.RunAsync();
            await InitializeGameAsync();
            await RunSplashOutAsync();
        }

        private void RegisterConfigs(ServiceLocator locator, AppConfig appConfig)
        {
            locator.Register(appConfig.LogsConfig);
            locator.Register(appConfig.ScreenConfig);
            locator.Register(appConfig.RemoteImageConfig);
            locator.Register(appConfig.PremiumPopupConfig);

            locator.Register(appConfig.GalleryConfig);
            locator.Register(appConfig.CarouselConfig);
        }

        private async UniTask InitializeGameAsync()
        {
            var locator = ServiceLocator.Instance;
            _stateMachine = new StateMachine();
            _splashPresenter = new SplashPresenter(
                splashScreenView,
                locator.Get<IAssetService>()
            );

            _puzzleScreenPresenter = new PuzzleScreenPresenter(
                locator.Get<IAssetService>(),
                uiRoot
            );

            var presenterFactory = new MenuPresenterFactory(locator);

            var carouselConfig = locator.Get<CarouselConfig>();
            var galleryConfig = locator.Get<GalleryConfig>();

            var mainMenuPresenter = new MainMenuScreenPresenter(
                locator.Get<IAssetService>(),
                uiRoot,
                carouselConfig,
                galleryConfig,
                presenterFactory
            );

            var splashState = new SplashState(_stateMachine);
            var mainMenuState = new MainMenuState(mainMenuPresenter);
            var puzzleState = new PuzzleState(_stateMachine, _puzzleScreenPresenter);

            _stateMachine.RegisterState(splashState);
            _stateMachine.RegisterState(mainMenuState);
            _stateMachine.RegisterState(puzzleState);

            var eventBus = locator.Get<GlobalEventBus>();
            eventBus.Subscribe<ImageClickedEvent>(HandleImageClicked);

            await UniTask.Yield();
        }

        private async UniTask RunSplashOutAsync()
        {
            await splashScreenView.FadeOutAsync(_appConfig.SplashFadeDuration);
            await _stateMachine.TransitionToAsync<MainMenuState>();
        }

        private void HandleImageClicked(ImageClickedEvent evt)
        {
            if (evt.ImageData.IsPremium)
            {
                ShowPremiumPopupAsync().Forget();
            }
            else
            {
                GoToPuzzleAsync(evt.ImageData).Forget();
            }
        }

        private async UniTaskVoid ShowPremiumPopupAsync()
        {
            var popupService = ServiceLocator.Instance.Get<IPopupService>();
            await popupService.ShowAsync<PremiumPurchasePopup>(new PremiumPurchasePopupData());
        }

        private async UniTaskVoid GoToPuzzleAsync(GalleryItemData imageData)
        {
            if (_stateMachine.CurrentState is PuzzleState puzzleState)
            {
                puzzleState.SetImageData(imageData);
            }
            else
            {
                var puzzleStateObj = new PuzzleState(_stateMachine, _puzzleScreenPresenter);
                puzzleStateObj.SetImageData(imageData);
                await _stateMachine.TransitionToAsync(puzzleStateObj);
            }
        }

        private void Update()
        {
            _stateMachine?.Update();
        }
    }
}