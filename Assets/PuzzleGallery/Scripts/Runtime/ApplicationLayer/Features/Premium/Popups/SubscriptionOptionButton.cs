using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGallery.Features.Premium
{
    public sealed class SubscriptionOptionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private TextMeshProUGUI _savingsText;
        [SerializeField] private Image _indicatorImage;
        [SerializeField] private RectTransform _indicatorRoot;

        [Header("Selection Colors")]
        [SerializeField] private Color _normalTextColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color _selectedTextColor = Color.white;

        [Header("Indicator Sprites")]
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Sprite _unselectedSprite;

        [Header("Indicator Settings")]
        [SerializeField] private float _selectedIndicatorScale = 1.3f;
        [SerializeField] private float _indicatorAnimationDuration = 0.2f;

        [Header("Animation Settings")]
        [SerializeField] private float _selectedScale = 1.05f;
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private bool _enablePressAnimation = true;
        [SerializeField] private float _pressScale = 0.95f;
        [SerializeField] private float _pressDuration = 0.15f;

        public Button Button => _button;
        public SubscriptionType SubscriptionType { get; private set; }

        private bool _isSelected;
        private bool _isInitialized;
        private Tween _scaleTween;
        private Tween _priceColorTween;
        private Tween _durationColorTween;
        private Tween _savingsColorTween;
        private Tween _indicatorColorTween;
        private Tween _indicatorScaleTween;
        private Vector3 _baseScale = Vector3.one;
        private Vector3 _indicatorBaseScale = Vector3.one;

        private void OnDestroy()
        {
            _scaleTween?.Kill();
            _priceColorTween?.Kill();
            _durationColorTween?.Kill();
            _savingsColorTween?.Kill();
            _indicatorColorTween?.Kill();
            _indicatorScaleTween?.Kill();
        }

        public void SetData(SubscriptionOption option)
        {
            SubscriptionType = option.Type;
            SetSelected(false, animated : false);
        }

        public void SetSelected(bool selected, bool animated = true)
        {
            if (_isSelected == selected && _isInitialized)
            {
                return;
            }

            _isSelected = selected;
            _isInitialized = true;

            _scaleTween?.Kill();
            _priceColorTween?.Kill();
            _durationColorTween?.Kill();
            _savingsColorTween?.Kill();
            _indicatorColorTween?.Kill();
            _indicatorScaleTween?.Kill();

            float targetScale = selected ? _selectedScale : 1f;
            Color targetTextColor = selected ? _selectedTextColor : _normalTextColor;

            if (animated)
            {
                _scaleTween = _content
                    .DOScale(_baseScale * targetScale, _animationDuration)
                    .SetEase(selected ? Ease.OutBack : Ease.OutQuad);

                _priceColorTween = _priceText
                    .DOColor(targetTextColor, _animationDuration)
                    .SetEase(Ease.OutQuad);

                if (_savingsText != null)
                {
                    _savingsColorTween = _savingsText
                        .DOColor(targetTextColor, _animationDuration)
                        .SetEase(Ease.OutQuad);
                }
            }
            else
            {
                _content.localScale = _baseScale * targetScale;
                _priceText.color = targetTextColor;
                if (_savingsText)
                {
                    _savingsText.color = targetTextColor;
                }
            }

            UpdateIndicator(selected, animated);
        }

        private void UpdateIndicator(bool selected, bool animated)
        {
            var targetSprite = selected ? _selectedSprite : _unselectedSprite;
            _indicatorImage.sprite = targetSprite;

            Color targetIndicatorColor = selected ? _selectedTextColor : _normalTextColor;
            float targetIndicatorScale = selected ? _selectedIndicatorScale : 1f;

            if (animated)
            {
                _indicatorColorTween = _indicatorImage
                    .DOColor(targetIndicatorColor, _indicatorAnimationDuration)
                    .SetEase(Ease.OutQuad);

                if (_indicatorRoot != null)
                {
                    _indicatorScaleTween = _indicatorRoot
                        .DOScale(_indicatorBaseScale * targetIndicatorScale, _indicatorAnimationDuration)
                        .SetEase(Ease.OutQuad);
                }
            }
            else
            {
                _indicatorImage.color = targetIndicatorColor;
                _indicatorRoot.localScale = _indicatorBaseScale * targetIndicatorScale;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_enablePressAnimation)
            {
                return;
            }

            _scaleTween?.Kill();
            float targetScale = _isSelected ? _selectedScale * _pressScale : _pressScale;
            _scaleTween = _content
                .DOScale(_baseScale * targetScale, _pressDuration)
                .SetEase(Ease.OutQuad);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_enablePressAnimation)
            {
                return;
            }

            _scaleTween?.Kill();
            float targetScale = _isSelected ? _selectedScale : 1f;
            _scaleTween = _content
                .DOScale(_baseScale * targetScale, _pressDuration)
                .SetEase(Ease.OutBack);
        }
    }
}