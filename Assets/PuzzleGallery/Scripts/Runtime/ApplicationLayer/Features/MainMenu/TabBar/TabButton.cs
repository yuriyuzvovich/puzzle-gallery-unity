using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGallery.Features.MainMenu
{
    public class TabButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _content;
        [SerializeField] private TMP_Text _label;

        [Header("Settings")]
        [SerializeField] private FilterType _filterType;
        [SerializeField] private float _selectedScale = 1.1f;
        [SerializeField] private float _animationDuration = 0.3f;

        [Header("Label Colors")]
        [SerializeField] private Color _normalColor = Color.gray;
        [SerializeField] private Color _selectedColor = Color.white;

        [Header("Press Animation")]
        [SerializeField] private bool _enablePressAnimation = true;
        [SerializeField] private float _pressScale = 0.9f;
        [SerializeField] private float _pressDuration = 0.1f;

        public FilterType FilterType => _filterType;
        public Button Button => _button;

        private bool _isSelected;
        private Tween _scaleTween;
        private Tween _colorTween;
        private Vector3 _baseScale;

        private void Awake()
        {
            if (!_button) throw new System.Exception("Button reference is not assigned in TabButton.");
            if (!_content) throw new System.Exception("Content reference is not assigned in TabButton.");

            _baseScale = _content.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_enablePressAnimation || _content == null)
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
            if (!_enablePressAnimation || _content == null)
            {
                return;
            }

            _scaleTween?.Kill();
            float targetScale = _isSelected ? _selectedScale : 1f;
            _scaleTween = _content
                .DOScale(_baseScale * targetScale, _pressDuration)
                .SetEase(Ease.OutBack);
        }

        public void SetSelected(bool selected, bool animated = true)
        {
            if (_isSelected == selected)
            {
                return;
            }

            _isSelected = selected;

            _scaleTween?.Kill();
            _colorTween?.Kill();

            float targetScale = selected ? _selectedScale : 1f;
            Color targetColor = selected ? _selectedColor : _normalColor;

            if (animated)
            {
                _scaleTween = _content
                    .DOScale(_baseScale * targetScale, _animationDuration)
                    .SetEase(selected ? Ease.OutBack : Ease.OutQuad);

                if (_label != null)
                {
                    _colorTween = _label
                        .DOColor(targetColor, _animationDuration)
                        .SetEase(Ease.OutQuad);
                }
            }
            else
            {
                _content.localScale = _baseScale * targetScale;

                if (_label != null)
                {
                    _label.color = targetColor;
                }
            }
        }

        private void OnDestroy()
        {
            _scaleTween?.Kill();
            _colorTween?.Kill();
        }
    }
}