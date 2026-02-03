using DG.Tweening;
using PuzzleGallery.Core;
using PuzzleGallery.Core.Devices;
using PuzzleGallery.Core.ServiceLocator;
using PuzzleGallery.Features.MainMenu.Data;
using PuzzleGallery.Services.Screen.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Features.MainMenu.Layout
{
    public sealed class TabBarLayoutController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _containerRect;
        [SerializeField] private RectTransform[] _tabRects;
        [SerializeField] private RectTransform _selectionIndicator;
        [SerializeField] private TabBarLayoutConfig _layoutConfig;
        [SerializeField] private TabBarView _tabBarView;

        [Header("Indicator Styling")]
        [SerializeField] private Image _indicatorImage;
        [SerializeField] private Color _indicatorColor = new Color(1f, 0f, 0.5f, 1f);

        [Header("Dividers")]
        [SerializeField] private RectTransform[] _dividers;
        [SerializeField] private Color _dividerColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        [Header("Animation")]
        [SerializeField] private float _indicatorAnimDuration = 0.25f;
        [SerializeField] private Ease _indicatorEase = Ease.OutQuad;

        private IScreenService _screenService;
        private int _currentSelectedIndex;
        private Tween _indicatorTween;

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

            if (_tabBarView != null)
            {
                _tabBarView.OnTabIndexChanged += HandleTabIndexChanged;
            }

            InitializeIndicator();
            InitializeDividers();
            ApplyLayout();
            UpdateIndicatorPosition(animated: false);
        }

        private void OnDestroy()
        {
            _indicatorTween?.Kill();

            if (_screenService != null)
            {
                _screenService.OnOrientationChanged -= HandleOrientationChanged;
                _screenService.OnAppDeviceTypeChanged -= HandleDeviceTypeChanged;
            }

            if (_tabBarView != null)
            {
                _tabBarView.OnTabIndexChanged -= HandleTabIndexChanged;
            }
        }

        private void HandleOrientationChanged(ScreenOrientation orientation)
        {
            ApplyLayout();
            UpdateIndicatorPosition(animated: false);
        }

        private void HandleDeviceTypeChanged(AppDeviceType deviceType)
        {
            ApplyLayout();
            UpdateIndicatorPosition(animated: false);
        }

        private void HandleTabIndexChanged(int tabIndex)
        {
            SetSelectedTab(tabIndex, animated: true);
        }

        public void ApplyLayout()
        {
            if (_tabRects == null || _tabRects.Length == 0)
            {
                return;
            }

            var profile = GetCurrentProfile();
            int tabCount = _tabRects.Length;
            float edgePadding = profile.EdgePadding;
            float tabSpacing = profile.TabSpacing;

            float containerWidth = _containerRect != null ? _containerRect.rect.width : 0f;

            if (containerWidth <= 0f)
            {
                return;
            }

            float edgePaddingRatio = edgePadding / containerWidth;
            float spacingRatio = tabSpacing / containerWidth;

            float usableWidthRatio = 1f - (edgePaddingRatio * 2f);
            float totalSpacingRatio = spacingRatio * (tabCount - 1);
            float tabWidthRatio = (usableWidthRatio - totalSpacingRatio) / tabCount;

            for (int i = 0; i < tabCount; i++)
            {
                if (_tabRects[i] == null)
                {
                    continue;
                }

                float startX = edgePaddingRatio + (i * (tabWidthRatio + spacingRatio));
                float endX = startX + tabWidthRatio;

                SetTabAnchors(_tabRects[i], startX, endX);
            }

            UpdateDividerPositions(profile);
            UpdateIndicatorSize(profile);
            UpdateIndicatorPosition(animated: false);
        }

        private void UpdateDividerPositions(TabBarLayoutProfile profile)
        {
            if (_dividers == null || _tabRects == null || _containerRect == null)
            {
                return;
            }

            int dividerCount = Mathf.Min(_dividers.Length, _tabRects.Length - 1);
            float verticalPadding = profile.DividerVerticalPadding;
            float containerWidth = _containerRect.rect.width;

            for (int i = 0; i < _dividers.Length; i++)
            {
                if (_dividers[i] == null)
                {
                    continue;
                }

                if (i >= dividerCount)
                {
                    _dividers[i].gameObject.SetActive(false);
                    continue;
                }

                _dividers[i].gameObject.SetActive(true);

                var leftTab = _tabRects[i];
                var rightTab = _tabRects[i + 1];

                if (leftTab == null || rightTab == null)
                {
                    continue;
                }

                float leftAnchorMax = leftTab.anchorMax.x;
                float rightAnchorMin = rightTab.anchorMin.x;
                float centerAnchorX = (leftAnchorMax + rightAnchorMin) / 2f;

                _dividers[i].anchorMin = new Vector2(centerAnchorX, 0f);
                _dividers[i].anchorMax = new Vector2(centerAnchorX, 1f);
                _dividers[i].pivot = new Vector2(0.5f, 0.5f);

                _dividers[i].sizeDelta = new Vector2(profile.DividerWidth, 0f);
                _dividers[i].anchoredPosition = Vector2.zero;
                _dividers[i].offsetMin = new Vector2(-profile.DividerWidth / 2f, verticalPadding);
                _dividers[i].offsetMax = new Vector2(profile.DividerWidth / 2f, -verticalPadding);
            }
        }

        public void SetSelectedTab(int tabIndex, bool animated = true)
        {
            if (tabIndex < 0 || _tabRects == null || tabIndex >= _tabRects.Length)
            {
                return;
            }

            _currentSelectedIndex = tabIndex;
            UpdateIndicatorPosition(animated);
        }

        private void InitializeIndicator()
        {
            if (_indicatorImage != null)
            {
                _indicatorImage.color = _indicatorColor;
            }

            if (_selectionIndicator != null)
            {
                _selectionIndicator.anchorMin = new Vector2(0f, 0f);
                _selectionIndicator.anchorMax = new Vector2(0f, 0f);
                _selectionIndicator.pivot = new Vector2(0f, 0f);
            }
        }

        private void InitializeDividers()
        {
            if (_dividers == null)
            {
                return;
            }

            foreach (var divider in _dividers)
            {
                if (divider == null)
                {
                    continue;
                }

                var image = divider.GetComponent<Image>();
                if (image != null)
                {
                    image.color = _dividerColor;
                }
            }
        }

        private void UpdateIndicatorPosition(bool animated)
        {
            if (_selectionIndicator == null || _tabRects == null || _tabRects.Length == 0)
            {
                return;
            }

            int tabIndex = Mathf.Clamp(_currentSelectedIndex, 0, _tabRects.Length - 1);
            var targetTab = _tabRects[tabIndex];
            if (targetTab == null || _containerRect == null)
            {
                return;
            }

            _selectionIndicator.anchorMin = new Vector2(0f, 0f);
            _selectionIndicator.anchorMax = new Vector2(0f, 0f);
            _selectionIndicator.pivot = new Vector2(0f, 0f);

            var profile = GetCurrentProfile();

            Vector3[] tabCorners = new Vector3[4];
            targetTab.GetWorldCorners(tabCorners);

            Vector3 bottomLeft = _containerRect.InverseTransformPoint(tabCorners[0]);
            Vector3 bottomRight = _containerRect.InverseTransformPoint(tabCorners[3]);
            Vector3 topLeft = _containerRect.InverseTransformPoint(tabCorners[1]);

            Rect containerRect = _containerRect.rect;
            float targetX = bottomLeft.x - containerRect.xMin;
            float targetWidth = bottomRight.x - bottomLeft.x;
            float tabHeight = topLeft.y - bottomLeft.y;

            float indicatorHeight = tabHeight * profile.IndicatorHeightRatio;
            float indicatorOffset = tabHeight * profile.IndicatorBottomOffsetRatio;

            Vector2 targetPos = new Vector2(targetX, indicatorOffset);
            Vector2 targetSize = new Vector2(targetWidth, indicatorHeight);

            _indicatorTween?.Kill();

            if (animated && gameObject.activeInHierarchy)
            {
                _indicatorTween = DOTween.Sequence()
                    .Join(_selectionIndicator.DOAnchorPos(targetPos, _indicatorAnimDuration).SetEase(_indicatorEase))
                    .Join(_selectionIndicator.DOSizeDelta(targetSize, _indicatorAnimDuration).SetEase(_indicatorEase));
            }
            else
            {
                _selectionIndicator.anchoredPosition = targetPos;
                _selectionIndicator.sizeDelta = targetSize;
            }
        }

        private void UpdateIndicatorSize(TabBarLayoutProfile profile)
        {
            if (_selectionIndicator == null || _containerRect == null)
            {
                return;
            }

            float containerHeight = _containerRect.rect.height;
            float indicatorHeight = containerHeight * profile.IndicatorHeightRatio;

            var currentSize = _selectionIndicator.sizeDelta;
            _selectionIndicator.sizeDelta = new Vector2(currentSize.x, indicatorHeight);
        }

        private static void SetTabAnchors(RectTransform target, float minX, float maxX)
        {
            target.anchorMin = new Vector2(minX, 0f);
            target.anchorMax = new Vector2(maxX, 1f);
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }

        private TabBarLayoutProfile GetCurrentProfile()
        {
            if (_layoutConfig == null)
            {
                return new TabBarLayoutProfile
                {
                    TabSpacing = 8f,
                    EdgePadding = 16f,
                    IndicatorHeightRatio = 0.05f,
                    IndicatorBottomOffsetRatio = 0f,
                    DividerWidth = 1f,
                    DividerVerticalPadding = 8f
                };
            }

            bool isTablet = _screenService != null && _screenService.IsTablet;
            return _layoutConfig.GetProfile(isTablet ? AppDeviceType.Tablet : AppDeviceType.Phone);
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyLayout();
            UpdateIndicatorPosition(animated: false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_indicatorImage != null)
            {
                _indicatorImage.color = _indicatorColor;
            }

            if (_dividers != null)
            {
                foreach (var divider in _dividers)
                {
                    if (divider == null)
                    {
                        continue;
                    }

                    var image = divider.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = _dividerColor;
                    }
                }
            }
        }
#endif
    }
}
