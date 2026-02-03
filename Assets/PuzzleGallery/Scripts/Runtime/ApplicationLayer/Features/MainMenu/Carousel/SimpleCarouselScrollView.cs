using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PuzzleGallery.Core;
using PuzzleGallery.Core.ServiceLocator;
using PuzzleGallery.Services.Asset;
using PuzzleGallery.Services.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class SimpleCarouselScrollView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public enum ScrollState
        {
            Idle,
            Dragging,
            Snapping
        }

        [Header("References")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private RectTransform _viewport;

        [Header("Scroll Settings")]
        [SerializeField] private float _snapDuration = 0.25f;
        [SerializeField] private float _swipeThreshold = 50f;

        [Header("Auto Scroll")]
        [SerializeField] private float _autoScrollInterval = 5f;
        [SerializeField] private bool _autoScrollEnabled = true;

        private IAssetService _assetService;
        private readonly List<RectTransform> _bannerItems = new();
        private readonly List<GameObject> _bannerInstances = new();
        private readonly List<int> _bannerDataIndices = new();
        private CancellationTokenSource _loadCts;

        private ScrollState _state = ScrollState.Idle;
        private Vector2 _dragStartLocal;
        private Vector2 _contentStartPos;
        private float _itemWidth;
        private float _autoScrollTimer;
        private Tween _snapTween;
        private bool _isInitialized;
        private int _currentDataIndex;

        public int CurrentIndex => _currentDataIndex;

        public float AutoScrollInterval
        {
            get => _autoScrollInterval;
            set => _autoScrollInterval = Mathf.Max(0.1f, value);
        }

        public bool AutoScrollEnabled
        {
            get => _autoScrollEnabled;
            set
            {
                _autoScrollEnabled = value;
                if (value)
                {
                    _autoScrollTimer = 0f;
                }
            }
        }

        public ScrollState State => _state;

        public event Action<int> OnSelectionChanged;
        public event Action<int> OnBannerClicked;
        public event Action OnSnapComplete;

        private void Awake()
        {
            if (ServiceLocator.Instance.TryGet<IAssetService>(out var assetService))
            {
                _assetService = assetService;
            }
        }

        private void Update()
        {
            if (!_autoScrollEnabled || !_isInitialized || _bannerItems.Count < 2)
            {
                return;
            }

            if (_state != ScrollState.Idle)
            {
                return;
            }

            _autoScrollTimer += Time.deltaTime;
            if (_autoScrollTimer >= _autoScrollInterval)
            {
                _autoScrollTimer = 0f;
                ScrollToNext();
            }
        }

        private void OnDestroy()
        {
            _snapTween?.Kill();
            CancelLoad();
            ClearBanners();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (gameObject.activeInHierarchy && _isInitialized && _bannerItems.Count > 0)
            {
                RecalculateLayoutNextFrame().Forget();
            }
        }

        public void SetBanners(IReadOnlyList<BannerData> banners)
        {
            var bannerList = banners ?? Array.Empty<BannerData>();
            _state = ScrollState.Idle;
            _autoScrollTimer = 0f;
            _isInitialized = false;
            _currentDataIndex = 0;

            CancelLoad();
            ClearBanners();

            if (bannerList.Count == 0)
            {
                return;
            }

            _loadCts = new CancellationTokenSource();
            CreateBannersAsync(bannerList, _loadCts.Token).Forget();
        }

        public void ScrollToIndex(int index, bool animated = true)
        {
            if (_bannerItems.Count == 0)
            {
                return;
            }

            index = Mathf.Clamp(index, 0, _bannerItems.Count - 1);

            int visualIndex = _bannerDataIndices.IndexOf(index);
            if (visualIndex < 0)
            {
                visualIndex = Mathf.Clamp(index, 0, _bannerItems.Count - 1);
            }

            if (visualIndex == 0)
            {
                SnapByDirection(0, 0, animated);
                return;
            }

            int forwardSteps = visualIndex;
            int backwardSteps = _bannerItems.Count - visualIndex;

            if (backwardSteps == 1)
            {
                SnapByDirection(-1, 1, animated);
                return;
            }

            SnapByDirection(1, forwardSteps, animated);
        }

        public void ScrollToNext()
        {
            if (_bannerItems.Count < 2)
            {
                return;
            }

            SnapByDirection(1, 1, true);
        }

        public void ScrollToPrevious()
        {
            if (_bannerItems.Count < 2)
            {
                return;
            }

            SnapByDirection(-1, 1, true);
        }

        public void PauseAutoScroll()
        {
            _autoScrollEnabled = false;
        }

        public void ResumeAutoScroll()
        {
            _autoScrollEnabled = true;
            _autoScrollTimer = 0f;
        }

        public void ResetAutoScrollTimer()
        {
            _autoScrollTimer = 0f;
        }

        public void StopScrolling()
        {
            _snapTween?.Kill();
            _state = ScrollState.Idle;
            _autoScrollTimer = 0f;

            if (_content == null || _bannerItems.Count == 0)
            {
                return;
            }

            _content.anchoredPosition = new Vector2(0f, _content.anchoredPosition.y);

            if (_itemWidth > 0f)
            {
                NormalizeItemPositions();
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (_content == null || _viewport == null)
            {
                return;
            }

            if (_bannerItems.Count < 2 || !_isInitialized)
            {
                return;
            }

            if (!TryGetLocalPointerPosition(eventData, out _dragStartLocal))
            {
                return;
            }

            _state = ScrollState.Dragging;
            _autoScrollTimer = 0f;
            _contentStartPos = _content.anchoredPosition;
            _snapTween?.Kill();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (_content == null || _viewport == null)
            {
                return;
            }

            if (_state != ScrollState.Dragging || _bannerItems.Count < 2)
            {
                return;
            }

            if (!TryGetLocalPointerPosition(eventData, out var localPos))
            {
                return;
            }

            float deltaX = localPos.x - _dragStartLocal.x;
            float newX = _contentStartPos.x + deltaX;
            _content.anchoredPosition = new Vector2(newX, _content.anchoredPosition.y);

            RebalanceForInfiniteDrag();
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (_content == null || _viewport == null)
            {
                _state = ScrollState.Idle;
                return;
            }

            if (_state != ScrollState.Dragging)
            {
                return;
            }

            if (_bannerItems.Count == 0)
            {
                _state = ScrollState.Idle;
                return;
            }

            float dragDelta = 0f;
            if (TryGetLocalPointerPosition(eventData, out var localPos))
            {
                dragDelta = localPos.x - _dragStartLocal.x;
            }

            float offset = _content.anchoredPosition.x;
            int direction = 0;

            if (Mathf.Abs(dragDelta) > _swipeThreshold)
            {
                direction = dragDelta < 0f ? 1 : -1;
            }
            else if (offset <= -_itemWidth * 0.5f)
            {
                direction = 1;
            }
            else if (offset >= _itemWidth * 0.5f)
            {
                direction = -1;
            }

            SnapByDirection(direction, 1, true);
        }

        private bool TryGetLocalPointerPosition(PointerEventData eventData, out Vector2 localPoint)
        {
            localPoint = Vector2.zero;
            if (_viewport == null)
            {
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _viewport,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);
        }

        private void RebalanceForInfiniteDrag()
        {
            if (_content == null)
            {
                return;
            }

            if (_bannerItems.Count < 2 || _itemWidth <= 0f)
            {
                return;
            }

            float contentX = _content.anchoredPosition.x;
            bool didReorder = false;

            while (contentX <= -_itemWidth)
            {
                MoveFirstToEnd();
                contentX += _itemWidth;
                _contentStartPos.x += _itemWidth;
                _dragStartLocal.x += _itemWidth;
                didReorder = true;
            }

            while (contentX >= _itemWidth)
            {
                MoveLastToFront();
                contentX -= _itemWidth;
                _contentStartPos.x -= _itemWidth;
                _dragStartLocal.x -= _itemWidth;
                didReorder = true;
            }

            _content.anchoredPosition = new Vector2(contentX, _content.anchoredPosition.y);

            if (didReorder)
            {
                NormalizeItemPositions();
            }
        }

        private void MoveFirstToEnd()
        {
            if (_bannerItems.Count < 2)
            {
                return;
            }

            var firstItem = _bannerItems[0];
            int firstDataIndex = _bannerDataIndices[0];
            _bannerItems.RemoveAt(0);
            _bannerItems.Add(firstItem);
            _bannerDataIndices.RemoveAt(0);
            _bannerDataIndices.Add(firstDataIndex);
        }

        private void MoveLastToFront()
        {
            if (_bannerItems.Count < 2)
            {
                return;
            }

            int lastDataIndex = _bannerDataIndices[^1];
            var lastItem = _bannerItems[^1];
            _bannerItems.RemoveAt(_bannerItems.Count - 1);
            _bannerItems.Insert(0, lastItem);
            _bannerDataIndices.RemoveAt(_bannerDataIndices.Count - 1);
            _bannerDataIndices.Insert(0, lastDataIndex);
        }

        private void SnapByDirection(int direction, int steps, bool animated)
        {
            if (_bannerItems.Count == 0 || _itemWidth <= 0f || _content == null)
            {
                _state = ScrollState.Idle;
                return;
            }

            if (direction == 0 || steps <= 0 || _bannerItems.Count < 2)
            {
                _state = ScrollState.Snapping;
                _snapTween?.Kill();

                if (animated)
                {
                    _snapTween = _content.DOAnchorPosX(0f, _snapDuration)
                        .SetEase(Ease.OutCubic)
                        .OnComplete(() => CompleteSnap(0, 0));
                }
                else
                {
                    _content.anchoredPosition = new Vector2(0f, _content.anchoredPosition.y);
                    CompleteSnap(0, 0);
                }

                return;
            }

            steps = Mathf.Clamp(steps, 1, _bannerItems.Count - 1);
            float targetX = direction > 0 ? -_itemWidth * steps : _itemWidth * steps;

            _state = ScrollState.Snapping;
            _snapTween?.Kill();

            if (animated)
            {
                _snapTween = _content.DOAnchorPosX(targetX, _snapDuration)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() => CompleteSnap(direction, steps));
            }
            else
            {
                _content.anchoredPosition = new Vector2(targetX, _content.anchoredPosition.y);
                CompleteSnap(direction, steps);
            }
        }

        private void CompleteSnap(int direction, int steps)
        {
            if (_content == null)
            {
                _state = ScrollState.Idle;
                return;
            }

            if (direction > 0)
            {
                for (int i = 0; i < steps; i++)
                {
                    MoveFirstToEnd();
                }
            }
            else if (direction < 0)
            {
                for (int i = 0; i < steps; i++)
                {
                    MoveLastToFront();
                }
            }

            NormalizeItemPositions();
            _content.anchoredPosition = new Vector2(0f, _content.anchoredPosition.y);

            _state = ScrollState.Idle;
            _autoScrollTimer = 0f;

            int newIndex = _bannerDataIndices.Count > 0 ? _bannerDataIndices[0] : 0;
            if (_currentDataIndex != newIndex)
            {
                _currentDataIndex = newIndex;
                OnSelectionChanged?.Invoke(newIndex);
            }

            OnSnapComplete?.Invoke();
        }

        private void NormalizeItemPositions()
        {
            if (_bannerItems.Count == 0 || _itemWidth <= 0f)
            {
                return;
            }

            if (_bannerItems.Count == 1)
            {
                var singleItem = _bannerItems[0];
                if (singleItem != null)
                {
                    singleItem.anchoredPosition = new Vector2(0f, 0f);
                }
                return;
            }

            for (int i = 0; i < _bannerItems.Count; i++)
            {
                var item = _bannerItems[i];
                if (item != null)
                {
                    float posX = i == _bannerItems.Count - 1 ? -_itemWidth : i * _itemWidth;
                    item.anchoredPosition = new Vector2(posX, 0f);
                }
            }
        }

        private async UniTaskVoid CreateBannersAsync(IReadOnlyList<BannerData> banners, CancellationToken ct)
        {
            if (_viewport == null || _content == null)
            {
                Logs.Error("SimpleCarouselScrollView: Viewport or Content is null");
                return;
            }

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, ct);

            _itemWidth = _viewport.rect.width;
            if (_itemWidth <= 0f)
            {
                await UniTask.Yield(ct);
                _itemWidth = _viewport.rect.width;
            }

            if (_itemWidth <= 0f)
            {
                Logs.Error($"SimpleCarouselScrollView: Invalid viewport width: {_itemWidth}");
                return;
            }

            _content.anchoredPosition = Vector2.zero;

            for (int i = 0; i < banners.Count; i++)
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                var banner = banners[i];
                if (banner == null || string.IsNullOrWhiteSpace(banner.AssetKey))
                {
                    continue;
                }

                GameObject instance = null;
                try
                {
                    if (_assetService != null)
                    {
                        instance = await _assetService.InstantiateAsync(banner.AssetKey, _content);
                    }
                }
                catch (Exception ex)
                {
                    Logs.Exception(ex, $"SimpleCarouselScrollView: Failed to load banner '{banner.AssetKey}'.");
                    continue;
                }

                if (ct.IsCancellationRequested)
                {
                    if (instance != null && _assetService != null)
                    {
                        _assetService.ReleaseInstance(instance);
                    }
                    return;
                }

                if (instance == null)
                {
                    continue;
                }

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, ct);

                _bannerInstances.Add(instance);

                var rt = instance.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 0.5f);
                    rt.sizeDelta = new Vector2(_itemWidth, 0f);
                    rt.anchoredPosition = new Vector2(_bannerItems.Count * _itemWidth, 0f);

                    _bannerItems.Add(rt);
                    _bannerDataIndices.Add(i);

                    var button = instance.GetComponent<Button>();
                    if (button != null)
                    {
                        var capturedRt = rt;
                        button.onClick.AddListener(() => HandleBannerClick(capturedRt));
                    }
                }
            }

            _isInitialized = true;
            _currentDataIndex = _bannerDataIndices.Count > 0 ? _bannerDataIndices[0] : 0;
            NormalizeItemPositions();
            _content.anchoredPosition = Vector2.zero;
        }

        private void HandleBannerClick(RectTransform clickedItem)
        {
            int visualIndex = _bannerItems.IndexOf(clickedItem);
            if (visualIndex >= 0 && visualIndex < _bannerDataIndices.Count)
            {
                OnBannerClicked?.Invoke(_bannerDataIndices[visualIndex]);
            }
        }

        private async UniTaskVoid RecalculateLayoutNextFrame()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            if (this == null || !gameObject.activeInHierarchy || _viewport == null || _content == null)
            {
                return;
            }

            float newWidth = _viewport.rect.width;
            if (newWidth <= 0f || Mathf.Approximately(newWidth, _itemWidth))
            {
                return;
            }

            _snapTween?.Kill();
            _state = ScrollState.Idle;
            _itemWidth = newWidth;

            for (int i = 0; i < _bannerItems.Count; i++)
            {
                var item = _bannerItems[i];
                if (item != null)
                {
                    item.sizeDelta = new Vector2(_itemWidth, 0f);
                }
            }

            NormalizeItemPositions();

            _content.anchoredPosition = new Vector2(0f, _content.anchoredPosition.y);
            _autoScrollTimer = 0f;
            _currentDataIndex = _bannerDataIndices.Count > 0 ? _bannerDataIndices[0] : 0;
        }

        private void CancelLoad()
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = null;
        }

        private void ClearBanners()
        {
            foreach (var instance in _bannerInstances)
            {
                if (instance == null)
                {
                    continue;
                }

                var button = instance.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }

                if (_assetService != null)
                {
                    _assetService.ReleaseInstance(instance);
                }
                else
                {
                    Destroy(instance);
                }
            }

            _bannerInstances.Clear();
            _bannerItems.Clear();
            _bannerDataIndices.Clear();
            _isInitialized = false;
        }
    }
}
