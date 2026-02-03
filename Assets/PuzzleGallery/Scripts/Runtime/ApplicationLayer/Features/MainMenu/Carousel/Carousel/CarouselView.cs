using System;
using System.Collections.Generic;
using PuzzleGallery.Scripts.UI;
using PuzzleGallery.Services.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Features.MainMenu
{
    public class CarouselView : MonoBehaviour, ICarouselView
    {
        [Header("References")]
        [SerializeField] private SimpleCarouselScrollView _scrollView;
        [SerializeField] private Transform _indicatorParent;
        [SerializeField] private GameObject _indicatorPrefab;

        [Header("Indicators Layout")]
        [SerializeField] private Vector2 _indicatorSize = new Vector2(10f, 10f);
        [SerializeField] private float _indicatorSpacing = 12f;

        [Header("Audio")]
        [SerializeField] private bool _playSound = true;

        private IReadOnlyList<BannerData> _banners;
        private CarouselIndicatorComponent[] _indicators;
        private int _currentIndex;
        private bool _configuredAutoScrollEnabled = true;

        public bool IsVisible => gameObject.activeSelf;
        public int CurrentIndex => _currentIndex;

        public float AutoScrollInterval
        {
            get => _scrollView != null ? _scrollView.AutoScrollInterval : 5f;
            set { if (_scrollView != null) _scrollView.AutoScrollInterval = value; }
        }

        public bool AutoScrollEnabled
        {
            get => _configuredAutoScrollEnabled;
            set
            {
                _configuredAutoScrollEnabled = value;
                if (_scrollView != null)
                {
                    _scrollView.AutoScrollEnabled = value;
                }
            }
        }

        public event Action<int> OnBannerClicked;
        public event Action<int> OnBannerChanged;

        private void Awake()
        {
            if (_scrollView != null)
            {
                _configuredAutoScrollEnabled = _scrollView.AutoScrollEnabled;
                _scrollView.OnSelectionChanged += HandleSelectionChanged;
                _scrollView.OnBannerClicked += HandleBannerClicked;
            }
        }

        private void OnDestroy()
        {
            if (_scrollView != null)
            {
                _scrollView.OnSelectionChanged -= HandleSelectionChanged;
                _scrollView.OnBannerClicked -= HandleBannerClicked;
            }

            if (_indicatorParent != null)
            {
                foreach (Transform child in _indicatorParent)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void SetBanners(IReadOnlyList<BannerData> banners)
        {
            var nextBanners = banners ?? Array.Empty<BannerData>();
            if (AreBannersEquivalent(_banners, nextBanners))
            {
                _banners = nextBanners;
                return;
            }

            _banners = nextBanners;
            _currentIndex = 0;

            CreateIndicators();

            if (_banners.Count == 0)
            {
                return;
            }

            if (_scrollView != null)
            {
                _scrollView.SetBanners(_banners);
            }
        }

        public void ScrollToIndex(int index, bool animated = true)
        {
            if (_scrollView == null || _banners == null || index < 0 || index >= _banners.Count)
            {
                return;
            }

            _scrollView.ScrollToIndex(index, animated);
        }

        public void ScrollToNext()
        {
            if (_scrollView == null || _banners == null || _banners.Count == 0)
            {
                return;
            }

            _scrollView.ScrollToNext();
        }

        public void Show()
        {
            gameObject.SetActive(true);

            if (_scrollView != null)
            {
                _scrollView.AutoScrollEnabled = _configuredAutoScrollEnabled;
                _scrollView.ResetAutoScrollTimer();
            }
        }

        public void Hide()
        {
            if (_scrollView != null)
            {
                _scrollView.StopScrolling();
                _scrollView.AutoScrollEnabled = false;
            }

            gameObject.SetActive(false);
        }

        private static bool AreBannersEquivalent(IReadOnlyList<BannerData> left, IReadOnlyList<BannerData> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                bool leftEmpty = left == null || left.Count == 0;
                bool rightEmpty = right == null || right.Count == 0;
                return leftEmpty && rightEmpty;
            }

            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                var leftBanner = left[i];
                var rightBanner = right[i];

                if (leftBanner == null || rightBanner == null)
                {
                    if (leftBanner != rightBanner)
                    {
                        return false;
                    }

                    continue;
                }

                if (!string.Equals(leftBanner.AssetKey, rightBanner.AssetKey, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private void HandleBannerClicked(int index)
        {
            OnBannerClicked?.Invoke(index);
        }

        private void HandleSelectionChanged(int index)
        {
            if (_currentIndex == index)
            {
                return;
            }

            _currentIndex = index;
            UpdateIndicators();
            PlaySwipeSound();
            OnBannerChanged?.Invoke(_currentIndex);
        }

        private void CreateIndicators()
        {
            if (_indicatorParent != null)
            {
                foreach (Transform child in _indicatorParent)
                {
                    Destroy(child.gameObject);
                }
            }

            if (_indicatorParent == null || _banners == null || _banners.Count <= 1)
            {
                if (_indicatorParent != null)
                {
                    _indicatorParent.gameObject.SetActive(false);
                }

                _indicators = null;
                return;
            }

            _indicatorParent.gameObject.SetActive(true);

            _indicators = new CarouselIndicatorComponent[_banners.Count];

            for (int i = 0; i < _banners.Count; i++)
            {
                var go = Instantiate(_indicatorPrefab, _indicatorParent);
                var indicator = go.GetComponent<CarouselIndicatorComponent>();
                if (indicator == null)
                {
                    Logs.Warning("CarouselView: Indicator prefab missing IndicatorComponent.", go);
                    continue;
                }

                _indicators[i] = indicator;
            }

            ApplyIndicatorLayout();

            UpdateIndicators(animated: false);
        }

        private void UpdateIndicators(bool animated = true)
        {
            if (_indicators == null)
            {
                return;
            }

            ApplyIndicatorLayout();

            for (int i = 0; i < _indicators.Length; i++)
            {
                if (_indicators[i] == null)
                {
                    continue;
                }

                bool isSelected = i == _currentIndex;
                _indicators[i].SetSelected(isSelected, animated);
            }
        }

        private void ApplyIndicatorLayout()
        {
            if (_indicatorParent == null)
            {
                return;
            }

            if (_indicatorParent.TryGetComponent<HorizontalLayoutGroup>(out var horizontalLayout))
            {
                horizontalLayout.spacing = _indicatorSpacing;
            }

            if (_indicatorParent.TryGetComponent<VerticalLayoutGroup>(out var verticalLayout))
            {
                verticalLayout.spacing = _indicatorSpacing;
            }

            if (_indicatorParent.TryGetComponent<GridLayoutGroup>(out var gridLayout))
            {
                gridLayout.spacing = new Vector2(_indicatorSpacing, _indicatorSpacing);
                gridLayout.cellSize = _indicatorSize;
            }

            if (_indicators != null)
            {
                for (int i = 0; i < _indicators.Length; i++)
                {
                    if (_indicators[i] == null)
                    {
                        continue;
                    }

                    var rt = _indicators[i].GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.sizeDelta = _indicatorSize;
                    }
                }
            }

            if (_indicatorParent is RectTransform parentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }
        }

        private void PlaySwipeSound()
        {
        }
    }
}
