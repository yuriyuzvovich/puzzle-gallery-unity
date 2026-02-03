using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.MVP;
using PuzzleGallery.Services.Asset;
using UnityEngine;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class MainMenuScreenPresenter : IPresenter
    {
        private const string MainMenuCanvasAddress = "MainMenuCanvas";

        private readonly IAssetService _assetService;
        private readonly Transform _uiRoot;
        private readonly CarouselConfig _carouselConfig;
        private readonly GalleryConfig _galleryConfig;
        private readonly IMenuPresenterFactory _presenterFactory;

        private IMainMenuScreenView _screenView;
        private GameObject _menuRoot;

        private CarouselPresenter _carouselPresenter;
        private TabBarPresenter _tabBarPresenter;
        private GalleryPresenter _galleryPresenter;

        private bool _isInitialized;
        private bool _isFirstShow = true;
        private bool _isFirstTabInitialization = true;

        public MainMenuScreenPresenter(
            IAssetService assetService,
            Transform uiRoot,
            CarouselConfig carouselConfig,
            GalleryConfig galleryConfig,
            IMenuPresenterFactory presenterFactory)
        {
            _assetService = assetService;
            _uiRoot = uiRoot;
            _carouselConfig = carouselConfig;
            _galleryConfig = galleryConfig;
            _presenterFactory = presenterFactory;
        }

        public void Initialize()
        {
        }

        public async UniTask InitializeAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            await LoadViewAsync();

            _isInitialized = true;
        }

        public async UniTask ShowAsync()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            CreateAndInitializePresenters();

            _screenView.Show();
            await UniTask.Yield();

            if (_isFirstShow)
            {
                _isFirstShow = false;
                await _galleryPresenter.ShowAsync();
            }
            else
            {
                _galleryPresenter.Show();
            }
        }

        public async UniTask HideAsync()
        {
            if (_screenView != null)
            {
                _screenView.CarouselView.Hide();
            }

            DisposePresenters();
            _screenView.Hide();
            await UniTask.Yield();
        }

        public void Dispose()
        {
            DisposePresenters();

            _isInitialized = false;
        }

        private async UniTask LoadViewAsync()
        {
            _menuRoot = await _assetService.InstantiateAsync(MainMenuCanvasAddress, _uiRoot);

            if (!_menuRoot)
            {
                throw new InvalidOperationException($"Failed to load MainMenuCanvas from address: {MainMenuCanvasAddress}");
            }

            await UniTask.Yield();

            _screenView = _menuRoot.GetComponent<MainMenuScreenView>();

            if (_screenView == null)
            {
                _assetService.ReleaseInstance(_menuRoot);
                _menuRoot = null;
                throw new InvalidOperationException("MainMenuCanvas prefab does not contain MainMenuScreenView component");
            }

            if (_screenView.CarouselView == null)
            {
                _assetService.ReleaseInstance(_menuRoot);
                _menuRoot = null;
                throw new InvalidOperationException("MainMenuScreenView does not have CarouselView assigned");
            }

            if (_screenView.TabBarView == null)
            {
                _assetService.ReleaseInstance(_menuRoot);
                _menuRoot = null;
                throw new InvalidOperationException("MainMenuScreenView does not have TabBarView assigned");
            }

            if (_screenView.GalleryView == null)
            {
                _assetService.ReleaseInstance(_menuRoot);
                _menuRoot = null;
                throw new InvalidOperationException("MainMenuScreenView does not have GalleryView assigned");
            }
        }

        private void CreateAndInitializePresenters()
        {
            DisposePresenters();

            _carouselPresenter = _presenterFactory.CreateCarouselPresenter(
                _screenView.CarouselView,
                _carouselConfig);

            _tabBarPresenter = _presenterFactory.CreateTabBarPresenter(
                _screenView.TabBarView,
                _isFirstTabInitialization);

            _galleryPresenter = _presenterFactory.CreateGalleryPresenter(
                _screenView.GalleryView,
                _galleryConfig);

            _carouselPresenter.Initialize();
            _tabBarPresenter.Initialize();
            _galleryPresenter.Initialize();

            _isFirstTabInitialization = false;
        }

        private void DisposePresenters()
        {
            _carouselPresenter?.Dispose();
            _tabBarPresenter?.Dispose();
            _galleryPresenter?.Dispose();

            _carouselPresenter = null;
            _tabBarPresenter = null;
            _galleryPresenter = null;
        }
    }
}
