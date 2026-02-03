using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.MVP;
using PuzzleGallery.Features.MainMenu;
using PuzzleGallery.Services;
using PuzzleGallery.Services.Asset;
using UnityEngine;

namespace PuzzleGallery.Features.Puzzle
{
    public sealed class PuzzleScreenPresenter : IPresenter
    {
        private const string PuzzleScreenViewAddress = "PuzzleScreenView";

        private readonly IAssetService _assetService;
        private readonly Transform _uiRoot;

        private IPuzzleScreenView _screenView;
        private GameObject _viewRoot;
        private GalleryItemData _currentImageData;
        private bool _isTransitioning;
        private bool _isInitialized;

        public event Action OnBackRequested;

        public PuzzleScreenPresenter(IAssetService assetService, Transform uiRoot)
        {
            _assetService = assetService;
            _uiRoot = uiRoot;
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

            _screenView.OnBackClicked += HandleBackClicked;
            _isInitialized = true;
        }

        public void Dispose()
        {
            if (_screenView != null)
            {
                _screenView.OnBackClicked -= HandleBackClicked;
            }
        }

        public void DestroyView()
        {
            Dispose();

            if (_viewRoot != null)
            {
                _assetService.ReleaseInstance(_viewRoot);
                _viewRoot = null;
                _screenView = null;
            }

            _isInitialized = false;
        }

        public async UniTask ShowWithImageAsync(GalleryItemData imageData)
        {
            if (_isTransitioning)
            {
                return;
            }

            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            _isTransitioning = true;
            _currentImageData = imageData;
            _screenView.SetImageUrl(imageData.Url);
            await _screenView.ShowAsync();
            _isTransitioning = false;
        }

        public async UniTask HideAsync()
        {
            if (_isTransitioning || _screenView == null)
            {
                return;
            }

            _isTransitioning = true;
            await _screenView.HideAsync();
            _currentImageData = null;
            _isTransitioning = false;
        }

        private void HandleBackClicked()
        {
            if (!_isTransitioning)
            {
                OnBackRequested?.Invoke();
            }
        }

        public bool HandleHardwareBackButton()
        {
            if (_screenView != null && _screenView.IsVisible && !_isTransitioning)
            {
                OnBackRequested?.Invoke();
                return true;
            }
            return false;
        }

        private async UniTask LoadViewAsync()
        {
            _viewRoot = await _assetService.InstantiateAsync(PuzzleScreenViewAddress, _uiRoot);

            if (_viewRoot == null)
            {
                throw new InvalidOperationException($"Failed to load PuzzleScreenView from address: {PuzzleScreenViewAddress}");
            }

            await UniTask.Yield();

            _screenView = _viewRoot.GetComponent<IPuzzleScreenView>();

            if (_screenView == null)
            {
                _assetService.ReleaseInstance(_viewRoot);
                _viewRoot = null;
                throw new InvalidOperationException("PuzzleScreenView prefab does not contain IPuzzleScreenView component");
            }

            _screenView.Hide();
        }
    }
}
