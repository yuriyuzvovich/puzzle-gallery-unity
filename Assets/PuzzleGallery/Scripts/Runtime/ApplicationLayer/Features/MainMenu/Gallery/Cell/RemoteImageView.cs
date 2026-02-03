using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PuzzleGallery.Core;
using PuzzleGallery.Core.ServiceLocator;
using PuzzleGallery.Services.Logging;
using PuzzleGallery.Services.RemoteImage;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Features.MainMenu.Cell
{
    [RequireComponent(typeof(Image))]
    public class RemoteImageView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _image;
        [SerializeField] private GameObject _loadingIndicator;
        [SerializeField] private ImageLoadingSpinnerView _loadingSpinner;
        [SerializeField] private GameObject _errorIndicator;

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.3f;

        private IRemoteImageService _imageService;
        private CancellationTokenSource _cts;
        private string _currentUrl;
        private bool _isInitialized;

        private void Awake()
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }
        }

        private void OnEnable()
        {
            TryInitialize();
        }

        private void TryInitialize()
        {
            if (_isInitialized)
            {
                return;
            }

            if (ServiceLocator.Instance.TryGet<IRemoteImageService>(out var service))
            {
                _imageService = service;
                _isInitialized = true;
            }
        }

        public void LoadImage(string url)
        {
            TryInitialize();

            if (!_isInitialized)
            {
                Logs.Warning("RemoteImage: IRemoteImageService not available");
                return;
            }

            if (_currentUrl == url && _image.sprite != null)
            {
                return;
            }

            CancelLoad();

            _currentUrl = url;
            _cts = new CancellationTokenSource();
            LoadImageAsync(url, _cts.Token).Forget();
        }

        public void LoadImageProgressive(string thumbnailUrl, string fullUrl)
        {
            TryInitialize();

            if (!_isInitialized)
            {
                Logs.Warning("RemoteImage: IRemoteImageService not available");
                return;
            }

            CancelLoad();

            _currentUrl = fullUrl;
            _cts = new CancellationTokenSource();
            LoadImageProgressiveAsync(thumbnailUrl, fullUrl, _cts.Token).Forget();
        }

        public void CancelLoad()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void Clear()
        {
            CancelLoad();
            _currentUrl = null;
            _image.sprite = null;
            _image.color = Color.white;
            _image.enabled = false;
            SetLoadingState(false);
            SetErrorState(false);
        }

        private async UniTaskVoid LoadImageAsync(string url, CancellationToken ct)
        {
            bool wasCached = _imageService.IsImageCached(url);

            if (!wasCached)
            {
                _image.enabled = false;
                SetLoadingState(true);
            }
            SetErrorState(false);

            try
            {
                var sprite = await _imageService.LoadImageAsync(url, ct);

                if (ct.IsCancellationRequested)
                {
                    return;
                }

                _image.sprite = sprite;
                _image.enabled = true;
                SetLoadingState(false);

                if (!wasCached)
                {
                    _image.color = new Color(1, 1, 1, 0);
                    var tween = DOTween.To(() => _image.color, color => _image.color = color, Color.white, _fadeInDuration)
                        .SetEase(Ease.OutQuad)
                        .SetTarget(_image);
                    while (tween.active && !tween.IsComplete())
                    {
                        if (ct.IsCancellationRequested)
                        {
                            tween.Kill();
                            ct.ThrowIfCancellationRequested();
                        }
                        await UniTask.Yield();
                    }
                }
                else
                {
                    _image.color = Color.white;
                }
            }
            catch (System.OperationCanceledException)
            {
            }
            catch (System.Exception ex)
            {
                Logs.Warning($"Failed to load image from {url}: {ex.Message}");
                SetLoadingState(false);
                SetErrorState(true);
            }
        }

        private async UniTaskVoid LoadImageProgressiveAsync(string thumbnailUrl, string fullUrl, CancellationToken ct)
        {
            _image.enabled = false;
            SetLoadingState(true);
            SetErrorState(false);

            try
            {
                if (!string.IsNullOrEmpty(thumbnailUrl))
                {
                    var thumbnail = await _imageService.LoadImageAsync(thumbnailUrl, ct);

                    if (!ct.IsCancellationRequested)
                    {
                        _image.sprite = thumbnail;
                        _image.enabled = true;
                        _image.color = Color.white;
                    }
                }

                var fullSprite = await _imageService.LoadImageAsync(fullUrl, ct);

                if (ct.IsCancellationRequested)
                {
                    return;
                }

                _image.sprite = fullSprite;
                _image.enabled = true;
                SetLoadingState(false);

                if (!string.IsNullOrEmpty(thumbnailUrl))
                {
                    _image.color = new Color(1, 1, 1, 0.7f);
                    var tween = DOTween.To(() => _image.color, color => _image.color = color, Color.white, _fadeInDuration * 0.5f)
                        .SetEase(Ease.OutQuad)
                        .SetTarget(_image);
                    while (tween.active && !tween.IsComplete())
                    {
                        if (ct.IsCancellationRequested)
                        {
                            tween.Kill();
                            ct.ThrowIfCancellationRequested();
                        }
                        await UniTask.Yield();
                    }
                }
            }
            catch (System.OperationCanceledException)
            {
            }
            catch (System.Exception ex)
            {
                Logs.Warning($"Failed to load progressive image from {fullUrl}: {ex.Message}");
                SetLoadingState(false);
                SetErrorState(true);
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            if (_loadingIndicator != null)
            {
                _loadingIndicator.SetActive(isLoading);
            }

            if (_loadingSpinner != null)
            {
                if (isLoading)
                {
                    _loadingSpinner.Show();
                }
                else
                {
                    _loadingSpinner.Hide();
                }
            }
        }

        private void SetErrorState(bool hasError)
        {
            if (_errorIndicator != null)
            {
                _errorIndicator.SetActive(hasError);
            }
        }

        private void OnDestroy()
        {
            CancelLoad();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }
        }
#endif
    }
}
