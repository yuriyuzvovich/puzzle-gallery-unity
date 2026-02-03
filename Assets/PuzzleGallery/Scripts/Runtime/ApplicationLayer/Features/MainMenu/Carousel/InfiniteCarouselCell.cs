using System.Threading;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Core;
using PuzzleGallery.Core.ServiceLocator;
using PuzzleGallery.Services.Asset;
using PuzzleGallery.Services.Logging;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class InfiniteCarouselCell : FancyCell<BannerData, CarouselContext>
    {
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private Button _clickArea;
        [SerializeField] private Animator _animator;

        [Header("Position Animation")]
        [SerializeField] private bool _useScaleEffect = false;
        [SerializeField] private float _selectedScale = 1f;
        [SerializeField] private float _unselectedScale = 0.9f;

        private static readonly int ScrollHash = Animator.StringToHash("scroll");
        private RectTransform _rectTransform;
        private float _currentPosition;

        private IAssetService _assetService;
        private GameObject _bannerInstance;
        private BannerData _currentData;
        private CancellationTokenSource _loadCts;
        private bool _isInitialized;

        public override void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            _rectTransform = GetComponent<RectTransform>();

            if (ServiceLocator.Instance.TryGet<IAssetService>(out var assetService))
            {
                _assetService = assetService;
            }

            if (_clickArea != null)
            {
                _clickArea.onClick.AddListener(OnClick);
            }
        }

        public override void UpdateContent(BannerData itemData)
        {
            if (_currentData?.AssetKey == itemData?.AssetKey)
            {
                return;
            }

            _currentData = itemData;
            LoadBannerAsync(itemData).Forget();
        }

        public override void UpdatePosition(float position)
        {
            _currentPosition = position;

            if (_rectTransform == null)
            {
                return;
            }

            var parentRect = _rectTransform.parent as RectTransform;
            if (parentRect == null)
            {
                return;
            }

            float parentWidth = parentRect.rect.width;
            float cellInterval = Context?.CellInterval ?? 1f;

            float normalizedX = (position - 0.5f) / cellInterval;
            float anchoredX = normalizedX * parentWidth;

            _rectTransform.anchoredPosition = new Vector2(anchoredX, _rectTransform.anchoredPosition.y);

            if (_useScaleEffect)
            {
                float distanceFromCenter = Mathf.Abs(position - 0.5f) * 2f;
                float scale = Mathf.Lerp(_selectedScale, _unselectedScale, distanceFromCenter);
                _rectTransform.localScale = Vector3.one * scale;
            }

            if (_animator != null)
            {
                _animator.Play(ScrollHash, -1, position);
                _animator.speed = 0;
            }
        }

        private async UniTaskVoid LoadBannerAsync(BannerData data)
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = new CancellationTokenSource();
            var ct = _loadCts.Token;

            ReleaseBannerInstance();

            if (data == null || string.IsNullOrWhiteSpace(data.AssetKey))
            {
                return;
            }

            if (_assetService == null)
            {
                Logs.Warning("InfiniteCarouselCell: IAssetService not available.");
                return;
            }

            try
            {
                var instance = await _assetService.InstantiateAsync(data.AssetKey, _contentRoot);

                if (ct.IsCancellationRequested)
                {
                    if (instance != null)
                    {
                        _assetService.ReleaseInstance(instance);
                    }
                    return;
                }

                _bannerInstance = instance;

                if (_bannerInstance != null)
                {
                    var rt = _bannerInstance.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.offsetMin = Vector2.zero;
                        rt.offsetMax = Vector2.zero;
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (!ct.IsCancellationRequested)
                {
                    Logs.Exception(ex, $"InfiniteCarouselCell: Failed to load banner '{data.AssetKey}'.");
                }
            }
        }

        private void ReleaseBannerInstance()
        {
            if (_bannerInstance == null)
            {
                return;
            }

            if (_assetService != null)
            {
                _assetService.ReleaseInstance(_bannerInstance);
            }
            else
            {
                Destroy(_bannerInstance);
            }

            _bannerInstance = null;
        }

        private void OnClick()
        {
            Context?.OnBannerClicked?.Invoke(Index);
        }

        private void OnDestroy()
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = null;

            ReleaseBannerInstance();

            if (_clickArea != null)
            {
                _clickArea.onClick.RemoveListener(OnClick);
            }
        }
    }
}
